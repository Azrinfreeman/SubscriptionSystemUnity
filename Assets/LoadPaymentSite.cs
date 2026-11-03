using System;
using System.Collections;
using Gpm.WebView;
using UnityEngine;

public class LoadPaymentSite : MonoBehaviour
{
    [SerializeField]
    GameObject subscriptionPanel;
    bool opening;
    float nextOpenTime;
    public void OpenPilihPlanPayment()
    {
        Open("choose");
    }

    public void OpenCustomUrl(string url)
    {
        var settings = Player.Settings;
        if (settings != null && (url == "tukarplan.php" || url == settings.changePlanPage))
            Open("change");
        else if (settings != null && (url == "manageSub.php" || url == settings.managePlanPage))
            Open("manage");
        else
            Feedback("Halaman langganan ini tidak dikonfigurasi.");
    }

    void Open(string action)
    {
        var manager = PlayerManager.instance;
        if (manager == null || opening || Time.unscaledTime < nextOpenTime)
            return;
        if (!manager.IsVerified || manager.IsLoggingOut)
        {
            Feedback("Sahkan email anda sebelum mengurus langganan.");
            return;
        }

        if (Player.Settings == null)
        {
            Feedback("Assign SubscriptionSettings to Domains.");
            return;
        }

        opening = true;
        StartCoroutine(RequestLink(manager, action));
    }

    IEnumerator RequestLink(PlayerManager manager, string action)
    {
        int version = manager.SessionVersion;
        try
        {
            var form = new WWWForm();
            form.AddField("action", action);
            form.AddField("_game_name", Player.Settings.gameIdentifier);
            Player.Result result = null;
            yield return manager.Requests.Post(Player.Settings.billingLinkEndpoint, form, value => result = value);
            if (version != manager.SessionVersion || !manager.IsLoggedIn)
                yield break;
            if (result == null || !result.success)
            {
                Feedback(result?.error ?? "Tiada respons server.");
                yield break;
            }

            Player.BillingLink link = null;
            try
            {
                link = JsonUtility.FromJson<Player.BillingLink>(result.body);
            }
            catch (ArgumentException)
            {
            }

            if (link == null || !Player.Settings.AllowsBillingUrl(link.url) || link.expires_at <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                Feedback("Server returned an invalid or expired billing link. Please retry.");
                yield break;
            }

            nextOpenTime = Time.unscaledTime + 1;
            manager.NeedsBillingRefresh = true;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            GpmWebView.ShowUrl(link.url, new GpmWebViewRequest.Configuration
            {
                style = GpmWebViewStyle.FULLSCREEN, orientation = GpmOrientation.UNSPECIFIED,
                isClearCookie = true, isClearCache = true, backgroundColor = "#FFFFFF",
                isNavigationBarVisible = true, navigationBarColor = "#4B96E6", title = action == "choose" ? "Subscribe Plan" : "Managing Your Plans",
                isBackButtonVisible = true, isForwardButtonVisible = true, isCloseButtonVisible = true, supportMultipleWindows = true
            }, OnCallback, null);
#else
            Application.OpenURL(link.url);
#endif
        }
        finally
        {
            opening = false;
        }
    }

    void Feedback(string message)
    {
        if (subscriptionPanel != null)
            subscriptionPanel.SetActive(true);
        if (PlayerManager.instance != null)
            PlayerManager.instance.SetError(message);
    }

    void OnCallback(GpmWebViewCallback.CallbackType type, string data, GpmWebViewError error)
    {
        var manager = PlayerManager.instance;
        if (manager == null)
            return;
        if (error != null)
            Feedback("Halaman pembayaran tidak dapat dibuka atau dimuat. Sila cuba lagi.");
        if (type == GpmWebViewCallback.CallbackType.Close)
            manager.RefreshAfterPayment();
#if UNITY_ANDROID
        if (type == GpmWebViewCallback.CallbackType.BackButtonClose) manager.RefreshAfterPayment();
#endif
    }

    void OnDisable()
    {
        StopAllCoroutines();
        opening = false;
    }
}
