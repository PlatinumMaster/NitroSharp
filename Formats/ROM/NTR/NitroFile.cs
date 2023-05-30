using System.IO;

namespace NitroSharp.Formats.ROM.NTR {
    public class NitroFile {
        private byte[] _fileData;

        public NitroFile(string name, uint id, uint offset, uint size, NitroDirectory parent) {
            this.name = name;
            this.id = id;
            this.offset = offset;
            this.size = size;
            this.parent = parent;
        }

        public uint id { get; set; }
        public uint offset { get; set; }
        public uint size { get; private set; }
        public string name { get; set; }
        public string path { get; set; }
        public NitroDirectory parent { get; set; }

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