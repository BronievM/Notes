using System;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Course.Classes
{
    [Serializable]
    public class Account
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public bool? IsUsedLast { get; set; }
        public SettingsA SettingsOfAccount { get; set; }
        public Account()
        {
            this.Name = "User";
            this.Password = string.Empty;
            this.Email = string.Empty;
            this.IsUsedLast = false;
            this.SettingsOfAccount = new SettingsA();
        }

        public void LastAccountUsed()
        {
            this.IsUsedLast = true;
        }

        public bool Equals(Account other)
        {
            if (this.Name != other.Name) return false;
            if (this.Password != other.Password) return false;
            return true;
        }

        public void SetPassword(string password)
        {
            Password = BCryptNet.HashPassword(password);
        }

        public bool VerifyPassword(string password)
        {
            return BCryptNet.Verify(password, Password);
        }
    }
}
