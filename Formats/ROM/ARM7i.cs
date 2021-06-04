using System;
using System.Collections.Generic;
using System.Text;

namespace NitroSharp.Formats.ROM
{
    public class ARM7i : ARMBinary
    {
        public ARM7i(uint EntryAddress, uint Offset, uint Size, uint RAMAddress) : base(EntryAddress, Offset, Size, RAMAddress) { }
    }
}
