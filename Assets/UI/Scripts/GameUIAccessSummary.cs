using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class GameUIAccessSummary : MonoBehaviour
{
    TMP_Text label;
    void OnEnable()
    {
        label = GetComponent<TMP_Text>();
        PlayerManager.StateChanged += Render;
        Render();
    }

    void Render()
    {
        var manager = PlayerManager.instance;
        if (manager != null && manager.IsRefreshing)
            label.text = "Menyemak akses...";
        else if (manager == null || !manager.SubscriptionFresh)
            label.text = "Status akses belum tersedia";
        else
            label.text = manager.CanAccessPremium() ? "Akses aktif" : "Tiada langganan aktif";
    }

    void OnDisable()
    {
        PlayerManager.StateChanged -= Render;
    }
}
