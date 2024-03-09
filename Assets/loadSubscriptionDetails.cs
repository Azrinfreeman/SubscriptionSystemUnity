using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class loadSubscriptionDetails : MonoBehaviour
{
    // This is an example of a UNIX timestamp for the date/time 11-04-2005 09:25.


    // First make a System.DateTime equivalent to the UNIX Epoch.
    System.DateTime start_date = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
    System.DateTime end_date = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);

    private string printDate1;
    private string printDate2;

    // Start is called before the first frame update
    void Start()
    {
        if (!PlayerPrefs.GetString("plan").Equals(""))
        {
        
            //load details into profile
            transform.GetChild(0).GetComponent<TMP_InputField>().text = PlayerPrefs.GetString(
                "plan"
            );
            transform.GetChild(1).GetComponent<TMP_InputField>().text =
                "Started at: " + PlayerPrefs.GetString("start_date");
            transform.GetChild(2).GetComponent<TMP_InputField>().text =
                "Canceled at: " + PlayerPrefs.GetString("end_date");
            if (!PlayerPrefs.GetString("status").Equals("active"))
            {
                transform.GetChild(3).GetComponent<TMP_InputField>().text =
                    "Subscription Status: " + PlayerPrefs.GetString("status").ToUpper();
                transform.GetChild(3).transform.GetChild(1).gameObject.SetActive(true);
                transform.GetChild(3).transform.GetChild(2).gameObject.SetActive(false);
            }
            else
            {
                transform.GetChild(3).GetComponent<TMP_InputField>().text =
                    "Subscription Status: " + PlayerPrefs.GetString("status").ToUpper();
                transform.GetChild(3).transform.GetChild(1).gameObject.SetActive(false);
                transform.GetChild(3).transform.GetChild(2).gameObject.SetActive(true);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!PlayerPrefs.GetString("plan").Equals(""))
        {


            transform.GetChild(0).GetComponent<TMP_InputField>().text = PlayerPrefs.GetString(
                "plan"
            );
            transform.GetChild(1).GetComponent<TMP_InputField>().text =
                "Started at : " + PlayerPrefs.GetString("start_date");
            transform.GetChild(2).GetComponent<TMP_InputField>().text =
                "Canceled at : " + PlayerPrefs.GetString("end_date");
            if (!PlayerPrefs.GetString("status").Equals("active"))
            {
                transform.GetChild(3).GetComponent<TMP_InputField>().text =
                    "Subscription Status: " + PlayerPrefs.GetString("status").ToUpper();
                transform.GetChild(3).transform.GetChild(1).gameObject.SetActive(true);
                transform.GetChild(3).transform.GetChild(2).gameObject.SetActive(false);
            }
            else
            {
                transform.GetChild(3).GetComponent<TMP_InputField>().text =
                    "Subscription Status: " + PlayerPrefs.GetString("status").ToUpper();
                transform.GetChild(3).transform.GetChild(1).gameObject.SetActive(false);
                transform.GetChild(3).transform.GetChild(2).gameObject.SetActive(true);
            }
        }
        else
        {
            transform.GetChild(0).GetComponent<TMP_InputField>().text = "No detail";
            transform.GetChild(1).GetComponent<TMP_InputField>().text = "No detail";
            transform.GetChild(2).GetComponent<TMP_InputField>().text = "No detail";

            transform.GetChild(3).GetComponent<TMP_InputField>().text =
                "Subscription Status: Trial";
            transform.GetChild(3).transform.GetChild(1).gameObject.SetActive(true);
            transform.GetChild(3).transform.GetChild(2).gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        //fetch subscription data using FetchPlayerSubscription function
        Player player = gameObject.AddComponent<Player>();
        player.FetchPlayerSubscription(PlayerPrefs.GetString("email"));

        
    }
}
