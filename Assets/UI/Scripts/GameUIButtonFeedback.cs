using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class GameUIButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
{
    public GameUITheme theme;
    [Tooltip("Visual child only; the button's hit area stays fixed.")]
    public RectTransform visual;
    Button button;
    Outline focusOutline;
    GameUIThemeBinding themeBinding;
    bool hovered;
    bool selected;
    bool pressed;
    void Awake()
    {
        button = GetComponent<Button>();
        if (visual != null)
        {
            focusOutline = visual.GetComponent<Outline>();
            themeBinding = visual.GetComponent<GameUIThemeBinding>();
        }
    }

    void Update()
    {
        if (visual == null)
            return;
        bool usable = button != null && button.IsInteractable();
        UpdateFocusOutline(usable);

        if (GameUIPreferences.ReducedMotion)
        {
            visual.localScale = Vector3.one;
            return;
        }

        float target = TargetScale(usable);
        visual.localScale = Vector3.Lerp(visual.localScale, Vector3.one * target, 1 - Mathf.Exp(-Time.unscaledDeltaTime * 4 / (theme != null ? theme.feedbackSeconds : 0.12f)));
    }

    void UpdateFocusOutline(bool usable)
    {
        if (focusOutline == null)
            return;

        focusOutline.enabled = usable && (hovered || selected);
        if (theme != null)
        {
            bool paleControl = themeBinding != null && themeBinding.role == GameUIThemeBinding.Role.Field;
            focusOutline.effectColor = paleControl ? theme.ink : theme.highlight;
        }
    }

    float TargetScale(bool usable)
    {
        if (!usable) return 1;
        if (pressed) return 0.96f;
        return hovered || selected ? 1.025f : 1;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        hovered = true;
    }

    public void OnPointerExit(PointerEventData e)
    {
        hovered = pressed = false;
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (e.button == PointerEventData.InputButton.Left)
            pressed = true;
    }

    public void OnPointerUp(PointerEventData e)
    {
        pressed = false;
    }

    public void OnSelect(BaseEventData e)
    {
        selected = true;
    }

    public void OnDeselect(BaseEventData e)
    {
        selected = pressed = false;
    }

    void OnDisable()
    {
        hovered = selected = pressed = false;
        if (visual != null)
            visual.localScale = Vector3.one;
    }
}
