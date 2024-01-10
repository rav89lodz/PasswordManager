namespace PasswordManager.Entities
{ 
    public class TableRow
    {
        public string UUID { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Description { get; set; }

        public TableRow(string uuid, string login, string password, string description)
        {
            UUID = uuid;
            Login = login;
            Password = password;
            Description = description;
        }
    }
}
