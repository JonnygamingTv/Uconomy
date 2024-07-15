using Rocket.API;

namespace Uconomy
{
    public class UconomyConfiguration : IRocketPluginConfiguration
    {
        public bool Xpmode;
        public string DatabaseAddress;
        public string DatabaseName;
        public string DatabaseUsername;
        public string DatabasePassword;
        public int DatabasePort;
        public string UconomyTableName;
        public decimal InitialBalance;
        public string UconomyCurrencyName;

        public void LoadDefaults()
        {
            Xpmode = true;
            DatabaseAddress = "127.0.0.1";
            DatabaseName = "unturned";
            DatabaseUsername = "admin";
            DatabasePassword = "root";
            DatabasePort = 3306;
            UconomyTableName = "uconomy";
            InitialBalance = 30;
            UconomyCurrencyName = "Credits";
        }
    }
}
