using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace Uconomy
{
    public class UconomyPlugin : RocketPlugin<UconomyConfiguration>
    {
        public static UconomyPlugin instance { get; private set; }
        public DatabaseMgr Database;
        protected override void Load()
        {
            instance = this;
            Database = new(this);
            if(!Configuration.Instance.xpMode) U.Events.OnPlayerConnected += OnPlayerConnected;
            else if (Configuration.Instance.InitialBalance != 0) U.Events.OnPlayerConnected += OnPlayerConnected2;
            instance = this;
            Logger.Log("Uconomy instanciated, restored by LeandroTheDev");
        }
        protected override void Unload()
        {
            Database.Close();
            Database = null;
            if (!Configuration.Instance.xpMode) U.Events.OnPlayerConnected -= OnPlayerConnected;
            else if (Configuration.Instance.InitialBalance != 0) U.Events.OnPlayerConnected -= OnPlayerConnected2;
            Logger.Log("Uconomy unloaded.");
        }

        private async void OnPlayerConnected(UnturnedPlayer player)
        {
            // Add player if not exist
            await System.Threading.Tasks.Task.Run(() =>
            {
                Database.AddNewPlayer(player.Id, Configuration.Instance.InitialBalance);
                if (Configuration.Instance.BalanceFgEffectKey != 0)
                {
                    string bal = Database.GetBalance(player.Id).ToString();
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>EffectManager.sendUIEffect(Configuration.Instance.BalanceFgEffectId, Configuration.Instance.BalanceFgEffectKey, true, bal));
                }
            });
        }
        private void OnPlayerConnected2(UnturnedPlayer player)
        {
            if(player.Experience == 0) player.Experience = (uint)Configuration.Instance.InitialBalance;
            if (Configuration.Instance.BalanceFgEffectKey != 0)
            {
                EffectManager.sendUIEffect(Configuration.Instance.BalanceFgEffectId, Configuration.Instance.BalanceFgEffectKey, true, Database.GetBalance(player.Id).ToString());
            }
        }

        public override TranslationList DefaultTranslations => new TranslationList
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