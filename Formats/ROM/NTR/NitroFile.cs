using System.IO;

namespace NitroSharp.Formats.ROM.NTR {
    public class NitroFile {
        private byte[] _FileData;

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

        public byte[] FileData {
            get => _FileData;
            set => UpdateFile(value);
        }

        public void GetFileFromRomStream(BinaryReader binary) {
            var originalPosition = binary.BaseStream.Position;
            binary.BaseStream.Position = offset;
            FileData = binary.ReadBytes((int) size);
            binary.BaseStream.Position = originalPosition;
        }

        private void UpdateFile(byte[] newFileData) {
            _FileData = newFileData;
            size = (uint) newFileData.Length;
        }
    }
}