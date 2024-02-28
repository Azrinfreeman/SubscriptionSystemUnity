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
        if(!PlayerPrefs.GetString("confirmation").Equals("1")){
            transform.GetComponent<Button>().interactable = false;
        }else{
            transform.GetComponent<Button>().interactable = true;
        }
    }
}
