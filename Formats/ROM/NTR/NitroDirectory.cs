using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NitroSharp.Formats.ROM {
    public class NitroDirectory {
        // Used by the offset assigning function ONLY.
        private static uint _baseOffset;

        public NitroDirectory(string name, uint id, NitroDirectory parent) {
            this.name = name;
            this.id = id;
            this.parent = parent;
            subdirectories = new List<NitroDirectory>();
            files = new List<NitroFile>();
        }

        public string name { get; set; }
        public string path { get; set; }
        public uint id { get; set; }
        public NitroDirectory parent { get; set; }
        public List<NitroDirectory> subdirectories { get; set; }
        public List<NitroFile> files { get; set; }

        public static void parseDirectory(NitroDirectory parent, BinaryReader binary, long origin,
            Dictionary<uint, uint> startOffsets, Dictionary<uint, uint> endOffsets) {
            var originalPosition = binary.BaseStream.Position;
            binary.BaseStream.Position = origin + 8 * (parent.id & 0xFFF);

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
                        path = string.Join("/", parent.path, name)
                    };
                    parent.subdirectories.Add(directory);
                    parseDirectory(directory, binary, origin, startOffsets, endOffsets);
                }
                else {
                    parent.files.Add(new NitroFile(name, fileId, startOffsets[fileId],
                        endOffsets[fileId] - startOffsets[fileId], parent) {
                        path = string.Join("/", parent.path, name)
                    });
                    parent.files.Last().getFileFromRomStream(binary);
                }
            }

            binary.BaseStream.Position = originalPosition;
        }

        public static NitroFile searchDirectoryForFile(NitroDirectory parent, string filePath) {
            // Perform a depth-first search, recursively.
            if (parent == null)
                return null;

            var match = parent.files.Find(x => x.path.Equals(filePath));
            if (match == null)
                foreach (var directory in parent.subdirectories) {
                    match = searchDirectoryForFile(directory, filePath);
                    if (match != null)
                        break;
                }

            // If we reach here, we found a match.
            return match;
        }

        public static void writeFileImageTable(BinaryWriter binary, NitroDirectory root) {
            root.files.ForEach(x => {
                binary.BaseStream.Position = x.offset;
                binary.Write(x.fileData);
            });

            root.subdirectories.ForEach(x => writeFileImageTable(binary, x));
        }

        public static void updateBaseOffset(uint @base) {
            _baseOffset = @base;
        }

        public static void updateOffsets(NitroDirectory root) {
            root.files.ForEach(x => {
                x.offset = _baseOffset;
                _baseOffset += x.size;
            });
            root.subdirectories.ForEach(x => updateOffsets(x));
        }

        public static void constructFileAllocationTable(BinaryWriter binary, NitroDirectory root,
            List<uint> arm9OverlayStartOffsets, List<uint> arm9OverlayEndOffsets,
            List<uint> arm7OverlayStartOffsets, List<uint> arm7OverlayEndOffsets) {
            if (root.id != 0xF000)
                throw new Exception("Nice try, yo. This isn't the root directory.");

            for (var i = 0; i < arm9OverlayStartOffsets.Count; ++i) {
                binary.Write(arm9OverlayStartOffsets[i]);
                binary.Write(arm9OverlayEndOffsets[i]);
            }

            for (var i = 0; i < arm7OverlayStartOffsets.Count; ++i) {
                binary.Write(arm7OverlayStartOffsets[i]);
                binary.Write(arm7OverlayEndOffsets[i]);
            }

            constructFileAllocationTable(binary, root);
        }

        private static void constructFileAllocationTable(BinaryWriter binary, NitroDirectory root) {
            foreach (var file in root.files) {
                binary.Write(file.offset);
                binary.Write(file.offset + file.size);
            }

            foreach (var directory in root.subdirectories)
                constructFileAllocationTable(binary, directory);
        }

        public static void constructFileNameTable(BinaryWriter binary, NitroDirectory root) {
            var numberOfDirectories = (uint) (numberOfSubdirectories(root) + root.subdirectories.Count + 1);
            MemoryStream mainTable = new MemoryStream(new byte[numberOfDirectories * 8]),
                subTable = new MemoryStream(new byte[subtableSize(root)]);

            if (root.id != 0xF000)
                throw new Exception("Nice try, yo. This isn't the root directory.");

            mainTable.Write(BitConverter.GetBytes(mainTable.Capacity));
            mainTable.Write(BitConverter.GetBytes((ushort) getFirstFileId(root)));
            mainTable.Write(BitConverter.GetBytes((ushort) numberOfDirectories));

            constructFileNameTable(root, ref mainTable, ref subTable);
            binary.Write(mainTable.ToArray());
            binary.Write(subTable.ToArray());
        }

        private static void constructFileNameTable(NitroDirectory root, ref MemoryStream mainTable,
            ref MemoryStream subTable) {
            foreach (var directory in root.subdirectories) {
                subTable.WriteByte((byte) (128 + directory.name.Length));
                subTable.Write(Encoding.UTF8.GetBytes(directory.name));
                subTable.Write(BitConverter.GetBytes((ushort) directory.id));
            }

            foreach (var file in root.files) {
                subTable.WriteByte((byte) file.name.Length);
                subTable.Write(Encoding.UTF8.GetBytes(file.name));
            }

            subTable.WriteByte(0);
            foreach (var directory in root.subdirectories) {
                mainTable.Write(BitConverter.GetBytes((uint) (mainTable.Capacity + subTable.Position)));
                mainTable.Write(BitConverter.GetBytes((ushort) getFirstFileId(directory)));
                mainTable.Write(BitConverter.GetBytes((ushort) directory.parent.id));
                constructFileNameTable(directory, ref mainTable, ref subTable);
            }
        }

        public static uint getFirstFileId(NitroDirectory root) {
            return root.files.Count > 0 ? root.files[0].id : getFirstFileId(root.subdirectories[0]);
        }

        public static uint numberOfSubdirectories(NitroDirectory root) {
            return root.subdirectories.Aggregate(0U,
                (acc, e) => (uint) (acc + e.subdirectories.Count + numberOfSubdirectories(e)));
        }

        public static uint subtableSize(NitroDirectory root) {
            return 1 + root.files.Aggregate(0U, (acc, e) => (uint) (acc + e.name.Length + 1))
                     + root.subdirectories.Aggregate(0U, (acc, e) => (uint) (acc + e.name.Length + 3))
                     + root.subdirectories.Aggregate(0U, (acc, e) => acc + subtableSize(e));
        }
    }
}