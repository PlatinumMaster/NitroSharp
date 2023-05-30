namespace NitroSharp.Formats.ROM.ARM {
    public class ARM7 : ArmBinary {
        public ARM7(uint entryAddress, uint offset, uint size, uint ramAddress) : base(entryAddress, offset, size,
            ramAddress) {
        }
    }
}