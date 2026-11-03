using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{
    [SerializeField]
    TMP_InputField emailText;
    [SerializeField]
    TMP_InputField passText;
    [SerializeField]
    Transform emailWarningTransform;
    [SerializeField]
    TMP_Text emailWarningText;
    [SerializeField]
    Button btnLog;
    [SerializeField]
    Transform Notification;
    [SerializeField]
    Transform Notice;
    [SerializeField]
    Transform LogoutPanel;
    [SerializeField]
    Animator notificationAnimator;
    [SerializeField]
    TMP_Text notificationText;
    [SerializeField]
    TMP_Text noticeText;
    [SerializeField]
    TMP_Text deviceText;
    [SerializeField]
    GameObject noticeLoading;
    [SerializeField]
    GameObject noticeError;
    [SerializeField]
    GameObject noticeSuccess;
    bool isPressed;
    int attemptVersion;
    public const string MatchEmailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";
    public static bool IsEmail(string email) => !string.IsNullOrWhiteSpace(email) && email.Length <= 254 && Regex.IsMatch(email.Trim(), MatchEmailPattern);
    public static Player.PlayerInfo CreateFromJSON(string json) => Player.TryProfile(json, out var value) ? value : null;
    public static Player.PlayerSubscription CreateFromJSON2(string json) => Player.TrySubscription(json, out var value) ? value : null;
    void OnEnable()
    {
        if (emailText != null)
            emailText.onValueChanged.AddListener(OnInput);
        if (passText != null)
            passText.onValueChanged.AddListener(OnInput);
        PlayerManager.StateChanged += Render;
        Render();
    }

    void Start()
    {
        if (emailWarningTransform != null)
            emailWarningTransform.gameObject.SetActive(false);
        if (PlayerManager.instance != null && !string.IsNullOrEmpty(PlayerManager.instance.LastError))
            ShowMessage(PlayerManager.instance.LastError);
        Render();
    }

    void OnInput(string value)
    {
        CheckEmailValidation();
    }

    public void CheckEmailValidation()
    {
        bool invalid = emailText != null && !string.IsNullOrWhiteSpace(emailText.text) && !IsEmail(emailText.text);
        if (emailWarningTransform != null)
            emailWarningTransform.gameObject.SetActive(invalid);
        if (emailWarningText != null)
            emailWarningText.text = invalid ? "Invalid Email" : "";
        Render();
    }

    void Render()
    {
        if (btnLog != null)
            btnLog.interactable = CanSubmit();
    }

    bool CanSubmit()
    {
        var manager = PlayerManager.instance;
        bool sessionReady = manager != null && !manager.IsLoggingOut && !manager.IsAuthenticating;
        bool fieldsReady = emailText != null && passText != null
            && IsEmail(emailText.text) && !string.IsNullOrEmpty(passText.text);
        return !isPressed && sessionReady && fieldsReady;
    }

    public void LogMasuk()
    {
        if (isPressed || PlayerManager.instance == null || PlayerManager.instance.IsLoggingOut)
            return;
        if (emailText == null || passText == null || !IsEmail(emailText.text) || string.IsNullOrEmpty(passText.text))
        {
            ShowMessage("Sila isi email dan kata laluan yang sah.");
            return;
        }

        isPressed = true;
        Render();
        StartCoroutine(Login());
    }

    IEnumerator Login()
    {
        var manager = PlayerManager.instance;
        manager.BeginLogin();
        attemptVersion = manager.SessionVersion;
        ShowNotice("LOADING..", true);
        try
        {
            var form = new WWWForm();
            form.AddField("_email", emailText.text.Trim());
            form.AddField("_password", passText.text);
            form.AddField("_device_name", SystemInfo.deviceName + " " + SystemInfo.deviceModel);
            form.AddField("_game_name", Player.Settings != null ? Player.Settings.gameIdentifier : "");
            Player.Result result = null;
            yield return manager.Requests.Post(Player.Settings != null ? Player.Settings.loginEndpoint : "", form, value => result = value);
            if (manager.SessionVersion != attemptVersion)
                yield break;
            if (result == null || !result.success)
            {
                ShowNotice(result?.error ?? "Tiada respons server.", false);
                yield break;
            }

            if (!Player.TryProfile(result.body, out var profile))
            {
                string message = result.body == "invalid email" ? "Email tidak berdaftar." : result.body == "password is not verify" ? "Kata laluan tidak sah." : "Respons log masuk tidak sah. Sila cuba lagi.";
                ShowNotice(message, false);
                ShowMessage(message);
                yield break;
            }

            if (!string.Equals(profile.email, emailText.text.Trim(), System.StringComparison.OrdinalIgnoreCase))
            {
                ShowNotice("Respons akaun tidak sepadan.", false);
                yield break;
            }

            if (Player.Settings.enforceServerLoginStatus && profile.login_logs != 0)
            {
                manager.PendingLogoutUserId = profile.id_user;
                manager.PendingLogoutActionToken = profile.session_action_token;
                if (string.IsNullOrEmpty(profile.session_action_token))
                {
                    ShowNotice("Server session response is incomplete. Update the PHP backend.", false);
                    yield break;
                }

                if (LogoutPanel != null)
                    LogoutPanel.gameObject.SetActive(true);
                if (deviceText != null)
                    deviceText.text = profile.device_name ?? "Peranti lain";
                if (Notice != null)
                    Notice.gameObject.SetActive(false);
                ShowMessage("Anda sudah log masuk dari peranti yang lain. Log keluar dahulu.");
                yield break;
            }

            manager.AcceptLogin(profile);
            attemptVersion = manager.SessionVersion;
            yield return manager.RefreshNow();
            if (manager.SessionVersion != attemptVersion || !manager.IsLoggedIn)
            {
                ShowNotice(manager.LastError ?? "Sila log masuk semula.", false);
                yield break;
            }

            passText.SetTextWithoutNotify("");
            if (Notice != null)
                Notice.gameObject.SetActive(false);
            if (Application.CanStreamedLevelBeLoaded(manager.loadScene))
                SceneManager.LoadScene(manager.loadScene);
            else
                ShowNotice("Laman utama tiada dalam Build Settings.", false);
        }
        finally
        {
            isPressed = false;
            if (manager != null && manager.SessionVersion == attemptVersion)
                manager.EndLogin();
            Render();
        }
    }

    void ShowNotice(string message, bool loading)
    {
        if (Notice != null)
            Notice.gameObject.SetActive(true);
        if (noticeLoading != null)
            noticeLoading.SetActive(loading);
        if (noticeError != null)
            noticeError.SetActive(!loading);
        if (noticeSuccess != null)
            noticeSuccess.SetActive(false);
        if (noticeText != null)
            noticeText.text = message;
    // Keep actionable errors visible until retry or leaving the form.
    }

    void ShowMessage(string message)
    {
        if (notificationText != null)
            notificationText.text = message;
        if (notificationAnimator != null)
            notificationAnimator.Play("ShowNotificationStart");
    }

    void OnDisable()
    {
        PlayerManager.StateChanged -= Render;
        if (emailText != null)
            emailText.onValueChanged.RemoveListener(OnInput);
        if (passText != null)
            passText.onValueChanged.RemoveListener(OnInput);
        StopAllCoroutines();
        if (isPressed && PlayerManager.instance != null && PlayerManager.instance.SessionVersion == attemptVersion)
            PlayerManager.instance.EndLogin();
        isPressed = false;
        if (Notice != null)
            Notice.gameObject.SetActive(false);
    }
}
