using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class loadDetails : MonoBehaviour
{
    [SerializeField]
    TMP_InputField nameField;
    [SerializeField]
    TMP_InputField emailField;
    [SerializeField]
    TMP_InputField phoneField;
    [SerializeField]
    TMP_InputField verificationField;
    [SerializeField]
    GameObject unverifiedIcon;
    [SerializeField]
    GameObject verifiedIcon;
    [SerializeField]
    Button verificationButton;
    [SerializeField]
    TMP_Text verificationMessage;
    PlayerManager verificationOwner;
    int verificationVersion;
    const float VerificationCooldownSeconds = 10;
    void OnEnable()
    {
        PlayerManager.StateChanged += LoadDetailsUser;
        LoadDetailsUser();
        if (PlayerManager.instance != null)
            PlayerManager.instance.RequestRefresh();
    }

    void LoadDetailsUser()
    {
        var manager = PlayerManager.instance;
        Set(nameField, manager != null ? manager.nama : "");
        Set(emailField, manager != null ? manager.email : "");
        Set(phoneField, manager != null ? manager.notel : "");
        bool verified = manager != null && manager.IsVerified;
        string verificationStatus;
        if (manager == null || !manager.ProfileFresh)
            verificationStatus = "Status akaun belum tersedia";
        else
            verificationStatus = verified ? "Email disahkan" : "Email belum disahkan";
        Set(verificationField, verificationStatus);
        if (unverifiedIcon != null)
            unverifiedIcon.SetActive(!verified);
        if (verifiedIcon != null)
            verifiedIcon.SetActive(verified);
    }

    static void Set(TMP_InputField field, string value)
    {
        if (field != null)
        {
            field.readOnly = true;
            field.SetTextWithoutNotify(value ?? "");
        }
    }

    public void SendEmailToUser()
    {
        var manager = PlayerManager.instance;
        if (manager == null || !manager.IsLoggedIn || !manager.ProfileFresh || manager.IsVerified || manager.IsSendingVerification || Time.unscaledTime < manager.VerificationAllowedAt)
            return;
        verificationOwner = manager;
        verificationVersion = manager.SessionVersion;
        manager.IsSendingVerification = true;
        manager.VerificationAllowedAt = Time.unscaledTime + VerificationCooldownSeconds;
        manager.NotifyStateChanged();
        StartCoroutine(SendEmailVerification(manager));
    }

    IEnumerator SendEmailVerification(PlayerManager manager)
    {
        int version = manager.SessionVersion;
        try
        {
            var form = new WWWForm();
            form.AddField("_email", manager.email);
            form.AddField("_name", manager.nama);
            Player.Result result = null;
            yield return manager.Requests.Post(Player.Settings != null ? Player.Settings.verificationEndpoint : "", form, value => result = value);
            if (version != manager.SessionVersion)
                yield break;
            string message = VerificationResponse(result);
            if (verificationMessage != null)
                verificationMessage.text = message;
            manager.VerificationAllowedAt = Time.unscaledTime + VerificationCooldownSeconds;
        }
        finally
        {
            ReleaseVerification();
        }
    }

    static string VerificationResponse(Player.Result result)
    {
        if (result != null && result.success && result.body == "success")
            return "Berjaya dihantar. Sila semak email anda.";
        if (result != null && !result.success)
            return result.error;
        return "Email belum dapat dihantar. Sila cuba lagi.";
    }

    void ReleaseVerification()
    {
        var owner = verificationOwner;
        verificationOwner = null;
        // A closed panel must not release a newer account's verification request.
        if (owner != null && owner.SessionVersion == verificationVersion)
        {
            owner.IsSendingVerification = false;
            owner.NotifyStateChanged();
        }
    }

    void OnDisable()
    {
        PlayerManager.StateChanged -= LoadDetailsUser;
        StopAllCoroutines();
        ReleaseVerification();
    }
}
