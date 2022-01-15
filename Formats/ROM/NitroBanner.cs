using System.IO;

namespace NitroSharp.Formats.ROM {
    public class NitroBanner {
        private byte[] _fileData;

        public NitroBanner(uint offset, uint size) {
            this.offset = offset;
            this.size = size;
        }

        public uint offset { get; set; }
        public uint size { get; private set; }

        public byte[] fileData {
            get => _fileData;
            set => updateFile(value);
        }

        public void getFileFromRomStream(BinaryReader binary) {
            var originalPosition = binary.BaseStream.Position;
            binary.BaseStream.Position = offset;
            fileData = binary.ReadBytes((int) size);
            binary.BaseStream.Position = originalPosition;
        }

        private void updateFile(byte[] newFileData) {
            _fileData = newFileData;
            size = (uint) newFileData.Length;
        }
    }
}