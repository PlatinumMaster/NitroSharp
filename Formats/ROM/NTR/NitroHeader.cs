using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Nito.HashAlgorithms;

namespace NitroSharp.Formats.ROM.NTR {
    public class NitroHeader {
        public NitroHeader(BinaryReader Binary) {
            Binary.BaseStream.Position = 0x0;

            Title = new string(Binary.ReadChars(12));
            GameCode = new string(Binary.ReadChars(4));
            MakerCode = new string(Binary.ReadChars(2));
            UnitCode = Binary.ReadByte();
            EncryptionSeed = Binary.ReadByte();
            DeviceCapacity = Binary.ReadByte();
            Reserved = Binary.ReadBytes(7);
            TWLInternalFlags = Binary.ReadByte();
            PermitsFlags = Binary.ReadByte();
            RomVersion = Binary.ReadByte();
            InternalFlags = Binary.ReadByte();
            ARM9Offset = Binary.ReadUInt32();
            ARM9EntryAddress = Binary.ReadUInt32();
            ARM9RAMAddress = Binary.ReadUInt32();
            ARM9Size = Binary.ReadUInt32();
            ARM7Offset = Binary.ReadUInt32();
            ARM7EntryAddress = Binary.ReadUInt32();
            ARM7RAMAddress = Binary.ReadUInt32();
            ARM7Size = Binary.ReadUInt32();
            FileNameTableOffset = Binary.ReadUInt32();
            FileNameTableSize = Binary.ReadUInt32();
            FileAllocationTableOffset = Binary.ReadUInt32();
            FileAllocationTableSize = Binary.ReadUInt32();
            ARM9OverlayTableOffset = Binary.ReadUInt32();
            ARM9OverlayTableSize = Binary.ReadUInt32();
            ARM7OverlayTableOffset = Binary.ReadUInt32();
            ARM7OverlayTableSize = Binary.ReadUInt32();
            FlagsRead = Binary.ReadUInt32();
            FlagsInit = Binary.ReadUInt32();
            BannerOffset = Binary.ReadUInt32();
            SecureCrc16 = Binary.ReadUInt16();
            RomTimeout = Binary.ReadUInt16();
            ARM9Autoload = Binary.ReadUInt32();
            ARM7Autoload = Binary.ReadUInt32();
            SecureAreaDisable = Binary.ReadUInt64();
            RomSize = Binary.ReadUInt32();
            HeaderSize = Binary.ReadUInt32();
            Reserved2 = Binary.ReadBytes(56);
            Logo = Binary.ReadBytes(0x9C);
            LogoCRC = Binary.ReadUInt16();
            HeaderCRC = Binary.ReadUInt16();
            DebugRomOffset = Binary.ReadUInt32();
            DebugSize = Binary.ReadUInt32();
            DebugRamAddress = Binary.ReadUInt32();
            Reserved3 = Binary.ReadUInt32();
            Reserved4 = Binary.ReadBytes(0x10);

            if (HeaderSize > 0x200 && (UnitCode & 2) > 0) {
                GlobalMbkSetting = new List<byte[]>();
                ARM9MbkSetting = new List<uint>();
                ARM7MbkSetting = new List<uint>();
                Mbk9WramcntSetting = new List<uint>();

                // DSi-Enhanced Data
                for (var i = 0; i < 5; i++)
                    GlobalMbkSetting.Add(Binary.ReadBytes(0x4));
                for (var i = 0; i < 3; i++)
                    ARM9MbkSetting.Add(Binary.ReadUInt32());
                for (var i = 0; i < 3; i++)
                    ARM7MbkSetting.Add(Binary.ReadUInt32());

                Mbk9WramcntSetting.Add(Binary.ReadUInt32());

                RegionFlags = Binary.ReadUInt32();
                AccessControl = Binary.ReadUInt32();
                SCFGExtMask = Binary.ReadUInt32();
                AppFlags = Binary.ReadBytes(4);

                ARM9iOffset = Binary.ReadUInt32();
                ARM9iEntryAddress = Binary.ReadUInt32();
                ARM9iRamAddress = Binary.ReadUInt32();
                ARM9iSize = Binary.ReadUInt32();

                ARM7iOffset = Binary.ReadUInt32();
                ARM7iEntryAddress = Binary.ReadUInt32();
                ARM7iRamAddress = Binary.ReadUInt32();
                ARM7iSize = Binary.ReadUInt32();

                DigestNTROffset = Binary.ReadUInt32();
                DigestNTRSize = Binary.ReadUInt32();
                DigestTWLOffset = Binary.ReadUInt32();
                DigestTWLSize = Binary.ReadUInt32();

                SectorHashtableOffset = Binary.ReadUInt32();
                SectorHashtableSize = Binary.ReadUInt32();
                BlockHashtableOffset = Binary.ReadUInt32();
                BlockHashtableSize = Binary.ReadUInt32();

                DigestSectorSize = Binary.ReadUInt32();
                DigestBlockSectorCount = Binary.ReadUInt32();
                BannerSize = Binary.ReadUInt32();
                Offset_20C = Binary.ReadUInt32();

                TotalTWLROMSize = Binary.ReadUInt32();
                Offset_214 = Binary.ReadUInt32();
                Offset_218 = Binary.ReadUInt32();
                Offset_21C = Binary.ReadUInt32();

                ModcryptOffset = Binary.ReadUInt32();
                ModcryptSize = Binary.ReadUInt32();
                Modcrypt2Offset = Binary.ReadUInt32();
                Modcrypt2Size = Binary.ReadUInt32();

                TitleID = Binary.ReadUInt64();
                PublicSaveSize = Binary.ReadUInt32();
                PrivateSaveSize = Binary.ReadUInt32();

                Reserved5 = Binary.ReadBytes(0xB0);
                AgeRatings = Binary.ReadBytes(0x10);
                HMACARM9 = Binary.ReadBytes(20);
                HMACARM7 = Binary.ReadBytes(20);
                HMACDigestMaster = Binary.ReadBytes(20);
                HMACTitleIcon = Binary.ReadBytes(20);
                HMACARM9i = Binary.ReadBytes(20);
                HMACARM7i = Binary.ReadBytes(20);
                Reserved6 = Binary.ReadBytes(40);
                HMACARM9NoSecure = Binary.ReadBytes(20);
                Reserved7 = Binary.ReadBytes(0xA4C);
                DebugArguments = Binary.ReadBytes(0x180);
                RsaSignature = Binary.ReadBytes(0x80);
            }
        }

