using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Nito.HashAlgorithms;

namespace NitroSharp.Formats.ROM {
    public class NitroHeader {
        public NitroHeader(BinaryReader binary) {
            binary.BaseStream.Position = 0x0;

            title = new string(binary.ReadChars(12));
            gameCode = new string(binary.ReadChars(4));
            makerCode = new string(binary.ReadChars(2));
            unitCode = binary.ReadByte();
            encryptionSeed = binary.ReadByte();
            deviceCapacity = binary.ReadByte();
            reserved = binary.ReadBytes(7);
            twlInternalFlags = binary.ReadByte();
            permitsFlags = binary.ReadByte();
            romVersion = binary.ReadByte();
            internalFlags = binary.ReadByte();
            arm9Offset = binary.ReadUInt32();
            arm9EntryAddress = binary.ReadUInt32();
            arm9RamAddress = binary.ReadUInt32();
            arm9Size = binary.ReadUInt32();
            arm7Offset = binary.ReadUInt32();
            arm7EntryAddress = binary.ReadUInt32();
            arm7RamAddress = binary.ReadUInt32();
            arm7Size = binary.ReadUInt32();
            fileNameTableOffset = binary.ReadUInt32();
            fileNameTableSize = binary.ReadUInt32();
            fileAllocationTableOffset = binary.ReadUInt32();
            fileAllocationTableSize = binary.ReadUInt32();
            arm9OverlayTableOffset = binary.ReadUInt32();
            arm9OverlayTableSize = binary.ReadUInt32();
            arm7OverlayTableOffset = binary.ReadUInt32();
            arm7OverlayTableSize = binary.ReadUInt32();
            flagsRead = binary.ReadUInt32();
            flagsInit = binary.ReadUInt32();
            bannerOffset = binary.ReadUInt32();
            secureCrc16 = binary.ReadUInt16();
            romTimeout = binary.ReadUInt16();
            arm9Autoload = binary.ReadUInt32();
            arm7Autoload = binary.ReadUInt32();
            secureAreaDisable = binary.ReadUInt64();
            romSize = binary.ReadUInt32();
            headerSize = binary.ReadUInt32();
            reserved2 = binary.ReadBytes(56);
            logo = binary.ReadBytes(0x9C);
            logoCrc16 = binary.ReadUInt16();
            headerCrc16 = binary.ReadUInt16();
            debugRomOffset = binary.ReadUInt32();
            debugSize = binary.ReadUInt32();
            debugRamAddress = binary.ReadUInt32();
            reserved3 = binary.ReadUInt32();
            reserved4 = binary.ReadBytes(0x10);

            if (headerSize > 0x200 && (unitCode & 2) > 0) {
                globalMbkSetting = new List<byte[]>();
                arm9MbkSetting = new List<uint>();
                arm7MbkSetting = new List<uint>();
                mbk9WramcntSetting = new List<uint>();

                // DSi-Enhanced Data
                for (var i = 0; i < 5; i++)
                    globalMbkSetting.Add(binary.ReadBytes(0x4));
                for (var i = 0; i < 3; i++)
                    arm9MbkSetting.Add(binary.ReadUInt32());
                for (var i = 0; i < 3; i++)
                    arm7MbkSetting.Add(binary.ReadUInt32());

                mbk9WramcntSetting.Add(binary.ReadUInt32());

                regionFlags = binary.ReadUInt32();
                accessControl = binary.ReadUInt32();
                scfgExtMask = binary.ReadUInt32();
                appFlags = binary.ReadBytes(4);

                arm9IOffset = binary.ReadUInt32();
                arm9IEntryAddress = binary.ReadUInt32();
                arm9IRamAddress = binary.ReadUInt32();
                arm9ISize = binary.ReadUInt32();

                arm7IOffset = binary.ReadUInt32();
                arm7IEntryAddress = binary.ReadUInt32();
                arm7IRamAddress = binary.ReadUInt32();
                arm7ISize = binary.ReadUInt32();

                digestNtrOffset = binary.ReadUInt32();
                digestNtrSize = binary.ReadUInt32();
                digestTwlOffset = binary.ReadUInt32();
                digestTwlSize = binary.ReadUInt32();

                sectorHashtableOffset = binary.ReadUInt32();
                sectorHashtableSize = binary.ReadUInt32();
                blockHashtableOffset = binary.ReadUInt32();
                blockHashtableSize = binary.ReadUInt32();

                digestSectorSize = binary.ReadUInt32();
                digestBlockSectorCount = binary.ReadUInt32();
                bannerSize = binary.ReadUInt32();
                offset0X20C = binary.ReadUInt32();

                totalTwlromSize = binary.ReadUInt32();
                offset0X214 = binary.ReadUInt32();
                offset0X218 = binary.ReadUInt32();
                offset0X21C = binary.ReadUInt32();

                modcryptOffset = binary.ReadUInt32();
                modcryptSize = binary.ReadUInt32();
                modcrypt2Offset = binary.ReadUInt32();
                modcrypt2Size = binary.ReadUInt32();

                titleId = binary.ReadUInt64();
                publicSaveSize = binary.ReadUInt32();
                privateSaveSize = binary.ReadUInt32();

                reserved5 = binary.ReadBytes(0xB0);
                ageRatings = binary.ReadBytes(0x10);
                hmacArm9 = binary.ReadBytes(20);
                hmacArm7 = binary.ReadBytes(20);
                hmacDigestMaster = binary.ReadBytes(20);
                hmacTitleIcon = binary.ReadBytes(20);
                hmacArm9I = binary.ReadBytes(20);
                hmacArm7I = binary.ReadBytes(20);
                reserved6 = binary.ReadBytes(40);
                hmacArm9NoSecure = binary.ReadBytes(20);
                reserved7 = binary.ReadBytes(0xA4C);
                debugArguments = binary.ReadBytes(0x180);
                rsaSignature = binary.ReadBytes(0x80);
            }
        }

