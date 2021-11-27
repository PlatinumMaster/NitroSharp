﻿using System;

namespace NitroSharp.Util
{
    public class SecureAreaEncryptor
    {
        // ARM9 decryption check values
        private const uint MAGIC30 = 0x72636E65;
        private const uint MAGIC34 = 0x6A624F79;

        private static readonly byte[] encr_data =
        {
            0x99, 0xD5, 0x20, 0x5F, 0x57, 0x44, 0xF5, 0xB9, 0x6E, 0x19, 0xA4, 0xD9, 0x9E, 0x6A, 0x5A, 0x94,
            0xD8, 0xAE, 0xF1, 0xEB, 0x41, 0x75, 0xE2, 0x3A, 0x93, 0x82, 0xD0, 0x32, 0x33, 0xEE, 0x31, 0xD5,
            0xCC, 0x57, 0x61, 0x9A, 0x37, 0x06, 0xA2, 0x1B, 0x79, 0x39, 0x72, 0xF5, 0x55, 0xAE, 0xF6, 0xBE,
            0x5F, 0x1B, 0x69, 0xFB, 0xE5, 0x9D, 0xF1, 0xE9, 0xCE, 0x2C, 0xD9, 0xA1, 0x5E, 0x32, 0x05, 0xE6,
            0xFE, 0xD3, 0xFE, 0xCF, 0xD4, 0x62, 0x04, 0x0D, 0x8B, 0xF5, 0xEC, 0xB7, 0x2B, 0x60, 0x79, 0xBB,
            0x12, 0x95, 0x31, 0x0D, 0x6E, 0x3F, 0xDA, 0x2B, 0x88, 0x84, 0xF0, 0xF1, 0x3D, 0x12, 0x7E, 0x25,
            0x45, 0x22, 0xF1, 0xBB, 0x24, 0x06, 0x1A, 0x06, 0x11, 0xAD, 0xDF, 0x28, 0x8B, 0x64, 0x81, 0x34,
            0x2B, 0xEB, 0x33, 0x29, 0x99, 0xAA, 0xF2, 0xBD, 0x9C, 0x14, 0x95, 0x9D, 0x9F, 0xF7, 0xF5, 0x8C,
            0x72, 0x97, 0xA1, 0x29, 0x9D, 0xD1, 0x5F, 0xCF, 0x66, 0x4D, 0x07, 0x1A, 0xDE, 0xD3, 0x4A, 0x4B,
            0x85, 0xC9, 0xA7, 0xA3, 0x17, 0x95, 0x05, 0x3A, 0x3D, 0x49, 0x0A, 0xBF, 0x0A, 0x89, 0x8B, 0xA2,
            0x4A, 0x82, 0x49, 0xDD, 0x27, 0x90, 0xF1, 0x0B, 0xE9, 0xEB, 0x1C, 0x6A, 0x83, 0x76, 0x45, 0x05,
            0xBA, 0x81, 0x70, 0x61, 0x17, 0x3F, 0x4B, 0xDE, 0xAE, 0xCF, 0xAB, 0x39, 0x57, 0xF2, 0x3A, 0x56,
            0x48, 0x11, 0xAD, 0x8A, 0x40, 0xE1, 0x45, 0x3F, 0xFA, 0x9B, 0x02, 0x54, 0xCA, 0xA6, 0x93, 0xFB,
            0xEF, 0x4D, 0xFE, 0x6F, 0xA3, 0xD8, 0x87, 0x9C, 0x08, 0xBA, 0xD5, 0x48, 0x6A, 0x8D, 0x2D, 0xFD,
            0x6E, 0x15, 0xF8, 0x74, 0xBD, 0xBE, 0x52, 0x8B, 0x18, 0x22, 0x8A, 0x9E, 0xFB, 0x74, 0x37, 0x07,
            0x1B, 0x36, 0x6C, 0x4A, 0x19, 0xBA, 0x42, 0x62, 0xB9, 0x79, 0x91, 0x10, 0x7B, 0x67, 0x65, 0x96,
            0xFE, 0x02, 0x23, 0xE8, 0xEE, 0x99, 0x8C, 0x77, 0x3E, 0x5C, 0x86, 0x64, 0x4D, 0x6D, 0x78, 0x86,
            0xA5, 0x4F, 0x65, 0xE2, 0x1E, 0xB2, 0xDF, 0x5A, 0x0A, 0xD0, 0x7E, 0x08, 0x14, 0xB0, 0x71, 0xAC,
            0xBD, 0xDB, 0x83, 0x1C, 0xB9, 0xD7, 0xA1, 0x62, 0xCD, 0xC6, 0x63, 0x7C, 0x52, 0x69, 0xC3, 0xE6,
            0xBF, 0x75, 0xCE, 0x12, 0x44, 0x5D, 0x21, 0x04, 0xFA, 0xFB, 0xD3, 0x3C, 0x38, 0x11, 0x63, 0xD4,
            0x95, 0x85, 0x41, 0x49, 0x46, 0x09, 0xF2, 0x08, 0x43, 0x11, 0xDC, 0x1F, 0x76, 0xC0, 0x15, 0x6D,
            0x1F, 0x3C, 0x63, 0x70, 0xEA, 0x87, 0x80, 0x6C, 0xC3, 0xBD, 0x63, 0x8B, 0xC2, 0x37, 0x21, 0x37,
            0xDC, 0xEE, 0x09, 0x23, 0x2E, 0x37, 0x6A, 0x4D, 0x73, 0x90, 0xF7, 0x50, 0x30, 0xAC, 0x1C, 0x92,
            0x04, 0x10, 0x23, 0x91, 0x4F, 0xD2, 0x07, 0xAA, 0x68, 0x3E, 0x4F, 0x9A, 0xC9, 0x64, 0x60, 0x6A,
            0xC8, 0x14, 0x21, 0xF3, 0xD6, 0x22, 0x41, 0x12, 0x44, 0x24, 0xCF, 0xE6, 0x8A, 0x56, 0xDD, 0x0D,
            0x53, 0x4D, 0xE1, 0x85, 0x1E, 0x8C, 0x52, 0x5A, 0x9C, 0x19, 0x84, 0xC2, 0x03, 0x57, 0xF1, 0x6F,
            0xE3, 0x00, 0xBE, 0x58, 0xF6, 0x4C, 0xED, 0xD5, 0x21, 0x64, 0x9C, 0x1F, 0xBE, 0x55, 0x03, 0x3C,
            0x4A, 0xDC, 0xFF, 0xAA, 0xC9, 0xDA, 0xE0, 0x5D, 0x5E, 0xBF, 0xE6, 0xDE, 0xF5, 0xD8, 0xB1, 0xF8,
            0xFF, 0x36, 0xB3, 0xB9, 0x62, 0x67, 0x95, 0xDB, 0x31, 0x5F, 0x37, 0xED, 0x4C, 0x70, 0x67, 0x99,
            0x90, 0xB5, 0x18, 0x31, 0x6C, 0x3D, 0x99, 0x99, 0xE4, 0x42, 0xDA, 0xD3, 0x25, 0x42, 0x13, 0xA0,
            0xAE, 0xD7, 0x70, 0x6C, 0xB1, 0x55, 0xCF, 0xC7, 0xD7, 0x46, 0xD5, 0x43, 0x61, 0x17, 0x3D, 0x44,
            0x28, 0xE9, 0x33, 0x85, 0xD5, 0xD0, 0xA2, 0x93, 0xAA, 0x25, 0x12, 0x1F, 0xFB, 0xC5, 0x0B, 0x46,
            0xF5, 0x97, 0x76, 0x56, 0x45, 0xA6, 0xBE, 0x87, 0xB1, 0x94, 0x6B, 0xE8, 0xB1, 0xFE, 0x33, 0x99,
            0xAE, 0x1F, 0x3E, 0x6C, 0x39, 0x71, 0x1D, 0x09, 0x00, 0x90, 0x37, 0xE4, 0x10, 0x3E, 0x75, 0x74,
            0xFF, 0x8C, 0x83, 0x3B, 0xB0, 0xF1, 0xB0, 0xF9, 0x01, 0x05, 0x47, 0x42, 0x95, 0xF1, 0xD6, 0xAC,
            0x7E, 0x38, 0xE6, 0x9E, 0x95, 0x74, 0x26, 0x3F, 0xB4, 0x68, 0x50, 0x18, 0xD0, 0x43, 0x30, 0xB4,
            0x4C, 0x4B, 0xE3, 0x68, 0xBF, 0xE5, 0x4D, 0xB6, 0x95, 0x8B, 0x0A, 0xA0, 0x74, 0x25, 0x32, 0x77,
            0xCF, 0xA1, 0xF7, 0x2C, 0xD8, 0x71, 0x13, 0x5A, 0xAB, 0xEA, 0xC9, 0x51, 0xE8, 0x0D, 0xEE, 0xEF,
            0xE9, 0x93, 0x7E, 0x19, 0xA7, 0x1E, 0x43, 0x38, 0x81, 0x16, 0x2C, 0xA1, 0x48, 0xE3, 0x73, 0xCC,
            0x29, 0x21, 0x6C, 0xD3, 0x5D, 0xCE, 0xA0, 0xD9, 0x61, 0x71, 0x43, 0xA0, 0x15, 0x13, 0xB5, 0x64,
            0x92, 0xCF, 0x2A, 0x19, 0xDC, 0xAD, 0xB7, 0xA5, 0x9F, 0x86, 0x65, 0xF8, 0x1A, 0x9F, 0xE7, 0xFB,
            0xF7, 0xFD, 0xB8, 0x13, 0x6C, 0x27, 0xDB, 0x6F, 0xDF, 0x35, 0x1C, 0xF7, 0x8D, 0x2C, 0x5B, 0x9B,
            0x12, 0xAB, 0x38, 0x64, 0x06, 0xCC, 0xDE, 0x31, 0xE8, 0x4E, 0x75, 0x11, 0x64, 0xE3, 0xFA, 0xEA,
            0xEB, 0x34, 0x54, 0xC2, 0xAD, 0x3F, 0x34, 0xEB, 0x93, 0x2C, 0x7D, 0x26, 0x36, 0x9D, 0x56, 0xF3,
            0x5A, 0xE1, 0xF6, 0xB3, 0x98, 0x63, 0x4A, 0x9E, 0x32, 0x83, 0xE4, 0x9A, 0x84, 0x60, 0x7D, 0x90,
            0x2E, 0x13, 0x0E, 0xEE, 0x93, 0x4B, 0x36, 0xA2, 0x85, 0xEC, 0x16, 0x38, 0xE8, 0x88, 0x06, 0x02,
            0xBF, 0xF0, 0xA0, 0x3A, 0xED, 0xD7, 0x6A, 0x9A, 0x73, 0xE1, 0x57, 0xCF, 0xF8, 0x44, 0xB8, 0xDC,
            0x2E, 0x23, 0x59, 0xD1, 0xDF, 0x95, 0x52, 0x71, 0x99, 0x61, 0xA0, 0x4B, 0xD5, 0x7F, 0x6E, 0x78,
            0xBA, 0xA9, 0xC5, 0x30, 0xD3, 0x40, 0x86, 0x32, 0x9D, 0x32, 0x0C, 0x9C, 0x37, 0xB7, 0x02, 0x2F,
            0xBA, 0x54, 0x98, 0xA9, 0xC4, 0x13, 0x04, 0xC9, 0x8D, 0xBE, 0xC8, 0xE7, 0x5D, 0x97, 0x50, 0x2E,
            0x93, 0xD6, 0x22, 0x59, 0x0C, 0x27, 0xBC, 0x22, 0x92, 0xE0, 0xA7, 0x20, 0x0F, 0x93, 0x6F, 0x7F,
            0x4C, 0x9F, 0xD3, 0xB5, 0xA6, 0x2A, 0x0B, 0x74, 0x67, 0x49, 0x7D, 0x10, 0x26, 0xCB, 0xD1, 0xC5,
            0x86, 0x71, 0xE7, 0x8C, 0xA0, 0x9C, 0xE9, 0x5B, 0xB2, 0x1A, 0xF6, 0x01, 0xEE, 0x8C, 0x9E, 0x5E,
            0x83, 0xF2, 0x1A, 0xDB, 0xE6, 0xE5, 0xEA, 0x84, 0x59, 0x76, 0xD2, 0x7C, 0xF6, 0x8D, 0xA5, 0x49,
            0x36, 0x48, 0xC2, 0x16, 0x52, 0xBB, 0x83, 0xA3, 0x74, 0xB9, 0x07, 0x0C, 0x3B, 0xFF, 0x61, 0x28,
            0xE1, 0x61, 0xE9, 0xE4, 0xEF, 0x6E, 0x15, 0xAA, 0x4E, 0xBA, 0xE8, 0x5D, 0x05, 0x96, 0xBB, 0x32,
            0x56, 0xB0, 0xFB, 0x72, 0x52, 0x0F, 0x0E, 0xC8, 0x42, 0x25, 0x65, 0x76, 0x89, 0xAF, 0xF2, 0xDE,
            0x10, 0x27, 0xF0, 0x01, 0x4B, 0x74, 0xA7, 0x97, 0x07, 0xD5, 0x26, 0x54, 0x54, 0x09, 0x1F, 0x82,
            0x0A, 0x86, 0x7D, 0x30, 0x39, 0x0E, 0xB3, 0x26, 0x9B, 0x0B, 0x57, 0xBB, 0x36, 0x06, 0x31, 0xAF,
            0xFD, 0x79, 0xFC, 0xD9, 0x30, 0x10, 0x2B, 0x0C, 0xB3, 0xE1, 0x9B, 0xD7, 0x7B, 0xDC, 0x5F, 0xEF,
            0xD2, 0xF8, 0x13, 0x45, 0x4D, 0x47, 0x75, 0xBD, 0x46, 0x96, 0x3C, 0x7E, 0x75, 0xF3, 0x3E, 0xB5,
            0x67, 0xC5, 0x9A, 0x3B, 0xB0, 0x5B, 0x29, 0x6B, 0xDE, 0x80, 0x5B, 0xC8, 0x15, 0x05, 0xB1, 0x31,
            0xB6, 0xCE, 0x49, 0xDD, 0xAD, 0x84, 0xB5, 0xAE, 0x60, 0xDC, 0x67, 0x31, 0x34, 0x30, 0xFE, 0x4E,
            0xBD, 0x80, 0x2F, 0xA6, 0xBF, 0x63, 0x39, 0x21, 0x86, 0xD9, 0x35, 0x7F, 0x16, 0x68, 0x22, 0x05,
            0x54, 0xE9, 0x90, 0x26, 0x8C, 0x07, 0x6C, 0x51, 0xA4, 0x31, 0x55, 0xD7, 0x09, 0x07, 0xA8, 0x3E,
            0x2E, 0x53, 0x66, 0xC1, 0xF8, 0xF2, 0x7B, 0xC4, 0xF2, 0x58, 0xCF, 0xF1, 0x87, 0xC5, 0xA2, 0xE7,
            0x27, 0x8F, 0x30, 0x87, 0x58, 0xA0, 0x64, 0x62, 0x23, 0x18, 0xB9, 0x88, 0x7C, 0xFA, 0xCE, 0xC4,
            0x98, 0xAE, 0xAD, 0x17, 0xCC, 0x4A, 0x5B, 0xF3, 0xE9, 0x48, 0xD5, 0x56, 0xD3, 0x0D, 0xF2, 0xC8,
            0x92, 0x73, 0x8C, 0xDB, 0xD7, 0x2F, 0x56, 0xAC, 0x81, 0xF9, 0x92, 0x69, 0x4D, 0xC6, 0x32, 0xF6,
            0xE6, 0xC0, 0x8D, 0x21, 0xE2, 0x76, 0x80, 0x61, 0x11, 0xBC, 0xDC, 0x6C, 0x93, 0xAF, 0x19, 0x69,
            0x9B, 0xD0, 0xBF, 0xB9, 0x31, 0x9F, 0x02, 0x67, 0xA3, 0x51, 0xEE, 0x83, 0x06, 0x22, 0x7B, 0x0C,
            0xAB, 0x49, 0x42, 0x40, 0xB8, 0xD5, 0x01, 0x7D, 0xCE, 0x5E, 0xF7, 0x55, 0x53, 0x39, 0xC5, 0x99,
            0x46, 0xD8, 0x87, 0x9F, 0xBA, 0xF7, 0x64, 0xB4, 0xE3, 0x9A, 0xFA, 0xA1, 0x6D, 0x90, 0x68, 0x10,
            0x30, 0xCA, 0x8A, 0x54, 0xA7, 0x9F, 0x60, 0xC3, 0x19, 0xF5, 0x6B, 0x0D, 0x7A, 0x51, 0x98, 0xE6,
            0x98, 0x43, 0x51, 0xB4, 0xD6, 0x35, 0xE9, 0x4F, 0xC3, 0xDF, 0x0F, 0x7B, 0xD6, 0x2F, 0x5C, 0xBD,
            0x3A, 0x15, 0x61, 0x19, 0xF1, 0x4B, 0xCB, 0xAA, 0xDC, 0x6D, 0x64, 0xC9, 0xD3, 0xC6, 0x1E, 0x56,
            0xEF, 0x38, 0x4C, 0x50, 0x71, 0x86, 0x75, 0xCC, 0x0D, 0x0D, 0x4E, 0xE9, 0x28, 0xF6, 0x06, 0x5D,
            0x70, 0x1B, 0xAA, 0xD3, 0x45, 0xCF, 0xA8, 0x39, 0xAC, 0x95, 0xA6, 0x2E, 0xB4, 0xE4, 0x22, 0xD4,
            0x74, 0xA8, 0x37, 0x5F, 0x48, 0x7A, 0x04, 0xCC, 0xA5, 0x4C, 0x40, 0xD8, 0x28, 0xB4, 0x28, 0x08,
            0x0D, 0x1C, 0x72, 0x52, 0x41, 0xF0, 0x7D, 0x47, 0x19, 0x3A, 0x53, 0x4E, 0x58, 0x84, 0x62, 0x6B,
            0x93, 0xB5, 0x8A, 0x81, 0x21, 0x4E, 0x0D, 0xDC, 0xB4, 0x3F, 0xA2, 0xC6, 0xFC, 0xC9, 0x2B, 0x40,
            0xDA, 0x38, 0x04, 0xE9, 0x5E, 0x5A, 0x86, 0x6B, 0x0C, 0x22, 0x25, 0x85, 0x68, 0x11, 0x8D, 0x7C,
            0x92, 0x1D, 0x95, 0x55, 0x4D, 0xAB, 0x8E, 0xBB, 0xDA, 0xA6, 0xE6, 0xB7, 0x51, 0xB6, 0x32, 0x5A,
            0x05, 0x41, 0xDD, 0x05, 0x2A, 0x0A, 0x56, 0x50, 0x91, 0x17, 0x47, 0xCC, 0xC9, 0xE6, 0x7E, 0xB5,
            0x61, 0x4A, 0xDB, 0x73, 0x67, 0x51, 0xC8, 0x33, 0xF5, 0xDA, 0x6E, 0x74, 0x2E, 0x54, 0xC3, 0x37,
            0x0D, 0x6D, 0xAF, 0x08, 0xE8, 0x15, 0x8A, 0x5F, 0xE2, 0x59, 0x21, 0xCD, 0xA8, 0xDE, 0x0C, 0x06,
            0x5A, 0x77, 0x6B, 0x5F, 0xDB, 0x18, 0x65, 0x3E, 0xC8, 0x50, 0xDE, 0x78, 0xE0, 0xB8, 0x82, 0xB3,
            0x5D, 0x4E, 0x72, 0x32, 0x07, 0x4F, 0xC1, 0x34, 0x23, 0xBA, 0x96, 0xB7, 0x67, 0x4E, 0xA4, 0x28,
            0x1E, 0x34, 0x62, 0xEB, 0x2D, 0x6A, 0x70, 0xE9, 0x2F, 0x42, 0xC4, 0x70, 0x4E, 0x5A, 0x31, 0x9C,
            0xF9, 0x5B, 0x47, 0x28, 0xAA, 0xDA, 0x71, 0x6F, 0x38, 0x1F, 0xB3, 0x78, 0xC4, 0x92, 0x6B, 0x1C,
            0x9E, 0xF6, 0x35, 0x9A, 0xB7, 0x4D, 0x0E, 0xBF, 0xCC, 0x18, 0x29, 0x41, 0x03, 0x48, 0x35, 0x5D,
            0x55, 0xD0, 0x2B, 0xC6, 0x29, 0xAF, 0x5C, 0x60, 0x74, 0x69, 0x8E, 0x5E, 0x9B, 0x7C, 0xD4, 0xBD,
            0x7B, 0x44, 0x64, 0x7D, 0x3F, 0x92, 0x5D, 0x69, 0xB6, 0x1F, 0x00, 0x4B, 0xD4, 0x83, 0x35, 0xCF,
            0x7E, 0x64, 0x4E, 0x17, 0xAE, 0x8D, 0xD5, 0x2E, 0x9A, 0x28, 0x12, 0x4E, 0x2E, 0x2B, 0x49, 0x08,
            0x5C, 0xAE, 0xC6, 0x46, 0x85, 0xAE, 0x41, 0x61, 0x1E, 0x6F, 0x82, 0xD2, 0x51, 0x37, 0x16, 0x1F,
            0x0B, 0xF6, 0x59, 0xA4, 0x9A, 0xCA, 0x5A, 0xAF, 0x0D, 0xD4, 0x33, 0x8B, 0x20, 0x63, 0xF1, 0x84,
            0x80, 0x5C, 0xCB, 0xCF, 0x08, 0xB4, 0xB9, 0xD3, 0x16, 0x05, 0xBD, 0x62, 0x83, 0x31, 0x9B, 0x56,
            0x51, 0x98, 0x9F, 0xBA, 0xB2, 0x5B, 0xAA, 0xB2, 0x22, 0x6B, 0x2C, 0xB5, 0xD4, 0x48, 0xFA, 0x63,
            0x2B, 0x5F, 0x58, 0xFA, 0x61, 0xFA, 0x64, 0x09, 0xBB, 0x38, 0xE0, 0xB8, 0x9D, 0x92, 0x60, 0xA8,
            0x0D, 0x67, 0x6F, 0x0E, 0x37, 0xF5, 0x0D, 0x01, 0x9F, 0xC2, 0x77, 0xD4, 0xFE, 0xEC, 0xF1, 0x73,
            0x30, 0x39, 0xE0, 0x7D, 0xF5, 0x61, 0x98, 0xE4, 0x2C, 0x28, 0x55, 0x04, 0x56, 0x55, 0xDB, 0x2F,
            0x6B, 0xEC, 0xE5, 0x58, 0x06, 0xB6, 0x64, 0x80, 0x6A, 0x2A, 0x1A, 0x4E, 0x5B, 0x0F, 0xD8, 0xC4,
            0x0A, 0x2E, 0x52, 0x19, 0xD9, 0x62, 0xF5, 0x30, 0x48, 0xBE, 0x8C, 0x7B, 0x4F, 0x38, 0x9B, 0xA2,
            0xC3, 0xAF, 0xC9, 0xD3, 0xC7, 0xC1, 0x62, 0x41, 0x86, 0xB9, 0x61, 0x21, 0x57, 0x6F, 0x99, 0x4F,
            0xC1, 0xBA, 0xCE, 0x7B, 0xB5, 0x3B, 0x4D, 0x5E, 0x8A, 0x8B, 0x44, 0x57, 0x5F, 0x13, 0x5F, 0x70,
            0x6D, 0x5B, 0x29, 0x47, 0xDC, 0x38, 0xE2, 0xEC, 0x04, 0x55, 0x65, 0x12, 0x2A, 0xE8, 0x17, 0x43,
            0xE1, 0x8E, 0xDD, 0x2A, 0xB3, 0xE2, 0x94, 0xF7, 0x09, 0x6E, 0x5C, 0xE6, 0xEB, 0x8A, 0xF8, 0x6D,
            0x89, 0x49, 0x54, 0x48, 0xF5, 0x2F, 0xAD, 0xBF, 0xEA, 0x94, 0x4B, 0xCA, 0xFC, 0x39, 0x87, 0x82,
            0x5F, 0x8A, 0x01, 0xF2, 0x75, 0xF2, 0xE6, 0x71, 0xD6, 0xD8, 0x42, 0xDE, 0xF1, 0x2D, 0x1D, 0x28,
            0xA6, 0x88, 0x7E, 0xA3, 0xA0, 0x47, 0x1D, 0x30, 0xD9, 0xA3, 0x71, 0xDF, 0x49, 0x1C, 0xCB, 0x01,
            0xF8, 0x36, 0xB1, 0xF2, 0xF0, 0x22, 0x58, 0x5D, 0x45, 0x6B, 0xBD, 0xA0, 0xBB, 0xB2, 0x88, 0x42,
            0xC7, 0x8C, 0x28, 0xCE, 0x93, 0xE8, 0x90, 0x63, 0x08, 0x90, 0x7C, 0x89, 0x3C, 0xF5, 0x7D, 0xB7,
            0x04, 0x2D, 0x4F, 0x55, 0x51, 0x16, 0xFD, 0x7E, 0x79, 0xE8, 0xBE, 0xC1, 0xF2, 0x12, 0xD4, 0xF8,
            0xB4, 0x84, 0x05, 0x23, 0xA0, 0xCC, 0xD2, 0x2B, 0xFD, 0xE1, 0xAB, 0xAD, 0x0D, 0xD1, 0x55, 0x6C,
            0x23, 0x41, 0x94, 0x4D, 0x77, 0x37, 0x4F, 0x05, 0x28, 0x0C, 0xBF, 0x17, 0xB3, 0x12, 0x67, 0x6C,
            0x8C, 0xC3, 0x5A, 0xF7, 0x41, 0x84, 0x2A, 0x6D, 0xD0, 0x94, 0x12, 0x27, 0x2C, 0xB4, 0xED, 0x9C,
            0x4D, 0xEC, 0x47, 0x82, 0x97, 0xD5, 0x67, 0xB9, 0x1B, 0x9D, 0xC0, 0x55, 0x07, 0x7E, 0xE5, 0x8E,
            0xE2, 0xA8, 0xE7, 0x3E, 0x12, 0xE4, 0x0E, 0x3A, 0x2A, 0x45, 0x55, 0x34, 0xA2, 0xF9, 0x2D, 0x5A,
            0x1B, 0xAB, 0x52, 0x7C, 0x83, 0x10, 0x5F, 0x55, 0xD2, 0xF1, 0x5A, 0x43, 0x2B, 0xC6, 0xA7, 0xA4,
            0x89, 0x15, 0x95, 0xE8, 0xB4, 0x4B, 0x9D, 0xF8, 0x75, 0xE3, 0x9F, 0x60, 0x78, 0x5B, 0xD6, 0xE6,
            0x0D, 0x44, 0xE6, 0x21, 0x06, 0xBD, 0x47, 0x22, 0x53, 0xA4, 0x00, 0xAD, 0x8D, 0x43, 0x13, 0x85,
            0x39, 0xF7, 0xAA, 0xFC, 0x38, 0xAF, 0x7B, 0xED, 0xFC, 0xE4, 0x2B, 0x54, 0x50, 0x98, 0x4C, 0xFC,
            0x85, 0x80, 0xF7, 0xDF, 0x3C, 0x80, 0x22, 0xE1, 0x94, 0xDA, 0xDE, 0x24, 0xC6, 0xB0, 0x7A, 0x39,
            0x38, 0xDC, 0x0F, 0xA1, 0xA7, 0xF4, 0xF9, 0x6F, 0x63, 0x18, 0x57, 0x8B, 0x84, 0x41, 0x2A, 0x2E,
            0xD4, 0x53, 0xF2, 0xD9, 0x00, 0x0F, 0xD0, 0xDD, 0x99, 0x6E, 0x19, 0xA6, 0x0A, 0xD0, 0xEC, 0x5B,
            0x58, 0x24, 0xAB, 0xC0, 0xCB, 0x06, 0x65, 0xEC, 0x1A, 0x13, 0x38, 0x94, 0x0A, 0x67, 0x03, 0x2F,
            0x3F, 0xF7, 0xE3, 0x77, 0x44, 0x77, 0x33, 0xC6, 0x14, 0x39, 0xD0, 0xE3, 0xC0, 0xA2, 0x08, 0x79,
            0xBB, 0x40, 0x99, 0x57, 0x41, 0x0B, 0x01, 0x90, 0xCD, 0xE1, 0xCC, 0x48, 0x67, 0xDB, 0xB3, 0xAF,
            0x88, 0x74, 0xF3, 0x4C, 0x82, 0x8F, 0x72, 0xB1, 0xB5, 0x23, 0x29, 0xC4, 0x12, 0x6C, 0x19, 0xFC,
            0x8E, 0x46, 0xA4, 0x9C, 0xC4, 0x25, 0x65, 0x87, 0xD3, 0x6D, 0xBE, 0x8A, 0x93, 0x11, 0x03, 0x38,
            0xED, 0x83, 0x2B, 0xF3, 0x46, 0xA4, 0x93, 0xEA, 0x3B, 0x53, 0x85, 0x1D, 0xCE, 0xD4, 0xF1, 0x08,
            0x83, 0x27, 0xED, 0xFC, 0x9B, 0x1A, 0x18, 0xBC, 0xF9, 0x8B, 0xAE, 0xDC, 0x24, 0xAB, 0x50, 0x38,
            0xE9, 0x72, 0x4B, 0x10, 0x22, 0x17, 0x7B, 0x46, 0x5D, 0xAB, 0x59, 0x64, 0xF3, 0x40, 0xAE, 0xF8,
            0xBB, 0xE5, 0xC8, 0xF9, 0x26, 0x03, 0x4E, 0x55, 0x7D, 0xEB, 0xEB, 0xFE, 0xF7, 0x39, 0xE6, 0xE0,
            0x0A, 0x11, 0xBE, 0x2E, 0x28, 0xFF, 0x98, 0xED, 0xC0, 0xC9, 0x42, 0x56, 0x42, 0xC3, 0xFD, 0x00,
            0xF6, 0xAF, 0x87, 0xA2, 0x5B, 0x01, 0x3F, 0x32, 0x92, 0x47, 0x95, 0x9A, 0x72, 0xA5, 0x32, 0x3D,
            0xAE, 0x6B, 0xD0, 0x9B, 0x07, 0xD2, 0x49, 0x92, 0xE3, 0x78, 0x4A, 0xFA, 0xA1, 0x06, 0x7D, 0xF2,
            0x41, 0xCF, 0x77, 0x74, 0x04, 0x14, 0xB2, 0x0C, 0x86, 0x84, 0x64, 0x16, 0xD5, 0xBB, 0x51, 0xA1,
            0xE5, 0x6F, 0xF1, 0xD1, 0xF2, 0xE2, 0xF7, 0x5F, 0x58, 0x20, 0x4D, 0xB8, 0x57, 0xC7, 0xCF, 0xDD,
            0xC5, 0xD8, 0xBE, 0x76, 0x3D, 0xF6, 0x5F, 0x7E, 0xE7, 0x2A, 0x8B, 0x88, 0x24, 0x1B, 0x38, 0x3F,
            0x0E, 0x41, 0x23, 0x77, 0xF5, 0xF0, 0x4B, 0xD4, 0x0C, 0x1F, 0xFA, 0xA4, 0x0B, 0x80, 0x5F, 0xCF,
            0x45, 0xF6, 0xE0, 0xDA, 0x2F, 0x34, 0x59, 0x53, 0xFB, 0x20, 0x3C, 0x52, 0x62, 0x5E, 0x35, 0xB5,
            0x62, 0xFE, 0x8B, 0x60, 0x63, 0xE3, 0x86, 0x5A, 0x15, 0x1A, 0x6E, 0xD1, 0x47, 0x45, 0xBC, 0x32,
            0xB4, 0xEB, 0x67, 0x38, 0xAB, 0xE4, 0x6E, 0x33, 0x3A, 0xB5, 0xED, 0xA3, 0xAD, 0x67, 0xE0, 0x4E,
            0x41, 0x95, 0xEE, 0x62, 0x62, 0x71, 0x26, 0x1D, 0x31, 0xEF, 0x62, 0x30, 0xAF, 0xD7, 0x82, 0xAC,
            0xC2, 0xDC, 0x05, 0x04, 0xF5, 0x97, 0x07, 0xBF, 0x11, 0x59, 0x23, 0x07, 0xC0, 0x64, 0x02, 0xE8,
            0x97, 0xE5, 0x3E, 0xAF, 0x18, 0xAC, 0x59, 0xA6, 0x8B, 0x4A, 0x33, 0x90, 0x1C, 0x6E, 0x7C, 0x9C,
            0x20, 0x7E, 0x4C, 0x3C, 0x3E, 0x61, 0x64, 0xBB, 0xC5, 0x6B, 0x7C, 0x7E, 0x3E, 0x9F, 0xC5, 0x4C,
            0x9F, 0xEA, 0x73, 0xF5, 0xD7, 0x89, 0xC0, 0x4C, 0xF4, 0xFB, 0xF4, 0x2D, 0xEC, 0x14, 0x1B, 0x51,
            0xD5, 0xC1, 0x12, 0xC8, 0x10, 0xDF, 0x0B, 0x4A, 0x8B, 0x9C, 0xBC, 0x93, 0x45, 0x6A, 0x3E, 0x3E,
            0x7D, 0xC1, 0xA9, 0xBA, 0xCD, 0xC1, 0xB4, 0x07, 0xE4, 0xE1, 0x68, 0x86, 0x43, 0xB2, 0x6D, 0x38,
            0xF3, 0xFB, 0x0C, 0x5C, 0x66, 0x37, 0x71, 0xDE, 0x56, 0xEF, 0x6E, 0xA0, 0x10, 0x40, 0x65, 0xA7,
            0x98, 0xF7, 0xD0, 0xBE, 0x0E, 0xC8, 0x37, 0x36, 0xEC, 0x10, 0xCA, 0x7C, 0x9C, 0xAB, 0x84, 0x1E,
            0x05, 0x17, 0x76, 0x02, 0x1C, 0x4F, 0x52, 0xAA, 0x5F, 0xC1, 0xC6, 0xA0, 0x56, 0xB9, 0xD8, 0x04,
            0x84, 0x44, 0x4D, 0xA7, 0x59, 0xD8, 0xDE, 0x60, 0xE6, 0x38, 0x0E, 0x05, 0x8F, 0x03, 0xE1, 0x3B,
            0x6D, 0x81, 0x04, 0x33, 0x6F, 0x30, 0x0B, 0xCE, 0x69, 0x05, 0x21, 0x33, 0xFB, 0x26, 0xBB, 0x89,
            0x7D, 0xB6, 0xAE, 0x87, 0x7E, 0x51, 0x07, 0xE0, 0xAC, 0xF7, 0x96, 0x0A, 0x6B, 0xF9, 0xC4, 0x5C,
            0x1D, 0xE4, 0x44, 0x47, 0xB8, 0x5E, 0xFA, 0xE3, 0x78, 0x84, 0x55, 0x42, 0x4B, 0x48, 0x5E, 0xF7,
            0x7D, 0x47, 0x35, 0x86, 0x1D, 0x2B, 0x43, 0x05, 0x03, 0xEC, 0x8A, 0xB8, 0x1E, 0x06, 0x3C, 0x76,
            0x0C, 0x48, 0x1A, 0x43, 0xA7, 0xB7, 0x8A, 0xED, 0x1E, 0x13, 0xC6, 0x43, 0xEE, 0x10, 0xEF, 0xDB,
            0xEC, 0xFB, 0x3C, 0x83, 0xB2, 0x95, 0x44, 0xEF, 0xD8, 0x54, 0x51, 0x4E, 0x2D, 0x11, 0x44, 0x1D,
            0xFB, 0x36, 0x59, 0x1E, 0x7A, 0x34, 0xC1, 0xC3, 0xCA, 0x57, 0x00, 0x61, 0xEA, 0x67, 0xA5, 0x16,
            0x9B, 0x55, 0xD0, 0x55, 0xE1, 0x7F, 0xD9, 0x36, 0xD2, 0x40, 0x76, 0xAE, 0xDC, 0x01, 0xCE, 0xB0,
            0x7A, 0x83, 0xD5, 0xCB, 0x20, 0x98, 0xEC, 0x6B, 0xC1, 0x72, 0x92, 0x34, 0xF3, 0x82, 0x57, 0x37,
            0x62, 0x8A, 0x32, 0x36, 0x0C, 0x90, 0x43, 0xAE, 0xAE, 0x5C, 0x9B, 0x78, 0x8E, 0x13, 0x65, 0x02,
            0xFD, 0x68, 0x71, 0xC1, 0xFE, 0xB0, 0x31, 0xA0, 0x24, 0x82, 0xB0, 0xC3, 0xB1, 0x79, 0x69, 0xA7,
            0xF5, 0xD2, 0xEB, 0xD0, 0x82, 0xC0, 0x32, 0xDC, 0x9E, 0xC7, 0x26, 0x3C, 0x6D, 0x8D, 0x98, 0xC1,
            0xBB, 0x22, 0xD4, 0xD0, 0x0F, 0x33, 0xEC, 0x3E, 0xB9, 0xCC, 0xE1, 0xDC, 0x6A, 0x4C, 0x77, 0x36,
            0x14, 0x1C, 0xF9, 0xBF, 0x81, 0x9F, 0x28, 0x5F, 0x71, 0x85, 0x32, 0x29, 0x90, 0x75, 0x48, 0xC4,
            0xB3, 0x4A, 0xCE, 0xD8, 0x44, 0x8F, 0x14, 0x2F, 0xFD, 0x40, 0x57, 0xEF, 0xAA, 0x08, 0x75, 0xD9,
            0x46, 0xD1, 0xD6, 0x6E, 0x32, 0x55, 0x1F, 0xC3, 0x18, 0xFE, 0x84, 0x1F, 0xFC, 0x84, 0xD5, 0xFF,
            0x71, 0x5E, 0x1B, 0x48, 0xC3, 0x86, 0x95, 0x0E, 0x28, 0x08, 0x27, 0xD3, 0x38, 0x83, 0x71, 0x7B,
            0x4C, 0x80, 0x63, 0x54, 0x9A, 0x56, 0xB0, 0xAC, 0xCF, 0x80, 0xCA, 0x31, 0x09, 0xEF, 0xFE, 0xF3,
            0xBE, 0xAF, 0x24, 0x7E, 0xA6, 0xFE, 0x53, 0x3F, 0xC2, 0x8D, 0x4A, 0x33, 0x68, 0xD1, 0x22, 0xA6,
            0x66, 0xAD, 0x7B, 0xEA, 0xDE, 0xB6, 0x43, 0xB0, 0xA1, 0x25, 0x95, 0x00, 0xA3, 0x3F, 0x75, 0x46,
            0x14, 0x11, 0x44, 0xEC, 0xD7, 0x95, 0xBC, 0x92, 0xF0, 0x4F, 0xA9, 0x16, 0x53, 0x62, 0x97, 0x60,
            0x2A, 0x0F, 0x41, 0xF1, 0x71, 0x24, 0xBE, 0xEE, 0x94, 0x7F, 0x08, 0xCD, 0x60, 0x93, 0xB3, 0x85,
            0x5B, 0x07, 0x00, 0x3F, 0xD8, 0x0F, 0x28, 0x83, 0x9A, 0xD1, 0x69, 0x9F, 0xD1, 0xDA, 0x2E, 0xC3,
            0x90, 0x01, 0xA2, 0xB9, 0x6B, 0x4E, 0x2A, 0x66, 0x9D, 0xDA, 0xAE, 0xA6, 0xEA, 0x2A, 0xD3, 0x68,
            0x2F, 0x0C, 0x0C, 0x9C, 0xD2, 0x8C, 0x4A, 0xED, 0xE2, 0x9E, 0x57, 0x65, 0x9D, 0x09, 0x87, 0xA3,
            0xB4, 0xC4, 0x32, 0x5D, 0xC9, 0xD4, 0x32, 0x2B, 0xB1, 0xE0, 0x71, 0x1E, 0x64, 0x4D, 0xE6, 0x90,
            0x71, 0xE3, 0x1E, 0x40, 0xED, 0x7D, 0xF3, 0x84, 0x0E, 0xED, 0xC8, 0x78, 0x76, 0xAE, 0xC0, 0x71,
            0x27, 0x72, 0xBB, 0x05, 0xEA, 0x02, 0x64, 0xFB, 0xF3, 0x48, 0x6B, 0xB5, 0x42, 0x93, 0x3F, 0xED,
            0x9F, 0x13, 0x53, 0xD2, 0xF7, 0xFE, 0x2A, 0xEC, 0x1D, 0x47, 0x25, 0xDB, 0x3C, 0x91, 0x86, 0xC6,
            0x8E, 0xF0, 0x11, 0xFD, 0x23, 0x74, 0x36, 0xF7, 0xA4, 0xF5, 0x9E, 0x7A, 0x7E, 0x53, 0x50, 0x44,
            0xD4, 0x47, 0xCA, 0xD3, 0xEB, 0x38, 0x6D, 0xE6, 0xD9, 0x71, 0x94, 0x7F, 0x4A, 0xC6, 0x69, 0x4B,
            0x11, 0xF4, 0x52, 0xEA, 0x22, 0xFE, 0x8A, 0xB0, 0x36, 0x67, 0x8B, 0x59, 0xE8, 0xE6, 0x80, 0x2A,
            0xEB, 0x65, 0x04, 0x13, 0xEE, 0xEC, 0xDC, 0x9E, 0x5F, 0xB1, 0xEC, 0x05, 0x6A, 0x59, 0xE6, 0x9F,
            0x5E, 0x59, 0x6B, 0x89, 0xBF, 0xF7, 0x1A, 0xCA, 0x44, 0xF9, 0x5B, 0x6A, 0x71, 0x85, 0x03, 0xE4,
            0x29, 0x62, 0xE0, 0x70, 0x6F, 0x41, 0xC4, 0xCF, 0xB2, 0xB1, 0xCC, 0xE3, 0x7E, 0xA6, 0x07, 0xA8,
            0x87, 0xE7, 0x7F, 0x84, 0x93, 0xDB, 0x52, 0x4B, 0x6C, 0xEC, 0x7E, 0xDD, 0xD4, 0x24, 0x48, 0x10,
            0x69, 0x9F, 0x04, 0x60, 0x74, 0xE6, 0x48, 0x18, 0xF3, 0xE4, 0x2C, 0xB9, 0x4F, 0x2E, 0x50, 0x7A,
            0xDF, 0xD4, 0x54, 0x69, 0x2B, 0x8B, 0xA7, 0xF3, 0xCE, 0xFF, 0x1F, 0xF3, 0x3E, 0x26, 0x01, 0x39,
            0x17, 0x95, 0x84, 0x89, 0xB0, 0xF0, 0x4C, 0x4B, 0x82, 0x91, 0x9F, 0xC4, 0x4B, 0xAC, 0x9D, 0xA5,
            0x74, 0xAF, 0x17, 0x25, 0xC9, 0xCA, 0x32, 0xD3, 0xBC, 0x89, 0x8A, 0x84, 0x89, 0xCC, 0x0D, 0xAE,
            0x7C, 0xA2, 0xDB, 0x9C, 0x6A, 0x78, 0x91, 0xEE, 0xEA, 0x76, 0x5D, 0x4E, 0x87, 0x60, 0xF5, 0x69,
            0x15, 0x67, 0xD4, 0x02, 0xCF, 0xAF, 0x48, 0x36, 0x07, 0xEA, 0xBF, 0x6F, 0x66, 0x2D, 0x06, 0x8F,
            0xC4, 0x9A, 0xFE, 0xF9, 0xF6, 0x90, 0x87, 0x75, 0xB8, 0xF7, 0xAD, 0x0F, 0x76, 0x10, 0x5A, 0x3D,
            0x59, 0xB0, 0x2E, 0xB3, 0xC7, 0x35, 0x2C, 0xCC, 0x70, 0x56, 0x2B, 0xCB, 0xE3, 0x37, 0x96, 0xC5,
            0x2F, 0x46, 0x1B, 0x8A, 0x22, 0x46, 0xC7, 0x88, 0xA7, 0x26, 0x32, 0x98, 0x61, 0xDF, 0x86, 0x22,
            0x8A, 0xF4, 0x1C, 0x2F, 0x87, 0xA1, 0x09, 0xAA, 0xCC, 0xA9, 0xAE, 0xD3, 0xBD, 0x00, 0x45, 0x1C,
            0x9A, 0x54, 0x87, 0x86, 0x52, 0x87, 0xEF, 0xFF, 0x1E, 0x8F, 0xA1, 0x8F, 0xC1, 0x89, 0x5C, 0x35,
            0x1B, 0xDA, 0x2D, 0x3A, 0x2C, 0x16, 0xB2, 0xC2, 0xF1, 0x56, 0xE2, 0x78, 0xC1, 0x6B, 0x63, 0x97,
            0xC5, 0x56, 0x8F, 0xC9, 0x32, 0x7F, 0x2C, 0xAA, 0xAF, 0xA6, 0xA8, 0xAC, 0x20, 0x91, 0x22, 0x88,
            0xDE, 0xE4, 0x60, 0x8B, 0xF9, 0x4B, 0x42, 0x25, 0x1A, 0xE3, 0x7F, 0x9C, 0x2C, 0x19, 0x89, 0x3A,
            0x7E, 0x05, 0xD4, 0x36, 0xCC, 0x69, 0x58, 0xC2, 0xC1, 0x32, 0x8B, 0x2F, 0x90, 0x85, 0xEB, 0x7A,
            0x39, 0x50, 0xA5, 0xA1, 0x27, 0x92, 0xC5, 0x66, 0xB0, 0x20, 0x4F, 0x58, 0x7E, 0x55, 0x83, 0x43,
            0x2B, 0x45, 0xE2, 0x9C, 0xE4, 0xD8, 0x12, 0x90, 0x2C, 0x16, 0x83, 0x56, 0x16, 0x79, 0x03, 0xB3,
            0xAD, 0x2D, 0x61, 0x18, 0x1A, 0x13, 0x1F, 0x37, 0xE2, 0xE1, 0x9C, 0x73, 0x7B, 0x80, 0xD5, 0xFD,
            0x2D, 0x51, 0x87, 0xFC, 0x7B, 0xAA, 0xD7, 0x1F, 0x2C, 0x7A, 0x8E, 0xAF, 0xF4, 0x8D, 0xBB, 0xCD,
            0x95, 0x11, 0x7C, 0x72, 0x0B, 0xEE, 0x6F, 0xE2, 0xB9, 0xAF, 0xDE, 0x37, 0x83, 0xDE, 0x8C, 0x8D,
            0x62, 0x05, 0x67, 0xB7, 0x96, 0xC6, 0x8D, 0x56, 0xB6, 0x0D, 0xD7, 0x62, 0xBA, 0xD6, 0x46, 0x36,
            0xBD, 0x8E, 0xC8, 0xE6, 0xEA, 0x2A, 0x6C, 0x10, 0x14, 0xFF, 0x6B, 0x5B, 0xFA, 0x82, 0x3C, 0x46,
            0xB1, 0x30, 0x43, 0x46, 0x51, 0x8A, 0x7D, 0x9B, 0x92, 0x3E, 0x83, 0x79, 0x5B, 0x55, 0x5D, 0xB2,
            0x6C, 0x5E, 0xCE, 0x90, 0x62, 0x8E, 0x53, 0x98, 0xC9, 0x0D, 0x6D, 0xE5, 0x2D, 0x57, 0xCD, 0xC5,
            0x81, 0x57, 0xBA, 0xE1, 0xE8, 0xB8, 0x8F, 0x72, 0xE5, 0x4F, 0x13, 0xDC, 0xEA, 0x9D, 0x71, 0x15,
            0x10, 0xB2, 0x11, 0x88, 0xD5, 0x09, 0xD4, 0x7F, 0x5B, 0x65, 0x7F, 0x2C, 0x3B, 0x38, 0x4C, 0x11,
            0x68, 0x50, 0x8D, 0xFB, 0x9E, 0xB0, 0x59, 0xBF, 0x94, 0x80, 0x89, 0x4A, 0xC5, 0x1A, 0x18, 0x12,
            0x89, 0x53, 0xD1, 0x4A, 0x10, 0x29, 0xE8, 0x8C, 0x1C, 0xEC, 0xB6, 0xEA, 0x46, 0xC7, 0x17, 0x8B,
            0x25, 0x15, 0x31, 0xA8, 0xA2, 0x6B, 0x43, 0xB1, 0x9D, 0xE2, 0xDB, 0x0B, 0x87, 0x9B, 0xB0, 0x11,
            0x04, 0x0E, 0x71, 0xD2, 0x29, 0x77, 0x89, 0x82, 0x0A, 0x66, 0x41, 0x7F, 0x1D, 0x0B, 0x48, 0xFF,
            0x72, 0xBB, 0x24, 0xFD, 0xC2, 0x48, 0xA1, 0x9B, 0xFE, 0x7B, 0x7F, 0xCE, 0x88, 0xDB, 0x86, 0xD9,
            0x85, 0x3B, 0x1C, 0xB0, 0xDC, 0xA8, 0x33, 0x07, 0xBF, 0x51, 0x2E, 0xE3, 0x0E, 0x9A, 0x00, 0x97,
            0x1E, 0x06, 0xC0, 0x97, 0x43, 0x9D, 0xD8, 0xB6, 0x45, 0xC4, 0x86, 0x67, 0x5F, 0x00, 0xF8, 0x88,
            0x9A, 0xA4, 0x52, 0x9E, 0xC7, 0xAA, 0x8A, 0x83, 0x75, 0xEC, 0xC5, 0x18, 0xAE, 0xCE, 0xC3, 0x2F,
            0x1A, 0x2B, 0xF9, 0x18, 0xFF, 0xAE, 0x1A, 0xF5, 0x53, 0x0B, 0xB5, 0x33, 0x51, 0xA7, 0xFD, 0xE8,
            0xA8, 0xE1, 0xA2, 0x64, 0xB6, 0x22, 0x17, 0x43, 0x80, 0xCC, 0x0A, 0xD8, 0xAE, 0x3B, 0xBA, 0x40,
            0xD7, 0xD9, 0x92, 0x4A, 0x89, 0xDF, 0x04, 0x10, 0xEE, 0x9B, 0x18, 0x2B, 0x6A, 0x77, 0x69, 0x8A,
            0x68, 0xF4, 0xF9, 0xB9, 0xA2, 0x21, 0x15, 0x6E, 0xE6, 0x1E, 0x3B, 0x03, 0x62, 0x30, 0x9B, 0x60,
            0x41, 0x7E, 0x25, 0x9B, 0x9E, 0x8F, 0xC5, 0x52, 0x10, 0x08, 0xF8, 0xC2, 0x69, 0xA1, 0x21, 0x11,
            0x88, 0x37, 0x5E, 0x79, 0x35, 0x66, 0xFF, 0x10, 0x42, 0x18, 0x6E, 0xED, 0x97, 0xB6, 0x6B, 0x1C,
            0x4E, 0x36, 0xE5, 0x6D, 0x7D, 0xB4, 0xE4, 0xBF, 0x20, 0xB9, 0xE0, 0x05, 0x3A, 0x69, 0xD5, 0xB8,
            0xE3, 0xD5, 0xDC, 0xE0, 0xB9, 0xAC, 0x53, 0x3E, 0x07, 0xA4, 0x57, 0xAD, 0x77, 0xFF, 0x48, 0x18,
            0x76, 0x2A, 0xAC, 0x49, 0x2A, 0x8E, 0x47, 0x75, 0x6D, 0x9F, 0x67, 0x63, 0x30, 0x35, 0x8C, 0x39,
            0x05, 0x39, 0xD5, 0x6F, 0x64, 0x3A, 0x5B, 0xAD, 0xCA, 0x0B, 0xBB, 0x82, 0x52, 0x99, 0x45, 0xB1,
            0x93, 0x36, 0x36, 0x99, 0xAF, 0x13, 0x20, 0x44, 0x36, 0xD8, 0x02, 0x44, 0x09, 0x39, 0x92, 0x85,
            0xFF, 0x4A, 0x4A, 0x97, 0x87, 0xA6, 0x63, 0xD7, 0xC7, 0xB5, 0xB5, 0x24, 0xED, 0x0F, 0xB4, 0x6F,
            0x0C, 0x58, 0x52, 0x14, 0xD9, 0xA6, 0x7B, 0xD3, 0x79, 0xBC, 0x38, 0x58, 0xA1, 0xBD, 0x3B, 0x84,
            0x06, 0xD8, 0x1A, 0x06, 0xFD, 0x6B, 0xA8, 0xEA, 0x4B, 0x69, 0x28, 0x04, 0x37, 0xAD, 0x82, 0x99,
            0xFB, 0x0E, 0x1B, 0x85, 0xBD, 0xA8, 0x5D, 0x73, 0xCD, 0xDC, 0x58, 0x75, 0x0A, 0xBE, 0x63, 0x6C,
            0x48, 0xE7, 0x4C, 0xE4, 0x30, 0x2B, 0x04, 0x60, 0xB9, 0x15, 0xD8, 0xDA, 0x86, 0x81, 0x75, 0x8F,
            0x96, 0xD4, 0x8D, 0x1C, 0x5D, 0x70, 0x85, 0x7C, 0x1C, 0x67, 0x7B, 0xD5, 0x08, 0x67, 0xA6, 0xCE,
            0x4B, 0x0A, 0x66, 0x70, 0xB7, 0xE5, 0x63, 0xD4, 0x5B, 0x8A, 0x82, 0xEA, 0x10, 0x67, 0xCA, 0xE2,
            0xF4, 0xEF, 0x17, 0x85, 0x2F, 0x2A, 0x5F, 0x8A, 0x97, 0x82, 0xF8, 0x6A, 0xD6, 0x34, 0x10, 0xEA,
            0xEB, 0xC9, 0x5C, 0x3C, 0xE1, 0x49, 0xF8, 0x46, 0xEB, 0xDE, 0xBD, 0xF6, 0xA9, 0x92, 0xF1, 0xAA,
            0xA6, 0xA0, 0x18, 0xB0, 0x3A, 0xD3, 0x0F, 0x1F, 0xF3, 0x6F, 0xFF, 0x31, 0x45, 0x43, 0x44, 0xD3,
            0x50, 0x9A, 0xF7, 0x88, 0x09, 0x96, 0xC1, 0xCE, 0x76, 0xCC, 0xF2, 0x2C, 0x2C, 0xBA, 0xAD, 0x82,
            0x77, 0x8F, 0x18, 0x84, 0xC0, 0xD2, 0x07, 0x9C, 0x36, 0x90, 0x83, 0x4E, 0x0B, 0xA5, 0x4F, 0x43,
            0x3E, 0x04, 0xAB, 0x78, 0x4F, 0xD6, 0xFB, 0x09, 0x01, 0x24, 0x90, 0xDA, 0x6F, 0x3C, 0x3A, 0x61,
            0x0D, 0x7F, 0x69, 0x4A, 0xEB, 0x2B, 0x30, 0x02, 0xB4, 0xDB, 0xE0, 0x84, 0xA9, 0xEC, 0xD7, 0x35,
            0xBF, 0x37, 0x7D, 0x85, 0x58, 0xCE, 0xA9, 0x4E, 0xE4, 0x80, 0xC7, 0xA8, 0xD3, 0x30, 0x67, 0x48,
            0xEB, 0x29, 0xAF, 0x2F, 0x74, 0x6A, 0xB4, 0xA7, 0x3F, 0x0F, 0x3F, 0x92, 0xAF, 0xF3, 0xCA, 0xAC,
            0xAF, 0x4B, 0xD9, 0x94, 0xC0, 0x43, 0xCA, 0x81, 0x0D, 0x2F, 0x48, 0xA1, 0xB0, 0x27, 0xD5, 0xD2,
            0xEF, 0x4B, 0x05, 0x85, 0xA3, 0xDE, 0x4D, 0x93, 0x30, 0x3C, 0xF0, 0xBB, 0x4A, 0x8F, 0x30, 0x27,
            0x4C, 0xEB, 0xE3, 0x3E, 0x64, 0xED, 0x9A, 0x2F, 0x3B, 0xF1, 0x82, 0xF0, 0xBA, 0xF4, 0xCF, 0x7F,
            0x40, 0xCB, 0xB0, 0xE1, 0x7F, 0xBC, 0xAA, 0x57, 0xD3, 0xC9, 0x74, 0xF2, 0xFA, 0x43, 0x0D, 0x22,
            0xD0, 0xF4, 0x77, 0x4E, 0x93, 0xD7, 0x85, 0x70, 0x1F, 0x99, 0xBF, 0xB6, 0xDE, 0x35, 0xF1, 0x30,
            0xA7, 0x5E, 0x71, 0xF0, 0x6B, 0x01, 0x2D, 0x7B, 0x64, 0xF0, 0x33, 0x53, 0x0A, 0x39, 0x88, 0xF3,
            0x6B, 0x3A, 0xA6, 0x6B, 0x35, 0xD2, 0x2F, 0x43, 0xCD, 0x02, 0xFD, 0xB5, 0xE9, 0xBC, 0x5B, 0xAA,
            0xD8, 0xA4, 0x19, 0x7E, 0x0E, 0x5D, 0x94, 0x81, 0x9E, 0x6F, 0x77, 0xAD, 0xD6, 0x0E, 0x74, 0x93,
            0x96, 0xE7, 0xC4, 0x18, 0x5F, 0xAD, 0xF5, 0x19
        };

