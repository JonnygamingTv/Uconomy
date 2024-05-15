using System;
using MySql.Data.MySqlClient;
using Rocket.Core.Logging;

namespace Uconomy
{
    public class DatabaseMgr
    {
        private readonly UconomyPlugin _uconomy;

        internal DatabaseMgr(UconomyPlugin uconomy)
        {
            _uconomy = uconomy;
            CheckSchema();
        }

        private void CheckSchema()
        {
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();
                mySqlConnection.Open();
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
                mySqlConnection.Close();
            }
            catch (Exception exception)
            {
                Logger.LogError($"Database Crashed, reason: {exception.Message}");
            }
        }

        public MySqlConnection CreateConnection()
        {
            MySqlConnection mySqlConnection = null;
            try
            {
                mySqlConnection = new MySqlConnection(string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4};", _uconomy.Configuration.Instance.DatabaseAddress, _uconomy.Configuration.Instance.DatabaseName, _uconomy.Configuration.Instance.DatabaseUsername, _uconomy.Configuration.Instance.DatabasePassword, _uconomy.Configuration.Instance.DatabasePort));
            }
            catch (Exception exception)
            {
                Logger.LogError($"Database Crashed, reason: {exception.Message}");
            }
            return mySqlConnection;
        }

        public bool AddNewPlayer(string playerId, int balance)
        {
            try
            {
                MySqlConnection mySqlConnection = CreateConnection();
                MySqlCommand mySqlCommand = mySqlConnection.CreateCommand();

                mySqlCommand.CommandText = string.Concat("Insert into `", _uconomy.Configuration.Instance.UconomyTableName, "` (`steamId`, `balance`, `lastUpdated`) VALUES ('", playerId, "', '", balance, "', '", DateTime.Now.ToShortDateString(), "');");
                mySqlConnection.Open();
                int affected = mySqlCommand.ExecuteNonQuery();
                mySqlConnection.Close();
                if (affected > 0)
                {
                    return true;
                }

                return false;
            }
            catch (Exception exception)
            {
                Logger.LogError($"Database Crashed, reason: {exception.Message}");
                return false;
            }
        }
    }
}