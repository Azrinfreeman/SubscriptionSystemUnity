using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Domains : MonoBehaviour
{
    public static Domains instance;

    void Awake()
    {
        Domain = "https://techiestrading.com/stripe/";
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
