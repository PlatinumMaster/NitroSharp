

/*
namespace NitroSharp.Formats.ROM.TWL
{
    public class TwilightHeader
    {
        public static void UpdateHeader(NitroHeader Header, BinaryWriter Binary)
        {
            // Write DSi ARM sections and padding
            Binary.BaseStream.Position = Header.SectorHashtableOffset + Header.SectorHashtableSize;
            Binary.BaseStream.Position = Header.BlockHashtableOffset + Header.BlockHashtableSize;

            if (this.Header2Data != null && !Header.trimmedRom)
            {
                Binary.BaseStream.Position = Header.digest_twl_start - 0x3000;
                for (int j = 0; j < 3; j++) 
                    Binary.Write(this.Header2Data[j]);
            }

            Binary.Write(this.DSi9Data, 0, this.DSi9Data.Length);
            while (Binary.BaseStream.Position < Header.dsi7_rom_offset) Binary.Write((byte)0xFF);
            Binary.Write(this.DSi7Data, 0, this.DSi7Data.Length);
            while (Binary.BaseStream.Position < Header.total_rom_size) Binary.Write((byte)0xFF);
            long pos = Binary.BaseStream.Position;

            // Compute NTR Secure Area Hashtable
            int i = 0;
            HMACSHA1 hmac = new HMACSHA1(TWL.hmac_sha1_key);
            BinaryReader br = new BinaryReader(Binary.BaseStream);
            br.BaseStream.Position = Header.digest_ntr_start;
            byte[] saData = br.ReadBytes(0x4000);
            uint gameCode = BitConverter.ToUInt32(Encoding.ASCII.GetBytes(Header.gameCode), 0);
            SAEncryptor.EncryptSecureArea(gameCode, saData);
            while (i < 0x4000 / Header.digest_sector_size)
            {
                byte[] hash = hmac.ComputeHash(saData, (int)(i * Header.digest_sector_size), (int)Header.digest_sector_size);
                Binary.BaseStream.Position = Header.sector_hashtable_start + i * 0x14;
                Binary.Write(hash);
                i++;
            }

            // Compute NTR Hashtable
            br.BaseStream.Position = Header.digest_ntr_start + 0x4000;
            while (br.BaseStream.Position < Header.digest_ntr_start + Header.digest_ntr_size)
            {
                byte[] hash = hmac.ComputeHash(br.ReadBytes((int)Header.digest_sector_size));
                long tmp = br.BaseStream.Position;
                Binary.BaseStream.Position = Header.sector_hashtable_start + i * 0x14;
                Binary.Write(hash);
                br.BaseStream.Position = tmp;
                i++;
            }

            // Compute TWL Hashtable
            br.BaseStream.Position = Header.digest_twl_start;
            while (br.BaseStream.Position < Header.digest_twl_start + Header.digest_twl_size)
            {
                byte[] hash = hmac.ComputeHash(br.ReadBytes((int)Header.digest_sector_size));
                long tmp = br.BaseStream.Position;
                Binary.BaseStream.Position = Header.sector_hashtable_start + i * 0x14;
                Binary.Write(hash);
                br.BaseStream.Position = tmp;
                i++;
            }

            // Compute Secondary Hashtable
            i = 0;
            br.BaseStream.Position = Header.sector_hashtable_start;
            while (br.BaseStream.Position < Header.sector_hashtable_start + Header.sector_hashtable_size)
            {
                byte[] hash = hmac.ComputeHash(br.ReadBytes((int)Header.digest_block_sectorcount * 0x14));
                long tmp = br.BaseStream.Position;
                Binary.BaseStream.Position = Header.block_hashtable_start + i * 0x14;
                Binary.Write(hash);
                br.BaseStream.Position = tmp;
                i++;
            }

            // Compute Master Hashtable
            br.BaseStream.Position = Header.block_hashtable_start;
            digest_master_hash = hmac.ComputeHash(br.ReadBytes((int)Header.block_hashtable_size));

            // Encrypt DSi sections
            if (this.twlEncrypted)
            {
                byte[] key = AES128KeyGenerate(Header);
                byte[] counter9 = new byte[16];
                byte[] counter7 = new byte[16];
                Array.Copy(Header.hmac_arm9, 0, counter9, 0, 16);
                Array.Copy(Header.hmac_arm7, 0, counter7, 0, 16);
                Binary.BaseStream.Position = Header.dsi9_rom_offset;
                if (Header.modcrypt1_size > 0) Binary.Write(AES128CTRCrypt(key, counter9, this.DSi9Data, 0, Header.modcrypt1_size));
                Binary.BaseStream.Position = Header.dsi7_rom_offset;
                if (Header.modcrypt2_size > 0) Binary.Write(AES128CTRCrypt(key, counter7, this.DSi7Data, 0, Header.modcrypt2_size));
            }

            Binary.BaseStream.Position = pos;
        }
        public static void UpdateHeaderSignatures(BinaryWriter Binary, NitroHeader Header, byte[] SignedHeaderData)
        {
            long Position = Binary.BaseStream.Position;
            byte[] RSASignatureMask =
            {
                0x00, 0x01, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0xCC, 0xCC, 0xCC, 0xCC,
                0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC
            };
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(0x400);
            RSAParameters RSAKey = new RSAParameters();
            RSAKey.Exponent = new byte[] { 0x1, 0x0, 0x1 };
            RSAKey.Modulus = new byte[]{
                0x95, 0x6F, 0x79, 0x0D, 0xF0, 0x8B, 0xB8, 0x5A, 0x76, 0xAA, 0xEF, 0xA2, 0x7F, 0xE8, 0x74, 0x75,
                0x8B, 0xED, 0x9E, 0xDF, 0x9E, 0x9A, 0x67, 0x0C, 0xD8, 0x18, 0xBE, 0xB9, 0xB2, 0x88, 0x52, 0x03,
                0xB3, 0xFA, 0x11, 0xAE, 0xAA, 0x18, 0x65, 0x13, 0xB5, 0xD6, 0xBB, 0x85, 0xA3, 0x84, 0xD0, 0xD0,
                0xEF, 0xB3, 0x66, 0xCB, 0xC6, 0x05, 0x1A, 0xAA, 0x86, 0x82, 0x7A, 0xB7, 0x43, 0x11, 0xF5, 0x9C,
                0x9B, 0xFC, 0x6C, 0x70, 0x79, 0xD5, 0xF1, 0x7B, 0xD0, 0x81, 0x9F, 0x52, 0x20, 0x56, 0x73, 0x8C,
                0x72, 0x1F, 0x40, 0xCF, 0x23, 0x61, 0x93, 0x25, 0x90, 0xA3, 0xC5, 0xDC, 0x94, 0xCF, 0xD1, 0x7A,
                0x8C, 0xBC, 0x95, 0x4A, 0x91, 0x8A, 0xA8, 0x58, 0xF4, 0xD8, 0x04, 0xBA, 0xF7, 0xD3, 0xC1, 0xC4,
                0xD7, 0xB8, 0xF0, 0x77, 0x01, 0x2F, 0xA1, 0x70, 0x26, 0x0B, 0x2C, 0x04, 0x90, 0x56, 0xF3, 0xA5
            };
            RSAKey.D = null; // In future: here set Private key
            RSA.ImportParameters(RSAKey);

            // Update digest master hash
            Binary.BaseStream.Position = 0x328;
            Binary.Write(Header.HMAC_DigestMaster);
            Array.Copy(Header.HMAC_DigestMaster, 0, SignedHeaderData, 0x328, 0x14);
            if (!RSA.VerifyData(SignedHeaderData, new SHA1CryptoServiceProvider(), Header.RSASignature))
            {
                if (RSAKey.D != null)
                    Header.RSASignature = RSA.SignData(SignedHeaderData, new SHA1CryptoServiceProvider());
                else
                {
                    // Set unencrypted signature for no$gba compatible
                    Header.RSASignature = RSASignatureMask;
                    Array.Copy(new SHA1CryptoServiceProvider().ComputeHash(SignedHeaderData), 0, Header.RSASignature, 0x80 - 0x14, 0x14);
                }
                // Write signature
                Binary.BaseStream.Position = 0xF80;
                Binary.Write(Header.RSASignature, 0, 0x80);
            }
            Binary.BaseStream.Position = Position;
        }

        private static byte[] AES128CTRCrypt(byte[] key, byte[] counter, byte[] data, uint offset, uint size) => new AES128CounterMode(counter).CreateEncryptor(key, null).TransformFinalBlock(data, (int)offset, (int)size);

        private static byte[] AES128KeyGenerate(NitroHeader Header)
        {
            if ((Header.TWLInternalFlags & 4) > 0 || (Header.AppFlags[3] & 0x80) > 0)
            {
                byte[] key = new byte[16];
                Array.Copy(Encoding.ASCII.GetBytes(Header.Title), 0, key, 0, 12);
                Array.Copy(Encoding.ASCII.GetBytes(Header.GameCode), 0, key, 12, 4);
                return key;
            }

            byte[] keyX = new byte[16];
            byte[] keyY = new byte[16];
            Array.Copy(Encoding.ASCII.GetBytes("Nintendo"), 0, keyX, 0, 8);
            Array.Copy(Encoding.ASCII.GetBytes(Header.GameCode), 0, keyX, 8, 4);
            for (int j = 0; j < 4; j++) keyX[12 + j] = (byte)Header.GameCode[3 - j];
            //Array.Copy(BitConverter.GetBytes(hdr.tid_low), 0, keyX, 12, 4);
            Array.Copy(Header.HMAC_ARM9i, 0, keyY, 0, 16);
            return AES128KeyGenerate(keyX, keyY);
        }

        private static byte[] AES128KeyGenerate(byte[] KeyX, byte[] KeyY)
        {
            // Key = ((Key_X XOR Key_Y) + FFFEFB4E295902582A680F5F1A4F3E79h) ROL 42
            byte[] Key = new byte[16];
            byte[] ModcryptCommonKey =
            {
                0x79, 0x3E, 0x4F, 0x1A, 0x5F, 0x0F, 0x68, 0x2A, 0x58, 0x02, 0x59, 0x29, 0x4E,
                0xFB, 0xFE, 0xFF
            };
            for (int i = 0; i < 16; i++) 
                Key[i] = (byte)(KeyX[i] ^ KeyY[i]);

            ulong[] tmp = new ulong[2];
            ulong[] xyKey = { BitConverter.ToUInt64(Key, 0), BitConverter.ToUInt64(Key, 8) };
            ulong[] cKey = { BitConverter.ToUInt64(ModcryptCommonKey, 0), BitConverter.ToUInt64(ModcryptCommonKey, 8) };
            tmp[0] = (cKey[0] >> 1) + (xyKey[0] >> 1) + (cKey[0] & xyKey[0] & 1);
            tmp[0] >>= 63;
            cKey[0] += xyKey[0];
            cKey[1] += xyKey[1] + tmp[0];
            tmp = new[]{cKey[0] << 42, cKey[1] << 42};
            tmp[0] |= cKey[1] >> 0x16;
            tmp[1] |= cKey[0] >> 0x16;
            cKey = tmp;
            Array.Copy(BitConverter.GetBytes(cKey[0]), 0, Key, 0, 8);
            Array.Copy(BitConverter.GetBytes(cKey[1]), 0, Key, 8, 8);
            return Key;
        }
    }

    // public static void EncryptAndWriteDSiSections(byte[] ARM9i, byte[] ARM7i)
    // {
    //     byte[] key = AES128KeyGenerate(hdr);
    //     byte[] counter9 = new byte[16];
    //     byte[] counter7 = new byte[16];
    //     Array.Copy(hdr.hmac_arm9, 0, counter9, 0, 16);
    //     Array.Copy(hdr.hmac_arm7, 0, counter7, 0, 16);
    //     bw.BaseStream.Position = hdr.dsi9_rom_offset;
    //     if (hdr.modcrypt1_size > 0) 
    //         bw.Write(AES128CTRCrypt(key, counter9, this.DSi9Data, 0, hdr.modcrypt1_size));
    //     bw.BaseStream.Position = hdr.dsi7_rom_offset;
    //     if (hdr.modcrypt2_size > 0) 
    //         bw.Write(AES128CTRCrypt(key, counter7, this.DSi7Data, 0, hdr.modcrypt2_size));
    // }
    

}
*/