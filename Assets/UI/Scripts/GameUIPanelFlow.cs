using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

[RequireComponent(typeof(CanvasGroup))]
public class GameUIPanelFlow : MonoBehaviour
{
    [Tooltip("Shared theme supplies panel opening duration.")]
    public GameUITheme theme;
    [Tooltip("Visibility root moved above sibling panels when opened.")]
    public GameObject panelRoot;
    [Tooltip("Existing Back action invoked by Escape; keep its onClick bindings.")]
    public Button backButton;
    CanvasGroup group;
    GameObject previousFocus;
    float elapsed;
    void OnEnable()
    {
        group = GetComponent<CanvasGroup>();
        elapsed = 0;
        if (panelRoot != null)
            panelRoot.transform.SetAsLastSibling();
        previousFocus = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        group.alpha = GameUIPreferences.ReducedMotion ? 1 : 0;
        group.blocksRaycasts = group.interactable = true;
        ConfigureNavigation();
    }

    Selectable[] AvailableControls()
    {
        // Re-evaluate live controls: validation can disable a button after opening.
        return GetComponentsInChildren<Selectable>()
            .Where(control => control.IsInteractable() && control.navigation.mode != Navigation.Mode.None)
            .ToArray();
    }

    void ConfigureNavigation()
    {
        var controls = AvailableControls();
        for (int i = 0; i < controls.Length; i++)
        {
            var navigation = controls[i].navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnUp = navigation.selectOnLeft = controls[(i + controls.Length - 1) % controls.Length];
            navigation.selectOnDown = navigation.selectOnRight = controls[(i + 1) % controls.Length];
            controls[i].navigation = navigation;
        }

        foreach (var control in controls)
            if (control.IsInteractable() && control.navigation.mode != Navigation.Mode.None)
            {
                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(control.gameObject);
                break;
            }
    }

    void Update()
    {
        UpdateTransition();
        bool topPanel = panelRoot != null && panelRoot.transform.GetSiblingIndex() == panelRoot.transform.parent.childCount - 1;
        if (!topPanel)
            return;

        HandleTabNavigation();
        HandleBack();
    }

    void UpdateTransition()
    {
        elapsed += Time.unscaledDeltaTime;
        float t = GameUIPreferences.ReducedMotion ? 1 : Mathf.Clamp01(elapsed / (theme != null ? theme.panelSeconds : 0.22f));
        float eased = 1 - Mathf.Pow(1 - t, 3);
        group.alpha = eased;
        transform.localScale = Vector3.one * Mathf.Lerp(0.98f, 1, eased);
    }

    void HandleTabNavigation()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && EventSystem.current != null)
        {
            var controls = AvailableControls();
            int current = System.Array.FindIndex(controls, x => x.gameObject == EventSystem.current.currentSelectedGameObject);
            int direction = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? -1 : 1;
            if (controls.Length > 0)
                EventSystem.current.SetSelectedGameObject(controls[(current + direction + controls.Length) % controls.Length].gameObject);
        }

    }

    void HandleBack()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && backButton != null && backButton.IsInteractable() && transform.parent.GetSiblingIndex() == transform.parent.parent.childCount - 1)
            backButton.onClick.Invoke();
    }

    void OnDisable()
    {
        transform.localScale = Vector3.one;
        if (group != null)
        {
            group.alpha = 1;
            group.blocksRaycasts = group.interactable = false;
        }

        if (EventSystem.current != null && previousFocus != null && previousFocus.activeInHierarchy)
            EventSystem.current.SetSelectedGameObject(previousFocus);
    }
}
