using System;
using MyCryptoProject.lr3.Task1;
using MyCryptoProject.lr1.Task2.Interfaces;

namespace MyCryptoProject.lr3.Task2
{
    public class Rijndael : ISymmetricCipher
    {
        private readonly byte _irreduciblePolynomial;
        private byte[] _sBox;
        private byte[] _invSBox;
        private bool _isInitialized = false;

        private byte[][] _roundKeys;
        private int _numberOfRounds;

        private int _blockSizeBytes = 16;
        public int BlockSizeBytes => _blockSizeBytes;

        public Rijndael(byte irreduciblePolynomial)
        {
            if (!GaloisFieldService.IsIrreducible(irreduciblePolynomial))
            {
                throw new ArgumentException("Модуль должен быть неприводимым полиномом 8-й степени!", nameof(irreduciblePolynomial));
            }
            _irreduciblePolynomial = irreduciblePolynomial;
        }

        public void SetKey(byte[] key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));

            if (!_isInitialized)
            {
                InitializeSBoxes();
                _isInitialized = true;
            }

            var expansion = new RijndaelKeyExpansion(_irreduciblePolynomial, _sBox);
            _roundKeys = expansion.ExpandKey(key);
            _numberOfRounds = _roundKeys.Length - 1;
        }

        public byte[] EncryptBlock(byte[] block)
        {
            if (block.Length != 16 && block.Length != 24 && block.Length != 32)
                throw new ArgumentException("Длина блока должна быть 128, 192 или 256 бит (16, 24 или 32 байта).");
            
            if (_roundKeys == null) throw new InvalidOperationException("Ключ не установлен. Вызовите SetKey перед шифрованием.");

            _blockSizeBytes = block.Length;
            int nb = _blockSizeBytes / 4; // Количество столбцов (4, 6 или 8)

            byte[,] state = BytesToState(block, nb);

            state = AddRoundKey(state, _roundKeys[0], nb);

            for (int r = 1; r < _numberOfRounds; r++)
            {
                state = SubBytes(state, _sBox, nb);
                state = ShiftRows(state, nb);
                state = MixColumns(state, nb);
                state = AddRoundKey(state, _roundKeys[r], nb);
            }

            state = SubBytes(state, _sBox, nb);
            state = ShiftRows(state, nb);
            state = AddRoundKey(state, _roundKeys[_numberOfRounds], nb);

            return StateToBytes(state, nb);
        }

        public byte[] DecryptBlock(byte[] block)
        {
            if (block.Length != 16 && block.Length != 24 && block.Length != 32)
                throw new ArgumentException("Длина блока должна быть 128, 192 или 256 бит (16, 24 или 32 байта).");

            if (_roundKeys == null) throw new InvalidOperationException("Ключ не установлен. Вызовите SetKey перед дешифрованием.");

            _blockSizeBytes = block.Length;
            int nb = _blockSizeBytes / 4;

            byte[,] state = BytesToState(block, nb);

            // Финальный обратный раунд
            state = AddRoundKey(state, _roundKeys[_numberOfRounds], nb);
            state = ShiftRowsInv(state, nb);
            state = SubBytes(state, _invSBox, nb);

            // Основные обратные раунды
            for (int r = _numberOfRounds - 1; r >= 1; r--)
            {
                state = AddRoundKey(state, _roundKeys[r], nb);
                state = MixColumnsInv(state, nb);
                state = ShiftRowsInv(state, nb);
                state = SubBytes(state, _invSBox, nb);
            }

            // Начальный обратный раунд
            state = AddRoundKey(state, _roundKeys[0], nb);

            return StateToBytes(state, nb);
        }

        #region Математика раундов и S-боксов

        private void InitializeSBoxes()
        {
            _sBox = new byte[256];
            _invSBox = new byte[256];

            for (int i = 0; i < 256; i++)
            {
                byte b = (byte)i;
                byte inv = (b == 0) ? (byte)0 : GaloisFieldService.Inverse(b, _irreduciblePolynomial);
                _sBox[i] = AffineTransform(inv);
            }

            for (int i = 0; i < 256; i++)
            {
                byte b = (byte)i;
                byte invAff = InverseAffineTransform(b);
                _invSBox[i] = (invAff == 0) ? (byte)0 : GaloisFieldService.Inverse(invAff, _irreduciblePolynomial);
            }
        }

        private byte AffineTransform(byte b)
        {
            byte c = 0x63;
            for (int i = 0; i < 8; i++)
            {
                byte bit = (byte)(
                    ((b >> i) & 1) ^
                    ((b >> ((i + 4) % 8)) & 1) ^
                    ((b >> ((i + 5) % 8)) & 1) ^
                    ((b >> ((i + 6) % 8)) & 1) ^
                    ((b >> ((i + 7) % 8)) & 1)
                );
                c ^= (byte)(bit << i);
            }
            return c;
        }

        private byte InverseAffineTransform(byte b)
        {
            byte c = 0x05;
            for (int i = 0; i < 8; i++)
            {
                byte bit = (byte)(
                    ((b >> ((i + 2) % 8)) & 1) ^
                    ((b >> ((i + 5) % 8)) & 1) ^
                    ((b >> ((i + 7) % 8)) & 1)
                );
                c ^= (byte)(bit << i);
            }
            return c;
        }

        private byte[,] BytesToState(byte[] bytes, int nb)
        {
            byte[,] state = new byte[4, nb];
            int idx = 0;
            for (int c = 0; c < nb; c++)
                for (int r = 0; r < 4; r++)
                    state[r, c] = bytes[idx++];
            return state;
        }

        private byte[] StateToBytes(byte[,] state, int nb)
        {
            byte[] bytes = new byte[nb * 4];
            int idx = 0;
            for (int c = 0; c < nb; c++)
                for (int r = 0; r < 4; r++)
                    bytes[idx++] = state[r, c];
            return bytes;
        }

        private byte[,] SubBytes(byte[,] state, byte[] box, int nb)
        {
            for (int r = 0; r < 4; r++)
                for (int c = 0; c < nb; c++)
                    state[r, c] = box[state[r, c]];
            return state;
        }

        private byte[,] ShiftRows(byte[,] state, int nb)
        {
            int shift2 = (nb == 8) ? 3 : 2;
            int shift3 = (nb == 8) ? 4 : 3;

            byte tmp = state[1, 0];
            for (int c = 0; c < nb - 1; c++) state[1, c] = state[1, c + 1];
            state[1, nb - 1] = tmp;

            for (int s = 0; s < shift2; s++)
            {
                tmp = state[2, 0];
                for (int c = 0; c < nb - 1; c++) state[2, c] = state[2, c + 1];
                state[2, nb - 1] = tmp;
            }

            for (int s = 0; s < shift3; s++)
            {
                tmp = state[3, 0];
                for (int c = 0; c < nb - 1; c++) state[3, c] = state[3, c + 1];
                state[3, nb - 1] = tmp;
            }

            return state;
        }

        private byte[,] ShiftRowsInv(byte[,] state, int nb)
        {
            int shift2 = (nb == 8) ? 3 : 2;
            int shift3 = (nb == 8) ? 4 : 3;

            byte tmp = state[1, nb - 1];
            for (int c = nb - 1; c > 0; c--) state[1, c] = state[1, c - 1];
            state[1, 0] = tmp;

            for (int s = 0; s < shift2; s++)
            {
                tmp = state[2, nb - 1];
                for (int c = nb - 1; c > 0; c--) state[2, c] = state[2, c - 1];
                state[2, 0] = tmp;
            }

            for (int s = 0; s < shift3; s++)
            {
                tmp = state[3, nb - 1];
                for (int c = nb - 1; c > 0; c--) state[3, c] = state[3, c - 1];
                state[3, 0] = tmp;
            }

            return state;
        }

        private byte[,] MixColumns(byte[,] state, int nb)
        {
            for (int c = 0; c < nb; c++)
            {
                byte s0 = state[0, c], s1 = state[1, c], s2 = state[2, c], s3 = state[3, c];
                state[0, c] = (byte)(Mul02(s0) ^ Mul03(s1) ^ s2 ^ s3);
                state[1, c] = (byte)(s0 ^ Mul02(s1) ^ Mul03(s2) ^ s3);
                state[2, c] = (byte)(s0 ^ s1 ^ Mul02(s2) ^ Mul03(s3));
                state[3, c] = (byte)(Mul03(s0) ^ s1 ^ s2 ^ Mul02(s3));
            }
            return state;
        }

        private byte[,] MixColumnsInv(byte[,] state, int nb)
        {
            for (int c = 0; c < nb; c++)
            {
                byte s0 = state[0, c], s1 = state[1, c], s2 = state[2, c], s3 = state[3, c];
                state[0, c] = (byte)(Mul(0x0e, s0) ^ Mul(0x0b, s1) ^ Mul(0x0d, s2) ^ Mul(0x09, s3));
                state[1, c] = (byte)(Mul(0x09, s0) ^ Mul(0x0e, s1) ^ Mul(0x0b, s2) ^ Mul(0x0d, s3));
                state[2, c] = (byte)(Mul(0x0d, s0) ^ Mul(0x09, s1) ^ Mul(0x0e, s2) ^ Mul(0x0b, s3));
                state[3, c] = (byte)(Mul(0x0b, s0) ^ Mul(0x0d, s1) ^ Mul(0x09, s2) ^ Mul(0x0e, s3));
            }
            return state;
        }

        private byte[,] AddRoundKey(byte[,] state, byte[] roundKey, int nb)
        {
            int idx = 0;
            for (int c = 0; c < nb; c++)
                for (int r = 0; r < 4; r++)
                    state[r, c] ^= roundKey[idx++];
            return state;
        }

        private byte Mul02(byte b) => GaloisFieldService.Multiply(b, 0x02, _irreduciblePolynomial);
        private byte Mul03(byte b) => GaloisFieldService.Multiply(b, 0x03, _irreduciblePolynomial);
        private byte Mul(byte a, byte b) => GaloisFieldService.Multiply(a, b, _irreduciblePolynomial);

        #endregion
    }
}
