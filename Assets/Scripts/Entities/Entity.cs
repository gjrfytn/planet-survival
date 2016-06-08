using UnityEngine;
using System.IO;

public class Entity : MonoBehaviour, IBinaryReadableWriteable
{
    public virtual LocalPos Pos { get; set; }
    [SerializeField]
    bool Blocking_;
    public bool Blocking { get { return Blocking_; } private set { Blocking_ = value; } }

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
