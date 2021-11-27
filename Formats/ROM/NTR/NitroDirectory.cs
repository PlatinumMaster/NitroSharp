using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace NitroSharp.Formats.ROM
{
    public class NitroDirectory
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public uint ID { get; set; }
        public NitroDirectory Parent { get; set; }
        public List<NitroDirectory> Subdirectories { get; set; }
        public List<NitroFile> Files { get; set; }

        // Used by the offset assigning function ONLY.
        static uint BaseOffset;
        public NitroDirectory(string Name, uint ID, NitroDirectory Parent)
        {
            this.Name = Name;
            this.ID = ID;
            this.Parent = Parent;
            Subdirectories = new List<NitroDirectory>();
            Files = new List<NitroFile>();
        }

        public static void ParseDirectory(NitroDirectory Parent, BinaryReader Binary, long Origin, Dictionary<uint, uint> StartOffsets, Dictionary<uint, uint> EndOffsets)
        {
            long OriginalPosition = Binary.BaseStream.Position;
            Binary.BaseStream.Position = Origin + 8 * (Parent.ID & 0xFFF);

            uint SubtableOffset = Binary.ReadUInt32();
            ushort FirstFileID = Binary.ReadUInt16();

            Binary.BaseStream.Position = Origin + SubtableOffset;

            for (uint FileID = FirstFileID; ; ++FileID)
            {
                int EntryType = Binary.ReadByte();
                int NameLength = EntryType & 0x7F;

                if (NameLength == 0)
                    break;

                string Name = new string(Binary.ReadChars(NameLength));
                if ((EntryType & 0x80) > 0x7F)
                {
                    // Directory
                    uint ID = Binary.ReadUInt16();
                    NitroDirectory Directory = new NitroDirectory(Name, ID, Parent)
                    {
                        Path = string.Join("/", Parent.Path, Name)
                    };
                    Parent.Subdirectories.Add(Directory);
                    ParseDirectory(Directory, Binary, Origin, StartOffsets, EndOffsets);
                }
                else
                {
                    Parent.Files.Add(new NitroFile(Name, FileID, StartOffsets[FileID], EndOffsets[FileID] - StartOffsets[FileID], Parent)
                    {
                        Path = string.Join("/", Parent.Path, Name)
                    });
                    Parent.Files.Last().GetFileFromROMStream(Binary);
                }

            }

            Binary.BaseStream.Position = OriginalPosition;
        }

        public static NitroFile SearchDirectoryForFile(NitroDirectory Parent, string FilePath)
        {
            // Perform a depth-first search, recursively.
            if (Parent == null)
                return null;
            
            NitroFile Match = Parent.Files.Find(x => x.Path.Equals(FilePath));
            if (Match == null)
                foreach (NitroDirectory Directory in Parent.Subdirectories)
                {
                    Match = SearchDirectoryForFile(Directory, FilePath);
                    if (Match != null)
                        break;
                }
            // If we reach here, we found a match.
            return Match;
        }

        public static void WriteFileImageTable(BinaryWriter Binary, NitroDirectory Root)
        {
            Root.Files.ForEach(x => {
                Binary.BaseStream.Position = x.Offset;
                Binary.Write(x.FileData);
            });

            Root.Subdirectories.ForEach(x => WriteFileImageTable(Binary, x));
        }
        public static void UpdateBaseOffset(uint Base) {
            BaseOffset = Base;
        }
        public static void UpdateOffsets(NitroDirectory Root) {
            Root.Files.ForEach(x => {
                x.Offset = BaseOffset;
                BaseOffset += x.Size;
            });
            Root.Subdirectories.ForEach(x => UpdateOffsets(x));
        }

        public static void ConstructFileAllocationTable(BinaryWriter Binary, NitroDirectory Root, List<uint> ARM9OverlayStartOffsets, List<uint> ARM9OverlayEndOffsets,
            List<uint> ARM7OverlayStartOffsets, List<uint> ARM7OverlayEndOffsets)
        {
            if (Root.ID != 0xF000)
                throw new Exception("Nice try, yo. This isn't the root directory.");

            for (int i = 0; i < ARM9OverlayStartOffsets.Count; ++i) {
                Binary.Write(ARM9OverlayStartOffsets[i]);
                Binary.Write(ARM9OverlayEndOffsets[i]);
            }

            for (int i = 0; i < ARM7OverlayStartOffsets.Count; ++i)
            {
                Binary.Write(ARM7OverlayStartOffsets[i]);
                Binary.Write(ARM7OverlayEndOffsets[i]);
            }
            ConstructFileAllocationTable(Binary, Root);
        }

        private static void ConstructFileAllocationTable(BinaryWriter Binary, NitroDirectory Root)
        {
            foreach (NitroFile File in Root.Files)
            {
                Binary.Write(File.Offset);
                Binary.Write(File.Offset + File.Size);
            }
            foreach (NitroDirectory Directory in Root.Subdirectories)
                ConstructFileAllocationTable(Binary, Directory);
        }

        public static void ConstructFileNameTable(BinaryWriter Binary, NitroDirectory Root)
        {
            uint NumberOfDirectories = (uint)(NumberOfSubdirectories(Root) + Root.Subdirectories.Count + 1);
            MemoryStream MainTable = new MemoryStream(new byte[NumberOfDirectories * 8]),
                SubTable = new MemoryStream(new byte[SubtableSize(Root)]);
            
            if (Root.ID != 0xF000)
                throw new Exception("Nice try, yo. This isn't the root directory.");

            MainTable.Write(BitConverter.GetBytes(MainTable.Capacity));
            MainTable.Write(BitConverter.GetBytes((ushort)GetFirstFileID(Root)));
            MainTable.Write(BitConverter.GetBytes((ushort)NumberOfDirectories));

            ConstructFileNameTable(Root, ref MainTable, ref SubTable);
            Binary.Write(MainTable.ToArray());
            Binary.Write(SubTable.ToArray());
        }

        private static void ConstructFileNameTable(NitroDirectory Root, ref MemoryStream MainTable, ref MemoryStream SubTable)
        {
            foreach (NitroDirectory Directory in Root.Subdirectories)
            {
                SubTable.WriteByte((byte)(128 + Directory.Name.Length));
                SubTable.Write(Encoding.UTF8.GetBytes(Directory.Name));
                SubTable.Write(BitConverter.GetBytes((ushort)Directory.ID));
            }
            foreach (NitroFile File in Root.Files)
            {
                SubTable.WriteByte((byte)File.Name.Length);
                SubTable.Write(Encoding.UTF8.GetBytes(File.Name));
            }
            SubTable.WriteByte(0);
            foreach (NitroDirectory Directory in Root.Subdirectories)
            {
                MainTable.Write(BitConverter.GetBytes((uint)(MainTable.Capacity + SubTable.Position)));
                MainTable.Write(BitConverter.GetBytes((ushort)GetFirstFileID(Directory)));
                MainTable.Write(BitConverter.GetBytes((ushort)Directory.Parent.ID));
                ConstructFileNameTable(Directory, ref MainTable, ref SubTable);
            }
        }
        public static uint GetFirstFileID(NitroDirectory Root) => Root.Files.Count > 0 ? Root.Files[0].ID : GetFirstFileID(Root.Subdirectories[0]);
        public static uint NumberOfSubdirectories(NitroDirectory Root) => Root.Subdirectories.Aggregate(0U, (Acc, E) => (uint)(Acc + E.Subdirectories.Count + NumberOfSubdirectories(E)));
        public static uint SubtableSize(NitroDirectory Root) => 1 + Root.Files.Aggregate(0U, (Acc, E) => (uint)(Acc + E.Name.Length + 1))
            + Root.Subdirectories.Aggregate(0U, (Acc, E) => (uint)(Acc + E.Name.Length + 3)) 
            + Root.Subdirectories.Aggregate(0U, (Acc, E) => Acc + SubtableSize(E));
    }
}
