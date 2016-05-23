using UnityEngine;

public class ObjectInteraction : MonoBehaviour
{
    public void OnMouseEnter()
    {
        GetComponent<SpriteRenderer>().material.color = GetComponent<SpriteRenderer>().material.color * 1.2f;
    }

    public void OnMouseExit()
    {
        GetComponent<SpriteRenderer>().material.color = GetComponent<SpriteRenderer>().material.color * 0.833f;
    }
}
