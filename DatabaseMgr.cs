using System;
using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using Steamworks;

namespace ZaupShop
{
    public class DatabaseMgr
    {
        private readonly ZaupShop _zaupShop;
        private MySqlConnection _mySqlConnection = null;

        internal DatabaseMgr(ZaupShop zaupShop)
        {
            _zaupShop = zaupShop;
            CheckSchema();
        }
        public void Close()
        {
            _mySqlConnection.Close();
        }
        public void Open()
        {
            if(_mySqlConnection.State != System.Data.ConnectionState.Open) _mySqlConnection.Open();
        }

        private void CheckSchema()
        {
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("show tables like '", _zaupShop.Configuration.Instance.ItemShopTableName, "'");
                //mySqlConnection.Open();
                if (mySqlCommand.ExecuteScalar() == null)
                {
                    mySqlCommand.CommandText = string.Concat("CREATE TABLE `", _zaupShop.Configuration.Instance.ItemShopTableName, "` (`id` int(6) NOT NULL,`itemname` varchar(32) NOT NULL,`cost` decimal(15,2) NOT NULL DEFAULT '20.00',`buyback` decimal(15,2) NOT NULL DEFAULT '0.00',PRIMARY KEY (`id`)) ");
                    mySqlCommand.ExecuteNonQuery();
                }
                //mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[ZaupShop] Database Crashed by Console when trying to create or check existing table {_zaupShop.Configuration.Instance.ItemShopTableName}, reason: {exception.Message}");
            }
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("show tables like '", _zaupShop.Configuration.Instance.VehicleShopTableName, "'");
                //mySqlConnection.Open();
                if (mySqlCommand.ExecuteScalar() == null)
                {
                    mySqlCommand.CommandText = string.Concat("CREATE TABLE `", _zaupShop.Configuration.Instance.VehicleShopTableName, "` (`id` int(6) NOT NULL,`vehiclename` varchar(32) NOT NULL,`cost` decimal(15,2) NOT NULL DEFAULT '100.00',PRIMARY KEY (`id`)) ");
                    mySqlCommand.ExecuteNonQuery();
                }
                //mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[ZaupShop] Database Crashed by Console when trying to create or check existing table {_zaupShop.Configuration.Instance.VehicleShopTableName}, reason: {exception.Message}");
            }
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("show columns from `", _zaupShop.Configuration.Instance.ItemShopTableName, "` like 'buyback'");
                //mySqlConnection.Open();
                if (mySqlCommand.ExecuteScalar() == null)
                {
                    mySqlCommand.CommandText = string.Concat("ALTER TABLE `", _zaupShop.Configuration.Instance.ItemShopTableName, "` ADD `buyback` decimal(15,2) NOT NULL DEFAULT '0.00'");
                    mySqlCommand.ExecuteNonQuery();
                }
                //mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[ZaupShop] Database Crashed by Console when trying to create or check existing table {_zaupShop.Configuration.Instance.ItemShopTableName}, reason: {exception.Message}");
                Logger.LogError($"Database Crashed, reason: {exception.Message}");
            }
        }

        public MySqlConnection CreateConnection()
        {
            if(_mySqlConnection == null || _mySqlConnection.State != System.Data.ConnectionState.Open)
            try
            {
                _mySqlConnection = new MySqlConnection(string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", _zaupShop.Configuration.Instance.DatabaseAddress, _zaupShop.Configuration.Instance.DatabaseName, _zaupShop.Configuration.Instance.DatabaseUsername, _zaupShop.Configuration.Instance.DatabasePassword, _zaupShop.Configuration.Instance.DatabasePort));
                _mySqlConnection.Open();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[ZaupShop] Instance Connection Database Crashed by Console, reason: {exception.Message}");
            }
            return _mySqlConnection;
        }

        /// <summary>
        /// Get the item cost based on his id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public decimal GetItemCost(int id)
        {
            decimal num = 0;
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("select `cost` from `", _zaupShop.Configuration.Instance.ItemShopTableName, "` where `id` = '", id.ToString(), "';");
                //mySqlConnection.Open();
                object obj = mySqlCommand.ExecuteScalar();
                if (obj != null)
                {
                    decimal.TryParse(obj.ToString(), out num);
                }
                //mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[ZaupShop] Database Crashed by Console from function GetItemCost, reason: {exception.Message}");
            }
            return num;
        }

        /// <summary>
        /// Get the vehicle cost based on his id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public decimal GetVehicleCost(int id)
        {
            decimal num = 0;
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("select `cost` from `", _zaupShop.Configuration.Instance.VehicleShopTableName, "` where `id` = '", id.ToString(), "';");
                //mySqlConnection.Open();
                object obj = mySqlCommand.ExecuteScalar();
                if (obj != null)
                {
                    decimal.TryParse(obj.ToString(), out num);
                }
                //mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[ZaupShop] Database Crashed by Console from function GetVehicleCost, reason: {exception.Message}");
            }
            return num;
        }

        /// <summary>
        /// Get the player uconomy balance
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public decimal GetBalance(string id)
        {
            if (_zaupShop.Configuration.Instance.xpMode)
            {
                return (decimal)Rocket.Unturned.Player.UnturnedPlayer.FromCSteamID(new CSteamID(UInt64.Parse(id))).Experience;
            }
            decimal num = 0;
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("select `balance` from `", _zaupShop.Configuration.Instance.UconomyTableName, "` where `steamId` = '", id, "';");
                //mySqlConnection.Open();
                object obj = mySqlCommand.ExecuteScalar();
                if (obj != null)
                {
                    decimal.TryParse(obj.ToString(), out num);
                }
                //mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[ZaupShop] Database Crashed by {id} from function GetBalance, reason: {exception.Message}");
            }
            return num;
        }

        /// <summary>
        /// Remove a specific amount of player balance
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cost"></param>
        public void RemoveBalance(string id, decimal cost)
        {
            if (_zaupShop.Configuration.Instance.xpMode)
            {
                Rocket.Unturned.Player.UnturnedPlayer.FromCSteamID(new CSteamID(UInt64.Parse(id))).Experience -= (uint)cost;
                return;
            }
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = $"update `{_zaupShop.Configuration.Instance.UconomyTableName}` set `balance` = `balance` - {cost} where `steamId` = {id};";
                //mySqlConnection.Open();
                mySqlCommand.ExecuteNonQuery();
                //mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[ZaupShop] Database Crashed by {id} from function RemoveBalance, reason: {exception.Message}");
            }
        }

        /// <summary>
        /// Add a specific amount to the player balance
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quantity"></param>
        public void AddBalance(string id, decimal quantity)
        {
            if (_zaupShop.Configuration.Instance.xpMode)
            {
                Rocket.Unturned.Player.UnturnedPlayer.FromCSteamID(new CSteamID(UInt64.Parse(id))).Experience += (uint)quantity;
                return;
            }
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = $"update `{_zaupShop.Configuration.Instance.UconomyTableName}` set `balance` = `balance` + {quantity} where `steamId` = {id};";
                //mySqlConnection.Open();
                mySqlCommand.ExecuteNonQuery();
                //mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[ZaupShop] Database Crashed by {id} from function AddBalance, reason: {exception.Message}");
            }
        }

        /// <summary>
        /// Get the sell price from the item id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public decimal GetItemBuyPrice(int id)
        {
            decimal num = 0;
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("select `buyback` from `", _zaupShop.Configuration.Instance.ItemShopTableName, "` where `id` = '", id.ToString(), "';");
                //mySqlConnection.Open();
                object obj = mySqlCommand.ExecuteScalar();
                if (obj != null)
                {
                    decimal.TryParse(obj.ToString(), out num);
                }
                //mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[ZaupShop] Database Crashed by Console from function GetItemBuyPrice, reason: {exception.Message}");
            }
            return num;
        }
    }
}
