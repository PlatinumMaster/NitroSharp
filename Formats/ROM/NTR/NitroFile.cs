using System.IO;

namespace NitroSharp.Formats.ROM
{
    public class NitroFile
    {
        private byte[] _FileData;

        public NitroFile(string Name, uint ID, uint Offset, uint Size, NitroDirectory Parent)
        {
            this.Name = Name;
            this.ID = ID;
            this.Offset = Offset;
            this.Size = Size;
            this.Parent = Parent;
        }

        public uint ID { get; set; }
        public uint Offset { get; set; }
        public uint Size { get; private set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public NitroDirectory Parent { get; set; }
        
        public byte[] FileData
        {
            get => _FileData;
            set => UpdateFile(value);
        }

        public void GetFileFromROMStream(BinaryReader Binary)
        {
            var OriginalPosition = Binary.BaseStream.Position;
            Binary.BaseStream.Position = Offset;
            FileData = Binary.ReadBytes((int) Size);
            Binary.BaseStream.Position = OriginalPosition;
        }

        private void UpdateFile(byte[] NewFileData)
        {
            _FileData = NewFileData;
            Size = (uint) NewFileData.Length;
        }
    }
}