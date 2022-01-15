﻿using System;
using System.Collections.Generic;
using System.IO;

namespace NitroSharp.Formats {
    public class Narc {
        public Narc(byte[] data) {
            var binary = new BinaryReader(new MemoryStream(data));
            if (binary.ReadUInt32() != 0x4352414E)
                throw new Exception("Not a NARC!");
            if (binary.ReadUInt16() != 0xFFFE)
                throw new Exception("Little Endian NARCs only!");
            binary.BaseStream.Position = 0x10;
            fat = new Fatb(binary);
        }

        public Fatb fat { get; set; }

        public byte[] serialize() {
            var buffer = new MemoryStream();
            var @out = new BinaryWriter(buffer);

            // NARC Header
            @out.Write(0x4352414E);
            @out.Write((ushort) 0xFFFE);
            @out.Write((ushort) 0x10);
            @out.Write(0x0);
            @out.Write((ushort) 0x10);
            @out.Write((ushort) 0x3);

            // FATB Construction
            @out.Write((uint) 0x46415442);
            @out.Write((uint) (fat.entries.Count * 0x8 + 0xC));
            @out.Write((ushort) fat.entries.Count);
            @out.Write((ushort) 0x0);
            uint baseOffset = 0;
            fat.entries.ForEach(entry => {
                @out.Write(baseOffset);
                @out.Write((uint) (baseOffset + entry.buffer.Length));
                baseOffset += (uint) entry.buffer.Length;
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
            fat.entries.ForEach(entry => @out.Write(entry.buffer));
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

    public class Fatb {
        public Fatb(BinaryReader binary) {
            entries = new List<FatbEntry>();
            binary.BaseStream.Position = 0x18;
            var fileCount = binary.ReadUInt16();
            binary.BaseStream.Position += 0x2;
            for (var i = 0; i < fileCount; ++i)
                entries.Add(new FatbEntry(binary));
            binary.BaseStream.Position += 0x18;
            var fimgBase = (uint) binary.BaseStream.Position;
            entries.ForEach(entry => entry.readBuffer(binary, fimgBase));
        }

        public List<FatbEntry> entries { get; }
    }

    public class FatbEntry {
        private readonly long _destBegin;
        private readonly long _destEnd;

        public FatbEntry(BinaryReader binary) {
            _destBegin = binary.ReadUInt32();
            _destEnd = binary.ReadUInt32();
        }

        public FatbEntry(byte[] buffer) {
            this.buffer = buffer;
        }

        public byte[] buffer { get; set; }

        public void readBuffer(BinaryReader binary, uint fimgBase) {
            var origin = binary.BaseStream.Position;
            binary.BaseStream.Position = _destBegin + fimgBase;
            buffer = binary.ReadBytes((int) (_destEnd - _destBegin));
            binary.BaseStream.Position = origin;
        }
    }
}