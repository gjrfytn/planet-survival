using UnityEngine;
using System.Collections;

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
