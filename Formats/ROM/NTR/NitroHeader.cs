using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Nito.HashAlgorithms;

namespace NitroSharp.Formats.ROM.NTR {
    public class NitroHeader {
        public NitroHeader(BinaryReader binary) {
            binary.BaseStream.Position = 0x0;

            Title = new string(binary.ReadChars(12));
            GameCode = new string(binary.ReadChars(4));
            MakerCode = new string(binary.ReadChars(2));
            UnitCode = binary.ReadByte();
            EncryptionSeed = binary.ReadByte();
            DeviceCapacity = binary.ReadByte();
            Reserved = binary.ReadBytes(7);
            TwlInternalFlags = binary.ReadByte();
            PermitsFlags = binary.ReadByte();
            RomVersion = binary.ReadByte();
            InternalFlags = binary.ReadByte();
            Arm9Offset = binary.ReadUInt32();
            Arm9EntryAddress = binary.ReadUInt32();
            Arm9RamAddress = binary.ReadUInt32();
            Arm9Size = binary.ReadUInt32();
            Arm7Offset = binary.ReadUInt32();
            Arm7EntryAddress = binary.ReadUInt32();
            Arm7RamAddress = binary.ReadUInt32();
            Arm7Size = binary.ReadUInt32();
            FileNameTableOffset = binary.ReadUInt32();
            FileNameTableSize = binary.ReadUInt32();
            FileAllocationTableOffset = binary.ReadUInt32();
            FileAllocationTableSize = binary.ReadUInt32();
            Arm9OverlayTableOffset = binary.ReadUInt32();
            Arm9OverlayTableSize = binary.ReadUInt32();
            Arm7OverlayTableOffset = binary.ReadUInt32();
            Arm7OverlayTableSize = binary.ReadUInt32();
            FlagsRead = binary.ReadUInt32();
            FlagsInit = binary.ReadUInt32();
            BannerOffset = binary.ReadUInt32();
            SecureCrc16 = binary.ReadUInt16();
            RomTimeout = binary.ReadUInt16();
            Arm9Autoload = binary.ReadUInt32();
            Arm7Autoload = binary.ReadUInt32();
            SecureAreaDisable = binary.ReadUInt64();
            RomSize = binary.ReadUInt32();
            HeaderSize = binary.ReadUInt32();
            Reserved2 = binary.ReadBytes(56);
            Logo = binary.ReadBytes(0x9C);
            LogoCrc16 = binary.ReadUInt16();
            HeaderCrc16 = binary.ReadUInt16();
            DebugRomOffset = binary.ReadUInt32();
            DebugSize = binary.ReadUInt32();
            DebugRamAddress = binary.ReadUInt32();
            Reserved3 = binary.ReadUInt32();
            Reserved4 = binary.ReadBytes(0x10);

            if (HeaderSize > 0x200 && (UnitCode & 2) > 0) {
                GlobalMbkSetting = new List<byte[]>();
                Arm9MbkSetting = new List<uint>();
                Arm7MbkSetting = new List<uint>();
                Mbk9WramcntSetting = new List<uint>();

                // DSi-Enhanced Data
                for (var i = 0; i < 5; i++)
                    GlobalMbkSetting.Add(binary.ReadBytes(0x4));
                for (var i = 0; i < 3; i++)
                    Arm9MbkSetting.Add(binary.ReadUInt32());
                for (var i = 0; i < 3; i++)
                    Arm7MbkSetting.Add(binary.ReadUInt32());

                Mbk9WramcntSetting.Add(binary.ReadUInt32());

                RegionFlags = binary.ReadUInt32();
                AccessControl = binary.ReadUInt32();
                ScfgExtMask = binary.ReadUInt32();
                AppFlags = binary.ReadBytes(4);

                Arm9IOffset = binary.ReadUInt32();
                Arm9IEntryAddress = binary.ReadUInt32();
                Arm9IRamAddress = binary.ReadUInt32();
                Arm9ISize = binary.ReadUInt32();

                Arm7IOffset = binary.ReadUInt32();
                Arm7IEntryAddress = binary.ReadUInt32();
                Arm7IRamAddress = binary.ReadUInt32();
                Arm7ISize = binary.ReadUInt32();

                DigestNtrOffset = binary.ReadUInt32();
                DigestNtrSize = binary.ReadUInt32();
                DigestTwlOffset = binary.ReadUInt32();
                DigestTwlSize = binary.ReadUInt32();

                SectorHashtableOffset = binary.ReadUInt32();
                SectorHashtableSize = binary.ReadUInt32();
                BlockHashtableOffset = binary.ReadUInt32();
                BlockHashtableSize = binary.ReadUInt32();

                DigestSectorSize = binary.ReadUInt32();
                DigestBlockSectorCount = binary.ReadUInt32();
                BannerSize = binary.ReadUInt32();
                Offset0X20C = binary.ReadUInt32();

                TotalTwlromSize = binary.ReadUInt32();
                Offset0X214 = binary.ReadUInt32();
                Offset0X218 = binary.ReadUInt32();
                Offset0X21C = binary.ReadUInt32();

                ModcryptOffset = binary.ReadUInt32();
                ModcryptSize = binary.ReadUInt32();
                Modcrypt2Offset = binary.ReadUInt32();
                Modcrypt2Size = binary.ReadUInt32();

                TitleId = binary.ReadUInt64();
                PublicSaveSize = binary.ReadUInt32();
                PrivateSaveSize = binary.ReadUInt32();

                Reserved5 = binary.ReadBytes(0xB0);
                AgeRatings = binary.ReadBytes(0x10);
                HmacArm9 = binary.ReadBytes(20);
                HmacArm7 = binary.ReadBytes(20);
                HmacDigestMaster = binary.ReadBytes(20);
                HmacTitleIcon = binary.ReadBytes(20);
                HmacArm9I = binary.ReadBytes(20);
                HmacArm7I = binary.ReadBytes(20);
                Reserved6 = binary.ReadBytes(40);
                HmacArm9NoSecure = binary.ReadBytes(20);
                Reserved7 = binary.ReadBytes(0xA4C);
                DebugArguments = binary.ReadBytes(0x180);
                RsaSignature = binary.ReadBytes(0x80);
            }
        }

