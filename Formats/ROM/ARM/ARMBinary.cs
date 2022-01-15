using System.IO;

namespace NitroSharp.Formats.ROM {
    public class ArmBinary {
        private byte[] _data;

        public ArmBinary(uint entryAddress, uint offset, uint size, uint ramAddress) {
            this.entryAddress = entryAddress;
            this.offset = offset;
            this.size = size;
            this.ramAddress = ramAddress;
        }

        public uint entryAddress { get; set; }
        public uint offset { get; set; }
        public uint size { get; set; }
        public uint ramAddress { get; set; }

        public byte[] data {
            get => _data;
            set => updateBinary(value);
        }

        public void getFileFromRomStream(BinaryReader binary) {
            var originalPosition = binary.BaseStream.Position;
            binary.BaseStream.Position = offset;
            data = binary.ReadBytes((int) size);
            binary.BaseStream.Position = originalPosition;
        }

        private void updateBinary(byte[] newData) {
            _data = newData;
            size = (uint) newData.Length;
        }
    }
}