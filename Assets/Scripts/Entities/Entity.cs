using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour
{
    [HideInInspector]
    public Vector2 MapCoords;

    protected virtual void OnEnable()
    {
        EventManager.LocalMapLeft += Destroy;
    }

    protected virtual void OnDisable()
    {
        EventManager.LocalMapLeft -= Destroy;
    }

    protected void Destroy()
    {
        Destroy(gameObject);
    }
}