        public string Title { get; set; }
        public string GameCode { get; set; }
        public string MakerCode { get; set; }
        public byte UnitCode { get; set; }
        public byte EncryptionSeed { get; set; }
        public byte DeviceCapacity { get; }
        public byte[] Reserved { get; }
        public byte TwlInternalFlags { get; set; }
        public byte PermitsFlags { get; set; }
        public byte RomVersion { get; set; }
        public byte InternalFlags { get; set; }
        public uint Arm9Offset { get; set; }
        public uint Arm9EntryAddress { get; set; }
        public uint Arm9RamAddress { get; set; }
        public uint Arm9Size { get; set; }
        public uint Arm7Offset { get; set; }
        public uint Arm7EntryAddress { get; set; }
        public uint Arm7RamAddress { get; set; }
        public uint Arm7Size { get; set; }
        public uint FileNameTableOffset { get; set; }
        public uint FileNameTableSize { get; set; }
        public uint FileAllocationTableOffset { get; set; }
        public uint FileAllocationTableSize { get; set; }
        public uint Arm9OverlayTableOffset { get; set; }
        public uint Arm9OverlayTableSize { get; set; }
        public uint Arm7OverlayTableOffset { get; set; }
        public uint Arm7OverlayTableSize { get; set; }
        public uint FlagsRead { get; set; }
        public uint FlagsInit { get; set; }
        public uint BannerOffset { get; set; }
        public ushort SecureCrc16 { get; set; }
        public ushort RomTimeout { get; set; }
        public uint Arm9Autoload { get; set; }
        public uint Arm7Autoload { get; set; }
        public ulong SecureAreaDisable { get; set; }
        public uint RomSize { get; set; }
        public uint HeaderSize { get; set; }
        public byte[] Reserved2 { get; set; }
        public byte[] Logo { get; set; }
        public ushort LogoCrc16 { get; set; }
        public ushort HeaderCrc16 { get; set; }
        public bool SecureCrc { get; set; }
        public bool LogoCrc { get; set; }
        public bool HeaderCrc { get; set; }
        public uint DebugRomOffset { get; set; }
        public uint DebugSize { get; set; }
        public uint DebugRamAddress { get; set; }
        public uint Reserved3 { get; set; }
        public byte[] Reserved4 { get; set; }
        public List<byte[]> GlobalMbkSetting { get; set; }
        public List<uint> Arm9MbkSetting { get; set; }
        public List<uint> Arm7MbkSetting { get; set; }
        public List<uint> Mbk9WramcntSetting { get; set; }
        public uint RegionFlags { get; set; }
        public uint AccessControl { get; set; }
        public uint ScfgExtMask { get; set; }
        public byte[] AppFlags { get; set; }
        public uint Arm9IOffset { get; set; }
        public uint Arm9IEntryAddress { get; set; }
        public uint Arm9IRamAddress { get; set; }
        public uint Arm9ISize { get; set; }
        public uint Arm7IOffset { get; set; }
        public uint Arm7IEntryAddress { get; set; }
        public uint Arm7IRamAddress { get; set; }
        public uint Arm7ISize { get; set; }
        public uint DigestNtrOffset { get; set; }
        public uint DigestNtrSize { get; set; }
        public uint DigestTwlOffset { get; set; }
        public uint DigestTwlSize { get; set; }
        public uint SectorHashtableOffset { get; set; }
        public uint SectorHashtableSize { get; set; }
        public uint BlockHashtableOffset { get; set; }
        public uint BlockHashtableSize { get; set; }
        public uint DigestSectorSize { get; set; }
        public uint DigestBlockSectorCount { get; set; }
        public uint BannerSize { get; set; }
        public uint Offset0X20C { get; set; }
        public uint TotalTwlromSize { get; set; }
        public uint Offset0X214 { get; set; }
        public uint Offset0X218 { get; set; }
        public uint Offset0X21C { get; set; }
        public uint ModcryptOffset { get; set; }
        public uint ModcryptSize { get; set; }
        public uint Modcrypt2Offset { get; set; }
        public uint Modcrypt2Size { get; set; }
        public ulong TitleId { get; set; }
        public uint PublicSaveSize { get; set; }
        public uint PrivateSaveSize { get; set; }
        public byte[] Reserved5 { get; set; }
        public byte[] AgeRatings { get; set; }
        public byte[] HmacArm9 { get; set; }
        public byte[] HmacArm7 { get; set; }
        public byte[] HmacDigestMaster { get; set; }
        public byte[] HmacTitleIcon { get; set; }
        public byte[] HmacArm9I { get; set; }
        public byte[] HmacArm7I { get; set; }
        public byte[] Reserved6 { get; set; }
        public byte[] HmacArm9NoSecure { get; set; }
        public byte[] Reserved7 { get; set; }
        public byte[] DebugArguments { get; set; }
        public byte[] RsaSignature { get; set; }

