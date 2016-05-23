using UnityEngine;

public class EventButtonTest : MonoBehaviour
{
    [HideInInspector]
    public byte Index;

    public void Click()//C#6.0 EBD
    {
        GetComponentInParent<GameEventManager>().EventPanelButtonPress(Index);
    }
}
