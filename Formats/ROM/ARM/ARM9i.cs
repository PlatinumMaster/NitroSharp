namespace NitroSharp.Formats.ROM {
    public class Arm9I : ArmBinary {
        public Arm9I(uint entryAddress, uint offset, uint size, uint ramAddress) : base(entryAddress, offset, size,
            ramAddress) {
        }
    }
}