        public string title { get; set; }
        public string gameCode { get; set; }
        public string makerCode { get; set; }
        public byte unitCode { get; set; }
        public byte encryptionSeed { get; set; }
        public byte deviceCapacity { get; }
        public byte[] reserved { get; }
        public byte twlInternalFlags { get; set; }
        public byte permitsFlags { get; set; }
        public byte romVersion { get; set; }
        public byte internalFlags { get; set; }
        public uint arm9Offset { get; set; }
        public uint arm9EntryAddress { get; set; }
        public uint arm9RamAddress { get; set; }
        public uint arm9Size { get; set; }
        public uint arm7Offset { get; set; }
        public uint arm7EntryAddress { get; set; }
        public uint arm7RamAddress { get; set; }
        public uint arm7Size { get; set; }
        public uint fileNameTableOffset { get; set; }
        public uint fileNameTableSize { get; set; }
        public uint fileAllocationTableOffset { get; set; }
        public uint fileAllocationTableSize { get; set; }
        public uint arm9OverlayTableOffset { get; set; }
        public uint arm9OverlayTableSize { get; set; }
        public uint arm7OverlayTableOffset { get; set; }
        public uint arm7OverlayTableSize { get; set; }
        public uint flagsRead { get; set; }
        public uint flagsInit { get; set; }
        public uint bannerOffset { get; set; }
        public ushort secureCrc16 { get; set; }
        public ushort romTimeout { get; set; }
        public uint arm9Autoload { get; set; }
        public uint arm7Autoload { get; set; }
        public ulong secureAreaDisable { get; set; }
        public uint romSize { get; set; }
        public uint headerSize { get; set; }
        public byte[] reserved2 { get; set; }
        public byte[] logo { get; set; }
        public ushort logoCrc16 { get; set; }
        public ushort headerCrc16 { get; set; }
        public bool secureCrc { get; set; }
        public bool logoCrc { get; set; }
        public bool headerCrc { get; set; }
        public uint debugRomOffset { get; set; }
        public uint debugSize { get; set; }
        public uint debugRamAddress { get; set; }
        public uint reserved3 { get; set; }
        public byte[] reserved4 { get; set; }
        public List<byte[]> globalMbkSetting { get; set; }
        public List<uint> arm9MbkSetting { get; set; }
        public List<uint> arm7MbkSetting { get; set; }
        public List<uint> mbk9WramcntSetting { get; set; }
        public uint regionFlags { get; set; }
        public uint accessControl { get; set; }
        public uint scfgExtMask { get; set; }
        public byte[] appFlags { get; set; }
        public uint arm9IOffset { get; set; }
        public uint arm9IEntryAddress { get; set; }
        public uint arm9IRamAddress { get; set; }
        public uint arm9ISize { get; set; }
        public uint arm7IOffset { get; set; }
        public uint arm7IEntryAddress { get; set; }
        public uint arm7IRamAddress { get; set; }
        public uint arm7ISize { get; set; }
        public uint digestNtrOffset { get; set; }
        public uint digestNtrSize { get; set; }
        public uint digestTwlOffset { get; set; }
        public uint digestTwlSize { get; set; }
        public uint sectorHashtableOffset { get; set; }
        public uint sectorHashtableSize { get; set; }
        public uint blockHashtableOffset { get; set; }
        public uint blockHashtableSize { get; set; }
        public uint digestSectorSize { get; set; }
        public uint digestBlockSectorCount { get; set; }
        public uint bannerSize { get; set; }
        public uint offset0X20C { get; set; }
        public uint totalTwlromSize { get; set; }
        public uint offset0X214 { get; set; }
        public uint offset0X218 { get; set; }
        public uint offset0X21C { get; set; }
        public uint modcryptOffset { get; set; }
        public uint modcryptSize { get; set; }
        public uint modcrypt2Offset { get; set; }
        public uint modcrypt2Size { get; set; }
        public ulong titleId { get; set; }
        public uint publicSaveSize { get; set; }
        public uint privateSaveSize { get; set; }
        public byte[] reserved5 { get; set; }
        public byte[] ageRatings { get; set; }
        public byte[] hmacArm9 { get; set; }
        public byte[] hmacArm7 { get; set; }
        public byte[] hmacDigestMaster { get; set; }
        public byte[] hmacTitleIcon { get; set; }
        public byte[] hmacArm9I { get; set; }
        public byte[] hmacArm7I { get; set; }
        public byte[] reserved6 { get; set; }
        public byte[] hmacArm9NoSecure { get; set; }
        public byte[] reserved7 { get; set; }
        public byte[] debugArguments { get; set; }
        public byte[] rsaSignature { get; set; }

