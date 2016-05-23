using UnityEngine;
using System.IO;

public class Entity : MonoBehaviour, IBinaryReadableWriteable
{
    [HideInInspector]
    public LocalPos Pos;
    public bool Blocking; //TODO private set?

    public virtual void Write(BinaryWriter writer)
    {
        writer.Write(Pos.X);
        writer.Write(Pos.Y);
        writer.Write(Blocking);
    }

    public virtual void Read(BinaryReader reader)
    {
        Pos.X = reader.ReadUInt16();
        Pos.Y = reader.ReadUInt16();
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

    protected void Destroy()//C#6.0 EBD
    {
        Destroy(gameObject);
    }
}
