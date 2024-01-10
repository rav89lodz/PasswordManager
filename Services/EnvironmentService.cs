using System;

namespace PasswordManager.Services
{
    public sealed class EnvironmentService
    {
        private const string _pgSalt = "PG_SALT";
        private const string _pgPass = "PG_PASS";
        private const string _pgDBType = "PG_DB_TYPE";
        public string PgSalt { get { return _pgSalt; } }
        public string PgPass { get { return _pgPass; } }
        public string PgDBType { get { return _pgDBType; } }

        public string Salt { get; set; }
        public string Password { get; set; }
        public string DefaultDB { get; set; }

        public EnvironmentService()
        {

        }
        public EnvironmentService(string salt, string password, string defaultDb = "json")
        {
            Salt = salt;
            Password = password;
            DefaultDB = defaultDb;
            CreateEnvironments();
        }

        private void CreateEnvironments()
        {
            Environment.SetEnvironmentVariable(PgSalt, Salt, EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable(PgPass, Password, EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable(PgDBType, DefaultDB, EnvironmentVariableTarget.User);
        }

        public string GetEnvironmentVariable(string key)
        {
            string env = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User);
            if(env == null)
            {
                env = "";
            }
            return env;
        }
    }
}
