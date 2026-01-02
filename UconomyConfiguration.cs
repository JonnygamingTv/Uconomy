using Rocket.API;

namespace fr34kyn01535.Uconomy
{
    public class UconomyConfiguration : IRocketPluginConfiguration
    {
        public bool xpMode;
        public ushort BalanceBgEffectId;
        public ushort BalanceFgEffectId;
        public short BalanceBgEffectKey;
        public short BalanceFgEffectKey;
        public float SalaryInterval;
        public decimal InitialBalance;
        public int DatabasePort;
        public string DatabaseAddress;
        public string DatabaseName;
        public string DatabaseUsername;
        public string DatabasePassword;
        public string UconomyTableName;
        public string UconomyCurrencyName;
        public string CurrencySymbol;
        public string UIColor;
        public FindFirstorLast FirstOrLastSalaryPermission;

        public void LoadDefaults()
        {
            xpMode = true;
            InitialBalance = 30;
            BalanceBgEffectId = 42700;
            BalanceBgEffectKey = short.MaxValue;
            BalanceFgEffectId = 42701;
            BalanceFgEffectKey = short.MaxValue;
            SalaryInterval = 3600;
            DatabasePort = 3306;
            DatabaseAddress = "127.0.0.1";
            DatabaseName = "unturned";
            DatabaseUsername = "admin";
            DatabasePassword = "root";
            UconomyTableName = "uconomy";
            UconomyCurrencyName = "Credits";
            CurrencySymbol = "$";
            UIColor = "white";
            FirstOrLastSalaryPermission = FindFirstorLast.FIRST;
        }
    }
    public enum FindFirstorLast
    {
        FIRST,
        LAST
    }
}
