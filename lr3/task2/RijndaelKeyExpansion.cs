using System;
using MyCryptoProject.lr1.Task2.Interfaces;


namespace MyCryptoProject.lr3.Task2
{
    public class RijndaelKeyExpansion : IKeyExpansion
    {
        private readonly byte _irreduciblePolynomial;
        private readonly byte[] _sBox;

        private static readonly byte[] Rcon = {
            0x00, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80, 0x1B, 0x36, 0x6C, 0xD8, 0xAB, 0x4D
        };

        public RijndaelKeyExpansion(byte irreduciblePolynomial, byte[] sBox)
        {
            _irreduciblePolynomial = irreduciblePolynomial;
            _sBox = sBox;
        }

        public byte[][] ExpandKey(byte[] masterKey)
        {
            int keyLen = masterKey.Length;
            if (keyLen != 16 && keyLen != 24 && keyLen != 32)
                throw new ArgumentException("Длина ключа должна быть 128, 192 или 256 бит.");

            int nk = keyLen / 4;

            int nr = nk + 6; 
            int totalWords = 4 * (nr + 1);

            uint[] words = new uint[totalWords];

            for (int i = 0; i < nk; i++)
            {
                words[i] = (uint)(masterKey[4 * i] << 24 |
                                  masterKey[4 * i + 1] << 16 |
                                  masterKey[4 * i + 2] << 8 |
                                  masterKey[4 * i + 3]);
            }

            for (int i = nk; i < totalWords; i++)
            {
                uint temp = words[i - 1];

                if (i % nk == 0)
                {
                    temp = SubWord(RotWord(temp)) ^ ((uint)Rcon[i / nk] << 24);
                }
                else if (nk > 6 && i % nk == 4)
                {
                    temp = SubWord(temp);
                }

                words[i] = words[i - nk] ^ temp;
            }

            byte[][] roundKeys = new byte[nr + 1][];
            for (int r = 0; r <= nr; r++)
            {
                byte[] rKey = new byte[16];
                for (int w = 0; w < 4; w++)
                {
                    uint word = words[r * 4 + w];
                    rKey[w * 4]     = (byte)(word >> 24);
                    rKey[w * 4 + 1] = (byte)(word >> 16);
                    rKey[w * 4 + 2] = (byte)(word >> 8);
                    rKey[w * 4 + 3] = (byte)(word);
                }
                roundKeys[r] = rKey;
            }

            return roundKeys;
        }

        private uint RotWord(uint word) => (word << 8) | (word >> 24);

        private uint SubWord(uint word)
        {
            byte b3 = _sBox[(byte)(word >> 24)];
            byte b2 = _sBox[(byte)(word >> 16)];
            byte b1 = _sBox[(byte)(word >> 8)];
            byte b0 = _sBox[(byte)(word)];
            return (uint)(b3 << 24 | b2 << 16 | b1 << 8 | b0);
        }
    }
}
