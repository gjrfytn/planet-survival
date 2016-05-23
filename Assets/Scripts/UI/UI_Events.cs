using UnityEngine;

public class UI_Events : MonoBehaviour
{
    void OnEnable()
    {
        EventManager.OnUIShow();
    }

    void OnDisable()
    {
        EventManager.OnUIHide();
    }
}
