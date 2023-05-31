using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NitroSharp.Formats.ROM;
using NitroSharp.Formats.ROM.ARM;
using NitroSharp.Formats.ROM.NTR;

namespace NitroSharp.IO {
    public class SRL {
        public ARM7 ARM7;
        public ARM7i ARM7i;
        public List<NitroOverlay> ARM7Overlays;
        public NitroOverlayTable ARM7OverlayTable;
        public ARM9 ARM9;
        public ARM9i ARM9i;
        public List<NitroOverlay> ARM9Overlays;
        public NitroOverlayTable ARM9OverlayTable;
        public NitroBanner Banner;
        public NitroByteWrapper BlockHashtable, NTRDigest, TWLDigest, ModcryptArea, ModcryptArea2, SectorHashtable;
        

        public NitroDirectory Root { get; set; }
        public NitroHeader Header { get; set; }


        public SRL(string path) {
            BinaryReader Binary = new BinaryReader(File.Open(path, FileMode.Open), Encoding.ASCII);
            Dictionary<uint, uint> startOffsets = new Dictionary<uint, uint>(), endOffsets = new Dictionary<uint, uint>();
            ARM9Overlays = new List<NitroOverlay>();
            ARM7Overlays = new List<NitroOverlay>();
            Root = new NitroDirectory("/", 0xF000, null);
            Header = new NitroHeader(Binary);

            Binary.BaseStream.Position = Header.FileAllocationTableOffset;
            for (uint i = 0; i < Header.FileAllocationTableSize / 8; ++i) {
                startOffsets.Add(i, Binary.ReadUInt32());
                endOffsets.Add(i, Binary.ReadUInt32());
            }

            Binary.BaseStream.Position = Header.FileNameTableOffset;
            NitroDirectory.ParseDirectory(Root, Binary, Binary.BaseStream.Position, startOffsets, endOffsets);

            for (uint i = 0; i < Header.ARM9OverlayTableSize / 0x20; ++i) {
                ARM9Overlays.Add(new NitroOverlay(startOffsets[i], endOffsets[i] - startOffsets[i], Binary));
                ARM9Overlays[^1].GetFileFromRomStream(Binary);
            }

            for (uint i = 0; i < Header.ARM7OverlayTableSize / 0x20; ++i) {
                ARM7Overlays.Add(new NitroOverlay(startOffsets[i + Header.ARM7OverlayTableSize],
                    endOffsets[i + Header.ARM7OverlayTableSize] - startOffsets[i + Header.ARM7OverlayTableSize],
                    Binary));
                ARM7Overlays[^1].GetFileFromRomStream(Binary);
            }

            ARM9OverlayTable =
                new NitroOverlayTable(Header.ARM9OverlayTableOffset, Header.ARM9OverlayTableSize, Binary);
            ARM7OverlayTable =
                new NitroOverlayTable(Header.ARM7OverlayTableOffset, Header.ARM7OverlayTableSize, Binary);
            ARM9 = new ARM9(Header.ARM9EntryAddress, Header.ARM9Offset, Header.ARM9Size, Header.ARM9RAMAddress);
            ARM7 = new ARM7(Header.ARM7EntryAddress, Header.ARM7Offset, Header.ARM7Size, Header.ARM7RAMAddress);
            Banner = new NitroBanner(Header.BannerOffset, Header.BannerSize);

            ARM9OverlayTable.GetFileFromRomStream(Binary);
            ARM7OverlayTable.GetFileFromRomStream(Binary);
            Banner.GetFileFromRomStream(Binary);
            ARM9.GetFileFromRomStream(Binary);
            ARM7.GetFileFromRomStream(Binary);

            if (Header.HeaderSize > 0x200 && (Header.UnitCode & 2) > 0) {
                ARM9i = new ARM9i(Header.ARM9iEntryAddress, Header.ARM9iOffset, Header.ARM9iSize,
                    Header.ARM9iRamAddress);
                ARM7i = new ARM7i(Header.ARM7iEntryAddress, Header.ARM7iOffset, Header.ARM7iSize,
                    Header.ARM7iRamAddress);
                SectorHashtable =
                    new NitroByteWrapper(Header.SectorHashtableOffset, Header.SectorHashtableSize, Binary);
                BlockHashtable = new NitroByteWrapper(Header.BlockHashtableOffset, Header.BlockHashtableSize, Binary);
                NTRDigest = new NitroByteWrapper(Header.DigestNTROffset, Header.DigestNTRSize, Binary);
                TWLDigest = new NitroByteWrapper(Header.DigestTWLOffset, Header.DigestTWLSize, Binary);
                ModcryptArea = new NitroByteWrapper(Header.ModcryptOffset, Header.ModcryptSize, Binary);
                ModcryptArea2 = new NitroByteWrapper(Header.Modcrypt2Offset, Header.Modcrypt2Size, Binary);

                SectorHashtable.GetFileFromRomStream(Binary);
                BlockHashtable.GetFileFromRomStream(Binary);
                ARM9i.GetFileFromRomStream(Binary);
                ARM7i.GetFileFromRomStream(Binary);
                NTRDigest.GetFileFromRomStream(Binary);
                TWLDigest.GetFileFromRomStream(Binary);
                ModcryptArea.GetFileFromRomStream(Binary);
                ModcryptArea2.GetFileFromRomStream(Binary);
            }
            Binary.Close();
        }
        