        private readonly uint[] arg2 = new uint[3];
        private readonly uint[] card_hash = new uint[0x412];
        private uint global3_rand1;

        private uint global3_rand3;

        //int cardheader_devicetype = 0;
        private uint global3_x00, global3_x04; // RTC value

        private uint Lookup(uint[] magic, uint v)
        {
            var a = (v >> 24) & 0xFF;
            var b = (v >> 16) & 0xFF;
            var c = (v >> 8) & 0xFF;
            var d = (v >> 0) & 0xFF;

            a = magic[a + 18 + 0];
            b = magic[b + 18 + 256];
            c = magic[c + 18 + 512];
            d = magic[d + 18 + 768];

            return d + (c ^ (b + a));
        }

        private void Encrypt(uint[] magic, ref uint arg1, ref uint arg2)
        {
            uint a, b, c;
            a = arg1;
            b = arg2;
            for (var i = 0; i < 16; i++)
            {
                c = magic[i] ^ a;
                a = b ^ Lookup(magic, c);
                b = c;
            }

            arg2 = a ^ magic[16];
            arg1 = b ^ magic[17];
        }

        private void Decrypt(uint[] magic, ref uint arg1, ref uint arg2)
        {
            uint a, b, c;
            a = arg1;
            b = arg2;
            for (var i = 17; i > 1; i--)
            {
                c = magic[i] ^ a;
                a = b ^ Lookup(magic, c);
                b = c;
            }

            arg1 = b ^ magic[0];
            arg2 = a ^ magic[1];
        }

