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

            for (uint i = 0; i < Header.Arm9OverlayTableSize / 0x20; ++i) {
                ARM9Overlays.Add(new NitroOverlay(startOffsets[i], endOffsets[i] - startOffsets[i], Binary));
                ARM9Overlays[^1].GetFileFromRomStream(Binary);
            }

            for (uint i = 0; i < Header.Arm7OverlayTableSize / 0x20; ++i) {
                ARM7Overlays.Add(new NitroOverlay(startOffsets[i + Header.Arm7OverlayTableSize],
                    endOffsets[i + Header.Arm7OverlayTableSize] - startOffsets[i + Header.Arm7OverlayTableSize],
                    Binary));
                ARM7Overlays[^1].GetFileFromRomStream(Binary);
            }

            ARM9OverlayTable =
                new NitroOverlayTable(Header.Arm9OverlayTableOffset, Header.Arm9OverlayTableSize, Binary);
            ARM7OverlayTable =
                new NitroOverlayTable(Header.Arm7OverlayTableOffset, Header.Arm7OverlayTableSize, Binary);
            ARM9 = new ARM9(Header.Arm9EntryAddress, Header.Arm9Offset, Header.Arm9Size, Header.Arm9RamAddress);
            ARM7 = new ARM7(Header.Arm7EntryAddress, Header.Arm7Offset, Header.Arm7Size, Header.Arm7RamAddress);
            Banner = new NitroBanner(Header.BannerOffset, Header.BannerSize);

            ARM9OverlayTable.GetFileFromRomStream(Binary);
            ARM7OverlayTable.GetFileFromRomStream(Binary);
            Banner.getFileFromRomStream(Binary);
            ARM9.getFileFromRomStream(Binary);
            ARM7.getFileFromRomStream(Binary);

            if (Header.HeaderSize > 0x200 && (Header.UnitCode & 2) > 0) {
                ARM9i = new ARM9i(Header.Arm9IEntryAddress, Header.Arm9IOffset, Header.Arm9ISize,
                    Header.Arm9IRamAddress);
                ARM7i = new ARM7i(Header.Arm7IEntryAddress, Header.Arm7IOffset, Header.Arm7ISize,
                    Header.Arm7IRamAddress);
                SectorHashtable =
                    new NitroByteWrapper(Header.SectorHashtableOffset, Header.SectorHashtableSize, Binary);
                BlockHashtable = new NitroByteWrapper(Header.BlockHashtableOffset, Header.BlockHashtableSize, Binary);
                NTRDigest = new NitroByteWrapper(Header.DigestNtrOffset, Header.DigestNtrSize, Binary);
                TWLDigest = new NitroByteWrapper(Header.DigestTwlOffset, Header.DigestTwlSize, Binary);
                ModcryptArea = new NitroByteWrapper(Header.ModcryptOffset, Header.ModcryptSize, Binary);
                ModcryptArea2 = new NitroByteWrapper(Header.Modcrypt2Offset, Header.Modcrypt2Size, Binary);

                SectorHashtable.GetFileFromRomStream(Binary);
                BlockHashtable.GetFileFromRomStream(Binary);
                ARM9i.getFileFromRomStream(Binary);
                ARM7i.getFileFromRomStream(Binary);
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
            Header.Arm9Offset = (uint) binary.BaseStream.Position;
            Header.Arm9Size = ARM9.size;
            binary.Write(ARM9.data);

            Header.Arm9OverlayTableOffset = (uint) binary.BaseStream.Position;
            Header.Arm9OverlayTableSize = ARM9OverlayTable.Size;
            binary.Write(ARM9OverlayTable.Data);

            foreach (var overlay in ARM9Overlays) {
                arm9OverlayStartOffsets.Add((uint) binary.BaseStream.Position);
                binary.Write(overlay.Data);
                arm9OverlayEndOffsets.Add((uint) binary.BaseStream.Position);
            }

            // Now the ARM7...
            Header.Arm7Offset = (uint) binary.BaseStream.Position;
            Header.Arm7Size = ARM7.size;
            binary.Write(ARM7.data);

            Header.Arm7OverlayTableOffset = (uint) binary.BaseStream.Position;
            Header.Arm7OverlayTableSize = ARM7OverlayTable.Size;
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
                Header.Arm9IOffset = (uint) binary.BaseStream.Position;
                Header.Arm9ISize = ARM9i.size;
                binary.Write(ARM9i.data);

                // ARM7i...
                Header.Arm7IOffset = (uint) binary.BaseStream.Position;
                Header.Arm7ISize = ARM7i.size;
                binary.Write(ARM7i.data);
            }

            // Calculate the CRC.
            NitroHeader.CalculateCrc(Header);
            
            // Header time!
            binary.BaseStream.Position = 0x0;

            binary.Write(Header.Title.Take(0xC).ToArray());
            binary.Write(Header.GameCode.Take(0x4).ToArray());
            binary.Write(Header.MakerCode.Take(0x2).ToArray());
            binary.Write(Header.UnitCode);
            binary.Write(Header.EncryptionSeed);
            binary.Write(Header.DeviceCapacity);
            binary.Write(new byte[7]);
            binary.Write(Header.TwlInternalFlags);
            binary.Write(Header.PermitsFlags);
            binary.Write(Header.RomVersion);
            binary.Write(Header.InternalFlags);
            binary.Write(Header.Arm9Offset);
            binary.Write(Header.Arm9EntryAddress);
            binary.Write(Header.Arm9RamAddress);
            binary.Write(Header.Arm9Size);
            binary.Write(Header.Arm7Offset);
            binary.Write(Header.Arm7EntryAddress);
            binary.Write(Header.Arm7RamAddress);
            binary.Write(Header.Arm7Size);
            binary.Write(Header.FileNameTableOffset);
            binary.Write(Header.FileNameTableSize);
            binary.Write(Header.FileAllocationTableOffset);
            binary.Write(Header.FileAllocationTableSize);
            binary.Write(Header.Arm9OverlayTableOffset);
            binary.Write(Header.Arm9OverlayTableSize);
            binary.Write(Header.Arm7OverlayTableOffset);
            binary.Write(Header.Arm7OverlayTableSize);
            binary.Write(Header.FlagsRead);
            binary.Write(Header.FlagsInit);
            binary.Write(Header.BannerOffset);
            binary.Write(Header.SecureCrc16);
            binary.Write(Header.RomTimeout);
            binary.Write(Header.Arm9Autoload);
            binary.Write(Header.Arm7Autoload);
            binary.Write(Header.SecureAreaDisable);
            binary.Write(Header.RomSize);
            binary.Write(Header.HeaderSize);
            binary.Write(Header.Reserved2);
            binary.Write(Header.Logo);
            binary.Write(Header.LogoCrc16);
            binary.Write(Header.HeaderCrc16);
            binary.Write(Header.DebugRomOffset);
            binary.Write(Header.DebugSize);
            binary.Write(Header.DebugRamAddress);
            binary.Write(Header.Reserved3);
            binary.Write(Header.Reserved4);

            if (Header.HeaderSize > 0x200) {
                // Write all the TWL stuff...
                foreach (var setting in Header.GlobalMbkSetting)
                    binary.Write(setting);
                foreach (var setting in Header.Arm9MbkSetting)
                    binary.Write(setting);
                foreach (var setting in Header.Arm7MbkSetting)
                    binary.Write(setting);
                foreach (var setting in Header.Mbk9WramcntSetting)
                    binary.Write(setting);

                binary.Write(Header.RegionFlags);
                binary.Write(Header.AccessControl);
                binary.Write(Header.ScfgExtMask);
                binary.Write(Header.AppFlags);
                binary.Write(Header.Arm9IOffset);
                binary.Write(Header.Arm9IEntryAddress);
                binary.Write(Header.Arm9IRamAddress);
                binary.Write(Header.Arm9ISize);
                binary.Write(Header.Arm7IOffset);
                binary.Write(Header.Arm7IEntryAddress);
                binary.Write(Header.Arm7IRamAddress);
                binary.Write(Header.Arm7ISize);
                binary.Write(Header.DigestNtrOffset);
                binary.Write(Header.DigestNtrSize);
                binary.Write(Header.DigestTwlOffset);
                binary.Write(Header.DigestTwlSize);
                binary.Write(Header.SectorHashtableOffset);
                binary.Write(Header.SectorHashtableSize);
                binary.Write(Header.BlockHashtableOffset);
                binary.Write(Header.BlockHashtableSize);
                binary.Write(Header.DigestSectorSize);
                binary.Write(Header.DigestBlockSectorCount);
                binary.Write(Header.BannerSize);
                binary.Write(Header.Offset0X20C);
                binary.Write(Header.TotalTwlromSize);
                binary.Write(Header.Offset0X214);
                binary.Write(Header.Offset0X218);
                binary.Write(Header.Offset0X21C);
                binary.Write(Header.ModcryptOffset);
                binary.Write(Header.ModcryptSize);
                binary.Write(Header.Modcrypt2Offset);
                binary.Write(Header.Modcrypt2Size);
                binary.Write(Header.TitleId);
                binary.Write(Header.PublicSaveSize);
                binary.Write(Header.PrivateSaveSize);
                binary.Write(Header.Reserved5);
                binary.Write(Header.AgeRatings);
                binary.Write(Header.HmacArm9);
                binary.Write(Header.HmacArm7);
                binary.Write(Header.HmacDigestMaster);
                binary.Write(Header.HmacTitleIcon);
                binary.Write(Header.HmacArm9I);
                binary.Write(Header.HmacArm7I);
                binary.Write(Header.Reserved6);
                binary.Write(Header.HmacArm9NoSecure);
                binary.Write(Header.Reserved7);
                binary.Write(Header.DebugArguments);
                binary.Write(Header.RsaSignature);
            }

            // If by some miracle we get here, that means we did a good job. Well, unless it doesn't boot. Then who knows.
            binary.Close();
        }
    }
}