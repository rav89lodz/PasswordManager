using PasswordManager.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PasswordManager.Services
{
    public class EncryptionService : IEncryptionService
    {
        private byte[] salt;
        private string pass;
        private string inlinePass;

        public string Encrypt(string text, string inlinePassword = null)
        {
            byte[] encryptBytes = Encoding.Unicode.GetBytes(text);
            var aes = Aes.Create();
            var pbkdf2 = new Rfc2898DeriveBytes(GetPassValue(), GetSaltValue(), 2000);
            if(inlinePassword != null)
            {
                pbkdf2 = new Rfc2898DeriveBytes(inlinePassword, GetSaltValue(), 2000);
            }
            aes.Key = pbkdf2.GetBytes(32);
            aes.IV = pbkdf2.GetBytes(16);
            var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(encryptBytes, 0, encryptBytes.Length);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt(string text, string inlinePassword = null)
        {
            byte[] encryptBytes = Convert.FromBase64String(text);
            var aes = Aes.Create();
            var pbkdf2 = new Rfc2898DeriveBytes(GetPassValue(), GetSaltValue(), 2000);
            if (inlinePassword != null)
            {
                pbkdf2 = new Rfc2898DeriveBytes(inlinePassword, GetSaltValue(), 2000);
            }
            aes.Key = pbkdf2.GetBytes(32);
            aes.IV = pbkdf2.GetBytes(16);
            var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(encryptBytes, 0, encryptBytes.Length);
            }
            return Encoding.Unicode.GetString(ms.ToArray());
        }

        public byte[] GetSaltValue()
        {
            salt = Encoding.Unicode.GetBytes(GetSaltOrPass("salt", 30));
            return salt; // "IJSDOsaibciuasISODXcsiua891298rncsjnoic98ce328ncasai893ncxxsaincx"
        }

        public string GetPassValue()
        {
            pass = GetSaltOrPass("pass", 15);
            return pass; // "abc89743nsaidiasudOIDUASHU892"
        }

        public string GetInlinePassValue()
        {
            var password = new PasswordService();
            inlinePass = password.CreateRandomString(15, false);
            return inlinePass;
        }

        private static string GetSaltOrPass(string name, int loopRounds)
        {
            var environmentService = new EnvironmentService();
            string toReturn = environmentService.GetEnvironmentVariable(environmentService.PgSalt);
            if(name == "pass")
            {
                toReturn = environmentService.GetEnvironmentVariable(environmentService.PgPass);
            }
            if(string.IsNullOrEmpty(toReturn))
            {
                var password = new PasswordService();
                toReturn = password.CreateRandomString(loopRounds, false);
            }
            return toReturn;
        }
    }
}
