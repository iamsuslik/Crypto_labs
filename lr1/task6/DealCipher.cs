using System;
using MyCryptoProject.lr1.Task2.Interfaces;
using MyCryptoProject.lr1.Task3;


namespace MyCryptoProject.lr1.Task6
{
    public class DealCipher : ISymmetricCipher
    {
        private readonly FeistelNetwork _feistelCipher;

        public int BlockSizeBytes => 16;

        public DealCipher(ISymmetricCipher desInstance)
        {
            if (desInstance == null) 
                throw new ArgumentNullException(nameof(desInstance));

            IBlockCipherTransform adapter = new DesAdapter(desInstance);

            IKeyExpansion keyExpansion = new DealKeyExpansion(desInstance, rounds: 6);

            _feistelCipher = new FeistelNetwork(keyExpansion, adapter, blockSizeBytes: 16);
        }

        public void SetKey(byte[] key)
        {
            _feistelCipher.SetKey(key);
        }

        public byte[] EncryptBlock(byte[] block)
        {
            return _feistelCipher.EncryptBlock(block);
        }

        public byte[] DecryptBlock(byte[] block)
        {
            return _feistelCipher.DecryptBlock(block);
        }
    }
}
