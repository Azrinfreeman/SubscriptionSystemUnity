using TMPro;
using UnityEngine;

public class GameUIPreferences : MonoBehaviour
{
    const string MotionKey = "GameUI.ReducedMotion";
    public TMP_Text label;
    public static bool ReducedMotion => PlayerPrefs.GetInt(MotionKey, 0) == 1;

    void OnEnable()
    {
        Render();
    }

    public void ToggleMotion()
    {
        PlayerPrefs.SetInt(MotionKey, ReducedMotion ? 0 : 1);
        PlayerPrefs.Save();
        Render();
    }

    void Render()
    {
        if (label != null)
            label.text = ReducedMotion ? "Animasi: minimum" : "Animasi: penuh";
    }
}
