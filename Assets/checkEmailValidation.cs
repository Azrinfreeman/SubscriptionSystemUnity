using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class checkEmailValidation : MonoBehaviour
{
    [Tooltip("Require current subscription/customer details for change/manage actions.")]
    [SerializeField]
    bool requireSubscription;
    Button button;
    void OnEnable()
    {
        button = GetComponent<Button>();
        PlayerManager.StateChanged += Render;
        Render();
    }

    void Render()
    {
        var manager = PlayerManager.instance;
        bool accountReady = manager != null && manager.IsVerified && !manager.IsLoggingOut;
        bool subscriptionReady = manager != null && manager.SubscriptionFresh
            && !string.IsNullOrEmpty(manager.stripe_id)
            && !string.IsNullOrEmpty(PlayerPrefs.GetString("price_id"));
        button.interactable = accountReady && (!requireSubscription || subscriptionReady);
    }

    void OnDisable()
    {
        PlayerManager.StateChanged -= Render;
    }
}
