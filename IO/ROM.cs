using NitroSharp.Formats.ROM;
using NitroSharp.Formats.ROM.TWL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace NitroSharp.IO
{
    public class ROM
    {
        public NitroDirectory Root { get; set; }
        public NitroHeader Header { get; set; }
        public List<NitroOverlay> ARM9Overlays;
        public List<NitroOverlay> ARM7Overlays;
        public NitroOverlayTable ARM9OverlayTable;
        public NitroOverlayTable ARM7OverlayTable;
        public ARM9 ARM9Binary;
        public ARM7 ARM7Binary;
        public ARM9i ARM9iBinary;
        public ARM7i ARM7iBinary;
        public NitroBanner Banner;
        public NitroByteWrapper SectorHashtable;
        public NitroByteWrapper BlockHashtable;
        public NitroByteWrapper DigestNTR;
        public NitroByteWrapper DigestTWL;
        public NitroByteWrapper ModcryptArea;
        public NitroByteWrapper ModcryptArea2;
        public ROM(string Path)
        {
            BinaryReader Binary = new BinaryReader(File.Open(Path, FileMode.Open), Encoding.ASCII);
            Dictionary<uint, uint> StartOffsets = new Dictionary<uint, uint>(), EndOffsets = new Dictionary<uint, uint>();
            ARM9Overlays = new List<NitroOverlay>();
            ARM7Overlays = new List<NitroOverlay>();
            Root = new NitroDirectory("/", 0xF000, null);
            Header = new NitroHeader(Binary);

            Binary.BaseStream.Position = Header.FileAllocationTableOffset;
            for (uint i = 0; i < Header.FileAllocationTableSize / 8; ++i)
            {
                StartOffsets.Add(i, Binary.ReadUInt32());
                EndOffsets.Add(i, Binary.ReadUInt32());
            }

            Binary.BaseStream.Position = Header.FileNameTableOffset;
            NitroDirectory.ParseDirectory(Root, Binary, Binary.BaseStream.Position, StartOffsets, EndOffsets);

            for (uint i = 0; i < Header.ARM9OverlayTableSize / 0x20; ++i)
            {
                ARM9Overlays.Add(new NitroOverlay(StartOffsets[i], EndOffsets[i] - StartOffsets[i]));
                ARM9Overlays[^1].GetFileFromROMStream(Binary);
            }

            for (uint i = 0; i < Header.ARM7OverlayTableSize / 0x20; ++i)
            {
                ARM7Overlays.Add(new NitroOverlay(StartOffsets[i + Header.ARM7OverlayTableSize], EndOffsets[i + Header.ARM7OverlayTableSize] - StartOffsets[i + Header.ARM7OverlayTableSize]));
                ARM7Overlays[^1].GetFileFromROMStream(Binary);
            }
            
            ARM9OverlayTable = new NitroOverlayTable(Header.ARM9OverlayTableOffset, Header.ARM9OverlayTableSize);
            ARM7OverlayTable = new NitroOverlayTable(Header.ARM7OverlayTableOffset, Header.ARM7OverlayTableSize);
            ARM9Binary = new ARM9(Header.ARM9EntryAddress, Header.ARM9Offset, Header.ARM9Size, Header.ARM9RAMAddress);
            ARM7Binary = new ARM7(Header.ARM7EntryAddress, Header.ARM7Offset, Header.ARM7Size, Header.ARM7RAMAddress);
            Banner = new NitroBanner(Header.BannerOffset, Header.BannerSize);

            ARM9OverlayTable.GetFileFromROMStream(Binary);
            ARM7OverlayTable.GetFileFromROMStream(Binary);
            Banner.GetFileFromROMStream(Binary);
            ARM9Binary.GetFileFromROMStream(Binary);
            ARM7Binary.GetFileFromROMStream(Binary);
            
            if (Header.HeaderSize > 0x200 && (Header.UnitCode & 2) > 0) {
                ARM9iBinary = new ARM9i(Header.ARM9iEntryAddress, Header.ARM9iOffset, Header.ARM9iSize, Header.ARM9iRAMAddress);
                ARM7iBinary = new ARM7i(Header.ARM7iEntryAddress, Header.ARM7iOffset, Header.ARM7iSize, Header.ARM7iRAMAddress);
                SectorHashtable = new NitroByteWrapper(Header.SectorHashtableOffset, Header.SectorHashtableSize);
                BlockHashtable = new NitroByteWrapper(Header.BlockHashtableOffset, Header.BlockHashtableSize);
                DigestNTR = new NitroByteWrapper(Header.DigestNTROffset, Header.DigestNTRSize);
                DigestTWL = new NitroByteWrapper(Header.DigestTWLOffset, Header.DigestTWLSize);
                ModcryptArea = new NitroByteWrapper(Header.ModcryptOffset, Header.ModcryptSize);
                ModcryptArea2 = new NitroByteWrapper(Header.Modcrypt2Offset, Header.Modcrypt2Size);

                SectorHashtable.GetFileFromROMStream(Binary);
                BlockHashtable.GetFileFromROMStream(Binary);
                ARM9iBinary.GetFileFromROMStream(Binary);
                ARM7iBinary.GetFileFromROMStream(Binary);
                DigestNTR.GetFileFromROMStream(Binary);
                DigestTWL.GetFileFromROMStream(Binary);
                ModcryptArea.GetFileFromROMStream(Binary);
                ModcryptArea2.GetFileFromROMStream(Binary);
            }

        }

        public void Serialize(string Path)
        {
            // Death trap inbound.
            BinaryWriter Binary = new BinaryWriter(File.OpenWrite(Path));
            Binary.Write(new byte[0x4000]);

            List<uint> ARM9OverlayStartOffsets = new List<uint>(), ARM7OverlayStartOffsets = new List<uint>(),
                 ARM9OverlayEndOffsets = new List<uint>(), ARM7OverlayEndOffsets = new List<uint>();

            // Write the ARM9...
            Header.ARM9Offset = (uint)Binary.BaseStream.Position;
            Header.ARM9Size = ARM9Binary.Size;
            Binary.Write(ARM9Binary.Data);

            Header.ARM9OverlayTableOffset = (uint)Binary.BaseStream.Position;
            Header.ARM9OverlayTableSize = ARM9OverlayTable.Size;
            Binary.Write(ARM9OverlayTable.Data);

            foreach (NitroOverlay Overlay in ARM9Overlays)
            {
                ARM9OverlayStartOffsets.Add((uint)Binary.BaseStream.Position);
                Binary.Write(Overlay.Data);
                ARM9OverlayEndOffsets.Add((uint)Binary.BaseStream.Position);
            }

            // Now the ARM7...
            Header.ARM7Offset = (uint)Binary.BaseStream.Position;
            Header.ARM7Size = ARM7Binary.Size;
            Binary.Write(ARM7Binary.Data);

            Header.ARM7OverlayTableOffset = (uint)Binary.BaseStream.Position;
            Header.ARM7OverlayTableSize = ARM7OverlayTable.Size;
            Binary.Write(ARM7OverlayTable.Data);

            foreach (NitroOverlay Overlay in ARM7Overlays)
            {
                ARM9OverlayStartOffsets.Add((uint)Binary.BaseStream.Position);
                Binary.Write(Overlay.Data);
                ARM9OverlayEndOffsets.Add((uint)Binary.BaseStream.Position);
            }

            // File Name Table
            Header.FileNameTableOffset = (uint)Binary.BaseStream.Position;
            NitroDirectory.ConstructFileNameTable(Binary, Root);
            Header.FileNameTableSize = (uint)(Binary.BaseStream.Position - Header.FileNameTableOffset);

            // File Allocation Table
            ulong FileImageOffset = (ulong)(0x4000
            + ARM9Binary.Size + ARM9OverlayTable.Size + ARM9Overlays.Aggregate(0U, (Acc, E) => Acc + E.Size)
            + ARM7Binary.Size + ARM7OverlayTable.Size + ARM7Overlays.Aggregate(0U, (Acc, E) => Acc + E.Size)
            + ARM9Overlays.Count * 8 + ARM7Overlays.Count * 8 + 0x840);
            Header.FileAllocationTableOffset = (uint)Binary.BaseStream.Position;
            NitroDirectory.UpdateOffsets(Root, (uint)FileImageOffset);
            NitroDirectory.ConstructFileAllocationTable(Binary, Root, ARM9OverlayStartOffsets, ARM9OverlayEndOffsets, ARM7OverlayStartOffsets, ARM7OverlayEndOffsets);
            Header.FileAllocationTableSize = (uint)(Binary.BaseStream.Position - Header.FileAllocationTableOffset);

            // Banner
            Header.BannerOffset = (uint)Binary.BaseStream.Position;
            Header.BannerSize = Banner.Size;
            Binary.Write(Banner.FileData);

            // The File Image
            NitroDirectory.WriteFileImageTable(Binary, Root);
            Header.ROMSize = (uint)Binary.BaseStream.Position;

            if (Header.HeaderSize > 0x200)
            {
                // NTR Digest
                Header.DigestNTROffset = (uint)Binary.BaseStream.Position;
                Header.DigestNTRSize = DigestNTR.Size;

                // DSi Binary time...
                // ARM9i...
                Header.ARM9iOffset = (uint)Binary.BaseStream.Position;
                Header.ARM9iSize = ARM9iBinary.Size;
                Binary.Write(ARM9iBinary.Data);

                // ARM7i...
                Header.ARM7iOffset = (uint)Binary.BaseStream.Position;
                Header.ARM7iSize = ARM7iBinary.Size;
                Binary.Write(ARM7iBinary.Data);

                // Sector Hashtable
                Header.SectorHashtableOffset = (uint)Binary.BaseStream.Position;
                Header.SectorHashtableSize = SectorHashtable.Size;
                Binary.Write(SectorHashtable.Data);

                // Block Hashtable
                Header.BlockHashtableOffset = (uint)Binary.BaseStream.Position;
                Header.BlockHashtableSize = BlockHashtable.Size;
                Binary.Write(BlockHashtable.Data);

                // Modcrypt 
                Header.ModcryptOffset = (uint)Binary.BaseStream.Position;
                Header.ModcryptSize = ModcryptArea.Size;
                Binary.Write(ModcryptArea.Data);

                // Modcrypt 2
                Header.Modcrypt2Offset = (uint)Binary.BaseStream.Position;
                Header.Modcrypt2Size = ModcryptArea2.Size;
                Binary.Write(ModcryptArea2.Data);

                // TWL Digest
                Header.DigestTWLOffset = (uint)Binary.BaseStream.Position;
                Header.DigestTWLSize = DigestTWL.Size;

                Header.TotalTWLROMSize = (uint)Binary.BaseStream.Position;
            }
            
            // Calculate the CRC.
            NitroHeader.CalculateCRC(Header);
            // Header time!
            Binary.BaseStream.Position = 0x0;

            Binary.Write(Header.Title.ToCharArray());
            Binary.Write(Header.GameCode.ToCharArray());
            Binary.Write(Header.MakerCode.ToCharArray());
            Binary.Write(Header.UnitCode);
            Binary.Write(Header.EncryptionSeed);
            Binary.Write(Header.DeviceCapacity);
            Binary.Write(new byte[7]);
            Binary.Write(Header.TWLInternalFlags);
            Binary.Write(Header.PermitsFlags);
            Binary.Write(Header.ROMVersion);
            Binary.Write(Header.InternalFlags);
            Binary.Write(Header.ARM9Offset);
            Binary.Write(Header.ARM9EntryAddress);
            Binary.Write(Header.ARM9RAMAddress);
            Binary.Write(Header.ARM9Size);
            Binary.Write(Header.ARM7Offset);
            Binary.Write(Header.ARM7EntryAddress);
            Binary.Write(Header.ARM7RAMAddress);
            Binary.Write(Header.ARM7Size);
            Binary.Write(Header.FileNameTableOffset);
            Binary.Write(Header.FileNameTableSize);
            Binary.Write(Header.FileAllocationTableOffset);
            Binary.Write(Header.FileAllocationTableSize);
            Binary.Write(Header.ARM9OverlayTableOffset);
            Binary.Write(Header.ARM9OverlayTableSize);
            Binary.Write(Header.ARM7OverlayTableOffset);
            Binary.Write(Header.ARM7OverlayTableSize);
            Binary.Write(Header.FlagsRead);
            Binary.Write(Header.FlagsInit);
            Binary.Write(Header.BannerOffset);
            Binary.Write(Header.SecureCRC16);
            Binary.Write(Header.ROMTimeout);
            Binary.Write(Header.ARM9Autoload);
            Binary.Write(Header.ARM7Autoload);
            Binary.Write(Header.SecureAreaDisable);
            Binary.Write(Header.ROMSize);
            Binary.Write(Header.HeaderSize);
            Binary.Write(Header.Reserved2);
            Binary.Write(Header.Logo);
            Binary.Write(Header.LogoCRC16);
            Binary.Write(Header.HeaderCRC16);
            Binary.Write(Header.DebugROMOffset);
            Binary.Write(Header.DebugSize);
            Binary.Write(Header.DebugRAMAddress);
            Binary.Write(Header.Reserved3);
            Binary.Write(Header.Reserved4);

            if (Header.HeaderSize > 0x200)
            {
                // Write all the TWL stuff...
                foreach (byte[] Setting in Header.GlobalMBKSetting)
                    Binary.Write(Setting);
                foreach (uint Setting in Header.ARM9MBKSetting)
                    Binary.Write(Setting);
                foreach (uint Setting in Header.ARM7MBKSetting)
                    Binary.Write(Setting);
                foreach (uint Setting in Header.mbk9_wramcnt_setting)
                    Binary.Write(Setting);

                Binary.Write(Header.RegionFlags);
                Binary.Write(Header.AccessControl);
                Binary.Write(Header.SCFGExtMask);
                Binary.Write(Header.AppFlags);
                Binary.Write(Header.ARM9iOffset);
                Binary.Write(Header.ARM9iEntryAddress);
                Binary.Write(Header.ARM9iRAMAddress);
                Binary.Write(Header.ARM9iSize);
                Binary.Write(Header.ARM7iOffset);
                Binary.Write(Header.ARM7iEntryAddress);
                Binary.Write(Header.ARM7iRAMAddress);
                Binary.Write(Header.ARM7iSize);
                Binary.Write(Header.DigestNTROffset);
                Binary.Write(Header.DigestNTRSize);
                Binary.Write(Header.DigestTWLOffset);
                Binary.Write(Header.DigestTWLSize);
                Binary.Write(Header.SectorHashtableOffset);
                Binary.Write(Header.SectorHashtableSize);
                Binary.Write(Header.BlockHashtableOffset);
                Binary.Write(Header.BlockHashtableSize);
                Binary.Write(Header.DigestSectorSize);
                Binary.Write(Header.DigestBlockSectorCount);
                Binary.Write(Header.BannerSize);
                Binary.Write(Header.offset_0x20C);
                Binary.Write(Header.TotalTWLROMSize);
                Binary.Write(Header.offset_0x214);
                Binary.Write(Header.offset_0x218);
                Binary.Write(Header.offset_0x21C);
                Binary.Write(Header.ModcryptOffset);
                Binary.Write(Header.ModcryptSize);
                Binary.Write(Header.Modcrypt2Offset);
                Binary.Write(Header.Modcrypt2Size);
                Binary.Write(Header.TitleID);
                Binary.Write(Header.PublicSaveSize);
                Binary.Write(Header.PrivateSaveSize);
                Binary.Write(Header.Reserved5);
                Binary.Write(Header.AgeRatings);
                Binary.Write(Header.HMAC_ARM9);
                Binary.Write(Header.HMAC_ARM7);
                Binary.Write(Header.HMAC_DigestMaster);
                Binary.Write(Header.HMAC_TitleIcon);
                Binary.Write(Header.HMAC_ARM9i);
                Binary.Write(Header.HMAC_ARM7i);
                Binary.Write(Header.Reserved6);
                Binary.Write(Header.HMAC_ARM9NoSecure);
                Binary.Write(Header.Reserved7);
                Binary.Write(Header.DebugArguments);
                Binary.Write(Header.RSASignature);
            }

            // If by some miracle we get here, that means we did a good job. Well, unless it doesn't boot. Then who knows.
            Binary.Close();
        }
    }
}
