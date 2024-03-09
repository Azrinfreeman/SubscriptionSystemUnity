using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class loadDetails : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        LoadDetailsUser();
    }

    // Update is called once per frame
    void Update()
    {
        LoadDetailsUser();
    }

    void OnEnable()
    {
        Player player = gameObject.AddComponent<Player>();
        player.FetchUserInfo();
    }

    void LoadDetailsUser()
    {
        //load details into profile using PlayerManager or PlayerPrefs
        transform.GetChild(0).GetComponent<TMP_InputField>().text = PlayerManager.instance.nama;
        transform.GetChild(1).GetComponent<TMP_InputField>().text = PlayerManager.instance.email;
        transform.GetChild(2).GetComponent<TMP_InputField>().text = PlayerManager.instance.notel;

        //if user tu belum verify email yang telah dihantar secara automatik,
        if (PlayerManager.instance.confirmation == "1")
        {
            transform.GetChild(3).GetComponent<TMP_InputField>().text = "User Verified";
            transform.GetChild(3).transform.GetChild(1).gameObject.SetActive(false);
            transform.GetChild(3).transform.GetChild(2).gameObject.SetActive(true);
        }
        else
        {
            transform.GetChild(3).GetComponent<TMP_InputField>().text = "User Not Verified";
            transform.GetChild(3).transform.GetChild(1).gameObject.SetActive(true);
            transform.GetChild(3).transform.GetChild(2).gameObject.SetActive(false);
        }
    }

    public void SendEmailToUser()
    {
        StartCoroutine(SendEmailVerification());
    }

    IEnumerator SendEmailVerification()
    {
        //disable button
        GameObject.Find("btnVerification").GetComponent<Button>().interactable = false;

        WWWForm form = new WWWForm();
        form.AddField("_email", PlayerPrefs.GetString("email"));
        form.AddField("_name", PlayerPrefs.GetString("name"));
        using (
            UnityWebRequest www = UnityWebRequest.Post(
                Domains.instance.Domain + "sendVerification.php",
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
                GameObject
                    .Find("btnVerification")
                    .GetComponent<Transform>()
                    .GetChild(1)
                    .GetComponent<TextMeshProUGUI>()
                    .text = www.error;
                yield return new WaitForSeconds(6f);
                GameObject.Find("btnVerification").GetComponent<Button>().interactable = true;
            }
            else
            {
                //show result as text .text
                //.Log(www.downloadHandler.text);
                //if it's an id
                if (www.downloadHandler.text == "error")
                {
                    Debug.Log("error..");
                    GameObject
                        .Find("btnVerification")
                        .GetComponent<Transform>()
                        .GetChild(1)
                        .GetComponent<TextMeshProUGUI>()
                        .text = "error..";
                }
                else
                {
                    if (www.downloadHandler.text.Equals("success"))
                    {
                        Debug.Log("berjaya dihantar..");
                        GameObject
                            .Find("btnVerification")
                            .GetComponent<Transform>()
                            .GetChild(1)
                            .GetComponent<TextMeshProUGUI>()
                            .text = "berjaya dihantar..";
                    }
                }
                yield return new WaitForSeconds(10f);
                GameObject.Find("btnVerification").GetComponent<Button>().interactable = true;
                GameObject
                    .Find("btnVerification")
                    .GetComponent<Transform>()
                    .GetChild(1)
                    .GetComponent<TextMeshProUGUI>()
                    .text = "";
                //show result as binary using []
                //binaryData = www.downloadHandler.data;
            }
        }
    }
}
