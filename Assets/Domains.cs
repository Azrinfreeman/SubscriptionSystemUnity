using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Domains : MonoBehaviour
{
    public static Domains instance;

    void Awake()
    {
        //tukar domain di sini jika perlu in the future 
        Domain = "https://hananaelearning.com/math-modul/";
        DontDestroyOnLoad(this);
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public string Domain;

    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }
}
