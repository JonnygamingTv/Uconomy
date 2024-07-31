using Rocket.API;

namespace ZaupShop
{
    public class ZaupShopConfiguration : IRocketPluginConfiguration
    {
        public string DatabaseAddress;
        public string DatabaseName;
        public string DatabaseUsername;
        public string DatabasePassword;
        public int DatabasePort;
        public string ItemShopTableName;
        public string VehicleShopTableName;
        public string UconomyTableName;
        public string UconomyCurrencyName;
        public bool CanBuyItems;
        public bool CanBuyVehicles;
        public bool CanSellItems;
        public bool QualityCounts;
        public bool xpMode;

        public void LoadDefaults()
        {
            DatabaseAddress = "127.0.0.1";
            DatabaseName = "unturned";
            DatabaseUsername = "admin";
            DatabasePassword = "root";
            DatabasePort = 3306;
            ItemShopTableName = "uconomyitemshop";
            VehicleShopTableName = "uconomyvehicleshop";
            UconomyTableName = "uconomy";
            UconomyCurrencyName = "Credits";
            CanBuyItems = true;
            CanBuyVehicles = false;
            CanSellItems = true;
            QualityCounts = true;
            xpMode = true;
        }
    }
}