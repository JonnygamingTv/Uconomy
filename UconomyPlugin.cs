using Rocket.API.Collections;
using Rocket.Core.Plugins;
using SDG.Unturned;

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
            Provider.onClientConnected += OnPlayerConnected;
            instance = this;
        }

        private void OnPlayerConnected()
        {

        }

        public override TranslationList DefaultTranslations => new()
        {
            {"command_balance_show", "Your current balance is: {0} {1}"},
            {"command_pay_invalid", "Invalid arguments"},
            {"command_pay_error_pay_self", "You cant pay yourself"},
            {"command_pay_error_invalid_amount", "Invalid amount"},
            {"command_pay_error_cant_afford", "Your balance does not allow this payment"},
            {"command_pay_error_player_not_found", "Failed to find player"},
            {"command_pay_private", "You paid {0} {1} {2}"},
            {"command_pay_console", "You received a payment of {0} {1} "},
            {"command_pay_other_private", "You received a payment of {0} {1} from {2}"}
        };
    }
}