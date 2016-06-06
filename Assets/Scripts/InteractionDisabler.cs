using UnityEngine;

public class InteractionDisabler : MonoBehaviour
{
    void OnEnable()
    {
        EventManager.ActionStarted += Toggle;
        EventManager.ActionEnded += Toggle;
        EventManager.PlayerMadeTurn += Toggle;
        EventManager.TurnMade += Toggle;
    }

    void OnDisable()
    {
        EventManager.ActionStarted -= Toggle;
        EventManager.ActionEnded -= Toggle;
        EventManager.PlayerMadeTurn -= Toggle;
        EventManager.TurnMade -= Toggle;
    }

    void Toggle(Action unused = null)
    {
        //foreach(Behaviour b in GetComponents<Behaviour>())
        //	b.enabled=false;
        //GetComponent<DuringActionDisabler>().enabled=true;

        //TODO Ждём C# 6.0
        if (GetComponent<SpriteRenderer>() != null)
            GetComponent<SpriteRenderer>().enabled = !GetComponent<SpriteRenderer>().enabled;
        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().enabled = !GetComponent<Collider2D>().enabled;
    }
}
