namespace MyCryptoProject.lr1.Task2.Interfaces
{
    public interface IBlockCipherTransform
    {
        byte[] EncryptBlock(byte[] block, byte[] roundKey);
        byte[] DecryptBlock(byte[] block, byte[] roundKey);
    }
}
