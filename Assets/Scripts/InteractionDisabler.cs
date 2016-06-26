using UnityEngine;

public class InteractionDisabler : MonoBehaviour
{
    void OnEnable()
    {
		EventManager.ActionStarted += Deactivate2;
		EventManager.ActionEnded += Activate;
		EventManager.BluesRendered += Activate;
		EventManager.BluesUnrendered += Deactivate1;
    }

    void OnDisable()
    {
		EventManager.ActionStarted -= Deactivate2;
		EventManager.ActionEnded -= Activate;
		EventManager.BluesRendered -= Activate;
		EventManager.BluesUnrendered -= Deactivate1;
    }

    void Activate()
    {
        //foreach(Behaviour b in GetComponents<Behaviour>())
        //	b.enabled=false;
        //GetComponent<DuringActionDisabler>().enabled=true;

        //TODO Ждём C# 6.0
        if (GetComponent<SpriteRenderer>() != null)
            GetComponent<SpriteRenderer>().enabled=true;
        if (GetComponent<Collider2D>() != null)
			GetComponent<Collider2D>().enabled = true;
    }

	void Deactivate1()
	{
		//foreach(Behaviour b in GetComponents<Behaviour>())
		//	b.enabled=false;
		//GetComponent<DuringActionDisabler>().enabled=true;

		//TODO Ждём C# 6.0
		if (GetComponent<SpriteRenderer>() != null)
			GetComponent<SpriteRenderer>().enabled =false;
		if (GetComponent<Collider2D>() != null)
			GetComponent<Collider2D>().enabled = false;
	}

	void Deactivate2(TimedAction unused=null)
	{
		//foreach(Behaviour b in GetComponents<Behaviour>())
		//	b.enabled=false;
		//GetComponent<DuringActionDisabler>().enabled=true;

		//TODO Ждём C# 6.0
		if (GetComponent<SpriteRenderer>() != null)
			GetComponent<SpriteRenderer>().enabled =false;
		if (GetComponent<Collider2D>() != null)
			GetComponent<Collider2D>().enabled = false;
	}
}
