using System.IO;

namespace NitroSharp.Formats.ROM
{
    public class ARMBinary
    {
        private byte[] _Data;

        public ARMBinary(uint EntryAddress, uint Offset, uint Size, uint RAMAddress)
        {
            this.EntryAddress = EntryAddress;
            this.Offset = Offset;
            this.Size = Size;
            this.RAMAddress = RAMAddress;
        }

        public uint EntryAddress { get; set; }
        public uint Offset { get; set; }
        public uint Size { get; set; }
        public uint RAMAddress { get; set; }

        public byte[] Data
        {
            get => _Data;
            set => UpdateBinary(value);
        }

        public void GetFileFromROMStream(BinaryReader Binary)
        {
            var OriginalPosition = Binary.BaseStream.Position;
            Binary.BaseStream.Position = Offset;
            Data = Binary.ReadBytes((int) Size);
            Binary.BaseStream.Position = OriginalPosition;
        }

        private void UpdateBinary(byte[] NewData)
        {
            _Data = NewData;
            Size = (uint) NewData.Length;
        }
    }
}