        private void Encrypt(uint[] magic, ref ulong cmd)
        {
            var arg1 = (uint) (cmd >> 32);
            var arg2 = (uint) (cmd >> 0);
            Encrypt(magic, ref arg1, ref arg2);
            cmd = ((ulong) arg1 << 32) + arg2;
        }

        private void Decrypt(uint[] magic, ref ulong cmd)
        {
            var arg1 = (uint) (cmd >> 32);
            var arg2 = (uint) (cmd >> 0);
            Decrypt(magic, ref arg1, ref arg2);
            cmd = ((ulong) arg1 << 32) + arg2;
        }

        private void UpdateHashtable(uint[] magic, byte[] arg1)
        {
            for (var j = 0; j < 18; j++)
            {
                uint r3 = 0;
                for (var i = 0; i < 4; i++)
                {
                    r3 <<= 8;
                    r3 |= arg1[(j * 4 + i) & 7];
                }

                magic[j] ^= r3;
            }

            uint tmp1 = 0;
            uint tmp2 = 0;
            for (var i = 0; i < 18; i += 2)
            {
                Encrypt(magic, ref tmp1, ref tmp2);
                magic[i + 0] = tmp1;
                magic[i + 1] = tmp2;
            }

            for (var i = 0; i < 0x400; i += 2)
            {
                Encrypt(magic, ref tmp1, ref tmp2);
                magic[i + 18 + 0] = tmp1;
                magic[i + 18 + 1] = tmp2;
            }
        }

