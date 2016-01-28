using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour
{
    [HideInInspector]
    public Vector2 MapCoords;

    protected void OnEnable()
    {
        EventManager.LocalMapLeft += Destroy;
    }

    protected void OnDisable()
    {
        EventManager.LocalMapLeft -= Destroy;
    }

    protected void Destroy()
    {
        Destroy(gameObject);
    }
}
