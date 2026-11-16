using System;

namespace MyCryptoProject.lr1.Task1
{
    public static class PermutationBox
    {
        public static byte[] Permute(byte[] input, int[] pBox, int baseIndex = 1, bool isMsb0 = true)
        {
            if (input == null || pBox == null)
                throw new ArgumentNullException("Входной массив и pBox не могут быть null.");

            int outputBitLength = pBox.Length;
            int outputByteLength = (int)Math.Ceiling(outputBitLength / 8.0);
            byte[] output = new byte[outputByteLength];

            for (int targetBitIdx = 0; targetBitIdx < outputBitLength; targetBitIdx++)
            {
                int rawIdx = pBox[targetBitIdx];
                int idx = rawIdx - baseIndex;

                int byteOffset = idx / 8;
                int targetByteOffset = targetBitIdx / 8;

                int bitOffset;
                int targetBitOffset;

                if (isMsb0)
                {
                    bitOffset = 7 - (idx % 8);
                    targetBitOffset = 7 - (targetBitIdx % 8);
                }
                else
                {
                    bitOffset = idx % 8;
                    targetBitOffset = targetBitIdx % 8;
                }

                byte mask = (byte)(1 << bitOffset);
                byte targetMask = (byte)(1 << targetBitOffset);

                bool bitValue = (input[byteOffset] & mask) != 0;

                if (bitValue)
                {
                    output[targetByteOffset] |= targetMask;
                }
                else
                {
                    output[targetByteOffset] &= (byte)(~targetMask);
                }
            }

            return output;
        }
    }
}