        private void Init2(uint[] magic, uint[] a /*[3]*/)
        {
            Encrypt(magic, ref a[2], ref a[1]);
            Encrypt(magic, ref a[1], ref a[0]);
            var ab = new byte[8];
            Array.Copy(BitConverter.GetBytes(a[0]), 0, ab, 0, 4);
            Array.Copy(BitConverter.GetBytes(a[1]), 0, ab, 4, 4);
            UpdateHashtable(magic, ab);
        }

        private void Init1(uint cardheader_gamecode)
        {
            for (var i = 0; i < 1024 + 18; i++) card_hash[i] = BitConverter.ToUInt32(encr_data, 4 * i);
            arg2[0] = cardheader_gamecode;
            arg2[1] = cardheader_gamecode >> 1;
            arg2[2] = cardheader_gamecode << 1;
            Init2(card_hash, arg2);
            Init2(card_hash, arg2);
        }

        private void Init0(uint cardheader_gamecode)
        {
            Init1(cardheader_gamecode);
            Encrypt(card_hash, ref global3_x04, ref global3_x00);
            global3_rand1 = global3_x00 ^ global3_x04; // more RTC
            global3_rand3 = global3_x04 ^ 0x0380FEB2;
            Encrypt(card_hash, ref global3_rand3, ref global3_rand1);
        }


