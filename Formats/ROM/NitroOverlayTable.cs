using System;
using System.Collections.Generic;
using System.IO;

namespace NitroSharp.Formats.ROM
{
    public class NitroOverlayTable : NitroByteWrapper
    {
        public NitroOverlayTable(uint Offset, uint Size, BinaryReader Binary) : base(Offset, Size, Binary)
        {
            this.Offset = Offset;
            this.Size = Size;
            OverlayTableEntries = ParseOverlayTable(Data);
        }

        public NitroOverlayTable()
        {
            OverlayTableEntries = new List<NitroOverlayTableEntry>();
        }

        public List<NitroOverlayTableEntry> OverlayTableEntries { get; }

        public static List<NitroOverlayTableEntry> ParseOverlayTable(byte[] Bytes)
        {
            var Entries = new List<NitroOverlayTableEntry>();
            var Str = new BinaryReader(new MemoryStream(Bytes));
            for (var i = 0; i < Bytes.Length / 0x20; ++i)
                Entries.Add(new NitroOverlayTableEntry
                {
                    ID = Str.ReadUInt32(),
                    RAMAddress = Str.ReadUInt32(),
                    RAMSize = Str.ReadUInt32(),
                    BSSSize = Str.ReadUInt32(),
                    StaticInitStart = Str.ReadUInt32(),
                    StaticInitEnd = Str.ReadUInt32(),
                    FileID = Str.ReadUInt32(),
                    CompressedSizeAndFlag = Str.ReadUInt32()
                });

            return Entries;
        }

        public byte[] Serialize()
        {
            var Str = new MemoryStream();
            Str.Capacity = OverlayTableEntries.Count * 0x20;
            OverlayTableEntries.ForEach(x =>
            {
                Str.Write(BitConverter.GetBytes(x.ID));
                Str.Write(BitConverter.GetBytes(x.RAMAddress));
                Str.Write(BitConverter.GetBytes(x.RAMSize));
                Str.Write(BitConverter.GetBytes(x.BSSSize));
                Str.Write(BitConverter.GetBytes(x.StaticInitStart));
                Str.Write(BitConverter.GetBytes(x.StaticInitEnd));
                Str.Write(BitConverter.GetBytes(x.FileID));
                Str.Write(BitConverter.GetBytes(x.CompressedSizeAndFlag));
            });
            return Str.GetBuffer();
        }
    }

    public class NitroOverlayTableEntry
    {
        public uint BSSSize;
        public uint CompressedSizeAndFlag;
        public uint FileID;
        public uint ID;
        public uint RAMAddress;
        public uint RAMSize;
        public uint StaticInitEnd;
        public uint StaticInitStart;
    }
}