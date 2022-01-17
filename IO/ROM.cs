using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NitroSharp.Formats.ROM;

namespace NitroSharp.IO {
    public class Rom {
        public Arm7 arm7Binary;
        public Arm7I arm7IBinary;
        public List<NitroOverlay> arm7Overlays;
        public NitroOverlayTable arm7OverlayTable;
        public Arm9 arm9Binary;
        public Arm9I arm9IBinary;
        public List<NitroOverlay> arm9Overlays;
        public NitroOverlayTable arm9OverlayTable;
        public NitroBanner banner;
        public NitroByteWrapper blockHashtable;
        public NitroByteWrapper digestNtr;
        public NitroByteWrapper digestTwl;
        public NitroByteWrapper modcryptArea;
        public NitroByteWrapper modcryptArea2;
        public NitroByteWrapper sectorHashtable;

        public Rom(string path) {
            var binary = new BinaryReader(File.Open(path, FileMode.Open), Encoding.ASCII);
            Dictionary<uint, uint> startOffsets = new Dictionary<uint, uint>(),
                endOffsets = new Dictionary<uint, uint>();
            arm9Overlays = new List<NitroOverlay>();
            arm7Overlays = new List<NitroOverlay>();
            root = new NitroDirectory("/", 0xF000, null);
            header = new NitroHeader(binary);

            binary.BaseStream.Position = header.fileAllocationTableOffset;
            for (uint i = 0; i < header.fileAllocationTableSize / 8; ++i) {
                startOffsets.Add(i, binary.ReadUInt32());
                endOffsets.Add(i, binary.ReadUInt32());
            }

            binary.BaseStream.Position = header.fileNameTableOffset;
            NitroDirectory.parseDirectory(root, binary, binary.BaseStream.Position, startOffsets, endOffsets);

            for (uint i = 0; i < header.arm9OverlayTableSize / 0x20; ++i) {
                arm9Overlays.Add(new NitroOverlay(startOffsets[i], endOffsets[i] - startOffsets[i], binary));
                arm9Overlays[^1].getFileFromRomStream(binary);
            }

            for (uint i = 0; i < header.arm7OverlayTableSize / 0x20; ++i) {
                arm7Overlays.Add(new NitroOverlay(startOffsets[i + header.arm7OverlayTableSize],
                    endOffsets[i + header.arm7OverlayTableSize] - startOffsets[i + header.arm7OverlayTableSize],
                    binary));
                arm7Overlays[^1].getFileFromRomStream(binary);
            }

            arm9OverlayTable =
                new NitroOverlayTable(header.arm9OverlayTableOffset, header.arm9OverlayTableSize, binary);
            arm7OverlayTable =
                new NitroOverlayTable(header.arm7OverlayTableOffset, header.arm7OverlayTableSize, binary);
            arm9Binary = new Arm9(header.arm9EntryAddress, header.arm9Offset, header.arm9Size, header.arm9RamAddress);
            arm7Binary = new Arm7(header.arm7EntryAddress, header.arm7Offset, header.arm7Size, header.arm7RamAddress);
            banner = new NitroBanner(header.bannerOffset, header.bannerSize);

            arm9OverlayTable.getFileFromRomStream(binary);
            arm7OverlayTable.getFileFromRomStream(binary);
            banner.getFileFromRomStream(binary);
            arm9Binary.getFileFromRomStream(binary);
            arm7Binary.getFileFromRomStream(binary);

            if (header.headerSize > 0x200 && (header.unitCode & 2) > 0) {
                arm9IBinary = new Arm9I(header.arm9IEntryAddress, header.arm9IOffset, header.arm9ISize,
                    header.arm9IRamAddress);
                arm7IBinary = new Arm7I(header.arm7IEntryAddress, header.arm7IOffset, header.arm7ISize,
                    header.arm7IRamAddress);
                sectorHashtable =
                    new NitroByteWrapper(header.sectorHashtableOffset, header.sectorHashtableSize, binary);
                blockHashtable = new NitroByteWrapper(header.blockHashtableOffset, header.blockHashtableSize, binary);
                digestNtr = new NitroByteWrapper(header.digestNtrOffset, header.digestNtrSize, binary);
                digestTwl = new NitroByteWrapper(header.digestTwlOffset, header.digestTwlSize, binary);
                modcryptArea = new NitroByteWrapper(header.modcryptOffset, header.modcryptSize, binary);
                modcryptArea2 = new NitroByteWrapper(header.modcrypt2Offset, header.modcrypt2Size, binary);

                sectorHashtable.getFileFromRomStream(binary);
                blockHashtable.getFileFromRomStream(binary);
                arm9IBinary.getFileFromRomStream(binary);
                arm7IBinary.getFileFromRomStream(binary);
                digestNtr.getFileFromRomStream(binary);
                digestTwl.getFileFromRomStream(binary);
                modcryptArea.getFileFromRomStream(binary);
                modcryptArea2.getFileFromRomStream(binary);
            }
            binary.Close();
        }

