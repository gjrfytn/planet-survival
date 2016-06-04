using UnityEngine;

public class PopupButton : MonoBehaviour
{
    public Action Action;

    public void Click()//C# 6.0
    {
        EventManager.OnPopupButtonClick(this);
    }
}
