using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameUIInitialFocus : MonoBehaviour
{
    void Start()
    {
        if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject != null)
            return;
        foreach (var control in GetComponentsInChildren<Selectable>())
            if (control.IsInteractable())
            {
                EventSystem.current.SetSelectedGameObject(control.gameObject);
                break;
            }
    }
}
