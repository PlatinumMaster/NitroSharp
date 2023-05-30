using System;
using System.Collections.Generic;
using System.IO;

namespace NitroSharp.Formats.NARC {
    public class NARC {
        public NARC(byte[] data) {
            var binary = new BinaryReader(new MemoryStream(data));
            if (binary.ReadUInt32() != 0x4352414E)
                throw new Exception("Not a NARC!");
            if (binary.ReadUInt16() != 0xFFFE)
                throw new Exception("Little Endian NARCs only!");
            binary.BaseStream.Position = 0x10;
            FAT = new FATB(binary);
        }

        public FATB FAT { get; set; }

        public byte[] Serialize() {
            var buffer = new MemoryStream();
            var @out = new BinaryWriter(buffer);

            // NARC Header
            @out.Write(0x4352414E);
            @out.Write((ushort) 0xFFFE);
            @out.Write((ushort) 0x1);
            @out.Write(0x0);
            @out.Write((ushort) 0x10);
            @out.Write((ushort) 0x3);

            // FATB Construction
            @out.Write((uint) 0x46415442);
            @out.Write((uint) (FAT.Entries.Count * 0x8 + 0xC));
            @out.Write((ushort) FAT.Entries.Count);
            @out.Write((ushort) 0x0);
            uint baseOffset = 0;
            FAT.Entries.ForEach(entry => {
                @out.Write(baseOffset);
                @out.Write((uint) (baseOffset + entry.Buffer.Length));
                baseOffset += (uint) entry.Buffer.Length;
            });

            // FNTB Construction (lmao)
            // We'll do this one day
            @out.Write(0x464E5442);
            @out.Write(0x10);
            @out.Write(0x4);
            @out.Write(0x10000);

            // FIMG Construction
            @out.Write(0x46494D47);
            var fimgChunkSizeOffset = @out.BaseStream.Position;
            @out.Write(0x0);
            FAT.Entries.ForEach(entry => @out.Write(entry.Buffer));
            var chunkSize = (uint) (@out.BaseStream.Position - fimgChunkSizeOffset + 0x4);
            var fileSize = (uint) @out.BaseStream.Position;
            @out.BaseStream.Position = fimgChunkSizeOffset;
            @out.Write(chunkSize);
            @out.Seek(0x8, SeekOrigin.Begin);
            @out.Write(fileSize);
            @out.Close();

            return buffer.ToArray();
        }
    }

    public class FATB {
        public FATB(BinaryReader binary) {
            Entries = new List<FATBEntry>();
            binary.BaseStream.Position = 0x18;
            var fileCount = binary.ReadUInt16();
            binary.BaseStream.Position += 0x2;
            for (var i = 0; i < fileCount; ++i)
                Entries.Add(new FATBEntry(binary));
            binary.BaseStream.Position += 0x18;
            var fimgBase = (uint) binary.BaseStream.Position;
            Entries.ForEach(entry => entry.readBuffer(binary, fimgBase));
        }

        public List<FATBEntry> Entries { get; }
    }

    public class FATBEntry {
        private readonly long _destBegin;
        private readonly long _destEnd;

        public FATBEntry(BinaryReader binary) {
            _destBegin = binary.ReadUInt32();
            _destEnd = binary.ReadUInt32();
        }

        public FATBEntry(byte[] buffer) {
            this.Buffer = buffer;
        }

        public byte[] Buffer { get; set; }

        public void readBuffer(BinaryReader binary, uint fimgBase) {
            var origin = binary.BaseStream.Position;
            binary.BaseStream.Position = _destBegin + fimgBase;
            Buffer = binary.ReadBytes((int) (_destEnd - _destBegin));
            binary.BaseStream.Position = origin;
        }
    }
}