using UnityEngine;
using System.IO;

public class Entity : MonoBehaviour, IBinaryReadableWriteable
{
    public virtual LocalPos Pos { get; set; }
    public bool Blocking; //TODO private set?

    public virtual void Write(BinaryWriter writer)
    {
        writer.Write(Pos.X);
        writer.Write(Pos.Y);
        writer.Write(Blocking);
    }

    public virtual void Read(BinaryReader reader)
    {
        LocalPos buf = new LocalPos(
            reader.ReadUInt16(),
            reader.ReadUInt16()
        );
        Pos = buf;
        Blocking = reader.ReadBoolean();
    }

    public virtual void Destroy()
    {
        EventManager.OnEntityDestroy(this);
        Destroy(gameObject);
    }
}
