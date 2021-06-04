using System;
using System.Collections.Generic;
using System.Text;

namespace NitroSharp.Formats.ROM
{
    public class ARM9 : ARMBinary
    {
        public ARM9(uint EntryAddress, uint Offset, uint Size, uint RAMAddress) : base(EntryAddress, Offset, Size, RAMAddress) { }
    }
}
