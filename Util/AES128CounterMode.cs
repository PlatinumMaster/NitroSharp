using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace NitroSharp.Util {
    public class Aes128CounterMode : SymmetricAlgorithm {
        private readonly AesManaged _aes;
        private readonly byte[] _counter;

        public Aes128CounterMode(byte[] counter) {
            if (counter == null) throw new ArgumentNullException("counter");
            if (counter.Length != 16)
                throw new ArgumentException(string.Format(
                    "Counter size must be same as block size (actual: {0}, expected: {1})",
                    counter.Length, 16));

            _aes = new AesManaged {
                Mode = CipherMode.ECB,
                Padding = PaddingMode.None
            };

            _counter = new byte[counter.Length];
            for (var i = 0; i < 16; i++) _counter[i] = counter[15 - i];
        }

        public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] ignoredParameter) {
            return new CounterModeCryptoTransform(_aes, rgbKey, _counter);
        }

        public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] ignoredParameter) {
            return new CounterModeCryptoTransform(_aes, rgbKey, _counter);
        }

        public override void GenerateKey() {
            _aes.GenerateKey();
        }

        public override void GenerateIV() {
            // IV not needed in Counter Mode
        }
    }

    public class CounterModeCryptoTransform : ICryptoTransform {
        private readonly byte[] _counter;
        private readonly ICryptoTransform _counterEncryptor;
        private readonly SymmetricAlgorithm _symmetricAlgorithm;
        private readonly Queue<byte> _xorMask = new Queue<byte>();

        public CounterModeCryptoTransform(SymmetricAlgorithm symmetricAlgorithm, byte[] key, byte[] counter) {
            if (symmetricAlgorithm == null) throw new ArgumentNullException("symmetricAlgorithm");
            if (key == null) throw new ArgumentNullException("key");
            if (counter == null) throw new ArgumentNullException("counter");
            if (counter.Length != symmetricAlgorithm.BlockSize / 8)
                throw new ArgumentException(string.Format(
                    "Counter size must be same as block size (actual: {0}, expected: {1})",
                    counter.Length, symmetricAlgorithm.BlockSize / 8));

            _symmetricAlgorithm = symmetricAlgorithm;
            _counter = counter;

            var zeroIv = new byte[_symmetricAlgorithm.BlockSize / 8];
            var keyswap = new byte[16];
            for (var i = 0; i < 16; i++) keyswap[i] = key[15 - i];
            _counterEncryptor = symmetricAlgorithm.CreateEncryptor(keyswap, zeroIv);
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) {
            var output = new byte[inputCount];
            TransformBlock(inputBuffer, inputOffset, inputCount, output, 0);
            return output;
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer,
            int outputOffset) {
            for (var i = 0; i < inputCount; i++) {
                if (needMoreXorMaskBytes()) encryptCounterThenIncrement();

                var mask = _xorMask.Dequeue();
                outputBuffer[outputOffset + i] = (byte) (inputBuffer[inputOffset + i] ^ mask);
            }

            return inputCount;
        }

        public int InputBlockSize => _symmetricAlgorithm.BlockSize / 8;
        public int OutputBlockSize => _symmetricAlgorithm.BlockSize / 8;
        public bool CanTransformMultipleBlocks => true;
        public bool CanReuseTransform => false;

        public void Dispose() {
        }

        private bool needMoreXorMaskBytes() {
            return _xorMask.Count == 0;
        }

        private void encryptCounterThenIncrement() {
            var counterModeBlock = new byte[_symmetricAlgorithm.BlockSize / 8];

            _counterEncryptor.TransformBlock(_counter, 0, _counter.Length, counterModeBlock, 0);
            incrementCounter();

            for (var i = counterModeBlock.Length - 1; i >= 0; i--) _xorMask.Enqueue(counterModeBlock[i]);
        }

        private void incrementCounter() {
            for (var i = _counter.Length - 1; i >= 0; i--)
                if (++_counter[i] != 0)
                    break;
        }
    }
}