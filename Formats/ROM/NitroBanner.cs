using System.IO;

namespace NitroSharp.Formats.ROM
{
    public class NitroBanner
    {
        private byte[] _FileData;

        public NitroBanner(uint Offset, uint Size)
        {
            this.Offset = Offset;
            this.Size = Size;
        }

        public uint Offset { get; set; }
        public uint Size { get; private set; }

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