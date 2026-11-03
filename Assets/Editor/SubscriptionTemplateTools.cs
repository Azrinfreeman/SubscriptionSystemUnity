using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SubscriptionTemplateTools
{
    public static string BuildWindowsValidation()
    {
        if (EditorApplication.isPlaying || BuildPipeline.isBuildingPlayer) throw new InvalidOperationException("Stop Play mode or wait for the current build.");
        const string directory = "Build/SubscriptionValidation";
        System.IO.Directory.CreateDirectory(directory);
        string result;
        try
        {
            var report = BuildPipeline.BuildPlayer(EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.path).ToArray(), directory + "/SubscriptionSystem.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
            result = report.summary.result + " | errors=" + report.summary.totalErrors + " | warnings=" + report.summary.totalWarnings;
        }
        catch (Exception error) { result = "FAILED: " + error.Message; }
        System.IO.File.WriteAllText(directory + "/build-result.txt", result);
        return result;
    }
    public const string SettingsPath = "Assets/Settings/SubscriptionSettings.asset";
    static readonly string[] ScenePaths = { "Assets/Scenes/LogMasuk.unity", "Assets/Scenes/LamanUtama.unity" };

    [MenuItem("Tools/Subscription System/Select Settings")]
    public static void SelectSettings() { Selection.activeObject = Settings(); EditorGUIUtility.PingObject(Selection.activeObject); }
    static SubscriptionSettings Settings()
    {
        var asset = AssetDatabase.LoadAssetAtPath<SubscriptionSettings>(SettingsPath);
        if (asset != null) return asset;
        if (!AssetDatabase.IsValidFolder("Assets/Settings")) AssetDatabase.CreateFolder("Assets", "Settings");
        asset = ScriptableObject.CreateInstance<SubscriptionSettings>(); AssetDatabase.CreateAsset(asset, SettingsPath); AssetDatabase.SaveAssets();
        return asset;
    }

    [MenuItem("Tools/Subscription System/Validate Current Scene")]
    public static void ValidateCurrentScene() { Debug.Log(Validate(SceneManager.GetActiveScene())); }
    public static string Validate(Scene scene)
    {
        var issues = new List<string>();
        var domains = Components<Domains>(scene).ToArray();
        if (domains.Length != 1) issues.Add("Expected one Domains object in this scene.");
        if (domains.Length == 1)
        {
            if (domains[0].settings == null) issues.Add("Assign the shared SubscriptionSettings asset.");
            else issues.AddRange(domains[0].settings.Validate());
        }
        if (Components<PlayerManager>(scene).Count() != 1) issues.Add("Expected one PlayerManager in this scene.");
        foreach (var root in scene.GetRootGameObjects()) foreach (var transform in root.GetComponentsInChildren<Transform>(true))
        {
            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) > 0) issues.Add("Missing script: " + transform.name);
            foreach (var component in transform.GetComponents<MonoBehaviour>())
            {
                if (component == null || component.GetType().Assembly != typeof(Player).Assembly) continue;
                foreach (var field in component.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
                    if (field.IsDefined(typeof(SerializeField), true) && typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType) && field.GetValue(component) as UnityEngine.Object == null)
                        issues.Add(component.GetType().Name + "." + field.Name + " is unassigned on " + component.name);
            }
        }
        foreach (var button in Components<Button>(scene))
            for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
            {
                var target = button.onClick.GetPersistentTarget(i); var method = button.onClick.GetPersistentMethodName(i);
                if (target == null || !target.GetType().GetMethods().Any(x => x.Name == method)) issues.Add("Invalid button event: " + button.name + " → " + method);
            }
        var enabled = EditorBuildSettings.scenes.Where(x => x.enabled).Select(x => x.path).ToArray();
        foreach (var path in ScenePaths) if (!enabled.Contains(path)) issues.Add("Enable existing build scene: " + path);
        return issues.Count == 0 ? "Subscription scene validation passed." : string.Join("\n", issues.Distinct());
    }

    // Initial migration utility: edits only the two existing scenes. Preserves their names,
    // GUIDs, layouts, graphics, navigation, animations and build list.
    public static string WireExistingScenes()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode first.");
        if (SceneManager.GetActiveScene().isDirty) throw new InvalidOperationException("Save current scene edits before wiring references.");
        string original = SceneManager.GetActiveScene().path;
        var settings = Settings();
        var reports = new List<string>();
        foreach (string path in ScenePaths)
        {
            var scene = EditorSceneManager.OpenScene(path);
            foreach (var domain in Components<Domains>(scene)) Bind(domain, "settings", AssetDatabase.LoadAssetAtPath<SubscriptionSettings>(SettingsPath));
            var manager = Components<PlayerManager>(scene).Single();
            if (manager.GetComponent<Player>() == null) manager.gameObject.AddComponent<Player>();
            foreach (var login in Components<LoginController>(scene))
            {
                Bind(login, "emailText", login.transform.Find("Email").GetComponent<TMP_InputField>());
                Bind(login, "passText", login.transform.Find("Password").GetComponent<TMP_InputField>());
                Bind(login, "emailWarningTransform", login.transform.Find("EmailWarning"));
                Bind(login, "emailWarningText", login.transform.Find("EmailWarning").GetComponentInChildren<TMP_Text>(true));
                Bind(login, "btnLog", login.transform.Find("LogBtn").GetComponentInChildren<Button>(true));
                var notification = Components<Animator>(scene).Single(x => x.name == "Notification");
                Bind(login, "Notification", notification.transform); Bind(login, "notificationAnimator", notification);
                Bind(login, "notificationText", notification.GetComponentInChildren<TMP_Text>(true));
                Bind(login, "deviceText", GetReference<Transform>(login, "LogoutPanel").Find("Panel/bg/peranti").GetComponent<TMP_Text>());
                WireNotice(login);
            }
            foreach (var register in Components<RegisterUserController>(scene))
            {
                Bind(register, "btnDaftar", register.transform.Find("DaftarBtn").GetComponentInChildren<Button>(true));
                Bind(register, "emailWarningTransform", register.transform.Find("EmailWarning"));
                Bind(register, "notelWarningTransform", register.transform.Find("PhoneWarning"));
                Bind(register, "emailWarningText", register.transform.Find("EmailWarning").GetComponentInChildren<TMP_Text>(true));
                Bind(register, "phoneWarningText", register.transform.Find("PhoneWarning").GetComponentInChildren<TMP_Text>(true));
                WireNotice(register);
            }
            foreach (var details in Components<loadDetails>(scene))
            {
                WireFields(details, new[] { "nameField", "emailField", "phoneField", "verificationField" });
                var status = details.transform.GetChild(3);
                Bind(details, "unverifiedIcon", status.GetChild(1).gameObject); Bind(details, "verifiedIcon", status.GetChild(2).gameObject);
                var verify = details.GetComponentInChildren<CheckEmailVerify>(true).GetComponent<Button>();
                Bind(details, "verificationButton", verify); Bind(details, "verificationMessage", verify.transform.GetChild(1).GetComponent<TMP_Text>());
            }
            foreach (var details in Components<loadSubscriptionDetails>(scene))
            {
                WireFields(details, new[] { "planField", "startField", "endField", "statusField" });
                Bind(details, "inactiveIcon", details.transform.GetChild(3).GetChild(1).gameObject); Bind(details, "activeIcon", details.transform.GetChild(3).GetChild(2).gameObject);
            }
            foreach (var payments in Components<LoadPaymentSite>(scene)) Bind(payments, "subscriptionPanel", Components<Animator>(scene).Single(x => x.name == "SubscriptionDetails").gameObject);
            foreach (var button in Components<checkEmailValidation>(scene))
            {
                var data = new SerializedObject(button); data.FindProperty("requireSubscription").boolValue = button.name == "Manage" || button.name == "TukarPlan"; data.ApplyModifiedPropertiesWithoutUndo();
            }
            int repaired = 0;
            foreach (var button in Components<Button>(scene))
            {
                var data = new SerializedObject(button);
                var calls = data.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
                for (int i = 0; i < calls.arraySize; i++)
                {
                    var call = calls.GetArrayElementAtIndex(i);
                    if (call.FindPropertyRelative("m_MethodName").stringValue == "Play" && call.FindPropertyRelative("m_Target").objectReferenceValue == null && button.name == "SubscriptionDetail")
                    {
                        var panel = button.onClick.GetPersistentTarget(0) as GameObject;
                        if (panel == null || panel.GetComponent<Animator>() == null) throw new InvalidOperationException("Subscription panel animator is missing.");
                        call.FindPropertyRelative("m_Target").objectReferenceValue = panel.GetComponent<Animator>();
                        repaired++;
                    }
                    if (call.FindPropertyRelative("m_MethodName").stringValue != "Reassign") continue;
                    call.FindPropertyRelative("m_Target").objectReferenceValue = manager;
                    call.FindPropertyRelative("m_TargetAssemblyTypeName").stringValue = typeof(PlayerManager).AssemblyQualifiedName;
                    call.FindPropertyRelative("m_MethodName").stringValue = "RequestRefresh";
                    call.FindPropertyRelative("m_Mode").enumValueIndex = (int)PersistentListenerMode.Void;
                    repaired++;
                }
                data.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            reports.Add(path + ": wired existing controls; repaired " + repaired + " refresh event(s). " + Validate(scene));
        }
        AssetDatabase.SaveAssets();
        if (!string.IsNullOrEmpty(original)) EditorSceneManager.OpenScene(original);
        return string.Join("\n", reports);
    }
    static IEnumerable<T> Components<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<T>(true));
    static void Bind(UnityEngine.Object target, string field, UnityEngine.Object value)
    {
        if (value == null) throw new InvalidOperationException("Missing reference: " + target.name + "." + field);
        var data = new SerializedObject(target); data.FindProperty(field).objectReferenceValue = value; data.ApplyModifiedPropertiesWithoutUndo();
    }
    static T GetReference<T>(UnityEngine.Object target, string field) where T : UnityEngine.Object => new SerializedObject(target).FindProperty(field).objectReferenceValue as T;
    static void WireNotice(UnityEngine.Object controller)
    {
        var bg = GetReference<Transform>(controller, "Notice").Find("bg");
        Bind(controller, "noticeLoading", bg.Find("Loading").gameObject); Bind(controller, "noticeError", bg.Find("failed").gameObject); Bind(controller, "noticeSuccess", bg.Find("Success").gameObject); Bind(controller, "noticeText", bg.Find("Text (TMP)").GetComponent<TMP_Text>());
    }
    static void WireFields(Component target, string[] fields)
    { for (int i = 0; i < fields.Length; i++) Bind(target, fields[i], target.transform.GetChild(i).GetComponent<TMP_InputField>()); }
}
