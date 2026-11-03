using UnityEngine;

[CreateAssetMenu(menuName = "Game UI/Theme")]
public class GameUITheme : ScriptableObject
{
    [Header("Colors")]
    [Tooltip("Body text and focus outline on pale secondary controls.")]
    public Color ink = new Color32(32, 54, 69, 255);
    [Tooltip("Main action surface. Check contrast with its white label after editing.")]
    public Color primary = new Color32(32, 95, 117, 255);
    [Tooltip("Panel surface behind form content.")]
    public Color surface = Color.white;
    [Tooltip("Input and read-only detail row background.")]
    public Color field = new Color32(234, 243, 246, 255);
    [Tooltip("Focus outline on primary controls; not intended for small body text.")]
    public Color highlight = new Color32(242, 191, 80, 255);
    [Tooltip("Error text. Always pair errors with a readable message.")]
    public Color error = new Color32(168, 50, 66, 255);
    [Header("Motion (seconds, unscaled time)")]
    [Tooltip("Response time for button hover and press feedback.")]
    [Range(0.05f, 0.4f)]
    public float feedbackSeconds = 0.12f;
    [Tooltip("Panel opening duration. Reduced motion skips the transition.")]
    [Range(0.1f, 0.4f)]
    public float panelSeconds = 0.22f;
    void OnValidate()
    {
        // Only loaded objects are updated here; unopened scenes apply on enable.
        foreach (var binding in FindObjectsOfType<GameUIThemeBinding>(true))
            if (binding.theme == this)
                binding.Apply();
    }
}
