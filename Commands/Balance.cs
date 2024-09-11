using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;

namespace fr34kyn01535.Uconomy.Commands
{
    public class Balance : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "balance";

        public string Help => "Shows your balance";

        public string Syntax => "/balance";

        public List<string> Aliases => new System.Collections.Generic.List<string> { "bal" };

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "uconomy.balance" };
            }
        }

        public async void Execute(IRocketPlayer caller, string[] command)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                UnturnedPlayer player = caller as UnturnedPlayer ?? null;
                if (player is null)
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>UnturnedChat.Say(caller, Uconomy.Instance.Translate("command_error_null")));
                }
                decimal bal = Uconomy.Instance.Database.GetBalance(player.Id);
                Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>UnturnedChat.Say(caller, Uconomy.Instance.Translate("command_balance_show", bal, Uconomy.Instance.Configuration.Instance.UconomyCurrencyName)));
            });
        }
    }
}
