namespace NitroSharp.Formats.ROM {
    public class Arm9 : ArmBinary {
        public Arm9(uint entryAddress, uint offset, uint size, uint ramAddress) : base(entryAddress, offset, size,
            ramAddress) {
        }
    }
}