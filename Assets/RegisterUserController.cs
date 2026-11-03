using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegisterUserController : MonoBehaviour
{
    [SerializeField]
    TMP_InputField namaText;
    [SerializeField]
    TMP_InputField emailText;
    [SerializeField]
    TMP_InputField notelText;
    [SerializeField]
    TMP_InputField passText;
    [SerializeField]
    Transform emailWarningTransform;
    [SerializeField]
    Transform notelWarningTransform;
    [SerializeField]
    TMP_Text emailWarningText;
    [SerializeField]
    TMP_Text phoneWarningText;
    public TextMeshProUGUI text;
    public Button btnDaftar;
    [SerializeField]
    Transform Notice;
    [SerializeField]
    Transform EmailNotice;
    [SerializeField]
    TMP_Text noticeText;
    [SerializeField]
    GameObject noticeLoading;
    [SerializeField]
    GameObject noticeError;
    [SerializeField]
    GameObject noticeSuccess;
    bool isLoading;
    enum NoticeState { Loading, Error, Success }
    public const string MatchEmailPattern = LoginController.MatchEmailPattern;
    public const string MatchPhoneNumberPattern = @"^01[0-46-9][0-9]{7,8}$";
    public static bool IsEmail(string email) => LoginController.IsEmail(email);
    public static bool IsPhoneNbr(string number) => number != null && Regex.IsMatch(number.Trim(), MatchPhoneNumberPattern);
    void OnEnable()
    {
        foreach (var field in new[]
        {
            namaText,
            emailText,
            notelText,
            passText
        })
            if (field != null)
                field.onValueChanged.AddListener(OnInput);
        if (emailWarningTransform != null)
            emailWarningTransform.gameObject.SetActive(false);
        if (notelWarningTransform != null)
            notelWarningTransform.gameObject.SetActive(false);
        Render();
    }

    void OnInput(string value)
    {
        Render();
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

    public void CheckPhoneValidation()
    {
        bool invalid = notelText != null && !string.IsNullOrWhiteSpace(notelText.text) && !IsPhoneNbr(notelText.text);
        if (notelWarningTransform != null)
            notelWarningTransform.gameObject.SetActive(invalid);
        if (phoneWarningText != null)
            phoneWarningText.text = invalid ? "Invalid Phone Number" : "";
        Render();
    }

    bool Valid()
    {
        bool fieldsAssigned = namaText != null && emailText != null && notelText != null && passText != null;
        if (!fieldsAssigned)
            return false;

        return !string.IsNullOrWhiteSpace(namaText.text)
            && IsEmail(emailText.text)
            && IsPhoneNbr(notelText.text)
            && !string.IsNullOrEmpty(passText.text);
    }
    void Render()
    {
        if (btnDaftar != null)
            btnDaftar.interactable = !isLoading && Valid();
    }

    public void DaftarPengguna()
    {
        if (isLoading || !Valid() || PlayerManager.instance == null)
            return;
        isLoading = true;
        Render();
        StartCoroutine(Register());
    }

    IEnumerator Register()
    {
        ShowNotice("LOADING..", NoticeState.Loading);
        try
        {
            var form = new WWWForm();
            form.AddField("_name", namaText.text.Trim());
            form.AddField("_email", emailText.text.Trim());
            form.AddField("_notel", notelText.text.Trim());
            form.AddField("_password", passText.text);
            form.AddField("_game_name", Player.Settings != null ? Player.Settings.gameIdentifier : "");
            Player.Result result = null;
            yield return PlayerManager.instance.Requests.Post(Player.Settings != null ? Player.Settings.registrationEndpoint : "", form, value => result = value);
            if (result == null || !result.success)
            {
                ShowNotice(result?.error ?? "Tiada respons server.", NoticeState.Error);
                yield break;
            }

            if (result.body == "phone number already registered")
                ShowNotice("Phone number already registered. Please try again!", NoticeState.Error);
            else if (result.body == "email already registered")
                ShowNotice("Email already registered. Please try again!", NoticeState.Error);
            else if (result.body.StartsWith("Successful", System.StringComparison.Ordinal))
            {
                passText.SetTextWithoutNotify("");
                ShowNotice("Tahniah! Anda berjaya mendaftar!", NoticeState.Success);
                if (EmailNotice != null)
                    EmailNotice.gameObject.SetActive(true);
            }
            else
                ShowNotice("Respons pendaftaran tidak sah. Sila cuba lagi.", NoticeState.Error);
        }
        finally
        {
            isLoading = false;
            Render();
        }
    }

    void ShowNotice(string message, NoticeState state)
    {
        if (Notice != null)
            Notice.gameObject.SetActive(true);
        if (noticeLoading != null)
            noticeLoading.SetActive(state == NoticeState.Loading);
        if (noticeError != null)
            noticeError.SetActive(state == NoticeState.Error);
        if (noticeSuccess != null)
            noticeSuccess.SetActive(state == NoticeState.Success);
        if (noticeText != null)
            noticeText.text = message;
    // Keep the response visible until retry or leaving the form.
    }

    void OnDisable()
    {
        foreach (var field in new[]
        {
            namaText,
            emailText,
            notelText,
            passText
        })
            if (field != null)
                field.onValueChanged.RemoveListener(OnInput);
        StopAllCoroutines();
        isLoading = false;
        Render();
        if (Notice != null)
            Notice.gameObject.SetActive(false);
    }
}
