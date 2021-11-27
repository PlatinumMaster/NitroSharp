namespace NitroSharp.Formats.ROM
{
    public class ARM9i : ARMBinary
    {
        public ARM9i(uint EntryAddress, uint Offset, uint Size, uint RAMAddress) : base(EntryAddress, Offset, Size,
            RAMAddress)
        {
        }
    }
}