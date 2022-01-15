namespace NitroSharp.Formats.ROM {
    public class Arm7I : ArmBinary {
        public Arm7I(uint entryAddress, uint offset, uint size, uint ramAddress) : base(entryAddress, offset, size,
            ramAddress) {
        }
    }
}