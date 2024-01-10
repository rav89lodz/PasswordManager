using PasswordManager.Interfaces;
using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace PasswordManager.Services
{
    public class UserService : IUserService
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        private IntPtr tokenHandle = new IntPtr(0);

        private readonly IDataBaseService _dataBase;
        private readonly IEncryptionService _encryptionService;

        public UserService()
        { }

        public UserService(IDataBaseService dataBase, IEncryptionService encryptionService)
        {
            _dataBase = dataBase;
            _encryptionService = encryptionService;
        }

        public int CheckUserIsSystemUser(string login, string password)
        {
            const int LOGON32_PROVIDER_DEFAULT = 0;
            const int LOGON32_LOGON_INTERACTIVE = 2;
            tokenHandle = IntPtr.Zero;

            if (LogonUser(login, Environment.MachineName, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out tokenHandle) == false)
            {
                return Marshal.GetLastWin32Error();
            }
            return 0;
        }

        public bool CheckUserHaveAccess(string login, string backupFile)
        {
            string user = _encryptionService.Decrypt(_dataBase.GetValueFromDataBase("Params", "Value1", backupFile));
            string machine = _encryptionService.Decrypt(_dataBase.GetValueFromDataBase("Params", "Value2", backupFile));
            
            if (Environment.MachineName != machine && login != user)
            {
                return false;
            }
            WindowsIdentity newId = new WindowsIdentity(tokenHandle);
            WindowsPrincipal userperm = new WindowsPrincipal(newId);
            CloseHandle(tokenHandle);
            return true;
        }

        public bool CheckUserHaveAccessToBackup(string backupFile, string backupPassword)
        {
            string password = _encryptionService.Decrypt(_dataBase.GetValueFromDataBase("Params", "Value3", backupFile), backupPassword);

            if (password != backupPassword)
            {
                return false;
            }
            return true;
        }
    }
}
