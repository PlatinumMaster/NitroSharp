using System;
using System.IO;
using System.Linq;

namespace NitroSharp.Formats.ROM
{
    public class NitroOverlay : NitroByteWrapper
    {
        public NitroOverlay(uint Offset, uint Size, BinaryReader Binary) : base(Offset, Size, Binary)
        {
            IsCompressed = !GetIsCompressed();
        }

        public NitroOverlay()
        {
            Offset = 0;
            Size = 0;
            IsCompressed = false;
        }

        public bool IsCompressed { get; private set; }

        public uint CompressionFlag => (uint) (IsCompressed ? 0x3000000 : 0x2000000);

        private uint DecompressedSize { get; set; }

        public byte[] Data
        {
            get => _Data;
            set
            {
                UpdateBinary(value);
                IsCompressed = GetIsCompressed();
            }
        }

        public bool GetIsCompressed()
        {
            if (Size < 8)
                return false;
            var BaseSize = BitConverter.ToUInt32(Data.Skip((int) (Size - 0x4)).Take(0x4).ToArray());

            if (BaseSize is 0)
                return false;

            if (Size < Data[Size - 0x5])
                return false;

            var ExpectedCompSize = (uint) ((Data[Size - 0x6] << 16) | (Data[Size - 0x7] << 8) | Data[Size - 0x8]);
            if (ExpectedCompSize != Size)
                return false;
            DecompressedSize = Size + BaseSize;
            return true;
        }

        public uint GetCompressedSize()
        {
            return IsCompressed ? Size : 0;
        }

        public uint GetUncompressedSize()
        {
            return IsCompressed ? DecompressedSize : Size;
        }
    }
}