using System;
using MyCryptoProject.lr1.Task2.Interfaces;

namespace MyCryptoProject.lr1.Task6
{
    public class DesAdapter : IBlockCipherTransform
    {
        private readonly ISymmetricCipher _desCipher;
        private static readonly object _syncLock = new object();

        public DesAdapter(ISymmetricCipher desCipher)
        {
            _desCipher = desCipher ?? throw new ArgumentNullException(nameof(desCipher));
        }

        public byte[] EncryptBlock(byte[] block, byte[] roundKey)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));
            if (roundKey == null) throw new ArgumentNullException(nameof(roundKey));

            lock (_syncLock)
            {
                _desCipher.SetKey(roundKey);
                return _desCipher.EncryptBlock(block);
            }
        }

        public byte[] DecryptBlock(byte[] block, byte[] roundKey)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));
            if (roundKey == null) throw new ArgumentNullException(nameof(roundKey));

            lock (_syncLock)
            {
                _desCipher.SetKey(roundKey);
                return _desCipher.DecryptBlock(block);
            }
        }
    }
}
