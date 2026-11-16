namespace MyCryptoProject.lr1.Task2.Interfaces
{
    public interface IKeyExpansion
    {
        byte[][] ExpandKey(byte[] masterKey);
    }
}
