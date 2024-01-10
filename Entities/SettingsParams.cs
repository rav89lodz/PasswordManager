using PasswordManager.Interfaces;
using System;

namespace PasswordManager.Entities
{
    public class SettingsParams
    {
        public string UUID { get; set; }
        public string UserName { get; set; }
        public string MachineName { get; set; }
        public string Salt { get; set; }
        public string Password { get; set; }

        public SettingsParams(IEncryptionService encryptionService)
        {
            Salt = Convert.ToBase64String(encryptionService.GetSaltValue());
            Password = encryptionService.GetPassValue();
            UserName = Environment.UserName;
            MachineName = Environment.MachineName;
        }
    }
}
