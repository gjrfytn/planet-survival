using UnityEngine;
using System.Collections;

public class Blocker : MonoBehaviour 
{
	void OnEnable()
	{
		EventManager.UIShowed+=Switch;
		EventManager.UIHided+=Switch;
	}

	void OnDisable()
	{
		EventManager.UIShowed-=Switch;
		EventManager.UIHided-=Switch;
	}

	void Switch()
	{
		GetComponent<SpriteRenderer>().enabled=!GetComponent<SpriteRenderer>().enabled;
		GetComponent<BoxCollider2D>().enabled=!GetComponent<BoxCollider2D>().enabled;
	}

	void Start () 
	{
		transform.localScale=new Vector2(Screen.width/ GetComponent<SpriteRenderer>().sprite.bounds.size.x,Screen.height/ GetComponent<SpriteRenderer>().sprite.bounds.size.y);
	}
}
