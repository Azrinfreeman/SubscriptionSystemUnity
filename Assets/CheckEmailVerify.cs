using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckEmailVerify : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // jika user sudah verify email, matikan interactivity butang ini
        if(PlayerPrefs.GetString("confirmation").Equals("1")){
            transform.GetComponent<Button>().interactable = false;
        }else{
            transform.GetComponent<Button>().interactable = true;
        }
    }
}
