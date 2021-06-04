using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NitroSharp.Formats.ROM.TWL
{
    public class NitroByteWrapper
    {

        public uint Offset { get; set; }
        public uint Size { get; set; }
        public byte[] Data
        {
            get => _Data;
            set => UpdateBinary(value);
        }
        private byte[] _Data;
        public NitroByteWrapper(uint Offset, uint Size) {
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
