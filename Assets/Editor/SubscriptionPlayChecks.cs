using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class SubscriptionPlayChecks
{
    const string Active = "SubscriptionPlayChecks.Active", ResultKey = "SubscriptionPlayChecks.Result";
    const string SnapshotPath = "Temp/SubscriptionPlayChecks.preferences.json";
    static int checks;
    [Serializable] sealed class Preference { public string key; public bool exists; public bool integer; public int number; public string text; }
    [Serializable] sealed class Snapshot { public List<Preference> values = new List<Preference>(); public string scene; public bool runInBackground; }
    static readonly string[] IntKeys = { "id_user", "login_logs", "isLogin", "GameUI.ReducedMotion" };
    static readonly string[] StringKeys = { "name", "email", "notel", "confirmation", "stripe_id", "stripe_sub_id", "pmc_id", "product_id", "price_id", "start_date", "end_date", "plan", "plan_desc", "status", "stripe_phone", "SubscriptionPlayChecks.Unrelated" };
    static SubscriptionPlayChecks() { EditorApplication.playModeStateChanged += OnPlayState; EditorApplication.update += Watchdog; }
    public static string LastResult => SessionState.GetString(ResultKey, "Not run.");
    [MenuItem("Tools/Subscription System/Run Offline Scene Checks")]
    public static void Start()
    {
        if (EditorApplication.isPlaying || SessionState.GetBool(Active, false)) throw new InvalidOperationException("Stop the current run first.");
        if (SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save scene changes before running checks.");
        var settings = AssetDatabase.LoadAssetAtPath<SubscriptionSettings>(SubscriptionTemplateTools.SettingsPath);
        if (settings == null || !string.IsNullOrEmpty(settings.backendBaseUrl)) throw new InvalidOperationException("Run offline scene checks with the template backend URL empty; no live endpoint is used.");
        var snapshot = new Snapshot { scene = SceneManager.GetActiveScene().path, runInBackground = PlayerSettings.runInBackground };
        foreach (var key in IntKeys) snapshot.values.Add(new Preference { key = key, exists = PlayerPrefs.HasKey(key), integer = true, number = PlayerPrefs.GetInt(key) });
        foreach (var key in StringKeys) snapshot.values.Add(new Preference { key = key, exists = PlayerPrefs.HasKey(key), text = PlayerPrefs.GetString(key) });
        File.WriteAllText(SnapshotPath, JsonUtility.ToJson(snapshot));
        PlayerPrefs.SetInt("isLogin", 0); PlayerPrefs.Save();
        SessionState.SetString(ResultKey, "Running…"); SessionState.SetBool(Active, true); SessionState.SetString("SubscriptionPlayChecks.Started", DateTime.UtcNow.Ticks.ToString());
        EditorSceneManager.OpenScene("Assets/Scenes/LogMasuk.unity");
        EditorApplication.EnterPlaymode();
    }
    static void OnPlayState(PlayModeStateChange change)
    {
        if (!SessionState.GetBool(Active, false)) return;
        if (change == PlayModeStateChange.EnteredPlayMode) EditorApplication.delayCall += Begin;
        if (change == PlayModeStateChange.EnteredEditMode)
        {
            Restore();
            SessionState.SetBool(Active, false);
            Debug.Log(LastResult);
        }
    }
    static void Begin()
    {
        Application.runInBackground = true;
        EditorApplication.isPaused = false;
        EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView")).Focus();
        var manager = PlayerManager.instance;
        if (manager == null) { Fail("PlayerManager missing"); return; }
        manager.enabled = false; checks = 0;
        manager.StartCoroutine(Run(manager));
    }
    static void Check(bool condition, string name) { if (!condition) { Fail(name); throw new InvalidOperationException(name); } checks++; }
    static void Fail(string name) { SessionState.SetString(ResultKey, "FAILED: " + name); EditorApplication.isPlaying = false; }
    static T Reference<T>(UnityEngine.Object target, string field) where T : class => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(target) as T;
    static IEnumerator Run(PlayerManager manager)
    {
        yield return null;
        Check(SceneManager.GetActiveScene().name == "LogMasuk", "start in original login scene");
        Check(manager.GetComponents<Player>().Length == 1, "one request component before tests");
        var login = UnityEngine.Object.FindObjectsOfType<LoginController>(true).Single();
        var register = UnityEngine.Object.FindObjectsOfType<RegisterUserController>(true).Single();
        Check(!Reference<Button>(login, "btnLog").interactable, "empty login form disabled");
        Capture("login");
        Transform signIn = login.transform; while (signIn != null && signIn.name != "SignIn") signIn = signIn.parent;
        signIn.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(.3f);
        var flow = signIn.GetComponentInChildren<GameUIPanelFlow>();
        Check(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.transform.IsChildOf(signIn), "opening form focuses its control");
        signIn.gameObject.SetActive(false);
        Check(!flow.GetComponent<CanvasGroup>().blocksRaycasts && flow.transform.localScale == Vector3.one, "closed panel releases raycasts and resets motion");
        signIn.gameObject.SetActive(true); signIn.gameObject.SetActive(false); signIn.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(.3f);
        Check(flow.GetComponent<CanvasGroup>().alpha == 1 && flow.transform.localScale == Vector3.one, "interrupted panel transition settles");
        PlayerPrefs.SetInt("GameUI.ReducedMotion", 1);
        signIn.gameObject.SetActive(false); signIn.gameObject.SetActive(true);
        yield return null;
        Check(flow.GetComponent<CanvasGroup>().alpha == 1 && flow.transform.localScale == Vector3.one, "reduced motion opens without scale or fade");
        PlayerPrefs.SetInt("GameUI.ReducedMotion", 0);
        var realRequests = manager.Requests;
        var fake = new SubscriptionTestPlayer();
        typeof(PlayerManager).GetProperty("Requests").SetValue(manager, fake);

        Reference<TMP_InputField>(login, "emailText").text = "offline@example.invalid";
        Reference<TMP_InputField>(login, "passText").text = "offline-password";
        Check(Reference<Button>(login, "btnLog").interactable, "valid login fields enable submit");
        login.LogMasuk(); login.LogMasuk();
        Check(fake.loginCalls == 1, "duplicate login clicks coalesce");
        while (manager.IsAuthenticating || manager.IsRefreshing) yield return null;
        for (int i = 0; i < 4; i++) yield return null;
        Check(SceneManager.GetActiveScene().name == "LamanUtama", "login uses original dashboard scene");
        Check(manager.ProfileFresh && manager.SubscriptionFresh && manager.CanAccessPremium(), "completed responses update access");
        Check(!JsonUtility.ToJson(manager).Contains(new string('a',64)), "session credential is not serialized into scene state");
        Check(manager.start_date == "start-test" && manager.end_date == "end-test", "dates remain correct through scene transition");
        Capture("dashboard");
        Check(UnityEngine.Object.FindObjectsOfType<PlayerManager>().Length == 1 && UnityEngine.Object.FindObjectsOfType<Domains>().Length == 1, "one persistent manager and domain after transition");
        var details = UnityEngine.Object.FindObjectsOfType<loadDetails>(true).Single();
        Transform profilePanel = details.transform; while (profilePanel != null && profilePanel.name != "Profile") profilePanel = profilePanel.parent;
        profilePanel.gameObject.SetActive(true);
        while (manager.IsRefreshing) yield return null;
        manager.confirmation = "0"; manager.NotifyStateChanged();
        Capture("profile");
        details.SendEmailToUser(); profilePanel.gameObject.SetActive(false);
        Check(!manager.IsSendingVerification, "closing profile releases verification loading state");

        fake.frames = 12; int before = fake.profileCalls;
        manager.RequestRefresh(); manager.RequestRefresh();
        Check(fake.profileCalls == before + 1 && manager.IsRefreshing, "overlapping refreshes share one operation");
        while (manager.IsRefreshing) yield return null;
        Check(manager.CanAccessPremium(), "slow response completes before access decisions");
        fake.failProfile = true; manager.RequestRefresh(); while (manager.IsRefreshing) yield return null;
        Check(manager.IsLoggedIn && !manager.CanAccessPremium() && !string.IsNullOrEmpty(manager.LastError), "network failure retains account and denies unvalidated access");
        fake.failProfile = false;
        manager.RequestRefresh(); manager.ClearAccount();
        while (manager.IsRefreshing) yield return null;
        Check(manager.id_user == 0 && !manager.IsLoggedIn && manager.stripe_id == "", "late refresh cannot restore cleared account");
        Check(string.IsNullOrEmpty(manager.SessionToken) && manager.SessionExpiresAt == 0, "account cleanup removes credentials");

        SceneManager.LoadScene("LogMasuk");
        for (int i = 0; i < 3; i++) yield return null;
        register = UnityEngine.Object.FindObjectsOfType<RegisterUserController>(true).Single();
        Transform signUp = register.transform; while (signUp != null && signUp.name != "SignUp") signUp = signUp.parent;
        signUp.gameObject.SetActive(true);
        Reference<TMP_InputField>(register, "namaText").text = "Offline Player";
        Reference<TMP_InputField>(register, "emailText").text = "offline@example.invalid";
        Reference<TMP_InputField>(register, "notelText").text = "0123456789";
        Reference<TMP_InputField>(register, "passText").text = "offline-password";
        int registrations = fake.registrationCalls;
        register.DaftarPengguna(); register.DaftarPengguna();
        Check(fake.registrationCalls == registrations + 1 && !register.btnDaftar.interactable, "registration disables duplicate submission");
        for (int i = 0; i < 20; i++) yield return null;
        Check(register.btnDaftar.interactable, "registration failure releases loading state");
        yield return new WaitForSecondsRealtime(4.2f);
        Check(Reference<Transform>(register, "Notice").gameObject.activeSelf && register.btnDaftar.interactable, "inline error persists while retry remains available");

        manager.AcceptLogin(TestLogin());
        yield return manager.RefreshNow();
        PlayerPrefs.SetString("SubscriptionPlayChecks.Unrelated", "keep");
        fake.failLogout = true;
        manager.Logout("LogMasuk");
        Check(!manager.IsLoggedIn && manager.id_user == 0, "logout clears local data immediately");
        while (manager.IsLoggingOut) yield return null;
        for (int i = 0; i < 3; i++) yield return null;
        Check(fake.logoutBody == "id_user=9", "remote logout uses captured user ID");
        Check(SceneManager.GetActiveScene().name == "LogMasuk" && !string.IsNullOrEmpty(manager.LastError), "failed remote logout still returns to login with feedback");
        Check(PlayerPrefs.GetString("SubscriptionPlayChecks.Unrelated") == "keep", "logout preserves game preferences");
        manager.AcceptLogin(TestLogin());
        typeof(PlayerManager).GetField("restorePending", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(manager, true);
        yield return manager.RefreshNow();
        for (int i = 0; i < 3; i++) yield return null;
        Check(SceneManager.GetActiveScene().name == "LamanUtama", "validated authenticated session enters existing dashboard");
        manager.ExpireSession(manager.SessionVersion - 1);
        Check(manager.IsLoggedIn, "an obsolete unauthorized response cannot clear a new session");
        manager.ClearAccount();
        typeof(PlayerManager).GetProperty("Requests").SetValue(manager, realRequests);
        yield return null;
        Check(manager.GetComponents<Player>().Length == 1, "no test transport remains");
        SessionState.SetString(ResultKey, checks + " offline scene checks passed in the original scenes. No external requests sent.");
        EditorApplication.isPlaying = false;
    }
    static void Watchdog()
    {
        if (!SessionState.GetBool(Active, false)) return;
        if (long.TryParse(SessionState.GetString("SubscriptionPlayChecks.Started", "0"), out var ticks) && DateTime.UtcNow.Ticks - ticks > TimeSpan.FromSeconds(180).Ticks)
        {
            if (EditorApplication.isPlaying) Fail("Scene check timed out");
            else { Restore(); SessionState.SetBool(Active, false); }
        }
    }
    static Player.PlayerInfo TestLogin() => new Player.PlayerInfo { id_user = 9, name = "Offline Player", email = "offline@example.invalid", confirmation = "1", login_logs = 1, session_token = new string('a',64), session_expires_at = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds() };
    static void Restore()
    {
        if (!File.Exists(SnapshotPath)) return;
        var snapshot = JsonUtility.FromJson<Snapshot>(File.ReadAllText(SnapshotPath));
        foreach (var item in snapshot.values)
        {
            if (!item.exists) PlayerPrefs.DeleteKey(item.key);
            else if (item.integer) PlayerPrefs.SetInt(item.key, item.number);
            else PlayerPrefs.SetString(item.key, item.text ?? "");
        }
        PlayerPrefs.Save();
        PlayerSettings.runInBackground = snapshot.runInBackground;
        if (!string.IsNullOrEmpty(snapshot.scene)) EditorSceneManager.OpenScene(snapshot.scene);
        File.Delete(SnapshotPath);
    }
    static void Capture(string name)
    {
        var camera = Camera.main;
        var canvases = UnityEngine.Object.FindObjectsOfType<Canvas>().Where(x => x.isRootCanvas).ToArray();
        var modes = canvases.Select(x => x.renderMode).ToArray();
        var cameras = canvases.Select(x => x.worldCamera).ToArray();
        var distances = canvases.Select(x => x.planeDistance).ToArray();
        if (camera == null) return;
        var texture = new RenderTexture(1280, 720, 24); texture.Create();
        var original = camera.targetTexture;
        var active = RenderTexture.active;
        Texture2D image = null;
        try
        {
            camera.targetTexture = texture;
            foreach (var canvas in canvases) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1; }
            Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = texture;
            image = new Texture2D(1280, 720, TextureFormat.RGB24, false); image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); image.Apply();
            Directory.CreateDirectory("Temp/ExistingRefactor/Previews");
            File.WriteAllBytes("Temp/ExistingRefactor/Previews/" + name + ".png", image.EncodeToPNG());
        }
        finally
        {
            for (int i = 0; i < canvases.Length; i++) { canvases[i].renderMode = modes[i]; canvases[i].worldCamera = cameras[i]; canvases[i].planeDistance = distances[i]; }
            camera.targetTexture = original; RenderTexture.active = active;
            if (image != null) UnityEngine.Object.Destroy(image); texture.Release(); UnityEngine.Object.Destroy(texture);
        }
    }
}
