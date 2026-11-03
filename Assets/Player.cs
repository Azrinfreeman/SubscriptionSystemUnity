using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

// Existing request component, shared by PlayerManager. Requests return data;
// only PlayerManager applies account data, so stale responses cannot restore a logged-out user.
public interface IPlayerRequests
{
    IEnumerator Post(string endpoint, WWWForm form, Action<Player.Result> completed);
}

public class Player : MonoBehaviour, IPlayerRequests
{
    [Serializable]
    public class PlayerInfo
    {
        public int id_user;
        public string name;
        public string email;
        public string notel;
        public string confirmation;
        public string device_name;
        public int login_logs;
        public string session_token;
        public string session_action_token;
        public long session_expires_at;
    }

    [Serializable]
    public class PlayerSubscription
    {
        public string stripe_id;
        public string stripe_sub_id;
        public string stripe_pmc_id;
        public string stripe_product_id;
        public string stripe_price_id;
        public string start_date;
        public string end_date;
        public string plan;
        public string plan_desc;
        public string status;
        public string stripe_phone;
        public long access_until;
    }

    [Serializable]
    public class BillingLink
    {
        public string url;
        public long expires_at;
    }

    [Serializable]
    class ServerError
    {
        public string code;
        public string message;
    }

    public sealed class Result
    {
        public bool success;
        public string body;
        public string error;
        public long statusCode;
    }

    public static SubscriptionSettings Settings => Domains.instance != null ? Domains.instance.settings : null;

    public virtual IEnumerator Post(string endpoint, WWWForm form, Action<Result> completed)
    {
        string url = null;
        string configurationError = null;
        try
        {
            if (Settings == null)
                throw new InvalidOperationException("Assign SubscriptionSettings to Domains in both scenes.");
            url = Settings.Url(endpoint);
        }
        catch (InvalidOperationException error)
        {
            configurationError = error.Message;
        }

        if (configurationError != null)
        {
            completed(new Result { error = configurationError });
            yield break;
        }

        using (var request = UnityWebRequest.Post(url, form))
        {
            var manager = PlayerManager.instance;
            int version = manager != null ? manager.SessionVersion : -1;
            string credential = manager != null ? manager.RequestCredential(endpoint) : "";
            bool publicEndpoint = endpoint == Settings.loginEndpoint || endpoint == Settings.registrationEndpoint;
            if (!string.IsNullOrEmpty(credential) && !publicEndpoint)
                request.SetRequestHeader("Authorization", "Bearer " + credential);
            request.timeout = Settings.requestTimeoutSeconds;
            request.redirectLimit = 0;
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                string message = ErrorMessage(request.downloadHandler.text, request.responseCode);
                completed(new Result { statusCode = request.responseCode, error = message });
                // Deliver the response first, then expire only the captured session.
                // A late 401 from an older account must not clear a newer login.
                if (request.responseCode == 401 && manager != null && !publicEndpoint && endpoint != Settings.logoutEndpoint)
                    manager.ExpireSession(version);
            }
            else
                completed(new Result { success = true, statusCode = request.responseCode, body = request.downloadHandler.text.Trim() });
        }
    }

    static string ErrorMessage(string body, long statusCode)
    {
        string fallback = "Tidak dapat menghubungi server. Sila semak sambungan dan cuba lagi. (" + statusCode + ")";
        try
        {
            var error = JsonUtility.FromJson<ServerError>(body);
            return !string.IsNullOrWhiteSpace(error?.message) ? error.message : fallback;
        }
        catch (ArgumentException)
        {
            // A host may return HTML instead of the expected JSON error.
            return fallback;
        }
    }

    public static bool TryProfile(string json, out PlayerInfo profile)
    {
        profile = null;
        if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{") || !json.Contains("\"login_logs\""))
            return false;
        try
        {
            profile = JsonUtility.FromJson<PlayerInfo>(json);
        }
        catch (ArgumentException)
        {
            return false;
        }

        return profile != null && profile.id_user > 0 && !string.IsNullOrWhiteSpace(profile.email) && profile.login_logs >= 0;
    }

    public static bool TrySubscription(string json, out PlayerSubscription subscription)
    {
        subscription = null;
        if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{") || !json.Contains("\"access_until\""))
            return false;
        try
        {
            subscription = JsonUtility.FromJson<PlayerSubscription>(json);
        }
        catch (ArgumentException)
        {
            return false;
        }

        return subscription != null && !string.IsNullOrWhiteSpace(subscription.status);
    }

    // Keep the old public entry points for existing UnityEvent/custom-code callers.
    public void FetchUserInfo()
    {
        if (PlayerManager.instance != null)
            PlayerManager.instance.RequestRefresh();
    }

    public void FetchPlayerSubscription(string emailText)
    {
        FetchUserInfo();
    }

    public void LogoutUser(string sceneName)
    {
        if (PlayerManager.instance != null)
            PlayerManager.instance.Logout(sceneName);
    }
}
