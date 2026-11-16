using System;
using System.Security.Cryptography;
using MyCryptoProject.lr1.Task2.Enums;

using PaddingMode = MyCryptoProject.lr1.Task2.Enums.PaddingMode;

namespace MyCryptoProject.lr1.Task2.Services
{
    public static class PaddingService
    {
        public static byte[] AddPadding(byte[] input, int blockSize, PaddingMode mode)
        {
            int paddingLength = blockSize - (input.Length % blockSize);

            if (paddingLength == 0)
                paddingLength = blockSize;

            byte[] padded = new byte[input.Length + paddingLength];

            Buffer.BlockCopy(input, 0, padded, 0, input.Length);

            switch (mode)
            {
                case PaddingMode.Zeros:
                    break;

                case PaddingMode.PKCS7:
                    for (int i = input.Length; i < padded.Length; i++)
                        padded[i] = (byte)paddingLength;
                    break;

                case PaddingMode.AnsiX923:
                    padded[padded.Length - 1] = (byte)paddingLength;
                    break;

                case PaddingMode.Iso10126:
                    byte[] randomBytes = new byte[paddingLength - 1];
                    RandomNumberGenerator.Fill(randomBytes);
                    Buffer.BlockCopy(randomBytes, 0, padded, input.Length, paddingLength - 1);
                    padded[padded.Length - 1] = (byte)paddingLength;
                    break;
            }

            return padded;
        }

        public static byte[] RemovePadding(byte[] input, int blockSize, PaddingMode mode)
        {
            if (input == null || input.Length == 0 || input.Length % blockSize != 0)
                return input;

            int paddingLength = 0;

            switch (mode)
            {
                case PaddingMode.Zeros:
                    int i = input.Length - 1;
                    while (i >= 0 && input[i] == 0)
                    {
                        paddingLength++;
                        i--;
                    }
                    break;

                case PaddingMode.PKCS7:
                case PaddingMode.AnsiX923:
                case PaddingMode.Iso10126:
                    paddingLength = input[input.Length - 1];
                    if (paddingLength < 1 || paddingLength > blockSize)
                        return input;
                    break;
            }

            int resultLength = input.Length - paddingLength;
            byte[] result = new byte[resultLength];
            Buffer.BlockCopy(input, 0, result, 0, resultLength);

            return result;
        }
    }
}
