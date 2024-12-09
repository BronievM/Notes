namespace Course.Classes
{
    public class AccountBuilder
    {
        public string Name;
        public string Email;
        public bool? IsUsedLast;
        public SettingsA SettingsOfAccount;

        public AccountBuilder()
        {
            Name = "User";
            Email = string.Empty;
            IsUsedLast = false;
            SettingsOfAccount = new SettingsA();
        }

        public AccountBuilder WithName(string name)
        {
            Name = name;
            return this;
        }

        public AccountBuilder WithEmail(string email)
        {
            Email = email;
            return this;
        }

        public AccountBuilder WithSettings(SettingsA settingsOfAccount)
        {
            SettingsOfAccount = settingsOfAccount;
            return this;
        }

        public Account Build()
        {
            return new Account
                { 
                Name = Name,
                Email = Email,
                SettingsOfAccount = SettingsOfAccount
            };
        }
    }
}
