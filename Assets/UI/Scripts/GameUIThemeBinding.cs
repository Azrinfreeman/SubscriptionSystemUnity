using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways, RequireComponent(typeof(Graphic))]
public class GameUIThemeBinding : MonoBehaviour
{
    public enum Role
    {
        Ink = 0,
        Primary = 1,
        Surface = 2,
        Field = 3,
        Highlight = 4,
        Error = 5
    }

    [Tooltip("Shared theme. Keep the same asset assigned in both menu scenes.")]
    public GameUITheme theme;
    [Tooltip("Color role applied to this object's UI Graphic or TMP text.")]
    public Role role;
    void OnEnable()
    {
        Apply();
    }

    void OnValidate()
    {
        Apply();
    }

    public void Apply()
    {
        if (theme == null)
            return;
        var graphic = GetComponent<Graphic>();
        graphic.color = ColorForRole();
    }

    Color ColorForRole()
    {
        switch (role)
        {
            case Role.Ink: return theme.ink;
            case Role.Primary: return theme.primary;
            case Role.Surface: return theme.surface;
            case Role.Field: return theme.field;
            case Role.Highlight: return theme.highlight;
            default: return theme.error;
        }
    }
}
