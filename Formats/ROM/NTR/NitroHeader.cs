using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Nito.HashAlgorithms;

namespace NitroSharp.Formats.ROM
{
    public class NitroHeader
    {
        public NitroHeader(BinaryReader Binary)
        {
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
            ROMVersion = Binary.ReadByte();
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
            SecureCRC16 = Binary.ReadUInt16();
            ROMTimeout = Binary.ReadUInt16();
            ARM9Autoload = Binary.ReadUInt32();
            ARM7Autoload = Binary.ReadUInt32();
            SecureAreaDisable = Binary.ReadUInt64();
            ROMSize = Binary.ReadUInt32();
            HeaderSize = Binary.ReadUInt32();
            Reserved2 = Binary.ReadBytes(56);
            Logo = Binary.ReadBytes(0x9C);
            LogoCRC16 = Binary.ReadUInt16();
            HeaderCRC16 = Binary.ReadUInt16();
            DebugROMOffset = Binary.ReadUInt32();
            DebugSize = Binary.ReadUInt32();
            DebugRAMAddress = Binary.ReadUInt32();
            Reserved3 = Binary.ReadUInt32();
            Reserved4 = Binary.ReadBytes(0x10);

            if (HeaderSize > 0x200 && (UnitCode & 2) > 0)
            {
                GlobalMBKSetting = new List<byte[]>();
                ARM9MBKSetting = new List<uint>();
                ARM7MBKSetting = new List<uint>();
                mbk9_wramcnt_setting = new List<uint>();

                // DSi-Enhanced Data
                for (var i = 0; i < 5; i++)
                    GlobalMBKSetting.Add(Binary.ReadBytes(0x4));
                for (var i = 0; i < 3; i++)
                    ARM9MBKSetting.Add(Binary.ReadUInt32());
                for (var i = 0; i < 3; i++)
                    ARM7MBKSetting.Add(Binary.ReadUInt32());

                mbk9_wramcnt_setting.Add(Binary.ReadUInt32());

                RegionFlags = Binary.ReadUInt32();
                AccessControl = Binary.ReadUInt32();
                SCFGExtMask = Binary.ReadUInt32();
                AppFlags = Binary.ReadBytes(4);

                ARM9iOffset = Binary.ReadUInt32();
                ARM9iEntryAddress = Binary.ReadUInt32();
                ARM9iRAMAddress = Binary.ReadUInt32();
                ARM9iSize = Binary.ReadUInt32();

                ARM7iOffset = Binary.ReadUInt32();
                ARM7iEntryAddress = Binary.ReadUInt32();
                ARM7iRAMAddress = Binary.ReadUInt32();
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
                offset_0x20C = Binary.ReadUInt32();

                TotalTWLROMSize = Binary.ReadUInt32();
                offset_0x214 = Binary.ReadUInt32();
                offset_0x218 = Binary.ReadUInt32();
                offset_0x21C = Binary.ReadUInt32();

                ModcryptOffset = Binary.ReadUInt32();
                ModcryptSize = Binary.ReadUInt32();
                Modcrypt2Offset = Binary.ReadUInt32();
                Modcrypt2Size = Binary.ReadUInt32();

                TitleID = Binary.ReadUInt64();
                PublicSaveSize = Binary.ReadUInt32();
                PrivateSaveSize = Binary.ReadUInt32();

                Reserved5 = Binary.ReadBytes(0xB0);
                AgeRatings = Binary.ReadBytes(0x10);
                HMAC_ARM9 = Binary.ReadBytes(20);
                HMAC_ARM7 = Binary.ReadBytes(20);
                HMAC_DigestMaster = Binary.ReadBytes(20);
                HMAC_TitleIcon = Binary.ReadBytes(20);
                HMAC_ARM9i = Binary.ReadBytes(20);
                HMAC_ARM7i = Binary.ReadBytes(20);
                Reserved6 = Binary.ReadBytes(40);
                HMAC_ARM9NoSecure = Binary.ReadBytes(20);
                Reserved7 = Binary.ReadBytes(0xA4C);
                DebugArguments = Binary.ReadBytes(0x180);
                RSASignature = Binary.ReadBytes(0x80);
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
        public byte ROMVersion { get; set; }
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
        public ushort SecureCRC16 { get; set; }
        public ushort ROMTimeout { get; set; }
        public uint ARM9Autoload { get; set; }
        public uint ARM7Autoload { get; set; }
        public ulong SecureAreaDisable { get; set; }
        public uint ROMSize { get; set; }
        public uint HeaderSize { get; set; }
        public byte[] Reserved2 { get; set; }
        public byte[] Logo { get; set; }
        public ushort LogoCRC16 { get; set; }
        public ushort HeaderCRC16 { get; set; }
        public bool SecureCRC { get; set; }
        public bool LogoCRC { get; set; }
        public bool HeaderCRC { get; set; }
        public uint DebugROMOffset { get; set; }
        public uint DebugSize { get; set; }
        public uint DebugRAMAddress { get; set; }
        public uint Reserved3 { get; set; }
        public byte[] Reserved4 { get; set; }
        public List<byte[]> GlobalMBKSetting { get; set; }
        public List<uint> ARM9MBKSetting { get; set; }
        public List<uint> ARM7MBKSetting { get; set; }
        public List<uint> mbk9_wramcnt_setting { get; set; }
        public uint RegionFlags { get; set; }
        public uint AccessControl { get; set; }
        public uint SCFGExtMask { get; set; }
        public byte[] AppFlags { get; set; }
        public uint ARM9iOffset { get; set; }
        public uint ARM9iEntryAddress { get; set; }
        public uint ARM9iRAMAddress { get; set; }
        public uint ARM9iSize { get; set; }
        public uint ARM7iOffset { get; set; }
        public uint ARM7iEntryAddress { get; set; }
        public uint ARM7iRAMAddress { get; set; }
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
        public uint offset_0x20C { get; set; }
        public uint TotalTWLROMSize { get; set; }
        public uint offset_0x214 { get; set; }
        public uint offset_0x218 { get; set; }
        public uint offset_0x21C { get; set; }
        public uint ModcryptOffset { get; set; }
        public uint ModcryptSize { get; set; }
        public uint Modcrypt2Offset { get; set; }
        public uint Modcrypt2Size { get; set; }
        public ulong TitleID { get; set; }
        public uint PublicSaveSize { get; set; }
        public uint PrivateSaveSize { get; set; }
        public byte[] Reserved5 { get; set; }
        public byte[] AgeRatings { get; set; }
        public byte[] HMAC_ARM9 { get; set; }
        public byte[] HMAC_ARM7 { get; set; }
        public byte[] HMAC_DigestMaster { get; set; }
        public byte[] HMAC_TitleIcon { get; set; }
        public byte[] HMAC_ARM9i { get; set; }
        public byte[] HMAC_ARM7i { get; set; }
        public byte[] Reserved6 { get; set; }
        public byte[] HMAC_ARM9NoSecure { get; set; }
        public byte[] Reserved7 { get; set; }
        public byte[] DebugArguments { get; set; }
        public byte[] RSASignature { get; set; }

        public static void CalculateCRC(NitroHeader Header)
        {
            var Temporary = new MemoryStream(new byte[0x8000]);
            Temporary.Write(Encoding.ASCII.GetBytes(Header.Title));
            Temporary.Write(Encoding.ASCII.GetBytes(Header.GameCode));
            Temporary.Write(Encoding.ASCII.GetBytes(Header.MakerCode));
            Temporary.Write(BitConverter.GetBytes(Header.UnitCode));
            Temporary.Write(BitConverter.GetBytes(Header.EncryptionSeed));
            Temporary.Write(BitConverter.GetBytes(Header.DeviceCapacity));
            Temporary.Write(new byte[7]);
            Temporary.Write(BitConverter.GetBytes(Header.TWLInternalFlags));
            Temporary.Write(BitConverter.GetBytes(Header.PermitsFlags));
            Temporary.Write(BitConverter.GetBytes(Header.ROMVersion));
            Temporary.Write(BitConverter.GetBytes(Header.InternalFlags));
            Temporary.Write(BitConverter.GetBytes(Header.ARM9Offset));
            Temporary.Write(BitConverter.GetBytes(Header.ARM9EntryAddress));
            Temporary.Write(BitConverter.GetBytes(Header.ARM9RAMAddress));
            Temporary.Write(BitConverter.GetBytes(Header.ARM9Size));
            Temporary.Write(BitConverter.GetBytes(Header.ARM7Offset));
            Temporary.Write(BitConverter.GetBytes(Header.ARM7EntryAddress));
            Temporary.Write(BitConverter.GetBytes(Header.ARM7RAMAddress));
            Temporary.Write(BitConverter.GetBytes(Header.ARM7Size));
            Temporary.Write(BitConverter.GetBytes(Header.FileNameTableOffset));
            Temporary.Write(BitConverter.GetBytes(Header.FileNameTableSize));
            Temporary.Write(BitConverter.GetBytes(Header.FileAllocationTableOffset));
            Temporary.Write(BitConverter.GetBytes(Header.FileAllocationTableSize));
            Temporary.Write(BitConverter.GetBytes(Header.ARM9OverlayTableOffset));
            Temporary.Write(BitConverter.GetBytes(Header.ARM9OverlayTableSize));
            Temporary.Write(BitConverter.GetBytes(Header.FlagsRead));
            Temporary.Write(BitConverter.GetBytes(Header.FlagsInit));
            Temporary.Write(BitConverter.GetBytes(Header.BannerOffset));
            Temporary.Write(BitConverter.GetBytes(Header.SecureCRC16));
            Temporary.Write(BitConverter.GetBytes(Header.ROMTimeout));
            Temporary.Write(BitConverter.GetBytes(Header.ARM9Autoload));
            Temporary.Write(BitConverter.GetBytes(Header.ARM7Autoload));
            Temporary.Write(BitConverter.GetBytes(Header.SecureAreaDisable));
            Temporary.Write(BitConverter.GetBytes(Header.ROMSize));
            Temporary.Write(BitConverter.GetBytes(Header.HeaderSize));
            Temporary.Write(Header.Reserved2);
            Temporary.Write(Header.Logo);
            Temporary.Write(BitConverter.GetBytes(Header.LogoCRC16));
            Temporary.Write(BitConverter.GetBytes(Header.HeaderCRC16));
            Temporary.Write(BitConverter.GetBytes(Header.DebugROMOffset));
            Temporary.Write(BitConverter.GetBytes(Header.DebugSize));
            Temporary.Write(BitConverter.GetBytes(Header.DebugRAMAddress));
            Temporary.Write(BitConverter.GetBytes(Header.Reserved3));
            Temporary.Write(Header.Reserved4);


            var Calculator = new CRC16();
            Header.HeaderCRC16 =
                BitConverter.ToUInt16(
                    Calculator.ComputeHash(new ArraySegment<byte>(Temporary.ToArray(), 0, 0x15E).Array));
            Header.LogoCRC16 =
                BitConverter.ToUInt16(
                    Calculator.ComputeHash(new ArraySegment<byte>(Temporary.ToArray(), 0xC0, 0x9C).Array));
            Header.SecureCRC16 = BitConverter.ToUInt16(Calculator.ComputeHash(
                new ArraySegment<byte>(Temporary.ToArray(), (int) Header.ARM9Offset,
                    (int) (0x8000 - 2 * Header.ARM9Offset)).Array));
            Temporary.Close();
        }
    }
}