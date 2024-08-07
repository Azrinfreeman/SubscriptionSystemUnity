using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public class PlayerInfo
    {
        public int id_user;
        public string name;
        public string email;
        public string notel;
        public string confirmation;
        public int login_logs;

        public string device_name;
    }

    public class PlayerSubscription
    {
        public string stripe_id;
        public string stripe_sub_id;
        public string stripe_pmc_id;
        public string stripe_product_id;

        public string stripe_price_id;
        public string start_date;

        public string end_date;

        public string plan;
        public string plan_desc;

        public string status;
        public string stripe_phone;
    }

    // Sets a string value in PlayerPrefs after encrypting it


    /*
    
    public void SavePlayerInfo(PlayerInfo player)
    {
        XmlDocument xmlDocument = new XmlDocument();

        XmlElement root = xmlDocument.CreateElement("UserInformation");
        root.SetAttribute("UserInformation", "UserInfo_01");

        #region CreateXML elements

        XmlElement id_userElement = xmlDocument.CreateElement("id_user");
        id_userElement.InnerText = player.id_user.ToString();
        root.AppendChild(id_userElement);

        XmlElement nameElement = xmlDocument.CreateElement("name");
        nameElement.InnerText = player.name.ToString();
        root.AppendChild(nameElement);

        XmlElement emailElement = xmlDocument.CreateElement("email");
        emailElement.InnerText = player.email.ToString();
        root.AppendChild(emailElement);

        XmlElement notelElement = xmlDocument.CreateElement("notel");
        notelElement.InnerText = player.notel.ToString();
        root.AppendChild(notelElement);

        XmlElement confirmationElement = xmlDocument.CreateElement("confirmation");
        confirmationElement.InnerText = player.confirmation.ToString();
        root.AppendChild(confirmationElement);

        #endregion
        xmlDocument.AppendChild(root);

        xmlDocument.Save(Application.dataPath + "/UserInfo.text");
        if (File.Exists(Application.dataPath + "/UserInfo.text"))
        {
            Debug.Log("XML FILE SAVED !");
        }
    }

    public void SavePlayerSubscription(PlayerSubscription playerSub)
    {
        XmlDocument xmlDocument = new XmlDocument();

        XmlElement root = xmlDocument.CreateElement("SubsInformation");
        root.SetAttribute("SubsInformation", "UserSub_01");

        #region CreateXML elements

        XmlElement stripe_idElement = xmlDocument.CreateElement("stripe_id");
        stripe_idElement.InnerText = playerSub.stripe_id.ToString();
        root.AppendChild(stripe_idElement);

        XmlElement stripe_sub_idElement = xmlDocument.CreateElement("stripe_sub_id");
        stripe_sub_idElement.InnerText = playerSub.stripe_sub_id.ToString();
        root.AppendChild(stripe_sub_idElement);

        XmlElement pmc_idElement = xmlDocument.CreateElement("pmc_id");
        pmc_idElement.InnerText = playerSub.stripe_pmc_id.ToString();
        root.AppendChild(pmc_idElement);

        XmlElement product_idElement = xmlDocument.CreateElement("product_id");
        product_idElement.InnerText = playerSub.stripe_product_id.ToString();
        root.AppendChild(product_idElement);

        XmlElement start_dateElement = xmlDocument.CreateElement("start_date");
        start_dateElement.InnerText = playerSub.start_date.ToString();
        root.AppendChild(start_dateElement);

        XmlElement end_dateElement = xmlDocument.CreateElement("end_date");
        end_dateElement.InnerText = playerSub.end_date.ToString();
        root.AppendChild(end_dateElement);

        XmlElement planElement = xmlDocument.CreateElement("plan");
        planElement.InnerText = playerSub.plan.ToString();
        root.AppendChild(planElement);

        XmlElement plan_descElement = xmlDocument.CreateElement("plan_desc");
        plan_descElement.InnerText = playerSub.plan_desc.ToString();
        root.AppendChild(plan_descElement);

        XmlElement statusElement = xmlDocument.CreateElement("status");
        statusElement.InnerText = playerSub.status.ToString();
        root.AppendChild(statusElement);

        XmlElement stripe_phoneElement = xmlDocument.CreateElement("stripe_phone");
        stripe_phoneElement.InnerText = playerSub.stripe_phone.ToString();
        root.AppendChild(stripe_phoneElement);

        #endregion
        xmlDocument.AppendChild(root);

        xmlDocument.Save(Application.dataPath + "/SubsInformation.text");
        if (File.Exists(Application.dataPath + "/SubsInformation.text"))
        {
            Debug.Log("XML FILE SAVED !");
        }
    }

    public void LoadPlayerInfo(PlayerInfo player)
    {
        if (File.Exists(Application.dataPath + "/UserInfo.text"))
        {
            //Load the saved
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(Application.dataPath + "/UserInfo.text");

            //marker
            XmlNodeList id_user = xmlDocument.GetElementsByTagName("id_user");
            int id = int.Parse(id_user[0].InnerText);
            player.id_user = id;

            XmlNodeList name = xmlDocument.GetElementsByTagName("name");
            string nameText = name[0].InnerText.ToString();
            player.name = nameText;

            XmlNodeList email = xmlDocument.GetElementsByTagName("email");
            string emailText = email[0].InnerText.ToString();
            player.email = emailText;

            XmlNodeList notel = xmlDocument.GetElementsByTagName("notel");
            string notelText = notel[0].InnerText.ToString();
            player.notel = notelText;

            XmlNodeList confirmation = xmlDocument.GetElementsByTagName("confirmation");
            string confirmationText = confirmation[0].InnerText.ToString();
            player.confirmation = confirmationText;

            //using playerprefs to store data
            PlayerPrefs.SetInt("id_user", player.id_user);
            PlayerPrefs.SetString("name", player.name);
            PlayerPrefs.SetString("email", player.email);
            PlayerPrefs.SetString("notel", player.notel);
            PlayerPrefs.SetString("confirmation", player.confirmation);

            PlayerPrefs.SetInt("isLogin", 1);
            PlayerManager.instance.id_user = player.id_user;
            PlayerManager.instance.nama = player.name;
            PlayerManager.instance.email = player.email;
            PlayerManager.instance.notel = player.notel;
            PlayerManager.instance.confirmation = player.confirmation;
        }
        else
        {
            Debug.Log("Saved file not found.");
        }
    }

    public void LoadPlayerSubscription(PlayerSubscription playerSub)
    {
        if (File.Exists(Application.dataPath + "/SubsInformation.text"))
        {
            //Load the saved
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(Application.dataPath + "/SubsInformation.text");

            //marker
            XmlNodeList stripe_id = xmlDocument.GetElementsByTagName("stripe_id");
            string stripeid = stripe_id[0].InnerText.ToString();
            playerSub.stripe_id = stripeid;

            XmlNodeList stripe_sub_id = xmlDocument.GetElementsByTagName("stripe_sub_id");
            string stripe_subid = stripe_sub_id[0].InnerText.ToString();
            playerSub.stripe_sub_id = stripe_subid;

            XmlNodeList stripe_pmc_id = xmlDocument.GetElementsByTagName("pmc_id");
            string stripe_pmcid = stripe_pmc_id[0].InnerText.ToString();
            playerSub.stripe_pmc_id = stripe_pmcid;

            XmlNodeList stripe_product_id = xmlDocument.GetElementsByTagName("product_id");
            string stripe_productid = stripe_product_id[0].InnerText.ToString();
            playerSub.stripe_product_id = stripe_productid;

            XmlNodeList start_date = xmlDocument.GetElementsByTagName("start_date");
            string start_date1 = start_date[0].InnerText.ToString();
            playerSub.start_date = start_date1;

            XmlNodeList end_date = xmlDocument.GetElementsByTagName("end_date");
            string end_date1 = end_date[0].InnerText.ToString();
            playerSub.end_date = end_date1;

            XmlNodeList plan = xmlDocument.GetElementsByTagName("plan");
            string plan1 = plan[0].InnerText.ToString();
            playerSub.plan = plan1;

            XmlNodeList plan_desc = xmlDocument.GetElementsByTagName("plan_desc");
            string plan_desc1 = plan_desc[0].InnerText.ToString();
            playerSub.plan_desc = plan_desc1;

            XmlNodeList status = xmlDocument.GetElementsByTagName("status");
            string status1 = status[0].InnerText.ToString();
            playerSub.status = status1;

            XmlNodeList stripe_phone = xmlDocument.GetElementsByTagName("stripe_phone");
            string stripe_phone1 = stripe_phone[0].InnerText.ToString();
            playerSub.stripe_phone = stripe_phone1;

            // Store using PLayerPrefs
            //ids
            PlayerPrefs.SetString("stripe_id", playerSub.stripe_id);
            PlayerPrefs.SetString("stripe_sub_id", playerSub.stripe_sub_id);
            PlayerPrefs.SetString("pmc_id", playerSub.stripe_pmc_id);
            PlayerPrefs.SetString("product_id", playerSub.stripe_product_id);
            ///

            PlayerPrefs.SetString("start_date", playerSub.start_date);
            PlayerPrefs.SetString("end_date", playerSub.end_date);
            PlayerPrefs.SetString("plan", playerSub.plan);
            PlayerPrefs.SetString("plan_desc", playerSub.plan_desc);
            PlayerPrefs.SetString("status", playerSub.status);
            PlayerPrefs.SetString("stripe_phone", playerSub.stripe_phone);

            PlayerManager.instance.stripe_id = playerSub.stripe_id;
            PlayerManager.instance.stripe_sub_id = playerSub.stripe_sub_id;
            PlayerManager.instance.stripe_pmc_id = playerSub.stripe_pmc_id;
            PlayerManager.instance.stripe_product_id = playerSub.stripe_product_id;

            PlayerManager.instance.start_date = playerSub.start_date;
            PlayerManager.instance.end_date = playerSub.end_date;
            PlayerManager.instance.plan = playerSub.plan;
            PlayerManager.instance.plan_desc = playerSub.plan_desc;
            PlayerManager.instance.status = playerSub.status;
            PlayerManager.instance.stripe_phone = playerSub.stripe_phone;
        }
        else
        {
            Debug.Log("Saved file not found.");
        }
    }

    */
    public void FetchPlayerSubscription(string emailText)
    {
        StartCoroutine(fetchSub(emailText));
    }

    public void FetchUserInfo()
    {
        StartCoroutine(fetchUser());
    }

    public void LogoutUser(string sceneName)
    {
        StartCoroutine(Logout(sceneName));
    }

    IEnumerator Logout(string sceneName)
    {
        WWWForm form = new WWWForm();
        form.AddField("id_user", PlayerPrefs.GetInt("id_user"));
        using (
            UnityWebRequest www = UnityWebRequest.Post(Domains.instance.Domain + "logout.php", form)
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
            }
            else
            {
                if (www.downloadHandler.text == "logout")
                {
                    Debug.Log(www.downloadHandler.text);
                    SceneManager.LoadScene(sceneName);
                }
            }
        }
    }

    IEnumerator fetchUser()
    {
        WWWForm form = new WWWForm();
        form.AddField("id_user", PlayerPrefs.GetInt("id_user"));
        form.AddField("_game_name", "Modul 2"); // Modul 1 atau Modul 2
        using (
            UnityWebRequest www = UnityWebRequest.Post(
                Domains.instance.Domain + "getinfouser.php",
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
            }
            else
            {
                //show result as text .text
                //.Log(www.downloadHandler.text);
                //if it's an id
                if (www.downloadHandler.text == "no result")
                {
                    //text.text = "no result";
                    Debug.Log(www.downloadHandler.text);
                }
                else
                {
                    Debug.Log(www.downloadHandler.text);

                    PlayerInfo player = JsonUtility.FromJson<PlayerInfo>(www.downloadHandler.text);

                    //User Subscription Info
                    //PlayerPrefs.SetString("name", player.name);
                    PlayerPrefs.SetString("email", player.email);
                    PlayerPrefs.SetString("notel", player.notel);
                    PlayerPrefs.SetString("confirmation", player.confirmation);
                    PlayerPrefs.SetInt("login_logs", player.login_logs);
                    //PlayerManager.instance.AssignInformation();
                }
            }
        }
    }

    IEnumerator fetchSub(string emailText)
    {
        WWWForm form = new WWWForm();
        form.AddField("_email", emailText);
        form.AddField("_game_name", "Modul 2"); // Modul 1 atau Modul 2
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
            }
            else
            {
                //show result as text .text
                //.Log(www.downloadHandler.text);
                //if it's an id
                if (www.downloadHandler.text == "no result")
                {
                    //tambah
                    //text.text = "no result";
                    Debug.Log(www.downloadHandler.text);
                    PlayerPrefs.DeleteKey("stripe_id");
                    PlayerPrefs.DeleteKey("stripe_sub_id");
                    PlayerPrefs.DeleteKey("pmc_id");
                    PlayerPrefs.DeleteKey("product_id");
                    PlayerPrefs.DeleteKey("price_id");
                    PlayerPrefs.DeleteKey("start_date");
                    PlayerPrefs.DeleteKey("end_date");
                    PlayerPrefs.DeleteKey("plan");
                    PlayerPrefs.DeleteKey("plan_desc");
                    PlayerPrefs.DeleteKey("status");
                    PlayerPrefs.DeleteKey("stripe_phone");
                    PlayerManager.instance.AssignInformation();
                }
                else
                {
                    //btnLog.interactable = false;

                    if (www.downloadHandler.text.Equals(""))
                    {
                        //text.text = www.downloadHandler.text;
                        //btnLog.interactable = true;
                    }
                    else
                    {
                        Debug.Log(www.downloadHandler.text);

                        PlayerSubscription PS = JsonUtility.FromJson<Player.PlayerSubscription>(
                            www.downloadHandler.text
                        );

                        //   player.SavePlayerSubscription(PS);
                        //    player.LoadPlayerSubscription(PS);
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
                        //btnLog.interactable = false;
                        //text.text = www.downloadHandler.text;
                        //   CheckIfUserLogin.instance.Reassign();
                    }
                }

                //show result as binary using []
                //binaryData = www.downloadHandler.data;
            }
        }
    }

    void Awake() { }

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }
}
