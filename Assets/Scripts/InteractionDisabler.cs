using UnityEngine;

public class InteractionDisabler : MonoBehaviour
{
    void OnEnable()
    {
        EventManager.ActionStarted += Toggle1;
        EventManager.ActionEnded += Toggle2;
        EventManager.PlayerMadeTurn += Toggle2;
        EventManager.TurnMade += Toggle2;
    }

    void OnDisable()
    {
        EventManager.ActionStarted -= Toggle1;
        EventManager.ActionEnded -= Toggle2;
        EventManager.PlayerMadeTurn -= Toggle2;
        EventManager.TurnMade -= Toggle2;
    }

    void Toggle1(Action unused = null)
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

    void Toggle2()
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
