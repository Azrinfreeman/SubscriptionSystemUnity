using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RegisterUserController : MonoBehaviour
{
    public string Domain;

    [SerializeField]
    private TMP_InputField namaText;

    [SerializeField]
    private TMP_InputField emailText;
    private Transform emailWarningTransform;

    [SerializeField]
    private TMP_InputField notelText;
    private Transform notelWarningTransform;

    [SerializeField]
    private TMP_InputField passText;
    public TextMeshProUGUI text;

    [SerializeField]
    public Button btnDaftar;

    [SerializeField]
    private Transform Notice; // Big notice in the middle appear on screen when something happened

    [SerializeField]
    private Transform EmailNotice; // notification from above to notice the user

    bool isLoading;

    // Start is called before the first frame update
    void Start()
    {
        text = GameObject.Find("outputText").GetComponent<TextMeshProUGUI>();
        btnDaftar = GameObject.Find("DaftarBtn").transform.GetChild(0).GetComponent<Button>();
        emailWarningTransform = GameObject.Find("EmailWarning").GetComponent<Transform>();
        emailWarningTransform.gameObject.SetActive(false);
        notelWarningTransform = GameObject.Find("PhoneWarning").GetComponent<Transform>();
        notelWarningTransform.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (
            !emailWarningTransform.gameObject.activeSelf
            && !notelWarningTransform.gameObject.activeSelf
        )
        {
            if (
                !string.IsNullOrEmpty(namaText.text)
                && !string.IsNullOrEmpty(emailText.text)
                && !string.IsNullOrEmpty(notelText.text)
                && !string.IsNullOrEmpty(passText.text)
                && !isLoading
            )
            {
                //Debug.Log("email work");
                btnDaftar.interactable = true;
            }
            else
            {
                btnDaftar.interactable = false;
            }
        }
        else
        {
            btnDaftar.interactable = false;
        }
    }

    //regex for email
    public const string MatchEmailPattern =
        @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
        + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
        + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
        + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

    //regex for malaysian phone number
    public const string MatchPhoneNumberPattern = @"^(01)[0-46-9]*[0-9]{7,8}$";

    /// <summary>
    /// Checks whether the given Email-Parameter is a valid E-Mail address.
    /// </summary>
    /// <param name="email">Parameter-string that contains an E-Mail address.</param>
    /// <returns>True, wenn Parameter-string is not null and contains a valid E-Mail address;
    /// otherwise false.</returns>
    public static bool IsEmail(string email)
    {
        if (email != null)
            return Regex.IsMatch(email, MatchEmailPattern);
        else
            return false;
    }

    public static bool IsPhoneNbr(string number)
    {
        if (number != null)
            return Regex.IsMatch(number, MatchPhoneNumberPattern);
        else
            return false;
    }

    //validation check for phone number
    public void CheckPhoneValidation()
    {
        if (IsPhoneNbr(notelText.text))
        {
            notelWarningTransform.gameObject.SetActive(false);
            notelWarningTransform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
        }
        else
        {
            notelWarningTransform.gameObject.SetActive(true);
            notelWarningTransform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                "Invalid Phone Number";
        }
    }

    //validation for email
    public void CheckEmailValidation()
    {
        if (IsEmail(emailText.text))
        {
            emailWarningTransform.gameObject.SetActive(false);
            emailWarningTransform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
        }
        else
        {
            emailWarningTransform.gameObject.SetActive(true);
            emailWarningTransform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                "Invalid Email";
        }
    }

    //submit information to the database
    public void DaftarPengguna()
    {
        //btnDaftar = EventSystem.current.currentSelectedGameObject.transform.GetComponent<Button>();

        StartCoroutine(HantarDataPenggunaKeDB(Domain));
        btnDaftar.interactable = false;
    }

    //process for submitting user data to database
    IEnumerator HantarDataPenggunaKeDB(string Domain)
    {
        string nama = Regex.Replace(
            namaText.text,
            @"((^\w)|(\s|\p{P})\w)",
            match => match.Value.ToUpper()
        );
        //boolean for loading is true
        isLoading = true;

        namaText.text = nama;
        //active the loading with the spinning animation in the middle with text written "LOADING" under it
        Notice.transform.GetChild(0).gameObject.SetActive(true);
        Notice.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(true);
        Notice.transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(false);
        Notice.transform.GetChild(0).transform.GetChild(2).gameObject.SetActive(false);
        Notice
            .transform
            .GetChild(0)
            .transform
            .GetChild(3)
            .transform
            .GetComponent<TextMeshProUGUI>()
            .text = "LOADING..";

        WWWForm form = new WWWForm();
        //name for POST method
        form.AddField("_name", namaText.text);
        form.AddField("_email", emailText.text);
        form.AddField("_notel", notelText.text);
        form.AddField("_password", passText.text);
        form.AddField("_game_name", "Modul 2"); // Modul 1 atau Modul 2
        using (
            UnityWebRequest www = UnityWebRequest.Post(
                Domains.instance.Domain + "insertUser.php",
                form
            )
        )
        {
            yield return www.SendWebRequest();

            yield return new WaitForSeconds(2f);
            if (
                www.result == UnityWebRequest.Result.ConnectionError
                || www.result == UnityWebRequest.Result.ProtocolError
            )
            {
                Debug.Log(www.error);
                //text.text = www.error;

                //active the loading with the red cross in the middle with text written of www.error under it
                Notice.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(false);
                Notice.transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(true);
                Notice.transform.GetChild(0).transform.GetChild(2).gameObject.SetActive(false);
                Notice
                    .transform
                    .GetChild(0)
                    .transform
                    .GetChild(3)
                    .transform
                    .GetComponent<TextMeshProUGUI>()
                    .text = www.error;
            }
            else
            {
                //if an error occured in the code!
                if (www.downloadHandler.text == "error")
                {
                    //active the loading with the red cross in the middle with text written "Server error" under it
                    Notice.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(false);
                    Notice.transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(true);
                    Notice.transform.GetChild(0).transform.GetChild(2).gameObject.SetActive(false);
                    Notice
                        .transform
                        .GetChild(0)
                        .transform
                        .GetChild(3)
                        .transform
                        .GetComponent<TextMeshProUGUI>()
                        .text = "Server error!";

                    //text.text = www.downloadHandler.text;
                }
                else
                {
                    //if phone number already being used!
                    if (www.downloadHandler.text.Equals("phone number already registered"))
                    {
                        //text.text = "Phone number already registred. Please try again!";
                        //active the loading with the red cross in the middle with text written "phone number already registered" under it
                        Notice
                            .transform
                            .GetChild(0)
                            .transform
                            .GetChild(0)
                            .gameObject
                            .SetActive(false);
                        Notice
                            .transform
                            .GetChild(0)
                            .transform
                            .GetChild(1)
                            .gameObject
                            .SetActive(true);
                        Notice
                            .transform
                            .GetChild(0)
                            .transform
                            .GetChild(2)
                            .gameObject
                            .SetActive(false);
                        Notice
                            .transform
                            .GetChild(0)
                            .transform
                            .GetChild(3)
                            .transform
                            .GetComponent<TextMeshProUGUI>()
                            .text = "Phone number already registered. Please try again!";

                        // text.text = www.downloadHandler.text;
                    }
                    //if an email already being used!
                    else if (www.downloadHandler.text.Equals("email already registered"))
                    {
                        //text.text = "Email already registred. Please try again!";
                        //active the loading with the red cross in the middle with text written "email already registered" under it
                        Notice
                            .transform
                            .GetChild(0)
                            .transform
                            .GetChild(0)
                            .gameObject
                            .SetActive(false);
                        Notice
                            .transform
                            .GetChild(0)
                            .transform
                            .GetChild(1)
                            .gameObject
                            .SetActive(true);
                        Notice
                            .transform
                            .GetChild(0)
                            .transform
                            .GetChild(2)
                            .gameObject
                            .SetActive(false);
                        Notice
                            .transform
                            .GetChild(0)
                            .transform
                            .GetChild(3)
                            .transform
                            .GetComponent<TextMeshProUGUI>()
                            .text = "Email already registered. Please try again!";

                        //text.text = www.downloadHandler.text;
                    }
                    //if the server returned the word "Successful"
                    else if (www.downloadHandler.text.Contains("Successful"))
                    {
                        //active the loading with the red cross in the middle with text written "Tahniah! Anda Berjaya mendaftar" under it
                        Notice
                            .transform
                            .GetChild(0)
                            .transform
                            .GetChild(0)
                            .gameObject
                            .SetActive(false);
                        Notice
                            .transform
                            .GetChild(0)
                            .transform
                            .GetChild(1)
                            .gameObject
                            .SetActive(false);
                        Notice
                            .transform
                            .GetChild(0)
                            .transform
                            .GetChild(2)
                            .gameObject
                            .SetActive(true);
                        Notice
                            .transform
                            .GetChild(0)
                            .transform
                            .GetChild(3)
                            .transform
                            .GetComponent<TextMeshProUGUI>()
                            .text = "Tahniah! Anda berjaya mendaftar!";

                        EmailNotice.gameObject.SetActive(true);
                        btnDaftar.interactable = false;
                    }
                }
                //show result as binary using []
                //binaryData = www.downloadHandler.data;
                Debug.Log(www.downloadHandler.text);

                yield return new WaitForSeconds(5f);
                isLoading = false;
                Notice.transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }
}
