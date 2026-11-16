using System;
using MyCryptoProject.lr1.Task2.Interfaces;
using MyCryptoProject.lr1.Task1;

namespace MyCryptoProject.lr1.Task4
{
    public class DesTransform : IBlockCipherTransform
    {
        public byte[] EncryptBlock(byte[] rightHalf, byte[] roundKey)
        {
            if (rightHalf == null || rightHalf.Length != 4)
                throw new ArgumentException("Входная половинка R должна быть 4 байта (32 бита).");

            byte[] expandedR = PermutationBox.Permute(rightHalf, DesTables.E, 1, true);

            byte[] xored = new byte[6]; // 48 бит = 6 байт
            for (int i = 0; i < 6; i++)
                xored[i] = (byte)(expandedR[i] ^ roundKey[i]);

            byte[] sboxOutput = ApplySBoxes(xored);

            return PermutationBox.Permute(sboxOutput, DesTables.P, 1, true);
        }

        public byte[] DecryptBlock(byte[] block, byte[] roundKey)
        {
            return EncryptBlock(block, roundKey);
        }

        private static byte[] ApplySBoxes(byte[] input48)
        {
            byte[] output32 = new byte[4];

            ulong bits = 0;
            for (int i = 0; i < 6; i++)
                bits = (bits << 8) | input48[i];

            uint result32 = 0;

            for (int i = 0; i < 8; i++)
            {
                int shift = 42 - (i * 6);
                int sixBits = (int)((bits >> shift) & 0x3F);

                int row = ((sixBits & 0x20) >> 4) | (sixBits & 0x01);
                int col = (sixBits >> 1) & 0x0F;

                byte sVal = DesTables.SBoxes[i, row, col];
                result32 = (result32 << 4) | sVal;
            }

            for (int i = 0; i < 4; i++)
                output32[i] = (byte)(result32 >> (24 - i * 8));

            return output32;
        }
    }
}
