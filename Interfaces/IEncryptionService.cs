namespace PasswordManager.Interfaces
{
    public interface IEncryptionService
    {
        string Encrypt(string text, string inlinePassword = null);
        string Decrypt(string text, string inlinePassword = null);
        byte[] GetSaltValue();
        string GetPassValue();
        string GetInlinePassValue();
    }
}
