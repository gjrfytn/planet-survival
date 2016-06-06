using UnityEngine;

public class SkipTurnButton : MonoBehaviour
{
    void OnEnable()
    {
        EventManager.PlayerMadeTurn += ToggleEnable;
        EventManager.TurnMade += ToggleEnable;
    }

    void OnDisable()
    {
        EventManager.PlayerMadeTurn -= ToggleEnable;
        EventManager.TurnMade -= ToggleEnable;
    }

    public void SkipTurn()
    {
        EventManager.OnPlayerTurn();
    }

    public void ToggleEnable()
    {
        GetComponent<UnityEngine.UI.Button>().interactable = !GetComponent<UnityEngine.UI.Button>().interactable;
    }
}
