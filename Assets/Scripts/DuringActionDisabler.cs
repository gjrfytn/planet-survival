using UnityEngine;

public class DuringActionDisabler : MonoBehaviour
{
    void OnEnable()
    {
        EventManager.ActionStarted += Disable;
        EventManager.ActionEnded += Enable;
    }

    void OnDisable()
    {
        EventManager.ActionStarted -= Disable;
        EventManager.ActionEnded -= Enable;
    }

    void Disable(ushort unused)
    {
        //foreach(Behaviour b in GetComponents<Behaviour>())
        //	b.enabled=false;
        //GetComponent<DuringActionDisabler>().enabled=true;

        //TODO Ждём C# 6.0
        if (GetComponent<SpriteRenderer>() != null)
            GetComponent<SpriteRenderer>().enabled = false;
        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().enabled = false;
    }

    void Enable()
    {
        //foreach(Behaviour b in GetComponents<Behaviour>())
        //	b.enabled=true;

        if (GetComponent<SpriteRenderer>() != null)
            GetComponent<SpriteRenderer>().enabled = true;
        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().enabled = true;
    }
}
