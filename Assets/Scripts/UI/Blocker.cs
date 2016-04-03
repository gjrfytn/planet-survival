using UnityEngine;
using System.Collections;

public class Blocker : MonoBehaviour
{
    void OnEnable()
    {
        EventManager.UIShowed += Show;
        EventManager.UIHided += Hide;
    }

    void OnDisable()
    {
        EventManager.UIShowed -= Show;
        EventManager.UIHided -= Hide;
    }

    void Show()
    {
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<BoxCollider2D>().enabled = true;
    }

    void Hide()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<BoxCollider2D>().enabled = false;
    }

    void Start()
    {
        transform.localScale = new Vector2(Screen.width / GetComponent<SpriteRenderer>().sprite.bounds.size.x, Screen.height / GetComponent<SpriteRenderer>().sprite.bounds.size.y);
    }
}
