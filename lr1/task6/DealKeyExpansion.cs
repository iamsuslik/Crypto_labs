using System;
using MyCryptoProject.lr1.Task2.Interfaces;


namespace MyCryptoProject.lr1.Task6
{
    public class DealKeyExpansion : IKeyExpansion
    {
        private readonly ISymmetricCipher _desCipher;
        private readonly int _rounds;

        public DealKeyExpansion(ISymmetricCipher desCipher, int rounds = 6)
        {
            _desCipher = desCipher ?? throw new ArgumentNullException(nameof(desCipher));
            _rounds = rounds;
        }

        public byte[][] ExpandKey(byte[] masterKey)
        {
            if (masterKey == null || masterKey.Length != 16)
                throw new ArgumentException("Для DEAL-128 мастер-ключ должен быть ровно 16 байт (128 бит).", nameof(masterKey));

            byte[] k1 = new byte[8];
            byte[] k2 = new byte[8];
            Buffer.BlockCopy(masterKey, 0, k1, 0, 8);
            Buffer.BlockCopy(masterKey, 8, k2, 0, 8);

            byte[][] roundKeys = new byte[_rounds][];

            byte[] currentVal = new byte[8];
            Buffer.BlockCopy(k1, 0, currentVal, 0, 8);

            for (int i = 0; i < _rounds; i++)
            {
                byte[] currentDesKey = (i % 2 == 0) ? k1 : k2;

                byte roundConstant = (byte)(1 << i);

                currentVal[currentVal.Length - 1] ^= roundConstant;
                
                _desCipher.SetKey(currentDesKey);
                currentVal = _desCipher.EncryptBlock(currentVal);
                
                roundKeys[i] = (byte[])currentVal.Clone();
            }

            return roundKeys;
        }
    }
}
