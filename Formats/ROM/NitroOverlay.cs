using System;
using System.IO;
using System.Linq;

namespace NitroSharp.Formats.ROM {
    public class NitroOverlay : NitroByteWrapper {
        public NitroOverlay(uint offset, uint size, BinaryReader binary) : base(offset, size, binary) {
            isCompressed = !getIsCompressed();
        }

        public NitroOverlay() {
            offset = 0;
            size = 0;
            isCompressed = false;
        }

        public bool isCompressed { get; private set; }

        public uint compressionFlag => (uint) (isCompressed ? 0x3000000 : 0x2000000);

        private uint decompressedSize { get; set; }

        public byte[] data {
            get => _data;
            set {
                updateBinary(value);
                isCompressed = getIsCompressed();
            }
        }

        public bool getIsCompressed() {
            if (size < 8)
                return false;
            var baseSize = BitConverter.ToUInt32(data.Skip((int) (size - 0x4)).Take(0x4).ToArray());

            if (baseSize is 0)
                return false;

            if (size < data[size - 0x5])
                return false;

            var expectedCompSize = (uint) ((data[size - 0x6] << 16) | (data[size - 0x7] << 8) | data[size - 0x8]);
            if (expectedCompSize != size)
                return false;
            decompressedSize = size + baseSize;
            return true;
        }

        public uint getCompressedSize() {
            return isCompressed ? size : 0;
        }

        public uint getUncompressedSize() {
            return isCompressed ? decompressedSize : size;
        }
    }
}