using TMPro;
using UnityEngine;

public class loadSubscriptionDetails : MonoBehaviour
{
    [SerializeField]
    TMP_InputField planField;
    [SerializeField]
    TMP_InputField startField;
    [SerializeField]
    TMP_InputField endField;
    [SerializeField]
    TMP_InputField statusField;
    [SerializeField]
    GameObject inactiveIcon;
    [SerializeField]
    GameObject activeIcon;
    void OnEnable()
    {
        PlayerManager.StateChanged += Render;
        Render();
        if (PlayerManager.instance != null)
            PlayerManager.instance.RequestRefresh();
    }

    public void Reassign()
    {
        if (PlayerManager.instance != null)
            PlayerManager.instance.RequestRefresh();
    }

    void Render()
    {
        var manager = PlayerManager.instance;
        bool available = manager != null && manager.SubscriptionFresh;
        bool active = available && manager.CanAccessPremium();
        Set(planField, manager != null && !string.IsNullOrEmpty(manager.plan) ? manager.plan : "Tiada pelan");
        Set(startField, "Bermula: " + Date(manager != null ? manager.start_date : null));
        Set(endField, "Akses tamat: " + Date(manager != null ? manager.end_date : null));
        Set(statusField, StatusMessage(manager, available));
        if (inactiveIcon != null)
            inactiveIcon.SetActive(!active);
        if (activeIcon != null)
            activeIcon.SetActive(active);
    }

    static string StatusMessage(PlayerManager manager, bool available)
    {
        // Keep request failures visible before displaying any cached status.
        if (!string.IsNullOrEmpty(manager?.LastError)) return manager.LastError;
        if (manager != null && manager.IsRefreshing) return "Menyemak langganan...";
        if (!available) return "Status langganan belum tersedia";
        string status = string.IsNullOrEmpty(manager.status) ? "TIADA LANGGANAN" : manager.status.ToUpperInvariant();
        return "Status langganan: " + status;
    }

    static string Date(string value) => string.IsNullOrWhiteSpace(value) || value == "0" ? "—" : value;
    static void Set(TMP_InputField field, string value)
    {
        if (field != null)
        {
            field.readOnly = true;
            field.SetTextWithoutNotify(value ?? "");
        }
    }

    void OnDisable()
    {
        PlayerManager.StateChanged -= Render;
    }
}
