using UnityEngine;

public class SkipTurnButton : MonoBehaviour
{
    void OnEnable()
    {
        EventManager.BluesRendered += Activate;
        EventManager.BluesUnrendered += Deactivate;
    }

    void OnDisable()
    {
        EventManager.BluesRendered -= Activate;
        EventManager.BluesUnrendered -= Deactivate;
    }

    public void SkipTurn()
    {
        EventManager.OnBluesUnrender();
        EventManager.OnLivingBeingEndTurn();
    }

    public void Activate()
    {
        GetComponent<UnityEngine.UI.Button>().interactable = true;
    }

    public void Deactivate()
    {
        GetComponent<UnityEngine.UI.Button>().interactable = false;
    }
}
