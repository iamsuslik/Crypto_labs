using System;
using MyCryptoProject.lr1.Task2.Interfaces;
using MyCryptoProject.lr1.Task1;

namespace MyCryptoProject.lr1.Task4
{
    public class DesKeyExpansion : IKeyExpansion
    {
        public byte[][] ExpandKey(byte[] masterKey)
        {
            if (masterKey == null || masterKey.Length != 8)
                throw new ArgumentException("Мастер-ключ DES должен быть ровно 8 байт (64 бита).");

            byte[] key56 = PermutationBox.Permute(masterKey, DesTables.PC1, 1, true);

            uint c = Extract28Bits(key56, 0);
            uint d = Extract28Bits(key56, 28);

            byte[][] roundKeys = new byte[16][];

            for (int r = 0; r < 16; r++)
            {
                int shift = DesTables.KeyShifts[r];
                c = RotateLeft28(c, shift);
                d = RotateLeft28(d, shift);

                byte[] cd = Combine28Bits(c, d);

                roundKeys[r] = PermutationBox.Permute(cd, DesTables.PC2, 1, true);
            }

            return roundKeys;
        }

        #region Битовая арифметика для 28 бит

        private static uint Extract28Bits(byte[] source, int bitOffset)
        {
            uint value = 0;
            for (int i = 0; i < 28; i++)
            {
                int bitIndex = bitOffset + i;
                int byteIdx = bitIndex / 8;
                int bitInByte = 7 - (bitIndex % 8);
                uint bit = (uint)((source[byteIdx] >> bitInByte) & 1);
                value = (value << 1) | bit;
            }
            return value;
        }

        private static uint RotateLeft28(uint val, int shift)
        {
            return ((val << shift) | (val >> (28 - shift))) & 0x0FFFFFFF;
        }

        private static byte[] Combine28Bits(uint c, uint d)
        {
            byte[] result = new byte[7]; // 56 бит = 7 байт
            ulong combined = ((ulong)c << 28) | (ulong)d;
            for (int i = 0; i < 7; i++)
            {
                result[i] = (byte)(combined >> (48 - i * 8));
            }
            return result;
        }

        #endregion
    }
}
