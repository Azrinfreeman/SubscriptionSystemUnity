using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Xml;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField emailText;
    private Transform emailWarningTransform;

    [SerializeField]
    private TMP_InputField passText;

    //ignore text variable, for testing


    [SerializeField]
    private Button btnLog;

    [SerializeField]
    private Transform Notification;

    [SerializeField]
    private Transform Notice;

    [SerializeField]
    private Transform LogoutPanel;
    bool isPressed;

    /// Checks whether the given Email-Parameter is a valid E-Mail address.
    public const string MatchEmailPattern =
        @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
        + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
        + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
        + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

    public static Player.PlayerInfo CreateFromJSON(string jsonString)
    {
        return JsonUtility.FromJson<Player.PlayerInfo>(jsonString);
    }

    public static Player.PlayerSubscription CreateFromJSON2(string jsonString)
    {
        return JsonUtility.FromJson<Player.PlayerSubscription>(jsonString);
    }

    public static bool IsEmail(string email)
    {
        if (email != null)
            return Regex.IsMatch(email, MatchEmailPattern);
        else
            return false;
    }

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

    // Start is called before the first frame update
    void Start()
    {
        btnLog = GameObject.Find("LogBtn").transform.GetChild(0).GetComponent<Button>();
        emailWarningTransform = GameObject.Find("EmailWarning").GetComponent<Transform>();
        emailText = GameObject.Find("Email").GetComponent<TMP_InputField>();
        passText = GameObject.Find("Password").GetComponent<TMP_InputField>();
        Notification = GameObject.Find("Notification").GetComponent<Transform>();

        emailWarningTransform.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!emailWarningTransform.gameObject.activeSelf)
        {
            if (
                !string.IsNullOrEmpty(emailText.text)
                && !string.IsNullOrEmpty(passText.text)
                && !isPressed
            )
            {
                //Debug.Log("email work");
                btnLog.interactable = true;
            }
        }
        else
        {
            btnLog.interactable = false;
        }
    }

    public void LogMasuk()
    {
        isPressed = true;
        StartCoroutine(Login());
    }

    IEnumerator Login()
    {
        //btnLog.interactable = false;
        Notice.gameObject.SetActive(true);
        Notice.transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(true);
        WWWForm form = new WWWForm();
        form.AddField("_email", emailText.text);
        form.AddField("_password", passText.text);
        form.AddField("_device_name", SystemInfo.deviceName + " " + SystemInfo.deviceModel);
        form.AddField("_game_name", "Modul 2"); // Modul 1 atau Modul 2
        //check detail yang dimasukkan oleh user, email atau password itu valid ke tak
        using (
            UnityWebRequest www = UnityWebRequest.Post(
                Domains.instance.Domain + "loginVerify.php",
                form
            )
        )
        {
            yield return www.SendWebRequest();
            if (
                www.result == UnityWebRequest.Result.ConnectionError
                || www.result == UnityWebRequest.Result.ProtocolError
            )
            {
                Debug.Log(www.error);
                btnLog.interactable = true;
                Notice.gameObject.SetActive(false);
                isPressed = false;
                yield break;
            }
            else
            {
                //show result as text .text
                //.Log(www.downloadHandler.text);
                //if it's an id
                if (www.downloadHandler.text == "error")
                {
                    btnLog.interactable = true;
                    Debug.Log(www.downloadHandler.text);
                    Notice.gameObject.SetActive(false);
                    isPressed = false;
                    yield break;
                }
                else
                {
                    if (www.downloadHandler.text.Equals("password is not verify"))
                    {
                        btnLog.interactable = true;
                        Debug.Log(www.downloadHandler.text);
                        Notice.gameObject.SetActive(false);
                        Notification.GetComponent<Animator>().Play("ShowNotificationStart");
                        Notification
                            .GetChild(0)
                            .transform
                            .GetChild(0)
                            .transform
                            .GetChild(1)
                            .GetComponent<TextMeshProUGUI>()
                            .text = "password is not verify..";
                        isPressed = false;
                        yield break;
                    }
                    else if (www.downloadHandler.text.Equals("invalid email"))
                    {
                        btnLog.interactable = true;
                        Debug.Log(www.downloadHandler.text);
                        Notice.gameObject.SetActive(false);
                        Notification.GetComponent<Animator>().Play("ShowNotificationStart");
                        Notification
                            .GetChild(0)
                            .transform
                            .GetChild(0)
                            .transform
                            .GetChild(1)
                            .GetComponent<TextMeshProUGUI>()
                            .text = "invalid email..";
                        isPressed = false;
                        yield break;
                    }
                    else
                    {
                        Debug.Log(www.downloadHandler.text);
                        Player.PlayerInfo player1 = JsonUtility.FromJson<Player.PlayerInfo>(
                            www.downloadHandler.text
                        );

                        // player.SavePlayerInfo(player1);
                        // player.LoadPlayerInfo(player1);
                        PlayerPrefs.SetInt("id_user", player1.id_user);
                        PlayerPrefs.SetString("name", player1.name);
                        PlayerPrefs.SetString("email", player1.email);
                        PlayerPrefs.SetString("notel", player1.notel);
                        PlayerPrefs.SetString("confirmation", player1.confirmation);

                        PlayerPrefs.SetInt("login_logs", player1.login_logs);
                        PlayerManager.instance.AssignInformation();
                        btnLog.interactable = false;
                        if (player1.login_logs != 0)
                        {
                            Notification.GetComponent<Animator>().Play("ShowNotificationStart");
                            Notification
                                .GetChild(0)
                                .transform
                                .GetChild(0)
                                .transform
                                .GetChild(1)
                                .GetComponent<TextMeshProUGUI>()
                                .text = "Anda sudah log masuk dari peranti yang lain...";

                            btnLog.interactable = true;
                            Notice.gameObject.SetActive(false);
                            isPressed = false;
                            LogoutPanel.gameObject.SetActive(true);
                            LogoutPanel
                                .GetChild(1)
                                .transform
                                .GetChild(0)
                                .transform
                                .GetChild(1)
                                .GetComponent<TextMeshProUGUI>()
                                .text = player1.device_name + " \n(name peranti)";
                            yield break;
                        }
                        else
                        {
                            PlayerPrefs.SetInt("isLogin", 1);
                        }

                        //keluarkan notifikasi dari atas yang user berjaya log masuk
                        Notification.GetComponent<Animator>().Play("ShowNotificationStart");
                    }
                }
            }
        }
        //fetch info tentang detail user yang dimasukkan berdasarkan email yang diletak
        using (
            UnityWebRequest www = UnityWebRequest.Post(
                Domains.instance.Domain + "getinfo.php",
                form
            )
        )
        {
            yield return www.SendWebRequest();
            if (
                www.result == UnityWebRequest.Result.ConnectionError
                || www.result == UnityWebRequest.Result.ProtocolError
            )
            {
                Debug.Log(www.error);

                //text.text = www.error;
                PlayerPrefs.SetString("status", "Trial");
            }
            else
            {
                if (www.downloadHandler.text == "no result")
                {
                    Debug.Log(www.downloadHandler.text);
                    PlayerPrefs.SetString("status", "Trial");
                }
                else
                {
                    btnLog.interactable = false;

                    if (www.downloadHandler.text.Equals(""))
                    {
                        //text.text = www.downloadHandler.text;
                        btnLog.interactable = true;
                        PlayerPrefs.SetString("status", "Trial");
                    }
                    else
                    {
                        Debug.Log(www.downloadHandler.text);

                        Player.PlayerSubscription PS =
                            JsonUtility.FromJson<Player.PlayerSubscription>(
                                www.downloadHandler.text
                            );
                        //User Subscription Info
                        PlayerPrefs.SetString("stripe_id", PS.stripe_id);
                        PlayerPrefs.SetString("stripe_sub_id", PS.stripe_sub_id);
                        PlayerPrefs.SetString("pmc_id", PS.stripe_pmc_id);
                        PlayerPrefs.SetString("product_id", PS.stripe_product_id);
                        PlayerPrefs.SetString("price_id", PS.stripe_price_id);
                        PlayerPrefs.SetString("start_date", PS.start_date);
                        PlayerPrefs.SetString("end_date", PS.end_date);
                        PlayerPrefs.SetString("plan", PS.plan);
                        PlayerPrefs.SetString("plan_desc", PS.plan_desc);
                        PlayerPrefs.SetString("status", PS.status);
                        PlayerPrefs.SetString("stripe_phone", PS.stripe_phone);
                        PlayerManager.instance.AssignInformation();
                        btnLog.interactable = false;
                    }
                }
            }
            isPressed = false;
            Notification.GetComponent<Animator>().Play("ShowNotificationStart"); //keluarkan notifikasi dari atas screen
            yield return new WaitForSeconds(2f);
            Notice.gameObject.SetActive(false);
            SceneManager.LoadScene("LamanUtama"); //load scene selepas log masuk
        }
    }
}
