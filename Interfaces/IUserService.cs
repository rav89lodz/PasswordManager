namespace PasswordManager.Interfaces
{
    public interface IUserService
    {
        int CheckUserIsSystemUser(string login, string password);
        bool CheckUserHaveAccess(string login, string backupFile);
        bool CheckUserHaveAccessToBackup(string backupFile, string backupPassword);
    }
}
