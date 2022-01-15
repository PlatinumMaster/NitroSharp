using System.IO;

namespace NitroSharp.Formats.ROM {
    public class NitroByteWrapper {
        protected byte[] _data;

        public NitroByteWrapper(uint offset, uint size, BinaryReader binary) {
            this.offset = offset;
            this.size = size;
            getFileFromRomStream(binary);
        }

        protected NitroByteWrapper() {
        }

        public uint offset { get; set; }
        public uint size { get; set; }

        public byte[] data {
            get => _data;
            set => updateBinary(value);
        }

        protected void updateBinary(byte[] newData) {
            _data = newData;
            size = (uint) newData.Length;
        }

        public void getFileFromRomStream(BinaryReader binary) {
            var originalPosition = binary.BaseStream.Position;
            binary.BaseStream.Position = offset;
            data = binary.ReadBytes((int) size);
            binary.BaseStream.Position = originalPosition;
        }
    }
}