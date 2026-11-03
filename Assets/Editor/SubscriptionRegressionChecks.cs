using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// Offline checks for the existing client. No network or payment service is contacted.
public static class SubscriptionRegressionChecks
{
    [MenuItem("Tools/Subscription System/Run Offline Regression Checks")]
    public static void RunMenu() { Debug.Log(Run()); }
    public static string Run()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode before running data regression checks.");
        int count = 0;
        Action<bool, string> check = (condition, description) => { if (!condition) throw new InvalidOperationException("FAILED: " + description); count++; };
        Player.PlayerInfo profile;
        Player.PlayerSubscription subscription;
        check(Player.TryProfile("{\"id_user\":9,\"email\":\"test@example.invalid\",\"login_logs\":1}", out profile), "accept expected profile JSON");
        check(!Player.TryProfile("error", out profile), "reject server error literal as profile");
        check(!Player.TryProfile("{\"id_user\":9,\"email\":\"test@example.invalid\"}", out profile), "reject missing session-status field");
        check(!Player.TryProfile("{broken", out profile), "reject malformed profile JSON");
        check(!Player.TrySubscription("{}", out subscription), "reject empty subscription object");
        check(Player.TrySubscription("{\"status\":\"active\",\"plan\":\"Premium\",\"access_until\":1900000000}", out subscription), "accept expected subscription JSON");
        check(LoginController.IsEmail("player+game@example.technology"), "accept long email suffix and plus addressing");
        check(!LoginController.IsEmail("bad email@example.com"), "reject whitespace in email");
        check(RegisterUserController.IsPhoneNbr("0123456789"), "accept Malaysian mobile number");
        check(!RegisterUserController.IsPhoneNbr("012345678901234"), "reject unbounded phone length");
        var settings = ScriptableObject.CreateInstance<SubscriptionSettings>();
        try
        {
            check(settings.Validate().Count > 0, "require backend configuration");
            settings.backendBaseUrl = "https://example.invalid/subscriptions/";
            check(settings.Url("getinfo.php") == "https://example.invalid/subscriptions/getinfo.php", "normalize endpoint joining");
            string url = SubscriptionSettings.Query(settings.Url(settings.choosePlanPage), "email", "player+test@example.invalid&admin=1");
            check(url.EndsWith("email=player%2Btest%40example.invalid%26admin%3D1"), "encode payment query values");
            check(!SubscriptionSettings.IsRelativePath("../other.php") && !SubscriptionSettings.IsRelativePath("https://other.invalid/"), "reject traversal and external endpoint paths");
            check(settings.AllowsBillingUrl("https://example.invalid/subscriptions/pilihplan.php?ticket=test"), "allow same-origin configured billing handoff");
            check(!settings.AllowsBillingUrl("https://attacker.invalid/pilihplan.php?ticket=test") && !settings.AllowsBillingUrl("https://example.invalid/other.php?ticket=test"), "reject foreign billing hosts and paths");
            settings.backendBaseUrl = "http://example.invalid/";
            check(settings.Validate().Count > 0, "reject insecure backend URL");
        }
        finally { UnityEngine.Object.DestroyImmediate(settings); }

        string[] integers = { "id_user", "login_logs", "isLogin" };
        string[] strings = { "name", "email", "notel", "confirmation", "stripe_id", "stripe_sub_id", "pmc_id", "product_id", "price_id", "start_date", "end_date", "plan", "plan_desc", "status", "stripe_phone", "SubscriptionRegression.Unrelated" };
        var savedInts = new Dictionary<string, int>(); var savedStrings = new Dictionary<string, string>();
        foreach (var key in integers) if (PlayerPrefs.HasKey(key)) savedInts[key] = PlayerPrefs.GetInt(key);
        foreach (var key in strings) if (PlayerPrefs.HasKey(key)) savedStrings[key] = PlayerPrefs.GetString(key);
        GameObject owner = null;
        try
        {
            owner = new GameObject("Offline subscription regression checks");
            var manager = owner.AddComponent<PlayerManager>();
            manager.ApplyProfile(new Player.PlayerInfo { id_user = 9, name = "Offline Check", email = "test@example.invalid", confirmation = "1", login_logs = 1 });
            PlayerPrefs.SetInt("isLogin", 1);
            check(!manager.IsLoggedIn, "cached login flag cannot replace an authentication credential");
            manager.ApplySubscription(new Player.PlayerSubscription {
                stripe_id = "customer-test", stripe_sub_id = "subscription-test", stripe_pmc_id = "method-test",
                stripe_product_id = "product-test", stripe_price_id = "price-test", stripe_phone = "phone-test",
                plan = "Premium", plan_desc = "description-test", status = "active", start_date = "start-test", end_date = "end-test"
            });
            check(PlayerPrefs.GetString("stripe_id") == "customer-test" && PlayerPrefs.GetString("stripe_sub_id") == "subscription-test"
                && PlayerPrefs.GetString("pmc_id") == "method-test" && PlayerPrefs.GetString("product_id") == "product-test"
                && PlayerPrefs.GetString("price_id") == "price-test", "map billing identifiers to their legacy cache keys");
            check(manager.plan == "Premium" && manager.plan_desc == "description-test" && manager.status == "active"
                && manager.stripe_phone == "phone-test", "round-trip subscription display fields without swapping values");
            check(manager.start_date == "start-test" && manager.end_date == "end-test", "use correct date preference keys");
            check(!manager.CanAccessPremium(), "cached active status alone does not grant fresh access");
            manager.ApplySubscription(new Player.PlayerSubscription());
            check(manager.stripe_id == "" && manager.plan == "" && PlayerPrefs.GetString("price_id") == ""
                && manager.start_date == "" && manager.end_date == "", "null subscription fields replace old cache values with empty strings");
            PlayerPrefs.SetString("SubscriptionRegression.Unrelated", "preserve-me");
            int version = manager.SessionVersion;
            manager.ClearAccount();
            check(manager.SessionVersion > version, "invalidate pending responses on account clear");
            check(manager.id_user == 0 && manager.email == "" && manager.stripe_id == "" && manager.plan == "", "clear saved and in-memory account data");
            check(PlayerPrefs.GetString("SubscriptionRegression.Unrelated") == "preserve-me", "preserve unrelated game preferences");
            check(owner.GetComponents<Player>().Length == 1, "one reusable Player component");
        }
        finally
        {
            if (owner != null) UnityEngine.Object.DestroyImmediate(owner);
            foreach (var key in integers) { if (savedInts.ContainsKey(key)) PlayerPrefs.SetInt(key, savedInts[key]); else PlayerPrefs.DeleteKey(key); }
            foreach (var key in strings) { if (savedStrings.ContainsKey(key)) PlayerPrefs.SetString(key, savedStrings[key]); else PlayerPrefs.DeleteKey(key); }
            PlayerPrefs.Save();
        }
        return count + " offline subscription regression checks passed. Existing preferences restored.";
    }
}
