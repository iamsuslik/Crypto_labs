using System;
using MyCryptoProject.lr1.Task2.Interfaces;

namespace MyCryptoProject.lr1.Task3
{
    public class FeistelNetwork : ISymmetricCipher
    {
        private readonly IKeyExpansion _keyExpansion;
        private readonly IBlockCipherTransform _roundFunction;
        private readonly int _blockSizeBytes;
        private byte[][] _roundKeys;
        public int BlockSizeBytes => _blockSizeBytes;

        public FeistelNetwork(
            IKeyExpansion keyExpansion,
            IBlockCipherTransform roundFunction,
            int blockSizeBytes = 8)
        {
            _keyExpansion = keyExpansion ?? throw new ArgumentNullException(nameof(keyExpansion));
            _roundFunction = roundFunction ?? throw new ArgumentNullException(nameof(roundFunction));

            if (blockSizeBytes % 2 != 0 || blockSizeBytes <= 0)
                throw new ArgumentException("Размер блока Сети Фейстеля должен быть четным положительным числом.", nameof(blockSizeBytes));

            _blockSizeBytes = blockSizeBytes;
        }

        public void SetKey(byte[] key)
        {
            if (key == null || key.Length == 0)
                throw new ArgumentException("Ключ шифрования не может быть пустым.", nameof(key));

            _roundKeys = _keyExpansion.ExpandKey(key);

            if (_roundKeys == null || _roundKeys.Length == 0)
                throw new InvalidOperationException("Процедура расширения ключа вернула пустой набор раундовых ключей.");
        }

        public byte[] EncryptBlock(byte[] block)
        {
            ValidateBlock(block);
            return ProcessFeistel(block, isDecrypt: false);
        }

        public byte[] DecryptBlock(byte[] block)
        {
            ValidateBlock(block);
            return ProcessFeistel(block, isDecrypt: true);
        }

        #region Ядро вычислений Сети Фейстеля

        private byte[] ProcessFeistel(byte[] block, bool isDecrypt)
        {
            if (_roundKeys == null)
                throw new InvalidOperationException("Ключ не установлен.");

            int halfSize = _blockSizeBytes / 2;
            byte[] L = new byte[halfSize];
            byte[] R = new byte[halfSize];

            Buffer.BlockCopy(block, 0, L, 0, halfSize);
            Buffer.BlockCopy(block, halfSize, R, 0, halfSize);

            int totalRounds = _roundKeys.Length;

            for (int r = 0; r < totalRounds; r++)
            {
                int keyIndex = isDecrypt ? (totalRounds - 1 - r) : r;
                byte[] currentRoundKey = _roundKeys[keyIndex];

                byte[] fResult = _roundFunction.EncryptBlock(R, currentRoundKey);

                byte[] newR = new byte[halfSize];
                for (int j = 0; j < halfSize; j++)
                {
                    newR[j] = (byte)(L[j] ^ fResult[j]);
                }

                if (r < totalRounds - 1)
                {
                    L = R;
                    R = newR;
                }
                else
                {
                    L = newR;
                }
            }

            byte[] result = new byte[_blockSizeBytes];
            Buffer.BlockCopy(L, 0, result, 0, halfSize);
            Buffer.BlockCopy(R, 0, result, halfSize, halfSize);

            return result;
        }

        private void ValidateBlock(byte[] block)
        {
            if (block == null)
                throw new ArgumentNullException(nameof(block));

            if (block.Length != _blockSizeBytes)
                throw new ArgumentException($"Размер блока должен быть ровно {_blockSizeBytes} байт. Получено: {block.Length}.");
        }

        #endregion
    }
}
