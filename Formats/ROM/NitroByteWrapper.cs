using System.IO;

namespace NitroSharp.Formats.ROM
{
    public class NitroByteWrapper
    {
        protected byte[] _Data;

        public NitroByteWrapper(uint Offset, uint Size, BinaryReader Binary)
        {
            this.Offset = Offset;
            this.Size = Size;
            GetFileFromROMStream(Binary);
        }

        protected NitroByteWrapper()
        {
        }

        public uint Offset { get; set; }
        public uint Size { get; set; }

        public byte[] Data
        {
            get => _Data;
            set => UpdateBinary(value);
        }

        protected void UpdateBinary(byte[] NewData)
        {
            _Data = NewData;
            Size = (uint) NewData.Length;
        }

        public void GetFileFromROMStream(BinaryReader Binary)
        {
            var OriginalPosition = Binary.BaseStream.Position;
            Binary.BaseStream.Position = Offset;
            Data = Binary.ReadBytes((int) Size);
            Binary.BaseStream.Position = OriginalPosition;
        }
    }
}