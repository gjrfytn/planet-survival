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

    public virtual void Read(SymBinaryReader reader)
    {
        LocalPos buf;
        reader.Read(out buf.X);
        reader.Read(out buf.Y);
        Pos = buf;
        bool buf2;
        reader.Read(out buf2);
        Blocking = buf2;
    }

    public virtual void Destroy()
    {
        EventManager.OnEntityDestroy(this);
        Destroy(gameObject);
    }
}
