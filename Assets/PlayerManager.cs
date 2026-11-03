using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-900), RequireComponent(typeof(Player))]
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance;
    public static event Action StateChanged;
    public string loadScene = "LamanUtama", logMasukScene = "LogMasuk";
    public int id_user;
    public int login_logs;
    public string nama;
    public string email;
    public string notel;
    public string confirmation;
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
    public IPlayerRequests Requests { get; private set; }
    public string SessionToken { get; private set; }
    public long SessionExpiresAt { get; private set; }
    public long AccessUntil { get; private set; }
    public string PendingLogoutActionToken { get; set; }

    string logoutCredential;
    public bool IsLoggedIn => PlayerPrefs.GetInt("isLogin") == 1 && id_user > 0 && !string.IsNullOrEmpty(SessionToken) && SessionExpiresAt > DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    public bool IsRefreshing { get; private set; }
    public bool IsAuthenticating { get; private set; }
    public bool IsLoggingOut { get; private set; }
    public bool ProfileFresh { get; private set; }
    public bool SubscriptionFresh { get; private set; }
    public bool IsVerified => IsLoggedIn && ProfileFresh && confirmation == "1";
    public string LastError { get; private set; }
    public int PendingLogoutUserId { get; set; }
    public int SessionVersion { get; private set; }
    public bool IsSendingVerification { get; set; }
    public float VerificationAllowedAt { get; set; }
    public bool NeedsBillingRefresh { get; set; }

    Coroutine refresh;
    float nextRefresh;
    bool restorePending;
    static readonly string[] AccountKeys =
    {
        "id_user",
        "name",
        "email",
        "notel",
        "confirmation",
        "login_logs",
        "isLogin"
    };
    static readonly string[] SubscriptionKeys =
    {
        "stripe_id",
        "stripe_sub_id",
        "pmc_id",
        "product_id",
        "price_id",
        "start_date",
        "end_date",
        "plan",
        "plan_desc",
        "status",
        "stripe_phone"
    };
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Requests = GetComponent<Player>();
        AssignInformation();
        // Credentials stay in memory. Editable cached flags cannot authenticate after restart.
        ClearAccount();
        if (Player.Settings != null)
        {
            loadScene = Player.Settings.homeScene;
            logMasukScene = Player.Settings.loginScene;
        }
    }

    void Start()
    {
        if (IsLoggedIn)
        {
            restorePending = SceneManager.GetActiveScene().name == logMasukScene;
            RequestRefresh();
        }
        else if (SceneManager.GetActiveScene().name == loadScene)
            SceneManager.LoadScene(logMasukScene);
    }

    void Update()
    {
        if (!string.IsNullOrEmpty(SessionToken) && SessionExpiresAt <= DateTimeOffset.UtcNow.ToUnixTimeSeconds() && !IsLoggingOut)
        {
            ExpireSession(SessionVersion);
            return;
        }

        if (IsLoggedIn && Time.unscaledTime >= nextRefresh && !IsRefreshing && !IsAuthenticating && !IsLoggingOut)
            RequestRefresh();
    }

    public void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }

    public void SetError(string error)
    {
        LastError = error;
        NotifyStateChanged();
    }

    public void RequestRefresh()
    {
        if (!IsLoggedIn || IsRefreshing || IsAuthenticating || IsLoggingOut)
            return;
        refresh = StartCoroutine(RefreshNow());
    }

    public IEnumerator RefreshNow()
    {
        if (IsRefreshing)
        {
            while (IsRefreshing)
                yield return null;
            yield break;
        }

        if (!IsLoggedIn || IsAuthenticating || IsLoggingOut)
            yield break;
        int version = SessionVersion;
        int userId = id_user;
        string userEmail = email;
        IsRefreshing = true;
        ProfileFresh = false;
        SubscriptionFresh = false;
        LastError = null;
        NotifyStateChanged();
        try
        {
            var form = new WWWForm();
            form.AddField("id_user", userId);
            form.AddField("_game_name", Player.Settings != null ? Player.Settings.gameIdentifier : "");
            Player.Result result = null;
            yield return Requests.Post(Player.Settings != null ? Player.Settings.profileEndpoint : "", form, value => result = value);
            if (version != SessionVersion)
                yield break;
            if (result == null || !result.success)
            {
                LastError = result?.error ?? "Tiada respons server.";
                yield break;
            }

            if (!Player.TryProfile(result.body, out var profile) || profile.id_user != userId || !string.Equals(profile.email, userEmail, StringComparison.OrdinalIgnoreCase))
            {
                LastError = "Respons profil tidak sah. Sila cuba lagi.";
                yield break;
            }

            if (Player.Settings.enforceServerLoginStatus && profile.login_logs == 0)
            {
                ClearAccount();
                LastError = "Sesi telah tamat. Sila log masuk semula.";
                if (SceneManager.GetActiveScene().name != logMasukScene)
                    SceneManager.LoadScene(logMasukScene);
                yield break;
            }

            ApplyProfile(profile);
            ProfileFresh = true;
            form = new WWWForm();
            form.AddField("_email", userEmail);
            form.AddField("_game_name", Player.Settings.gameIdentifier);
            result = null;
            yield return Requests.Post(Player.Settings.subscriptionEndpoint, form, value => result = value);
            if (version != SessionVersion)
                yield break;
            if (result == null || !result.success)
            {
                LastError = result?.error ?? "Tiada respons server.";
                yield break;
            }

            if (result.body == "no result")
            {
                ClearSubscription();
                SubscriptionFresh = true;
            }
            else if (Player.TrySubscription(result.body, out var subscription))
            {
                ApplySubscription(subscription);
                SubscriptionFresh = true;
            }
            else
                LastError = "Respons langganan tidak sah. Sila cuba lagi.";
        }
        finally
        {
            IsRefreshing = false;
            refresh = null;
            nextRefresh = Time.unscaledTime + (Player.Settings != null ? Mathf.Max(5, Player.Settings.refreshIntervalSeconds) : 30);
            NotifyStateChanged();
            if (restorePending && IsLoggedIn && ProfileFresh && !IsAuthenticating && !IsLoggingOut)
            {
                restorePending = false;
                if (SceneManager.GetActiveScene().name == logMasukScene && Application.CanStreamedLevelBeLoaded(loadScene))
                    SceneManager.LoadScene(loadScene);
            }
        }
    }

    public void BeginLogin()
    {
        CancelRefresh();
        ClearAccount();
        restorePending = false;
        IsAuthenticating = true;
        LastError = null;
        NotifyStateChanged();
    }

    public void EndLogin()
    {
        IsAuthenticating = false;
        NotifyStateChanged();
    }

    public void AcceptLogin(Player.PlayerInfo profile)
    {
        if (string.IsNullOrEmpty(profile.session_token) || profile.session_token.Length != 64 || profile.session_expires_at <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        {
            ClearAccount();
            IsAuthenticating = false;
            SetError("Server did not issue a valid session. Please sign in again.");
            return;
        }

        SessionToken = profile.session_token;
        SessionExpiresAt = profile.session_expires_at;
        ApplyProfile(profile);
        PlayerPrefs.SetInt("isLogin", 1);
        PlayerPrefs.Save();
        IsAuthenticating = false;
        ProfileFresh = false;
        SubscriptionFresh = false;
        nextRefresh = Time.unscaledTime;
        NotifyStateChanged();
    }

    public bool CanAccessPremium()
    {
        // A cached active label is insufficient. Both fresh responses and a future
        // server-provided paid-through timestamp are required, including after resume.
        bool currentAccount = IsLoggedIn && ProfileFresh && SubscriptionFresh && !IsLoggingOut;
        bool paidPeriodActive = string.Equals(status, "active", StringComparison.OrdinalIgnoreCase)
            && AccessUntil > DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return currentAccount && paidPeriodActive;
    }
    public string RequestCredential(string endpoint) => Player.Settings != null && endpoint == Player.Settings.logoutEndpoint ? logoutCredential : SessionToken;
    public void ExpireSession(int version)
    {
        if (version != SessionVersion)
            return;
        ClearAccount();
        IsAuthenticating = false;
        LastError = "Sesi telah tamat. Sila log masuk semula.";
        NotifyStateChanged();
        if (SceneManager.GetActiveScene().name != logMasukScene && Application.CanStreamedLevelBeLoaded(logMasukScene))
            SceneManager.LoadScene(logMasukScene);
    }

    public void ApplyProfile(Player.PlayerInfo profile)
    {
        PlayerPrefs.SetInt("id_user", profile.id_user);
        PlayerPrefs.SetString("name", profile.name ?? "");
        PlayerPrefs.SetString("email", profile.email ?? "");
        PlayerPrefs.SetString("notel", profile.notel ?? "");
        PlayerPrefs.SetString("confirmation", profile.confirmation ?? "");
        PlayerPrefs.SetInt("login_logs", profile.login_logs);
        AssignInformation();
    }

    public void ApplySubscription(Player.PlayerSubscription data)
    {
        AccessUntil = data.access_until;
        // Keep wire-field to legacy-cache mappings explicit. Do not reorder two
        // parallel arrays: a matching index can silently display the wrong field.
        PlayerPrefs.SetString("stripe_id", data.stripe_id ?? "");
        PlayerPrefs.SetString("stripe_sub_id", data.stripe_sub_id ?? "");
        PlayerPrefs.SetString("pmc_id", data.stripe_pmc_id ?? "");
        PlayerPrefs.SetString("product_id", data.stripe_product_id ?? "");
        PlayerPrefs.SetString("price_id", data.stripe_price_id ?? "");
        PlayerPrefs.SetString("start_date", data.start_date ?? "");
        PlayerPrefs.SetString("end_date", data.end_date ?? "");
        PlayerPrefs.SetString("plan", data.plan ?? "");
        PlayerPrefs.SetString("plan_desc", data.plan_desc ?? "");
        PlayerPrefs.SetString("status", data.status ?? "");
        PlayerPrefs.SetString("stripe_phone", data.stripe_phone ?? "");
        AssignInformation();
    }

    public void ClearSubscription()
    {
        foreach (var key in SubscriptionKeys)
            PlayerPrefs.DeleteKey(key);
        AccessUntil = 0;
        SubscriptionFresh = false;
        AssignInformation();
    }

    public void ClearAccount()
    {
        SessionVersion++;
        restorePending = false;
        foreach (var key in AccountKeys)
            PlayerPrefs.DeleteKey(key);
        ClearSubscription();
        SessionToken = null;
        SessionExpiresAt = 0;
        logoutCredential = null;
        PendingLogoutActionToken = null;
        ProfileFresh = false;
        PendingLogoutUserId = 0;
        IsSendingVerification = false;
        VerificationAllowedAt = 0;
        NeedsBillingRefresh = false;
        PlayerPrefs.Save();
        AssignInformation();
    }

    public void AssignInformation()
    {
        id_user = PlayerPrefs.GetInt("id_user");
        nama = PlayerPrefs.GetString("name");
        email = PlayerPrefs.GetString("email");
        notel = PlayerPrefs.GetString("notel");
        confirmation = PlayerPrefs.GetString("confirmation");
        login_logs = PlayerPrefs.GetInt("login_logs");
        stripe_id = PlayerPrefs.GetString("stripe_id");
        stripe_sub_id = PlayerPrefs.GetString("stripe_sub_id");
        stripe_pmc_id = PlayerPrefs.GetString("pmc_id");
        stripe_product_id = PlayerPrefs.GetString("product_id");
        start_date = PlayerPrefs.GetString("start_date");
        end_date = PlayerPrefs.GetString("end_date");
        plan = PlayerPrefs.GetString("plan");
        plan_desc = PlayerPrefs.GetString("plan_desc");
        status = PlayerPrefs.GetString("status");
        stripe_phone = PlayerPrefs.GetString("stripe_phone");
    }

    public void Logout(string sceneName)
    {
        if (IsLoggingOut)
            return;
        int capturedId = id_user > 0 ? id_user : PendingLogoutUserId;
        string capturedCredential = SessionToken, actionToken = PendingLogoutActionToken;
        CancelRefresh();
        ClearAccount();
        IsAuthenticating = false;
        IsLoggingOut = true;
        NotifyStateChanged();
        logoutCredential = capturedCredential;
        StartCoroutine(LogoutRoutine(capturedId, sceneName, actionToken));
    }

    IEnumerator LogoutRoutine(int capturedId, string sceneName, string actionToken)
    {
        try
        {
            if (capturedId > 0)
            {
                var form = new WWWForm();
                form.AddField("id_user", capturedId);
                Player.Result result = null;
                if (!string.IsNullOrEmpty(actionToken))
                    form.AddField("action_token", actionToken);
                yield return Requests.Post(Player.Settings != null ? Player.Settings.logoutEndpoint : "", form, value => result = value);
                if (result == null || !result.success || result.body != "logout")
                    LastError = "Log keluar pada peranti ini selesai. Server belum mengesahkan log keluar; cuba lagi sebelum log masuk pada peranti lain.";
            }
        }
        finally
        {
            logoutCredential = null;
            IsLoggingOut = false;
            NotifyStateChanged();
            string target = string.IsNullOrWhiteSpace(sceneName) ? logMasukScene : sceneName;
            if (Application.CanStreamedLevelBeLoaded(target))
                SceneManager.LoadScene(target);
        }
    }

    void CancelRefresh()
    {
        SessionVersion++;
        if (refresh != null)
            StopCoroutine(refresh);
        refresh = null;
        IsRefreshing = false;
    }

    IEnumerator RefreshAfterBilling()
    {
        int version = SessionVersion;
        for (int i = 0; i < 3 && version == SessionVersion && IsLoggedIn; i++)
        {
            yield return RefreshNow();
            if (i < 2)
                yield return new WaitForSecondsRealtime(2);
        }
    }

    public void RefreshAfterPayment()
    {
        if (!IsLoggedIn || IsAuthenticating || IsLoggingOut)
            return;
        if (NeedsBillingRefresh)
        {
            NeedsBillingRefresh = false;
            StartCoroutine(RefreshAfterBilling());
        }
        else
            RequestRefresh();
    }

    void OnApplicationFocus(bool focused)
    {
        if (focused)
            RefreshAfterPayment();
        else
        {
            ProfileFresh = false;
            SubscriptionFresh = false;
            NotifyStateChanged();
        }
    }

    void OnApplicationPause(bool paused)
    {
        OnApplicationFocus(!paused);
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
