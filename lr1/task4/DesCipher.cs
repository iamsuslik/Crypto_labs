using System;
using MyCryptoProject.lr1.Task2.Interfaces;
using MyCryptoProject.lr1.Task3;
using MyCryptoProject.lr1.Task1;

namespace MyCryptoProject.lr1.Task4
{
    public class DesCipher : ISymmetricCipher
    {
        private readonly FeistelNetwork _feistel;

        public int BlockSizeBytes => 8;

        public DesCipher()
        {
            var keyExpansion = new DesKeyExpansion();
            var transform = new DesTransform();
            _feistel = new FeistelNetwork(keyExpansion, transform, blockSizeBytes: 8);
        }

        public void SetKey(byte[] key)
        {
            _feistel.SetKey(key);
        }

        public byte[] EncryptBlock(byte[] block)
        {
            byte[] ipBlock = PermutationBox.Permute(block, DesTables.IP, 1, true);
            byte[] feistelOut = _feistel.EncryptBlock(ipBlock);
            return PermutationBox.Permute(feistelOut, DesTables.FP, 1, true);
        }

        public byte[] DecryptBlock(byte[] block)
        {
            byte[] ipBlock = PermutationBox.Permute(block, DesTables.IP, 1, true);
            byte[] feistelOut = _feistel.DecryptBlock(ipBlock);
            return PermutationBox.Permute(feistelOut, DesTables.FP, 1, true);
        }
    }
}
