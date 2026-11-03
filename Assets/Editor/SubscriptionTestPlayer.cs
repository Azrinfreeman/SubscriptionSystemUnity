using System;
using System.Collections;
using System.Text;
using UnityEngine;
using Result = Player.Result;

// Editor-only transport for regression checks; never compiled into a player build.
public sealed class SubscriptionTestPlayer : IPlayerRequests
{
    public int calls, profileCalls, registrationCalls, loginCalls, frames = 2;
    public bool failProfile, failLogout;
    public string logoutBody;
    public IEnumerator Post(string endpoint, WWWForm form, Action<Result> completed)
    {
        calls++;
        var settings = Player.Settings;
        if (endpoint == settings.profileEndpoint) profileCalls++;
        if (endpoint == settings.registrationEndpoint) registrationCalls++;
        if (endpoint == settings.loginEndpoint) loginCalls++;
        if (endpoint == settings.logoutEndpoint) logoutBody = Encoding.UTF8.GetString(form.data);
        for (int i = 0; i < frames; i++) yield return null;
        if (endpoint == settings.profileEndpoint && failProfile) { completed(new Result { error = "Offline test: network unavailable" }); yield break; }
        if (endpoint == settings.logoutEndpoint && failLogout) { completed(new Result { error = "Offline test: logout unavailable" }); yield break; }
        string body = endpoint == settings.profileEndpoint ? Profile(1) : endpoint == settings.loginEndpoint ? Profile(0) : endpoint == settings.subscriptionEndpoint ? "{\"stripe_id\":\"cus_test\",\"stripe_price_id\":\"price_test\",\"plan\":\"Test plan\",\"status\":\"active\",\"access_until\":" + DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds() + ",\"start_date\":\"start-test\",\"end_date\":\"end-test\"}" : endpoint == settings.logoutEndpoint ? "logout" : "error";
        completed(new Result { success = true, body = body });
    }
    static string Profile(int logs) => "{\"id_user\":9,\"name\":\"Offline Player\",\"email\":\"offline@example.invalid\",\"notel\":\"0123456789\",\"confirmation\":\"1\",\"session_token\":\"" + new string('a',64) + "\",\"session_expires_at\":" + DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds() + ",\"login_logs\":" + logs + "}";
}
