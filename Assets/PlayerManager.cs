using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;

    [Header("Load if user already login")]
    //load the scene if user already login
    public string loadScene;

    void Awake()
    {
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

    [Header("UserInfo")]
    public int id_user;
    public string nama;
    public string email;
    public string notel;
    public string confirmation;

    [Header("UserSubscription")]
    public string stripe_id;
    public string stripe_sub_id;
    public string stripe_pmc_id;
    public string stripe_product_id;
    public string start_date;

    public string end_date;

    public string plan;
    public string plan_desc;

    public string status;
    public string stripe_phone;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt("isLogin") > 0)
        {
            AssignInformation();

            //fetch data everytime app start
            Player player = gameObject.AddComponent<Player>();
            player.FetchPlayerSubscription(PlayerPrefs.GetString("email"));
            SceneManager.LoadScene(loadScene);
        }
        else
        {
            SceneManager.LoadScene("LogMasuk");
        }
    }

    public void AssignInformation()
    {
        //user info
        id_user = PlayerPrefs.GetInt("id_user");
        nama = PlayerPrefs.GetString("name");
        email = PlayerPrefs.GetString("email");
        notel = PlayerPrefs.GetString("notel");
        confirmation = PlayerPrefs.GetString("confirmation");

        //User Subscription Info
        stripe_id = PlayerPrefs.GetString("stripe_id");
        stripe_sub_id = PlayerPrefs.GetString("stripe_sub_id");
        stripe_pmc_id = PlayerPrefs.GetString("pmc_id");
        stripe_product_id = PlayerPrefs.GetString("product_id");
        start_date = PlayerPrefs.GetString("stripe_id");
        end_date = PlayerPrefs.GetString("stripe_id");
        plan = PlayerPrefs.GetString("plan");
        plan_desc = PlayerPrefs.GetString("plan_desc");
        status = PlayerPrefs.GetString("status");
        stripe_phone = PlayerPrefs.GetString("stripe_phone");
    }

    // Update is called once per frame
    void Update() { }
}
