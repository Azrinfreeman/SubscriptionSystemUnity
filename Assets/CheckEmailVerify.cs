using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CheckEmailVerify : MonoBehaviour
{
    Button button;
    Coroutine cooldown;
    void OnEnable()
    {
        button = GetComponent<Button>();
        PlayerManager.StateChanged += Render;
        Render();
    }

    void Render()
    {
        var manager = PlayerManager.instance;
        bool accountReady = manager != null && manager.IsLoggedIn && manager.ProfileFresh
            && !manager.IsVerified && !manager.IsLoggingOut;
        bool requestReady = manager != null && !manager.IsSendingVerification
            && Time.unscaledTime >= manager.VerificationAllowedAt;
        button.interactable = accountReady && requestReady;
        if (cooldown != null)
            StopCoroutine(cooldown);
        cooldown = null;
        if (manager != null && manager.VerificationAllowedAt > Time.unscaledTime && isActiveAndEnabled)
            cooldown = StartCoroutine(WaitForCooldown(manager.VerificationAllowedAt - Time.unscaledTime));
    }

    IEnumerator WaitForCooldown(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        cooldown = null;
        Render();
    }

    void OnDisable()
    {
        PlayerManager.StateChanged -= Render;
        StopAllCoroutines();
        cooldown = null;
    }
}
