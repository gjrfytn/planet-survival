using System.IO;

interface IBinaryReadableWriteable
{
    void Write(BinaryWriter writer);
    void Read(SymBinaryReader writer);
}
