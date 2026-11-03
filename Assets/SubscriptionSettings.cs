using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Subscription System/Settings")]
public sealed class SubscriptionSettings : ScriptableObject
{
    [Tooltip("Your HTTPS PHP backend folder, e.g. https://example.com/subscriptions/")]
    public string backendBaseUrl = "";
    [Tooltip("Exact game key in the server catalog, not the menu's display title.")]
    public string gameIdentifier = "Your Game";
    [Min(1)]
    public int requestTimeoutSeconds = 20;
    [Min(5)]
    public float refreshIntervalSeconds = 30;
    public string loginScene = "LogMasuk";
    public string homeScene = "LamanUtama";
    [Tooltip("Legacy contract: login response 0 means accepted; later profile responses >0 mean session valid. Confirm this with your PHP backend.")]
    public bool enforceServerLoginStatus = true;
    [Header("API endpoints (relative PHP paths)")]
    public string registrationEndpoint = "insertUser.php";
    public string loginEndpoint = "loginVerify.php";
    public string profileEndpoint = "getinfouser.php";
    public string subscriptionEndpoint = "getinfo.php";
    public string verificationEndpoint = "sendVerification.php";
    public string logoutEndpoint = "logout.php";
    public string billingLinkEndpoint = "createBillingLink.php";
    [Header("Allowed billing ticket pages")]
    public string choosePlanPage = "pilihplan.php";
    public string changePlanPage = "tukarplan.php";
    public string managePlanPage = "manageSub.php";
    public List<string> Validate()
    {
        var errors = new List<string>();
        if (!Uri.TryCreate(backendBaseUrl, UriKind.Absolute, out var uri) || uri.Scheme != "https" || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
            errors.Add("Set an HTTPS backend base URL on SubscriptionSettings.");
        if (string.IsNullOrWhiteSpace(gameIdentifier))
            errors.Add("Set a game/module identifier.");
        if (requestTimeoutSeconds < 1 || requestTimeoutSeconds > 120)
            errors.Add("Request timeout must be 1–120 seconds.");
        if (refreshIntervalSeconds < 5)
            errors.Add("Refresh interval must be at least five seconds.");
        if (string.IsNullOrWhiteSpace(loginScene) || string.IsNullOrWhiteSpace(homeScene))
            errors.Add("Set login and home scene names.");
        foreach (var path in new[]
        {
            registrationEndpoint,
            loginEndpoint,
            profileEndpoint,
            subscriptionEndpoint,
            verificationEndpoint,
            logoutEndpoint,
            billingLinkEndpoint,
            choosePlanPage,
            changePlanPage,
            managePlanPage
        })
            if (!IsRelativePath(path))
                errors.Add("Endpoint paths must be relative files without query strings: " + path);
        return errors;
    }

    public string Url(string relativePath)
    {
        var errors = Validate();
        if (errors.Count > 0)
            throw new InvalidOperationException(errors[0]);
        if (!IsRelativePath(relativePath))
            throw new InvalidOperationException("Use a configured relative endpoint/page path.");
        return backendBaseUrl.TrimEnd('/') + "/" + relativePath;
    }

    public static bool IsRelativePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        return !path.StartsWith("/") && !path.Contains(":") && !path.Contains("..")
            && !path.Contains("\\") && !path.Contains("?") && !path.Contains("#") && !path.Contains(" ");
    }
    public static string Query(string url, string name, string value) => url + (url.Contains("?") ? "&" : "?") + Uri.EscapeDataString(name) + "=" + Uri.EscapeDataString(value ?? "");
    public bool AllowsBillingUrl(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var candidate) || !Uri.TryCreate(backendBaseUrl.TrimEnd('/') + "/", UriKind.Absolute, out var root))
            return false;
        if (candidate.Scheme != "https" || candidate.Host != root.Host || candidate.Port != root.Port || !string.IsNullOrEmpty(candidate.UserInfo) || !string.IsNullOrEmpty(candidate.Fragment))
            return false;
        foreach (var path in new[]
        {
            choosePlanPage,
            changePlanPage,
            managePlanPage
        })
            if (candidate.AbsolutePath == new Uri(root, path).AbsolutePath && candidate.Query.StartsWith("?ticket=", StringComparison.Ordinal))
                return true;
        return false;
    }
}