        public void Serialize(string path) {
            List<uint> arm9OverlayStartOffsets = new List<uint>(),
                arm7OverlayStartOffsets = new List<uint>(),
                arm9OverlayEndOffsets = new List<uint>(),
                arm7OverlayEndOffsets = new List<uint>();

            // Temporary
            Header.HeaderSize = 0x200;

            var binary = new BinaryWriter(File.Create(path));
            binary.Write(new byte[Header.HeaderSize]);

            // Write the ARM9...
            Header.ARM9Offset = (uint) binary.BaseStream.Position;
            Header.ARM9Size = ARM9.size;
            binary.Write(ARM9.data);

            Header.ARM9OverlayTableOffset = (uint) binary.BaseStream.Position;
            Header.ARM9OverlayTableSize = ARM9OverlayTable.Size;
            binary.Write(ARM9OverlayTable.Data);

            foreach (var overlay in ARM9Overlays) {
                arm9OverlayStartOffsets.Add((uint) binary.BaseStream.Position);
                binary.Write(overlay.Data);
                arm9OverlayEndOffsets.Add((uint) binary.BaseStream.Position);
            }

            // Now the ARM7...
            Header.ARM7Offset = (uint) binary.BaseStream.Position;
            Header.ARM7Size = ARM7.size;
            binary.Write(ARM7.data);

            Header.ARM7OverlayTableOffset = (uint) binary.BaseStream.Position;
            Header.ARM7OverlayTableSize = ARM7OverlayTable.Size;
            binary.Write(ARM7OverlayTable.Data);

            foreach (var overlay in ARM7Overlays) {
                arm9OverlayStartOffsets.Add((uint) binary.BaseStream.Position);
                binary.Write(overlay.Data);
                arm9OverlayEndOffsets.Add((uint) binary.BaseStream.Position);
            }

            // File Name Table
            Header.FileNameTableOffset = (uint) binary.BaseStream.Position;
            NitroDirectory.ConstructFileNameTable(binary, Root);
            Header.FileNameTableSize = (uint) (binary.BaseStream.Position - Header.FileNameTableOffset);

            // File Allocation Table
            var fileImageOffset = (ulong) (0x4000 +
                                           + ARM9.size + ARM9OverlayTable.Size +
                                           ARM9Overlays.Aggregate(0U, (acc, e) => acc + e.Size)
                                           + ARM7.size + ARM7OverlayTable.Size +
                                           ARM7Overlays.Aggregate(0U, (acc, e) => acc + e.Size)
                                           + ARM9Overlays.Count * 8 + ARM7Overlays.Count * 8);
            Header.FileAllocationTableOffset = (uint) binary.BaseStream.Position;
            NitroDirectory.UpdateBaseOffset((uint) fileImageOffset);
            NitroDirectory.UpdateOffsets(Root);
            NitroDirectory.ConstructFileAllocationTable(binary, Root, arm9OverlayStartOffsets, arm9OverlayEndOffsets,
                arm7OverlayStartOffsets, arm7OverlayEndOffsets);
            Header.FileAllocationTableSize = (uint) (binary.BaseStream.Position - Header.FileAllocationTableOffset);

            // Banner
            Header.BannerOffset = (uint) binary.BaseStream.Position;
            Header.BannerSize = Banner.size;
            binary.Write(Banner.fileData);

            // The File Image
            NitroDirectory.WriteFileImageTable(binary, Root);
            Header.RomSize = (uint) binary.BaseStream.Position;

            if (Header.HeaderSize > 0x200) {
                // DSi Binary time...

                // ARM9i...
                Header.ARM9iOffset = (uint) binary.BaseStream.Position;
                Header.ARM9iSize = ARM9i.size;
                binary.Write(ARM9i.data);

                // ARM7i...
                Header.ARM7iOffset = (uint) binary.BaseStream.Position;
                Header.ARM7iSize = ARM7i.size;
                binary.Write(ARM7i.data);
            }

            // Calculate the CRC.
            NitroHeader.CalculateCRC(Header);
            
            // Header time!
            binary.BaseStream.Position = 0x0;

            binary.Write(Header.Title.Take(0xC).ToArray());
            binary.Write(Header.GameCode.Take(0x4).ToArray());
            binary.Write(Header.MakerCode.Take(0x2).ToArray());
            binary.Write(Header.UnitCode);
            binary.Write(Header.EncryptionSeed);
            binary.Write(Header.DeviceCapacity);
            binary.Write(new byte[7]);
            binary.Write(Header.TWLInternalFlags);
            binary.Write(Header.PermitsFlags);
            binary.Write(Header.RomVersion);
            binary.Write(Header.InternalFlags);
            binary.Write(Header.ARM9Offset);
            binary.Write(Header.ARM9EntryAddress);
            binary.Write(Header.ARM9RAMAddress);
            binary.Write(Header.ARM9Size);
            binary.Write(Header.ARM7Offset);
            binary.Write(Header.ARM7EntryAddress);
            binary.Write(Header.ARM7RAMAddress);
            binary.Write(Header.ARM7Size);
            binary.Write(Header.FileNameTableOffset);
            binary.Write(Header.FileNameTableSize);
            binary.Write(Header.FileAllocationTableOffset);
            binary.Write(Header.FileAllocationTableSize);
            binary.Write(Header.ARM9OverlayTableOffset);
            binary.Write(Header.ARM9OverlayTableSize);
            binary.Write(Header.ARM7OverlayTableOffset);
            binary.Write(Header.ARM7OverlayTableSize);
            binary.Write(Header.FlagsRead);
            binary.Write(Header.FlagsInit);
            binary.Write(Header.BannerOffset);
            binary.Write(Header.SecureCrc16);
            binary.Write(Header.RomTimeout);
            binary.Write(Header.ARM9Autoload);
            binary.Write(Header.ARM7Autoload);
            binary.Write(Header.SecureAreaDisable);
            binary.Write(Header.RomSize);
            binary.Write(Header.HeaderSize);
            binary.Write(Header.Reserved2);
            binary.Write(Header.Logo);
            binary.Write(Header.LogoCRC);
            binary.Write(Header.HeaderCRC);
            binary.Write(Header.DebugRomOffset);
            binary.Write(Header.DebugSize);
            binary.Write(Header.DebugRamAddress);
            binary.Write(Header.Reserved3);
            binary.Write(Header.Reserved4);

            if (Header.HeaderSize > 0x200) {
                // Write all the TWL stuff...
                foreach (var setting in Header.GlobalMbkSetting)
                    binary.Write(setting);
                foreach (var setting in Header.ARM9MbkSetting)
                    binary.Write(setting);
                foreach (var setting in Header.ARM7MbkSetting)
                    binary.Write(setting);
                foreach (var setting in Header.Mbk9WramcntSetting)
                    binary.Write(setting);

                binary.Write(Header.RegionFlags);
                binary.Write(Header.AccessControl);
                binary.Write(Header.SCFGExtMask);
                binary.Write(Header.AppFlags);
                binary.Write(Header.ARM9iOffset);
                binary.Write(Header.ARM9iEntryAddress);
                binary.Write(Header.ARM9iRamAddress);
                binary.Write(Header.ARM9iSize);
                binary.Write(Header.ARM7iOffset);
                binary.Write(Header.ARM7iEntryAddress);
                binary.Write(Header.ARM7iRamAddress);
                binary.Write(Header.ARM7iSize);
                binary.Write(Header.DigestNTROffset);
                binary.Write(Header.DigestNTRSize);
                binary.Write(Header.DigestTWLOffset);
                binary.Write(Header.DigestTWLSize);
                binary.Write(Header.SectorHashtableOffset);
                binary.Write(Header.SectorHashtableSize);
                binary.Write(Header.BlockHashtableOffset);
                binary.Write(Header.BlockHashtableSize);
                binary.Write(Header.DigestSectorSize);
                binary.Write(Header.DigestBlockSectorCount);
                binary.Write(Header.BannerSize);
                binary.Write(Header.Offset_20C);
                binary.Write(Header.TotalTWLROMSize);
                binary.Write(Header.Offset_214);
                binary.Write(Header.Offset_218);
                binary.Write(Header.Offset_21C);
                binary.Write(Header.ModcryptOffset);
                binary.Write(Header.ModcryptSize);
                binary.Write(Header.Modcrypt2Offset);
                binary.Write(Header.Modcrypt2Size);
                binary.Write(Header.TitleID);
                binary.Write(Header.PublicSaveSize);
                binary.Write(Header.PrivateSaveSize);
                binary.Write(Header.Reserved5);
                binary.Write(Header.AgeRatings);
                binary.Write(Header.HMACARM9);
                binary.Write(Header.HMACARM7);
                binary.Write(Header.HMACDigestMaster);
                binary.Write(Header.HMACTitleIcon);
                binary.Write(Header.HMACARM9i);
                binary.Write(Header.HMACARM7i);
                binary.Write(Header.Reserved6);
                binary.Write(Header.HMACARM9NoSecure);
                binary.Write(Header.Reserved7);
                binary.Write(Header.DebugArguments);
                binary.Write(Header.RsaSignature);
            }

            // If by some miracle we get here, that means we did a good job. Well, unless it doesn't boot. Then who knows.
            binary.Close();
        }
    }
}