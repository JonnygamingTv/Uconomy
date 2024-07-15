using Rocket.API;

namespace Uconomy
{
    public class UconomyConfiguration : IRocketPluginConfiguration
    {
        public bool xpMode;
        public decimal InitialBalance;
        public ushort BalanceFgEffectId;
        public short BalanceFgEffectKey;
        public int DatabasePort;
        public string DatabaseAddress;
        public string DatabaseName;
        public string DatabaseUsername;
        public string DatabasePassword;
        public string UconomyTableName;
        public string UconomyCurrencyName;

        public void LoadDefaults()
        {
            xpMode = true;
            InitialBalance = 30;
            BalanceFgEffectId = 42701;
            BalanceFgEffectKey = short.MaxValue;
            DatabasePort = 3306;
            DatabaseAddress = "127.0.0.1";
            DatabaseName = "unturned";
            DatabaseUsername = "admin";
            DatabasePassword = "root";
            UconomyTableName = "uconomy";
            UconomyCurrencyName = "Credits";
        }
    }
}
