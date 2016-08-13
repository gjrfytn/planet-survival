using UnityEngine;

public class WhileActionUI_Disabler : MonoBehaviour
{
    UnityEngine.UI.Selectable UI_Object;
    bool WasDisabled;

    void OnEnable()
    {
        EventManager.ActionStarted += Disable;
        EventManager.ActionEnded += Enable;
    }

    void OnDisable()
    {
        EventManager.ActionStarted -= Disable;
        EventManager.ActionEnded -= Enable;
    }

    void Start()
    {
        UI_Object = GetComponent<UnityEngine.UI.Selectable>();
    }

    void Enable() //C# 6.0
    {
        if (!WasDisabled)
            UI_Object.interactable = true;
    }

    void Disable(TimedAction unused) //C# 6.0
    {
        WasDisabled = !UI_Object.interactable;
        UI_Object.interactable = false;
    }
}
