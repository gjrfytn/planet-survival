using UnityEngine;
using System.Collections;
using System.IO;

public class Entity : MonoBehaviour, IBinaryReadableWriteable
{
    [HideInInspector]
    public Vector2 MapCoords;
    public bool Blocking; //TODO private set?

    public virtual void Write(BinaryWriter writer)
    {
        writer.Write(MapCoords.x);
        writer.Write(MapCoords.y);
        writer.Write(Blocking);
    }

    public virtual void Read(BinaryReader reader)
    {
        MapCoords.x = reader.ReadSingle();
        MapCoords.y = reader.ReadSingle();
        Blocking = reader.ReadBoolean();
    }

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
