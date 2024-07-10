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

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = caller as UnturnedPlayer ?? null;
            if (player is null)
            {
                UnturnedChat.Say(caller, UconomyPlugin.instance.Translate("commnad_error_null"));
            }
            UnturnedChat.Say(caller, UconomyPlugin.instance.Translate("command_balance_show", UconomyPlugin.instance.Database.GetBalance(player.Id), UconomyPlugin.instance.Configuration.Instance.UconomyCurrencyName));
        }
    }
}