        public static void calculateCrc(NitroHeader header) {
            var temporary = new MemoryStream(new byte[0x8000]);
            temporary.Write(Encoding.ASCII.GetBytes(header.title));
            temporary.Write(Encoding.ASCII.GetBytes(header.gameCode));
            temporary.Write(Encoding.ASCII.GetBytes(header.makerCode));
            temporary.Write(BitConverter.GetBytes(header.unitCode));
            temporary.Write(BitConverter.GetBytes(header.encryptionSeed));
            temporary.Write(BitConverter.GetBytes(header.deviceCapacity));
            temporary.Write(new byte[7]);
            temporary.Write(BitConverter.GetBytes(header.twlInternalFlags));
            temporary.Write(BitConverter.GetBytes(header.permitsFlags));
            temporary.Write(BitConverter.GetBytes(header.romVersion));
            temporary.Write(BitConverter.GetBytes(header.internalFlags));
            temporary.Write(BitConverter.GetBytes(header.arm9Offset));
            temporary.Write(BitConverter.GetBytes(header.arm9EntryAddress));
            temporary.Write(BitConverter.GetBytes(header.arm9RamAddress));
            temporary.Write(BitConverter.GetBytes(header.arm9Size));
            temporary.Write(BitConverter.GetBytes(header.arm7Offset));
            temporary.Write(BitConverter.GetBytes(header.arm7EntryAddress));
            temporary.Write(BitConverter.GetBytes(header.arm7RamAddress));
            temporary.Write(BitConverter.GetBytes(header.arm7Size));
            temporary.Write(BitConverter.GetBytes(header.fileNameTableOffset));
            temporary.Write(BitConverter.GetBytes(header.fileNameTableSize));
            temporary.Write(BitConverter.GetBytes(header.fileAllocationTableOffset));
            temporary.Write(BitConverter.GetBytes(header.fileAllocationTableSize));
            temporary.Write(BitConverter.GetBytes(header.arm9OverlayTableOffset));
            temporary.Write(BitConverter.GetBytes(header.arm9OverlayTableSize));
            temporary.Write(BitConverter.GetBytes(header.flagsRead));
            temporary.Write(BitConverter.GetBytes(header.flagsInit));
            temporary.Write(BitConverter.GetBytes(header.bannerOffset));
            temporary.Write(BitConverter.GetBytes(header.secureCrc16));
            temporary.Write(BitConverter.GetBytes(header.romTimeout));
            temporary.Write(BitConverter.GetBytes(header.arm9Autoload));
            temporary.Write(BitConverter.GetBytes(header.arm7Autoload));
            temporary.Write(BitConverter.GetBytes(header.secureAreaDisable));
            temporary.Write(BitConverter.GetBytes(header.romSize));
            temporary.Write(BitConverter.GetBytes(header.headerSize));
            temporary.Write(header.reserved2);
            temporary.Write(header.logo);
            temporary.Write(BitConverter.GetBytes(header.logoCrc16));
            temporary.Write(BitConverter.GetBytes(header.headerCrc16));
            temporary.Write(BitConverter.GetBytes(header.debugRomOffset));
            temporary.Write(BitConverter.GetBytes(header.debugSize));
            temporary.Write(BitConverter.GetBytes(header.debugRamAddress));
            temporary.Write(BitConverter.GetBytes(header.reserved3));
            temporary.Write(header.reserved4);


            var calculator = new CRC16();
            header.headerCrc16 =
                BitConverter.ToUInt16(
                    calculator.ComputeHash(new ArraySegment<byte>(temporary.ToArray(), 0, 0x15E).Array));
            header.logoCrc16 =
                BitConverter.ToUInt16(
                    calculator.ComputeHash(new ArraySegment<byte>(temporary.ToArray(), 0xC0, 0x9C).Array));
            header.secureCrc16 = BitConverter.ToUInt16(calculator.ComputeHash(
                new ArraySegment<byte>(temporary.ToArray(), (int) header.arm9Offset,
                    (int) (0x8000 - 2 * header.arm9Offset)).Array));
            temporary.Close();
        }
    }
}