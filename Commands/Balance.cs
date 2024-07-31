using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;

namespace Uconomy.Commands
{
    public class Balance : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "balance";

        public string Help => "Shows your balance";

        public string Syntax => "/balance";

        public List<string> Aliases => new();

        public List<string> Permissions => new();

        public async void Execute(IRocketPlayer caller, string[] command)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                UnturnedPlayer player = caller as UnturnedPlayer ?? null;
                if (player is null)
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>UnturnedChat.Say(caller, UconomyPlugin.instance.Translate("command_error_null")));
                }
                decimal bal = UconomyPlugin.instance.Database.GetBalance(player.Id);
                Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>UnturnedChat.Say(caller, UconomyPlugin.instance.Translate("command_balance_show", bal, UconomyPlugin.instance.Configuration.Instance.UconomyCurrencyName)));
            });
        }
    }
}
