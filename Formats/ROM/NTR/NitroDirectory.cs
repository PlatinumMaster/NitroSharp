using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NitroSharp.Formats.ROM.NTR {
    public class NitroDirectory {
        // Used by the offset assigning function ONLY.
        private static uint _baseOffset;

        public NitroDirectory(string name, uint id, NitroDirectory parent) {
            Name = name;
            ID = id;
            Parent = parent;
            Subdirectories = new List<NitroDirectory>();
            Files = new List<NitroFile>();
        }

        public string Name { get; set; }
        public string Path { get; set; }
        public uint ID { get; set; }
        public NitroDirectory Parent { get; set; }
        public List<NitroDirectory> Subdirectories { get; set; }
        public List<NitroFile> Files { get; set; }

        public static void ParseDirectory(NitroDirectory parent, BinaryReader binary, long origin,
            Dictionary<uint, uint> startOffsets, Dictionary<uint, uint> endOffsets) {
            var originalPosition = binary.BaseStream.Position;
            binary.BaseStream.Position = origin + 8 * (parent.ID & 0xFFF);

            var subtableOffset = binary.ReadUInt32();
            var firstFileId = binary.ReadUInt16();

            binary.BaseStream.Position = origin + subtableOffset;

            for (uint fileId = firstFileId;; ++fileId) {
                int entryType = binary.ReadByte();
                var nameLength = entryType & 0x7F;

                if (nameLength == 0)
                    break;

                var name = new string(binary.ReadChars(nameLength));
                if ((entryType & 0x80) > 0x7F) {
                    // Directory
                    uint id = binary.ReadUInt16();
                    var directory = new NitroDirectory(name, id, parent) {
                        Path = string.Join("/", parent.Path, name)
                    };
                    parent.Subdirectories.Add(directory);
                    ParseDirectory(directory, binary, origin, startOffsets, endOffsets);
                } else {
                    parent.Files.Add(new NitroFile(name, fileId, startOffsets[fileId],
                        endOffsets[fileId] - startOffsets[fileId], parent) {
                        path = string.Join("/", parent.Path, name)
                    });
                    // parent.Files.Last().GetFileFromRomStream(binary);
                }
            }

            binary.BaseStream.Position = originalPosition;
        }

        public static NitroFile SearchDirectoryForFile(NitroDirectory parent, string filePath) {
            // Perform a depth-first search, recursively.
            if (parent == null)
                return null;

            var match = parent.Files.Find(x => x.path.Equals(filePath));
            if (match == null)
                foreach (var directory in parent.Subdirectories) {
                    match = SearchDirectoryForFile(directory, filePath);
                    if (match != null)
                        break;
                }

            // If we reach here, we found a match.
            return match;
        }

        public static void WriteFileImageTable(BinaryWriter binary, NitroDirectory root) {
            root.Files.ForEach(x => {
                binary.BaseStream.Position = x.offset;
                binary.Write(x.FileData);
            });

            root.Subdirectories.ForEach(x => WriteFileImageTable(binary, x));
        }

        public static void UpdateBaseOffset(uint @base) {
            _baseOffset = @base;
        }

        public static void UpdateOffsets(NitroDirectory root) {
            root.Files.ForEach(x => {
                x.offset = _baseOffset;
                _baseOffset += x.size;
            });
            root.Subdirectories.ForEach(x => UpdateOffsets(x));
        }

        public static void ConstructFileAllocationTable(BinaryWriter binary, NitroDirectory root,
            List<uint> arm9OverlayStartOffsets, List<uint> arm9OverlayEndOffsets,
            List<uint> arm7OverlayStartOffsets, List<uint> arm7OverlayEndOffsets) {
            if (root.ID != 0xF000)
                throw new Exception("Nice try, yo. This isn't the root directory.");

            for (var i = 0; i < arm9OverlayStartOffsets.Count; ++i) {
                binary.Write(arm9OverlayStartOffsets[i]);
                binary.Write(arm9OverlayEndOffsets[i]);
            }

            for (var i = 0; i < arm7OverlayStartOffsets.Count; ++i) {
                binary.Write(arm7OverlayStartOffsets[i]);
                binary.Write(arm7OverlayEndOffsets[i]);
            }

            ConstructFileAllocationTable(binary, root);
        }

        private static void ConstructFileAllocationTable(BinaryWriter binary, NitroDirectory root) {
            foreach (var file in root.Files) {
                binary.Write(file.offset);
                binary.Write(file.offset + file.size);
            }

            foreach (var directory in root.Subdirectories)
                ConstructFileAllocationTable(binary, directory);
        }

        public static void ConstructFileNameTable(BinaryWriter binary, NitroDirectory root) {
            var numberOfDirectories = (uint) (NumberOfSubdirectories(root) + root.Subdirectories.Count + 1);
            MemoryStream mainTable = new MemoryStream(new byte[numberOfDirectories * 8]),
                subTable = new MemoryStream(new byte[SubtableSize(root)]);

            if (root.ID != 0xF000)
                throw new Exception("Nice try, yo. This isn't the root directory.");

            mainTable.Write(BitConverter.GetBytes(mainTable.Capacity));
            mainTable.Write(BitConverter.GetBytes((ushort) GetFirstFileId(root)));
            mainTable.Write(BitConverter.GetBytes((ushort) numberOfDirectories));

            ConstructFileNameTable(root, ref mainTable, ref subTable);
            binary.Write(mainTable.ToArray());
            binary.Write(subTable.ToArray());
        }

        private static void ConstructFileNameTable(NitroDirectory root, ref MemoryStream mainTable,
            ref MemoryStream subTable) {
            foreach (var directory in root.Subdirectories) {
                subTable.WriteByte((byte) (128 + directory.Name.Length));
                subTable.Write(Encoding.UTF8.GetBytes(directory.Name));
                subTable.Write(BitConverter.GetBytes((ushort) directory.ID));
            }

            foreach (var file in root.Files) {
                subTable.WriteByte((byte) file.name.Length);
                subTable.Write(Encoding.UTF8.GetBytes(file.name));
            }

            subTable.WriteByte(0);
            foreach (var directory in root.Subdirectories) {
                mainTable.Write(BitConverter.GetBytes((uint) (mainTable.Capacity + subTable.Position)));
                mainTable.Write(BitConverter.GetBytes((ushort) GetFirstFileId(directory)));
                mainTable.Write(BitConverter.GetBytes((ushort) directory.Parent.ID));
                ConstructFileNameTable(directory, ref mainTable, ref subTable);
            }
        }

        public static uint GetFirstFileId(NitroDirectory root) {
            return root.Files.Count > 0 ? root.Files[0].id : GetFirstFileId(root.Subdirectories[0]);
        }

        public static uint NumberOfSubdirectories(NitroDirectory root) {
            return root.Subdirectories.Aggregate(0U,
                (acc, e) => (uint) (acc + e.Subdirectories.Count + NumberOfSubdirectories(e)));
        }

        public static uint SubtableSize(NitroDirectory root) {
            return 1 + root.Files.Aggregate(0U, (acc, e) => (uint) (acc + e.name.Length + 1))
                     + root.Subdirectories.Aggregate(0U, (acc, e) => (uint) (acc + e.Name.Length + 3))
                     + root.Subdirectories.Aggregate(0U, (acc, e) => acc + SubtableSize(e));
        }
    }
}