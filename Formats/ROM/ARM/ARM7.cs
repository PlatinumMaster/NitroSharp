namespace NitroSharp.Formats.ROM {
    public class Arm7 : ArmBinary {
        public Arm7(uint entryAddress, uint offset, uint size, uint ramAddress) : base(entryAddress, offset, size,
            ramAddress) {
        }
    }
}