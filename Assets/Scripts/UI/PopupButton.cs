
public class PopupButton : UnityEngine.MonoBehaviour
{
    public PopupButtonsController Controller;

    public void Click()//C# 6.0
    {
        Controller.ButtonClick(this);
    }
}
