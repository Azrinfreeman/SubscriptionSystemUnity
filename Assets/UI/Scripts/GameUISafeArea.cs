using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class GameUISafeArea : MonoBehaviour
{
    Rect lastArea;
    Vector2 lastSize;
    void OnEnable()
    {
        Apply();
    }

    void Update()
    {
        if (lastArea != Screen.safeArea || lastSize != new Vector2(Screen.width, Screen.height))
            Apply();
    }

    void Apply()
    {
        if (Screen.width <= 0 || Screen.height <= 0)
            return;
        lastArea = Screen.safeArea;
        lastSize = new Vector2(Screen.width, Screen.height);
        var rect = (RectTransform)transform;
        rect.anchorMin = lastArea.min / lastSize;
        rect.anchorMax = lastArea.max / lastSize;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}
