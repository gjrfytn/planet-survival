using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour
{
    [HideInInspector]
    public Vector2 MapCoords;

    protected EventManager EventManager;

    protected void OnEnable()
    {
        EventManager.LocalMapLeft += Destroy;
    }

    protected void OnDisable()
    {
        EventManager.LocalMapLeft -= Destroy;
    }

    protected void Awake()
    {
        EventManager = GameObject.Find("EventManager").GetComponent<EventManager>();
    }

    protected void Destroy()
    {
        Destroy(gameObject);
    }
}
