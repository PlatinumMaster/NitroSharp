using System;
using System.Collections.Generic;
using System.IO;

namespace NitroSharp.Formats.ROM {
    public class NitroOverlayTable : NitroByteWrapper {
        public NitroOverlayTable(uint offset, uint size, BinaryReader binary) : base(offset, size, binary) {
            this.offset = offset;
            this.size = size;
            overlayTableEntries = parseOverlayTable(data);
        }

        public NitroOverlayTable() {
            overlayTableEntries = new List<NitroOverlayTableEntry>();
        }

        public List<NitroOverlayTableEntry> overlayTableEntries { get; }

        public static List<NitroOverlayTableEntry> parseOverlayTable(byte[] bytes) {
            var entries = new List<NitroOverlayTableEntry>();
            var str = new BinaryReader(new MemoryStream(bytes));
            for (var i = 0; i < bytes.Length / 0x20; ++i)
                entries.Add(new NitroOverlayTableEntry {
                    id = str.ReadUInt32(),
                    ramAddress = str.ReadUInt32(),
                    ramSize = str.ReadUInt32(),
                    bssSize = str.ReadUInt32(),
                    staticInitStart = str.ReadUInt32(),
                    staticInitEnd = str.ReadUInt32(),
                    fileId = str.ReadUInt32(),
                    compressedSizeAndFlag = str.ReadUInt32()
                });

            return entries;
        }

        public byte[] serialize() {
            var str = new MemoryStream();
            str.Capacity = overlayTableEntries.Count * 0x20;
            overlayTableEntries.ForEach(x => {
                str.Write(BitConverter.GetBytes(x.id));
                str.Write(BitConverter.GetBytes(x.ramAddress));
                str.Write(BitConverter.GetBytes(x.ramSize));
                str.Write(BitConverter.GetBytes(x.bssSize));
                str.Write(BitConverter.GetBytes(x.staticInitStart));
                str.Write(BitConverter.GetBytes(x.staticInitEnd));
                str.Write(BitConverter.GetBytes(x.fileId));
                str.Write(BitConverter.GetBytes(x.compressedSizeAndFlag));
            });
            return str.GetBuffer();
        }
    }

    public class NitroOverlayTableEntry {
        public uint bssSize;
        public uint compressedSizeAndFlag;
        public uint fileId;
        public uint id;
        public uint ramAddress;
        public uint ramSize;
        public uint staticInitEnd;
        public uint staticInitStart;
    }
}