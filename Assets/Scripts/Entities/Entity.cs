using UnityEngine;
using System.IO;

using LocalPos = U16Vec2;

public class Entity : MonoBehaviour, IBinaryReadableWriteable
{
    public virtual LocalPos Pos { get; set; }
    [SerializeField]
    bool Blocking_;
    public bool Blocking { get { return Blocking_; } }

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
        Blocking_ = buf2;
    }

    protected virtual void Start()
    {
        transform.position = WorldVisualizer.GetTransformPosFromMapPos(Pos);
    }

    public virtual void Destroy()
    {
        EventManager.OnEntityDestroy(this);
        Destroy(gameObject);
    }
}
