namespace PasswordManager.Interfaces
{
    public interface IPasswordService
    {
        char RandomLowerLetter();
        char RandomUpperLetter();
        string CreateRandomString(int iterations, bool withChars);
        string PassGen(int option);
    }
}
