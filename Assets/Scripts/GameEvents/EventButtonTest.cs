using UnityEngine;

public class EventButtonTest : MonoBehaviour
{
    [SerializeField]
    GameEventManager Manager;
    [HideInInspector]
    public byte Index;

    public void Click()//C#6.0 EBD
    {
        Manager.EventPanelButtonPress(Index);
    }
}
