using System.IO;

namespace NitroSharp.Formats.ROM {
    public class NitroByteWrapper {
        protected byte[] _Data;

        public NitroByteWrapper(uint offset, uint size, BinaryReader binary) {
            Offset = offset;
            Size = size;
            GetFileFromRomStream(binary);
        }

        protected NitroByteWrapper() {
        }

        public uint Offset { get; set; }
        public uint Size { get; set; }

        public byte[] Data {
            get => _Data;
            set => UpdateBinary(value);
        }

        protected void UpdateBinary(byte[] newData) {
            _Data = newData;
            Size = (uint) newData.Length;
        }

        public void GetFileFromRomStream(BinaryReader binary) {
            var OriginalPosition = binary.BaseStream.Position;
            binary.BaseStream.Position = Offset;
            Data = binary.ReadBytes((int)Size);
            binary.BaseStream.Position = OriginalPosition;
        }
    }
}