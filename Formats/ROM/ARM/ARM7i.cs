namespace NitroSharp.Formats.ROM.ARM {
    public class ARM7i : ArmBinary {
        public ARM7i(uint entryAddress, uint offset, uint size, uint ramAddress) : base(entryAddress, offset, size,
            ramAddress) {
        }
    }
}