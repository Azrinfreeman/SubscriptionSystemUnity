using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class loadNama : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TextMeshProUGUI>().text = "Selamat Datang " + PlayerPrefs.GetString("name");
    }

    // Update is called once per frame
    void Update()
    {
    
    }
}