        public static void CalculateCrc(NitroHeader header) {
            var temporary = new MemoryStream(new byte[0x8000]);
            temporary.Write(Encoding.ASCII.GetBytes(header.Title));
            temporary.Write(Encoding.ASCII.GetBytes(header.GameCode));
            temporary.Write(Encoding.ASCII.GetBytes(header.MakerCode));
            temporary.Write(BitConverter.GetBytes(header.UnitCode));
            temporary.Write(BitConverter.GetBytes(header.EncryptionSeed));
            temporary.Write(BitConverter.GetBytes(header.DeviceCapacity));
            temporary.Write(new byte[7]);
            temporary.Write(BitConverter.GetBytes(header.TwlInternalFlags));
            temporary.Write(BitConverter.GetBytes(header.PermitsFlags));
            temporary.Write(BitConverter.GetBytes(header.RomVersion));
            temporary.Write(BitConverter.GetBytes(header.InternalFlags));
            temporary.Write(BitConverter.GetBytes(header.Arm9Offset));
            temporary.Write(BitConverter.GetBytes(header.Arm9EntryAddress));
            temporary.Write(BitConverter.GetBytes(header.Arm9RamAddress));
            temporary.Write(BitConverter.GetBytes(header.Arm9Size));
            temporary.Write(BitConverter.GetBytes(header.Arm7Offset));
            temporary.Write(BitConverter.GetBytes(header.Arm7EntryAddress));
            temporary.Write(BitConverter.GetBytes(header.Arm7RamAddress));
            temporary.Write(BitConverter.GetBytes(header.Arm7Size));
            temporary.Write(BitConverter.GetBytes(header.FileNameTableOffset));
            temporary.Write(BitConverter.GetBytes(header.FileNameTableSize));
            temporary.Write(BitConverter.GetBytes(header.FileAllocationTableOffset));
            temporary.Write(BitConverter.GetBytes(header.FileAllocationTableSize));
            temporary.Write(BitConverter.GetBytes(header.Arm9OverlayTableOffset));
            temporary.Write(BitConverter.GetBytes(header.Arm9OverlayTableSize));
            temporary.Write(BitConverter.GetBytes(header.FlagsRead));
            temporary.Write(BitConverter.GetBytes(header.FlagsInit));
            temporary.Write(BitConverter.GetBytes(header.BannerOffset));
            temporary.Write(BitConverter.GetBytes(header.SecureCrc16));
            temporary.Write(BitConverter.GetBytes(header.RomTimeout));
            temporary.Write(BitConverter.GetBytes(header.Arm9Autoload));
            temporary.Write(BitConverter.GetBytes(header.Arm7Autoload));
            temporary.Write(BitConverter.GetBytes(header.SecureAreaDisable));
            temporary.Write(BitConverter.GetBytes(header.RomSize));
            temporary.Write(BitConverter.GetBytes(header.HeaderSize));
            temporary.Write(header.Reserved2);
            temporary.Write(header.Logo);
            temporary.Write(BitConverter.GetBytes(header.LogoCrc16));
            temporary.Write(BitConverter.GetBytes(header.HeaderCrc16));
            temporary.Write(BitConverter.GetBytes(header.DebugRomOffset));
            temporary.Write(BitConverter.GetBytes(header.DebugSize));
            temporary.Write(BitConverter.GetBytes(header.DebugRamAddress));
            temporary.Write(BitConverter.GetBytes(header.Reserved3));
            temporary.Write(header.Reserved4);


            var calculator = new CRC16();
            header.HeaderCrc16 =
                BitConverter.ToUInt16(
                    calculator.ComputeHash(new ArraySegment<byte>(temporary.ToArray(), 0, 0x15E).Array));
            header.LogoCrc16 =
                BitConverter.ToUInt16(
                    calculator.ComputeHash(new ArraySegment<byte>(temporary.ToArray(), 0xC0, 0x9C).Array));
            header.SecureCrc16 = BitConverter.ToUInt16(calculator.ComputeHash(
                new ArraySegment<byte>(temporary.ToArray(), (int) header.Arm9Offset,
                    (int) (0x8000 - 2 * header.Arm9Offset)).Array));
            temporary.Close();
        }
    }
}