namespace NitroSharp.Formats.ROM.ARM {
    public class ARM9i : ArmBinary {
        public ARM9i(uint entryAddress, uint offset, uint size, uint ramAddress) : base(entryAddress, offset, size,
            ramAddress) {
        }
    }
}