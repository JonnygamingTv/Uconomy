using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections;
using System.Threading.Tasks;

namespace fr34kyn01535.Uconomy
{
    public class Uconomy : RocketPlugin<UconomyConfiguration>
    {
        public static Uconomy Instance;
        public DatabaseManager Database;
        System.Threading.Timer Offload;
        protected override void Load()
        {
            Instance = this;
            Database = new(this);
            if(!Configuration.Instance.xpMode) U.Events.OnPlayerConnected += OnPlayerConnected;
            else if (Configuration.Instance.InitialBalance != 0) U.Events.OnPlayerConnected += OnPlayerConnected2;
            if(Configuration.Instance.SalaryInterval != 0)
            {
                if (Configuration.Instance.FirstOrLastSalaryPermission == FindFirstorLast.FIRST)
                {
                    Offload = new System.Threading.Timer(_ => Salloop(), null, System.TimeSpan.FromSeconds(Configuration.Instance.SalaryInterval), System.TimeSpan.FromSeconds(Configuration.Instance.SalaryInterval));
                }
                else
                {
                    Offload = new System.Threading.Timer(_ => SalloopB(), null, System.TimeSpan.FromSeconds(Configuration.Instance.SalaryInterval), System.TimeSpan.FromSeconds(Configuration.Instance.SalaryInterval));
                }
            }
            Instance = this;
            Logger.Log("Uconomy loaded, restored by LeandroTheDev, modified by JonHosting.com");
        }
        protected override void Unload()
        {
            Database.Close();
            Database = null;
            if (!Configuration.Instance.xpMode) U.Events.OnPlayerConnected -= OnPlayerConnected;
            else if (Configuration.Instance.InitialBalance != 0) U.Events.OnPlayerConnected -= OnPlayerConnected2;
            StopAllCoroutines();
            Offload?.Dispose();
            Logger.Log("Uconomy unloaded.");
        }

        public void SendUI(UnturnedPlayer player, string bal = "0") {
            EffectAsset _asset = (EffectAsset)Assets.find(EAssetType.EFFECT, Configuration.Instance.BalanceFgEffectId);
            if (Configuration.Instance.UIColor!="") {
                EffectManager.SendUIEffect(_asset, Configuration.Instance.BalanceFgEffectKey, player.Player.channel.GetOwnerTransportConnection(), true, Configuration.Instance.UIColor, Configuration.Instance.CurrencySymbol, bal);
            }
            else
            {
                EffectManager.SendUIEffect(_asset, Configuration.Instance.BalanceFgEffectKey, player.Player.channel.GetOwnerTransportConnection(), true, Configuration.Instance.CurrencySymbol, bal);
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
                    EffectAsset _asset = (EffectAsset)Assets.find(EAssetType.EFFECT, Configuration.Instance.BalanceBgEffectId);
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>
                    {
                        EffectManager.SendUIEffect(_asset, Configuration.Instance.BalanceBgEffectKey, player.Player.channel.GetOwnerTransportConnection(), true, Configuration.Instance.CurrencySymbol, bal);
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
                EffectAsset _asset = (EffectAsset)Assets.find(EAssetType.EFFECT, Configuration.Instance.BalanceBgEffectId);
                EffectManager.SendUIEffect(_asset, Configuration.Instance.BalanceBgEffectKey, player.Player.channel.GetOwnerTransportConnection(), true, Configuration.Instance.CurrencySymbol, bal);
                SendUI(player, bal);
            }
        }

        private void Salloop()
        {
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
        }
        private void SalloopB()
        {
            foreach (SteamPlayer client in Provider.clients)
            {
                UnturnedPlayer P = UnturnedPlayer.FromSteamPlayer(client);
                Rocket.API.Serialisation.Permission Salary = P.GetPermissions().FindLast(perm => perm.Name.Length > 19 && perm.Name.Substring(0, 19) == "avi.economy.salary.");
                if (Salary != null && decimal.TryParse(Salary.Name.Substring(19), out decimal Sal))
                {
                    Database.AddBalance(P.Id, Sal);
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => Rocket.Unturned.Chat.UnturnedChat.Say(P, Translate("salary_received", Sal.ToString())));
                }
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
            {"command_pay_other_private", "You received a payment of {0} {1} from {2}"},
            {"salary_received", "You received your salary of {0}." }
        };

        internal void BalanceUpdated(string steamId, decimal amt)
        {
            if (OnBalanceUpdate == null)
            {
                return;
            }
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(new Steamworks.CSteamID(ulong.Parse(steamId)));
            OnBalanceUpdate(player, amt);
        }
        internal void OnBalanceChecked(string steamId, decimal balance)
        {
            if (OnBalanceCheck == null)
            {
                return;
            }
            UnturnedPlayer player = UnturnedPlayer.FromCSteamID(new Steamworks.CSteamID(ulong.Parse(steamId)));
            OnBalanceCheck(player, balance);
        }
        internal void HasBeenPayed(UnturnedPlayer sender, string receiver, decimal amt)
        {
            PlayerPay onPlayerPay = OnPlayerPay;
            if (onPlayerPay == null)
            {
                return;
            }
            onPlayerPay(sender, receiver, amt);
        }

        // Token: 0x14000001 RID: 1
        // (add) Token: 0x0600001B RID: 27 RVA: 0x000029BC File Offset: 0x00000BBC
        // (remove) Token: 0x0600001C RID: 28 RVA: 0x000029F4 File Offset: 0x00000BF4
        public event PlayerBalanceUpdate OnBalanceUpdate;

        // Token: 0x14000002 RID: 2
        // (add) Token: 0x0600001D RID: 29 RVA: 0x00002A2C File Offset: 0x00000C2C
        // (remove) Token: 0x0600001E RID: 30 RVA: 0x00002A64 File Offset: 0x00000C64
        public event PlayerBalanceCheck OnBalanceCheck;

        // Token: 0x14000003 RID: 3
        // (add) Token: 0x0600001F RID: 31 RVA: 0x00002A9C File Offset: 0x00000C9C
        // (remove) Token: 0x06000020 RID: 32 RVA: 0x00002AD4 File Offset: 0x00000CD4
        public event PlayerPay OnPlayerPay;
        // Token: 0x02000007 RID: 7
        // (Invoke) Token: 0x06000028 RID: 40
        public delegate void PlayerBalanceUpdate(UnturnedPlayer player, decimal amt);

        // Token: 0x02000008 RID: 8
        // (Invoke) Token: 0x0600002C RID: 44
        public delegate void PlayerBalanceCheck(UnturnedPlayer player, decimal balance);

        // Token: 0x02000009 RID: 9
        // (Invoke) Token: 0x06000030 RID: 48
        public delegate void PlayerPay(UnturnedPlayer sender, string receiver, decimal amt);
    }
}