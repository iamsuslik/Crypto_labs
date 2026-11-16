namespace MyCryptoProject.lr1.Task2.Interfaces
{
    public interface ISymmetricCipher
    {
        int BlockSizeBytes { get; }

        void SetKey(byte[] key);

        byte[] EncryptBlock(byte[] block);

        byte[] DecryptBlock(byte[] block);
    }
}
