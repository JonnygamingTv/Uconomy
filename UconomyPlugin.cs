using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections;
using System.Threading.Tasks;

namespace Uconomy
{
    public class UconomyPlugin : RocketPlugin<UconomyConfiguration>
    {
        public static UconomyPlugin instance { get; private set; }
        public DatabaseMgr Database;
        Task Offload;
        protected override void Load()
        {
            instance = this;
            Database = new(this);
            if(!Configuration.Instance.xpMode) U.Events.OnPlayerConnected += OnPlayerConnected;
            else if (Configuration.Instance.InitialBalance != 0) U.Events.OnPlayerConnected += OnPlayerConnected2;
            if(Configuration.Instance.SalaryInterval != 0)
            {
                Offload = Task.Run(() => StartCoroutine(nameof(Salloop)));
            }
            instance = this;
            Logger.Log("Uconomy instanciated, restored by LeandroTheDev");
        }
        protected override void Unload()
        {
            Database.Close();
            Database = null;
            if (!Configuration.Instance.xpMode) U.Events.OnPlayerConnected -= OnPlayerConnected;
            else if (Configuration.Instance.InitialBalance != 0) U.Events.OnPlayerConnected -= OnPlayerConnected2;
            StopAllCoroutines();
            if (Configuration.Instance.SalaryInterval != 0) Offload.Dispose();
            Logger.Log("Uconomy unloaded.");
        }

        public void SendUI(UnturnedPlayer player, string bal = "0") {
            if (Configuration.Instance.UIColor!="") {
                EffectManager.sendUIEffect(Configuration.Instance.BalanceFgEffectId, Configuration.Instance.BalanceFgEffectKey, player.Player.channel.GetOwnerTransportConnection(), true, Configuration.Instance.UIColor, Configuration.Instance.CurrencySymbol, bal);
            }
            else
            {
                EffectManager.sendUIEffect(Configuration.Instance.BalanceFgEffectId, Configuration.Instance.BalanceFgEffectKey, player.Player.channel.GetOwnerTransportConnection(), true, Configuration.Instance.CurrencySymbol, bal);
            }
        }

        private async void OnPlayerConnected(UnturnedPlayer player)
        {
            // Add player if not exist
            await System.Threading.Tasks.Task.Delay(1500);
            await System.Threading.Tasks.Task.Run(() =>
            {
                Database.AddNewPlayer(player.Id, Configuration.Instance.InitialBalance);
                if (Configuration.Instance.BalanceFgEffectKey != 0)
                {
                    string bal = Database.GetBalance(player.Id).ToString();
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>
                    {
                        EffectManager.sendUIEffect(Configuration.Instance.BalanceBgEffectId, Configuration.Instance.BalanceBgEffectKey, player.Player.channel.GetOwnerTransportConnection(), true, Configuration.Instance.CurrencySymbol, bal);
                        SendUI(player, bal);
                        //EffectManager.sendUIEffectText(Configuration.Instance.BalanceFgEffectKey, player.Player.channel.GetOwnerTransportConnection(), true, player.CSteamID.ToString(), bal);
                    });
                }
            });
        }
        private void OnPlayerConnected2(UnturnedPlayer player)
        {
            if(player.Experience == 0) player.Experience = (uint)Configuration.Instance.InitialBalance;
            if (Configuration.Instance.BalanceFgEffectKey != 0)
            {
                string bal = Database.GetBalance(player.Id).ToString();
                EffectManager.sendUIEffect(Configuration.Instance.BalanceBgEffectId, Configuration.Instance.BalanceBgEffectKey, player.Player.channel.GetOwnerTransportConnection(), true, Configuration.Instance.CurrencySymbol, bal);
                SendUI(player, bal);
            }
        }

        private IEnumerator Salloop()
        {
            yield return new UnityEngine.WaitForSeconds(Configuration.Instance.SalaryInterval);
            foreach(SteamPlayer client in Provider.clients)
            {
                UnturnedPlayer P = UnturnedPlayer.FromSteamPlayer(client);
                Rocket.API.Serialisation.Permission Salary = P.GetPermissions().Find(perm => perm.Name.Length>19&& perm.Name.Substring(0, 19) == "avi.economy.salary.");
                if(Salary != null && decimal.TryParse(Salary.Name.Substring(19), out decimal Sal))
                {
                    Database.AddBalance(P.Id, Sal);
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => Rocket.Unturned.Chat.UnturnedChat.Say(P, Translate("salary_received", Sal.ToString())));
                }
            }
            StartCoroutine(nameof(Salloop));
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
            {"command_pay_other_private", "You received a payment of {0} {1} from {2}"},
            {"salary_received", "You received your salary of {0}." }
        };
    }
}