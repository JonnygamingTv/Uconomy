using System;
using MySql.Data.MySqlClient;
using Rocket.Core.Logging;
using SDG.Unturned;
using Steamworks;

namespace Uconomy
{
    public class DatabaseMgr
    {
        private readonly UconomyPlugin _uconomy;
        private MySqlConnection _mySqlConnection = null;

        internal DatabaseMgr(UconomyPlugin uconomy)
        {
            _uconomy = uconomy;
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
                //Open();
                mySqlCommand.CommandText = string.Concat(
                "CREATE TABLE IF NOT EXISTS `",
                _uconomy.Configuration.Instance.UconomyTableName,
                "` (",
                "`steamId` VARCHAR(32) NOT NULL,",
                "`balance` DOUBLE NOT NULL,",
                "`lastUpdated` VARCHAR(32) NOT NULL,",
                "PRIMARY KEY (`steamId`)",
                ");"
            );
                mySqlCommand.ExecuteNonQuery();
                //mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[Uconomy] Database Crashed by Console when trying to create or check existing table {_uconomy.Configuration.Instance.UconomyTableName}, reason: {exception.Message}");
            }
        }

        public MySqlConnection CreateConnection()
        {
            if(_mySqlConnection == null || _mySqlConnection.State != System.Data.ConnectionState.Open)
            try
            {
                _mySqlConnection = new MySqlConnection(string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", _uconomy.Configuration.Instance.DatabaseAddress, _uconomy.Configuration.Instance.DatabaseName, _uconomy.Configuration.Instance.DatabaseUsername, _uconomy.Configuration.Instance.DatabasePassword, _uconomy.Configuration.Instance.DatabasePort));
                _mySqlConnection.Open();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[Uconomy] Database Crashed, reason: {exception.Message}");
            }
            return _mySqlConnection;
        }

        /// <summary>
        /// Add a new player to the uconomy database if not exist
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="balance"></param>
        public void AddNewPlayer(string playerId, decimal balance)
        {
            try
            {
                // Instanciate connection
                MySqlConnection mySqlConnection = CreateConnection();
                // Instanciate command
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                // Command: Insert new player only if not exist the same steamId
                mySqlCommand.CommandText = string.Concat("Insert ignore into `", _uconomy.Configuration.Instance.UconomyTableName, "` (`steamId`, `balance`, `lastUpdated`) VALUES ('", playerId, "', '", balance, "', '", DateTime.Now.ToShortDateString(), "');");
                // Try to connect
                //Open();
                // Execute the command
                mySqlCommand.ExecuteNonQuery();
                // Close connection
                //mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[Uconomy] Database Crashed by {playerId} from function AddNewPlayer, reason: {exception.Message}");
            }
        }

        /// <summary>
        /// Returns the decimal player balance from the table uconomy
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public decimal GetBalance(string playerId)
        {
            if (_uconomy.Configuration.Instance.xpMode) return (decimal)Rocket.Unturned.Player.UnturnedPlayer.FromCSteamID(new CSteamID(UInt64.Parse(playerId))).Experience;
            decimal num = new(0);
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = string.Concat("select `balance` from `", _uconomy.Configuration.Instance.UconomyTableName, "` where `steamId` = '", playerId, "';");
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
                Logger.LogError($"[Uconomy] Database Crashed by {playerId} from function GetBalance, reason: {exception.Message}");
            }
            return num;
        }

        /// <summary>
        /// Make a pay query from other player, returns true if successfuly payed
        /// </summary>
        /// <param name="payingPlayerId"></param>
        /// <param name="receivedPlayerId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool PlayerPayPlayer(string payingPlayerId, string receivedPlayerId, decimal amount)
        {
            try
            {
                decimal payingPlayerBalance = GetBalance(payingPlayerId);
                if ((payingPlayerBalance - amount) < 0)
                {
                    return false;
                }

                RemoveBalance(payingPlayerId, amount);
                AddBalance(receivedPlayerId, amount);
                return true;
            }
            catch (Exception exception)
            {
                Logger.LogError($"[Uconomy] Database Crashed by {payingPlayerId} and {receivedPlayerId} from function PlayerPayPlayer, reason: {exception.Message}");
                return false;
            }
        }

        /// <summary>
        /// Remove a balance from the player
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cost"></param>
        public void RemoveBalance(string id, decimal cost)
        {
            if (_uconomy.Configuration.Instance.BalanceFgEffectKey != 0)
            {
                EffectManager.sendUIEffect(_uconomy.Configuration.Instance.BalanceFgEffectId, _uconomy.Configuration.Instance.BalanceFgEffectKey, true, (GetBalance(id) - cost).ToString());
            }
            if (_uconomy.Configuration.Instance.xpMode)
            {
                Rocket.Unturned.Player.UnturnedPlayer.FromCSteamID(new CSteamID(UInt64.Parse(id))).Experience -= (uint) cost;
                return;
            }
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = $"update `{_uconomy.Configuration.Instance.UconomyTableName}` set `balance` = `balance` - {cost} where `steamId` = {id};";
                //mySqlConnection.Open();
                mySqlCommand.ExecuteNonQuery();
                //mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[Uconomy] Database Crashed by {id} from function RemoveBalance, reason: {exception.Message}");
            }
        }

        /// <summary>
        /// Add more balance to the player
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quantity"></param>
        public void AddBalance(string id, decimal quantity)
        {
            if (_uconomy.Configuration.Instance.BalanceFgEffectKey != 0)
            {
                EffectManager.sendUIEffect(_uconomy.Configuration.Instance.BalanceFgEffectId, _uconomy.Configuration.Instance.BalanceFgEffectKey, true, (GetBalance(id)+quantity).ToString());
            }
            if (_uconomy.Configuration.Instance.xpMode)
            {
                Rocket.Unturned.Player.UnturnedPlayer.FromCSteamID(new CSteamID(UInt64.Parse(id))).Experience += (uint)quantity;
                return;
            }
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlCommand.CommandText = $"update `{_uconomy.Configuration.Instance.UconomyTableName}` set `balance` = `balance` + {quantity} where `steamId` = {id};";
                //mySqlConnection.Open();
                mySqlCommand.ExecuteNonQuery();
                //mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogError($"[Uconomy] Database Crashed by {id} from function AddBalance, reason: {exception.Message}");
            }
        }
    }
}
