namespace NitroSharp.Formats.ROM.ARM {
    public class ARM9 : ArmBinary {
        public ARM9(uint entryAddress, uint offset, uint size, uint ramAddress) : base(entryAddress, offset, size,
            ramAddress) {
        }
    }
}