        public string Title { get; set; }
        public string GameCode { get; set; }
        public string MakerCode { get; set; }
        public byte UnitCode { get; set; }
        public byte EncryptionSeed { get; set; }
        public byte DeviceCapacity { get; }
        public byte[] Reserved { get; }
        public byte TWLInternalFlags { get; set; }
        public byte PermitsFlags { get; set; }
        public byte RomVersion { get; set; }
        public byte InternalFlags { get; set; }
        public uint ARM9Offset { get; set; }
        public uint ARM9EntryAddress { get; set; }
        public uint ARM9RAMAddress { get; set; }
        public uint ARM9Size { get; set; }
        public uint ARM7Offset { get; set; }
        public uint ARM7EntryAddress { get; set; }
        public uint ARM7RAMAddress { get; set; }
        public uint ARM7Size { get; set; }
        public uint FileNameTableOffset { get; set; }
        public uint FileNameTableSize { get; set; }
        public uint FileAllocationTableOffset { get; set; }
        public uint FileAllocationTableSize { get; set; }
        public uint ARM9OverlayTableOffset { get; set; }
        public uint ARM9OverlayTableSize { get; set; }
        public uint ARM7OverlayTableOffset { get; set; }
        public uint ARM7OverlayTableSize { get; set; }
        public uint FlagsRead { get; set; }
        public uint FlagsInit { get; set; }
        public uint BannerOffset { get; set; }
        public ushort SecureCrc16 { get; set; }
        public ushort RomTimeout { get; set; }
        public uint ARM9Autoload { get; set; }
        public uint ARM7Autoload { get; set; }
        public ulong SecureAreaDisable { get; set; }
        public uint RomSize { get; set; }
        public uint HeaderSize { get; set; }
        public byte[] Reserved2 { get; set; }
        public byte[] Logo { get; set; }
        public ushort LogoCRC { get; set; }
        public ushort HeaderCRC { get; set; }
        public bool SecureCrc { get; set; }
        public bool LogoCrc { get; set; }
        public bool HeaderCrc { get; set; }
        public uint DebugRomOffset { get; set; }
        public uint DebugSize { get; set; }
        public uint DebugRamAddress { get; set; }
        public uint Reserved3 { get; set; }
        public byte[] Reserved4 { get; set; }
        public List<byte[]> GlobalMbkSetting { get; set; }
        public List<uint> ARM9MbkSetting { get; set; }
        public List<uint> ARM7MbkSetting { get; set; }
        public List<uint> Mbk9WramcntSetting { get; set; }
        public uint RegionFlags { get; set; }
        public uint AccessControl { get; set; }
        public uint SCFGExtMask { get; set; }
        public byte[] AppFlags { get; set; }
        public uint ARM9iOffset { get; set; }
        public uint ARM9iEntryAddress { get; set; }
        public uint ARM9iRamAddress { get; set; }
        public uint ARM9iSize { get; set; }
        public uint ARM7iOffset { get; set; }
        public uint ARM7iEntryAddress { get; set; }
        public uint ARM7iRamAddress { get; set; }
        public uint ARM7iSize { get; set; }
        public uint DigestNTROffset { get; set; }
        public uint DigestNTRSize { get; set; }
        public uint DigestTWLOffset { get; set; }
        public uint DigestTWLSize { get; set; }
        public uint SectorHashtableOffset { get; set; }
        public uint SectorHashtableSize { get; set; }
        public uint BlockHashtableOffset { get; set; }
        public uint BlockHashtableSize { get; set; }
        public uint DigestSectorSize { get; set; }
        public uint DigestBlockSectorCount { get; set; }
        public uint BannerSize { get; set; }
        public uint Offset_20C { get; set; }
        public uint TotalTWLROMSize { get; set; }
        public uint Offset_214 { get; set; }
        public uint Offset_218 { get; set; }
        public uint Offset_21C { get; set; }
        public uint ModcryptOffset { get; set; }
        public uint ModcryptSize { get; set; }
        public uint Modcrypt2Offset { get; set; }
        public uint Modcrypt2Size { get; set; }
        public ulong TitleID { get; set; }
        public uint PublicSaveSize { get; set; }
        public uint PrivateSaveSize { get; set; }
        public byte[] Reserved5 { get; set; }
        public byte[] AgeRatings { get; set; }
        public byte[] HMACARM9 { get; set; }
        public byte[] HMACARM7 { get; set; }
        public byte[] HMACDigestMaster { get; set; }
        public byte[] HMACTitleIcon { get; set; }
        public byte[] HMACARM9i { get; set; }
        public byte[] HMACARM7i { get; set; }
        public byte[] Reserved6 { get; set; }
        public byte[] HMACARM9NoSecure { get; set; }
        public byte[] Reserved7 { get; set; }
        public byte[] DebugArguments { get; set; }
        public byte[] RsaSignature { get; set; }

