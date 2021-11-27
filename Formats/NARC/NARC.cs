using System;
using System.Collections.Generic;
using System.IO;

namespace NitroSharp.Formats
{
    public class NARC
    {
        public NARC(byte[] Data)
        {
            var Binary = new BinaryReader(new MemoryStream(Data));
            if (Binary.ReadUInt32() != 0x4352414E)
                throw new Exception("Not a NARC!");
            if (Binary.ReadUInt16() != 0xFFFE)
                throw new Exception("Little Endian NARCs only!");
            Binary.BaseStream.Position = 0x10;
            FAT = new FATB(Binary);
        }

        public FATB FAT { get; set; }

        public byte[] Serialize()
        {
            var Buffer = new MemoryStream();
            var Out = new BinaryWriter(Buffer);

            // NARC Header
            Out.Write(0x4352414E);
            Out.Write((ushort) 0xFFFE);
            Out.Write((ushort) 0x10);
            Out.Write(0x0);
            Out.Write((ushort) 0x10);
            Out.Write((ushort) 0x3);

            // FATB Construction
            Out.Write((uint) 0x46415442);
            Out.Write((uint) (FAT.Entries.Count * 0x8 + 0xC));
            Out.Write((ushort) FAT.Entries.Count);
            Out.Write((ushort) 0x0);
            uint BaseOffset = 0;
            FAT.Entries.ForEach(Entry =>
            {
                Out.Write(BaseOffset);
                Out.Write((uint) (BaseOffset + Entry.Buffer.Length));
                BaseOffset += (uint) Entry.Buffer.Length;
            });

            // FNTB Construction (lmao)
            // We'll do this one day
            Out.Write(0x464E5442);
            Out.Write(0x10);
            Out.Write(0x4);
            Out.Write(0x10000);

            // FIMG Construction
            Out.Write(0x46494D47);
            var FIMG_ChunkSizeOffset = Out.BaseStream.Position;
            Out.Write(0x0);
            FAT.Entries.ForEach(Entry => Out.Write(Entry.Buffer));
            var ChunkSize = (uint) (Out.BaseStream.Position - FIMG_ChunkSizeOffset + 0x4);
            var FileSize = (uint) Out.BaseStream.Position;
            Out.BaseStream.Position = FIMG_ChunkSizeOffset;
            Out.Write(ChunkSize);
            Out.Seek(0x8, SeekOrigin.Begin);
            Out.Write(FileSize);
            Out.Close();

            return Buffer.ToArray();
        }
    }

    public class FATB
    {
        public FATB(BinaryReader Binary)
        {
            Entries = new List<FATB_Entry>();
            Binary.BaseStream.Position = 0x18;
            var FileCount = Binary.ReadUInt16();
            Binary.BaseStream.Position += 0x2;
            for (var i = 0; i < FileCount; ++i)
                Entries.Add(new FATB_Entry(Binary));
            Binary.BaseStream.Position += 0x18;
            var FIMG_Base = (uint) Binary.BaseStream.Position;
            Entries.ForEach(Entry => Entry.ReadBuffer(Binary, FIMG_Base));
        }

        public List<FATB_Entry> Entries { get; }
    }

    public class FATB_Entry
    {
        private readonly long Dest_Begin;
        private readonly long Dest_End;

        public FATB_Entry(BinaryReader Binary)
        {
            Dest_Begin = Binary.ReadUInt32();
            Dest_End = Binary.ReadUInt32();
        }

        public FATB_Entry(byte[] Buffer)
        {
            this.Buffer = Buffer;
        }

        public byte[] Buffer { get; set; }

        public void ReadBuffer(BinaryReader Binary, uint FIMG_Base)
        {
            var Origin = Binary.BaseStream.Position;
            Binary.BaseStream.Position = Dest_Begin + FIMG_Base;
            Buffer = Binary.ReadBytes((int) (Dest_End - Dest_Begin));
            Binary.BaseStream.Position = Origin;
        }
    }
}