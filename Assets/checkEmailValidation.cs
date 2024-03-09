using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class checkEmailValidation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //check sama ada user sudah verify account email mereka, jika sudah, user boleh buat langganan plan 
        if(PlayerPrefs.GetString("confirmation").Equals("1")){
            transform.GetComponent<Button>().interactable = true;
        }else{
            transform.GetComponent<Button>().interactable = false;
        }
    }
}