        public static void CalculateCRC(NitroHeader header) {
            var temporary = new MemoryStream(new byte[0x8000]);
            temporary.Write(Encoding.ASCII.GetBytes(header.Title));
            temporary.Write(Encoding.ASCII.GetBytes(header.GameCode));
            temporary.Write(Encoding.ASCII.GetBytes(header.MakerCode));
            temporary.Write(BitConverter.GetBytes(header.UnitCode));
            temporary.Write(BitConverter.GetBytes(header.EncryptionSeed));
            temporary.Write(BitConverter.GetBytes(header.DeviceCapacity));
            temporary.Write(new byte[7]);
            temporary.Write(BitConverter.GetBytes(header.TWLInternalFlags));
            temporary.Write(BitConverter.GetBytes(header.PermitsFlags));
            temporary.Write(BitConverter.GetBytes(header.RomVersion));
            temporary.Write(BitConverter.GetBytes(header.InternalFlags));
            temporary.Write(BitConverter.GetBytes(header.ARM9Offset));
            temporary.Write(BitConverter.GetBytes(header.ARM9EntryAddress));
            temporary.Write(BitConverter.GetBytes(header.ARM9RAMAddress));
            temporary.Write(BitConverter.GetBytes(header.ARM9Size));
            temporary.Write(BitConverter.GetBytes(header.ARM7Offset));
            temporary.Write(BitConverter.GetBytes(header.ARM7EntryAddress));
            temporary.Write(BitConverter.GetBytes(header.ARM7RAMAddress));
            temporary.Write(BitConverter.GetBytes(header.ARM7Size));
            temporary.Write(BitConverter.GetBytes(header.FileNameTableOffset));
            temporary.Write(BitConverter.GetBytes(header.FileNameTableSize));
            temporary.Write(BitConverter.GetBytes(header.FileAllocationTableOffset));
            temporary.Write(BitConverter.GetBytes(header.FileAllocationTableSize));
            temporary.Write(BitConverter.GetBytes(header.ARM9OverlayTableOffset));
            temporary.Write(BitConverter.GetBytes(header.ARM9OverlayTableSize));
            temporary.Write(BitConverter.GetBytes(header.FlagsRead));
            temporary.Write(BitConverter.GetBytes(header.FlagsInit));
            temporary.Write(BitConverter.GetBytes(header.BannerOffset));
            temporary.Write(BitConverter.GetBytes(header.SecureCrc16));
            temporary.Write(BitConverter.GetBytes(header.RomTimeout));
            temporary.Write(BitConverter.GetBytes(header.ARM9Autoload));
            temporary.Write(BitConverter.GetBytes(header.ARM7Autoload));
            temporary.Write(BitConverter.GetBytes(header.SecureAreaDisable));
            temporary.Write(BitConverter.GetBytes(header.RomSize));
            temporary.Write(BitConverter.GetBytes(header.HeaderSize));
            temporary.Write(header.Reserved2);
            temporary.Write(header.Logo);
            temporary.Write(BitConverter.GetBytes(header.LogoCRC));
            temporary.Write(BitConverter.GetBytes(header.HeaderCRC));
            temporary.Write(BitConverter.GetBytes(header.DebugRomOffset));
            temporary.Write(BitConverter.GetBytes(header.DebugSize));
            temporary.Write(BitConverter.GetBytes(header.DebugRamAddress));
            temporary.Write(BitConverter.GetBytes(header.Reserved3));
            temporary.Write(header.Reserved4);


            var calculator = new CRC16();
            header.HeaderCRC =
                BitConverter.ToUInt16(
                    calculator.ComputeHash(new ArraySegment<byte>(temporary.ToArray(), 0, 0x15E).Array));
            header.LogoCRC =
                BitConverter.ToUInt16(
                    calculator.ComputeHash(new ArraySegment<byte>(temporary.ToArray(), 0xC0, 0x9C).Array));
            header.SecureCrc16 = BitConverter.ToUInt16(calculator.ComputeHash(
                new ArraySegment<byte>(temporary.ToArray(), (int) header.ARM9Offset,
                    (int) (0x8000 - 2 * header.ARM9Offset)).Array));
            temporary.Close();
        }
    }
}