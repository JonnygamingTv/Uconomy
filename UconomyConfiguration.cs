using Rocket.API;

namespace Uconomy
{
    public class UconomyConfiguration : IRocketPluginConfiguration
    {
        public string DatabaseAddress = "127.0.0.1";
        public string DatabaseName = "unturned";
        public string DatabaseUsername = "admin";
        public string DatabasePassword = "root";
        public int DatabasePort = 3306;
        public string UconomyTableName = "uconomy";
        public decimal InitialBalance = 30;
        public string UconomyCurrencyName = "Credits";

        public void LoadDefaults()
        {
        }
    }
}
