using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NitroSharp.Formats.ROM
{
    public class NitroOverlay
    {
        public uint Offset;
        public uint Size;
        public byte[] Data
        {
            get => _Data;
            set => UpdateBinary(value);
        }
        private byte[] _Data;

        public NitroOverlay(uint Offset, uint Size)
        {
            this.Offset = Offset;
            this.Size = Size;
        }

        private void UpdateBinary(byte[] NewData)
        {
            _Data = NewData;
            Size = (uint)NewData.Length;
        }

        public void GetFileFromROMStream(BinaryReader Binary)
        {
            long OriginalPosition = Binary.BaseStream.Position;
            Binary.BaseStream.Position = Offset;
            Data = Binary.ReadBytes((int)Size);
            Binary.BaseStream.Position = OriginalPosition;
        }
    }
}
