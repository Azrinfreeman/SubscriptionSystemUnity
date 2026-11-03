using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class loadNama : MonoBehaviour
{
    TextMeshProUGUI label;
    void OnEnable()
    {
        label = GetComponent<TextMeshProUGUI>();
        PlayerManager.StateChanged += Render;
        Render();
    }

    void Render()
    {
        label.text = "Selamat Datang " + (PlayerManager.instance != null ? PlayerManager.instance.nama : "");
    }

    void OnDisable()
    {
        PlayerManager.StateChanged -= Render;
    }
}