        public NitroDirectory root { get; set; }
        public NitroHeader header { get; set; }

        public void serialize(string path) {
            List<uint> arm9OverlayStartOffsets = new List<uint>(),
                arm7OverlayStartOffsets = new List<uint>(),
                arm9OverlayEndOffsets = new List<uint>(),
                arm7OverlayEndOffsets = new List<uint>();

            // Temporary
            header.headerSize = 0x200;

            var binary = new BinaryWriter(File.Create(path));
            binary.Write(new byte[header.headerSize]);

            // Write the ARM9...
            header.arm9Offset = (uint) binary.BaseStream.Position;
            header.arm9Size = arm9Binary.size;
            binary.Write(arm9Binary.data);

            header.arm9OverlayTableOffset = (uint) binary.BaseStream.Position;
            header.arm9OverlayTableSize = arm9OverlayTable.size;
            binary.Write(arm9OverlayTable.data);

            foreach (var overlay in arm9Overlays) {
                arm9OverlayStartOffsets.Add((uint) binary.BaseStream.Position);
                binary.Write(overlay.data);
                arm9OverlayEndOffsets.Add((uint) binary.BaseStream.Position);
            }

            // Now the ARM7...
            header.arm7Offset = (uint) binary.BaseStream.Position;
            header.arm7Size = arm7Binary.size;
            binary.Write(arm7Binary.data);

            header.arm7OverlayTableOffset = (uint) binary.BaseStream.Position;
            header.arm7OverlayTableSize = arm7OverlayTable.size;
            binary.Write(arm7OverlayTable.data);

            foreach (var overlay in arm7Overlays) {
                arm9OverlayStartOffsets.Add((uint) binary.BaseStream.Position);
                binary.Write(overlay.data);
                arm9OverlayEndOffsets.Add((uint) binary.BaseStream.Position);
            }

            // File Name Table
            header.fileNameTableOffset = (uint) binary.BaseStream.Position;
            NitroDirectory.constructFileNameTable(binary, root);
            header.fileNameTableSize = (uint) (binary.BaseStream.Position - header.fileNameTableOffset);

            // File Allocation Table
            var fileImageOffset = (ulong) (0x4000 +
                                           +arm9Binary.size + arm9OverlayTable.size +
                                           arm9Overlays.Aggregate(0U, (acc, e) => acc + e.size)
                                           + arm7Binary.size + arm7OverlayTable.size +
                                           arm7Overlays.Aggregate(0U, (acc, e) => acc + e.size)
                                           + arm9Overlays.Count * 8 + arm7Overlays.Count * 8);
            header.fileAllocationTableOffset = (uint) binary.BaseStream.Position;
            NitroDirectory.updateBaseOffset((uint) fileImageOffset);
            NitroDirectory.updateOffsets(root);
            NitroDirectory.constructFileAllocationTable(binary, root, arm9OverlayStartOffsets, arm9OverlayEndOffsets,
                arm7OverlayStartOffsets, arm7OverlayEndOffsets);
            header.fileAllocationTableSize = (uint) (binary.BaseStream.Position - header.fileAllocationTableOffset);

            // Banner
            header.bannerOffset = (uint) binary.BaseStream.Position;
            header.bannerSize = banner.size;
            binary.Write(banner.fileData);

            // The File Image
            NitroDirectory.writeFileImageTable(binary, root);
            header.romSize = (uint) binary.BaseStream.Position;

            if (header.headerSize > 0x200) {
                // DSi Binary time...

                // ARM9i...
                header.arm9IOffset = (uint) binary.BaseStream.Position;
                header.arm9ISize = arm9IBinary.size;
                binary.Write(arm9IBinary.data);

                // ARM7i...
                header.arm7IOffset = (uint) binary.BaseStream.Position;
                header.arm7ISize = arm7IBinary.size;
                binary.Write(arm7IBinary.data);
            }

            // Calculate the CRC.
            NitroHeader.calculateCrc(header);
            // Header time!
            binary.BaseStream.Position = 0x0;

            binary.Write(header.title.Take(0xC).ToArray());
            binary.Write(header.gameCode.Take(0x4).ToArray());
            binary.Write(header.makerCode.Take(0x2).ToArray());
            binary.Write(header.unitCode);
            binary.Write(header.encryptionSeed);
            binary.Write(header.deviceCapacity);
            binary.Write(new byte[7]);
            binary.Write(header.twlInternalFlags);
            binary.Write(header.permitsFlags);
            binary.Write(header.romVersion);
            binary.Write(header.internalFlags);
            binary.Write(header.arm9Offset);
            binary.Write(header.arm9EntryAddress);
            binary.Write(header.arm9RamAddress);
            binary.Write(header.arm9Size);
            binary.Write(header.arm7Offset);
            binary.Write(header.arm7EntryAddress);
            binary.Write(header.arm7RamAddress);
            binary.Write(header.arm7Size);
            binary.Write(header.fileNameTableOffset);
            binary.Write(header.fileNameTableSize);
            binary.Write(header.fileAllocationTableOffset);
            binary.Write(header.fileAllocationTableSize);
            binary.Write(header.arm9OverlayTableOffset);
            binary.Write(header.arm9OverlayTableSize);
            binary.Write(header.arm7OverlayTableOffset);
            binary.Write(header.arm7OverlayTableSize);
            binary.Write(header.flagsRead);
            binary.Write(header.flagsInit);
            binary.Write(header.bannerOffset);
            binary.Write(header.secureCrc16);
            binary.Write(header.romTimeout);
            binary.Write(header.arm9Autoload);
            binary.Write(header.arm7Autoload);
            binary.Write(header.secureAreaDisable);
            binary.Write(header.romSize);
            binary.Write(header.headerSize);
            binary.Write(header.reserved2);
            binary.Write(header.logo);
            binary.Write(header.logoCrc16);
            binary.Write(header.headerCrc16);
            binary.Write(header.debugRomOffset);
            binary.Write(header.debugSize);
            binary.Write(header.debugRamAddress);
            binary.Write(header.reserved3);
            binary.Write(header.reserved4);

            if (header.headerSize > 0x200) {
                // Write all the TWL stuff...
                foreach (var setting in header.globalMbkSetting)
                    binary.Write(setting);
                foreach (var setting in header.arm9MbkSetting)
                    binary.Write(setting);
                foreach (var setting in header.arm7MbkSetting)
                    binary.Write(setting);
                foreach (var setting in header.mbk9WramcntSetting)
                    binary.Write(setting);

                binary.Write(header.regionFlags);
                binary.Write(header.accessControl);
                binary.Write(header.scfgExtMask);
                binary.Write(header.appFlags);
                binary.Write(header.arm9IOffset);
                binary.Write(header.arm9IEntryAddress);
                binary.Write(header.arm9IRamAddress);
                binary.Write(header.arm9ISize);
                binary.Write(header.arm7IOffset);
                binary.Write(header.arm7IEntryAddress);
                binary.Write(header.arm7IRamAddress);
                binary.Write(header.arm7ISize);
                binary.Write(header.digestNtrOffset);
                binary.Write(header.digestNtrSize);
                binary.Write(header.digestTwlOffset);
                binary.Write(header.digestTwlSize);
                binary.Write(header.sectorHashtableOffset);
                binary.Write(header.sectorHashtableSize);
                binary.Write(header.blockHashtableOffset);
                binary.Write(header.blockHashtableSize);
                binary.Write(header.digestSectorSize);
                binary.Write(header.digestBlockSectorCount);
                binary.Write(header.bannerSize);
                binary.Write(header.offset0X20C);
                binary.Write(header.totalTwlromSize);
                binary.Write(header.offset0X214);
                binary.Write(header.offset0X218);
                binary.Write(header.offset0X21C);
                binary.Write(header.modcryptOffset);
                binary.Write(header.modcryptSize);
                binary.Write(header.modcrypt2Offset);
                binary.Write(header.modcrypt2Size);
                binary.Write(header.titleId);
                binary.Write(header.publicSaveSize);
                binary.Write(header.privateSaveSize);
                binary.Write(header.reserved5);
                binary.Write(header.ageRatings);
                binary.Write(header.hmacArm9);
                binary.Write(header.hmacArm7);
                binary.Write(header.hmacDigestMaster);
                binary.Write(header.hmacTitleIcon);
                binary.Write(header.hmacArm9I);
                binary.Write(header.hmacArm7I);
                binary.Write(header.reserved6);
                binary.Write(header.hmacArm9NoSecure);
                binary.Write(header.reserved7);
                binary.Write(header.debugArguments);
                binary.Write(header.rsaSignature);
            }

            // If by some miracle we get here, that means we did a good job. Well, unless it doesn't boot. Then who knows.
            binary.Close();
        }
    }
}