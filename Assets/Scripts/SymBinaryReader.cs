
namespace System.IO
{
    public sealed class SymBinaryReader : BinaryReader
    {
        public SymBinaryReader(Stream input)
            : base(input)
        { }

        public void Read(out sbyte arg)
        {
            arg = ReadSByte();
        }

        public void Read(out byte arg)
        {
            arg = ReadByte();
        }

        public void Read(out short arg)
        {
            arg = ReadInt16();
        }

        public void Read(out ushort arg)
        {
            arg = ReadUInt16();
        }

        public void Read(out int arg)
        {
            arg = ReadInt32();
        }

        public void Read(out uint arg)
        {
            arg = ReadUInt32();
        }

        public void Read(out long arg)
        {
            arg = ReadInt64();
        }

        public void Read(out ulong arg)
        {
            arg = ReadUInt64();
        }

        public void Read(out float arg)
        {
            arg = ReadSingle();
        }

        public void Read(out double arg)
        {
            arg = ReadDouble();
        }

        public void Read(out bool arg)
        {
            arg = ReadBoolean();
        }

        public void Read(out string arg)
        {
            arg = ReadString();
        }
    }
}