        public static void DecryptSecureArea(uint cardheader_gamecode, byte[] data)
        {
            var dec = new SecureAreaEncryptor();
            dec.DecryptArm9(cardheader_gamecode, data);
        }

        public static void EncryptSecureArea(uint cardheader_gamecode, byte[] data)
        {
            var enc = new SecureAreaEncryptor();
            enc.EncryptArm9(cardheader_gamecode, data);
        }

        public void DecryptArm9(uint cardheader_gamecode, byte[] data)
        {
            var p = new uint[data.Length / 4];
            for (var i = 0; i < p.Length; i++) p[i] = BitConverter.ToUInt32(data, 4 * i);

            Init1(cardheader_gamecode);
            Decrypt(card_hash, ref p[1], ref p[0]);
            arg2[1] <<= 1;
            arg2[2] >>= 1;
            Init2(card_hash, arg2);
            Decrypt(card_hash, ref p[1], ref p[0]);

            if (p[0] != MAGIC30 || p[1] != MAGIC34)
                // "Decryption failed!"
                return;

            var pOffset = 0;
            p[pOffset++] = 0xE7FFDEFF;
            p[pOffset++] = 0xE7FFDEFF;
            uint size = 0x800 - 8;
            while (size > 0)
            {
                Decrypt(card_hash, ref p[pOffset + 1], ref p[pOffset]);
                pOffset += 2;
                size -= 8;
            }

            for (var i = 0; i < p.Length; i++)
            for (var j = 0; j < 4; j++)
                data[4 * i + j] = (byte) (p[i] >> (8 * j));
        }

        public void EncryptArm9(uint cardheader_gamecode, byte[] data)
        {
            var p = new uint[0x800 / 4];
            for (var i = 0; i < p.Length; i++) p[i] = BitConverter.ToUInt32(data, 4 * i);
            if (p[0] != 0xE7FFDEFF || p[1] != 0xE7FFDEFF)
                // "Encryption failed!"
                return;

            var pOffset = 2;

            Init1(cardheader_gamecode);

            arg2[1] <<= 1;
            arg2[2] >>= 1;

            Init2(card_hash, arg2);

            uint size = 0x800 - 8;
            while (size > 0)
            {
                Encrypt(card_hash, ref p[pOffset + 1], ref p[pOffset]);
                pOffset += 2;
                size -= 8;
            }

            // place header
            p[0] = MAGIC30;
            p[1] = MAGIC34;
            Encrypt(card_hash, ref p[1], ref p[0]);
            Init1(cardheader_gamecode);
            Encrypt(card_hash, ref p[1], ref p[0]);

            for (var i = 0; i < p.Length; i++)
            for (var j = 0; j < 4; j++)
                data[4 * i + j] = (byte) (p[i] >> (8 * j));
        }
    }
}