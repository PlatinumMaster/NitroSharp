namespace NitroSharp.Formats.ROM
{
    public class ARM7 : ARMBinary
    {
        public ARM7(uint EntryAddress, uint Offset, uint Size, uint RAMAddress) : base(EntryAddress, Offset, Size,
            RAMAddress)
        {
        }
    }
}