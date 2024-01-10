using PasswordManager.Entities;

namespace PasswordManager.Interfaces
{
    public interface IDataBaseService
    {
        void SaveToFile(TableRow tableRow);
        string GetValueFromDataBase(string descendants, string value, string fileName = null);
        TableRow[] GetData(string fileName = null);
        void RemoveElement(TableRow tableRow);
        void CreateFileOnStart();
        string GetFileName();
        string CreateBackup(string backupPassword);
        string RestoreBackup(string filePath, string backupPassword);
    }
}
