using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;

namespace Uconomy
{
    public class UconomyPlugin : RocketPlugin<UconomyConfiguration>
    {
        public static UconomyPlugin instance;
        public DatabaseMgr Database;
        public override void LoadPlugin()
        {
            Database = new(this);
            base.LoadPlugin();
            U.Events.OnPlayerConnected += OnPlayerConnected;
            instance = this;
        }

        private void OnPlayerConnected(UnturnedPlayer player)
        {
            // Add player if not exist
            Database.AddNewPlayer(player.Id, Configuration.Instance.InitialBalance);
        }

        public override TranslationList DefaultTranslations => new()
        {
            {"command_balance_show", "Your current balance is: {0} {1}"},
            {"command_pay_invalid", "Invalid arguments"},
            {"command_pay_error_pay_self", "You cant pay yourself"},
            {"command_pay_error_invalid_amount", "Invalid amount"},
            {"command_pay_error_cant_afford", "Your balance does not allow this payment"},
            {"command_pay_error_player_not_found", "Failed to find player"},
            {"command_pay_private", "You paid {0} to {1}"},
            {"command_pay_console", "You received a payment of {0} from {1} "},
            {"command_pay_other_private", "You received a payment of {0} {1} from {2}"}
        };
    }
}