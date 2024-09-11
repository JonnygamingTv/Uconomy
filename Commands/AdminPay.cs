using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;

namespace fr34kyn01535.Uconomy.Commands
{
    public class AdminPay : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "adminpay";

        public string Help => "Transfer money to a player";

        public string Syntax => "/apay [PlayerName] [Amount]";

        public List<string> Aliases => new System.Collections.Generic.List<string> { "apay" };

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "uconomy.apay" };
            }
        }

        public async void Execute(IRocketPlayer caller, string[] command)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                if (command.Length != 2)
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>UnturnedChat.Say(caller, Uconomy.Instance.Translate("command_pay_invalid")));
                    return;
                }
                // Get Received player
                UnturnedPlayer receivedPlayer = UnturnedPlayer.FromName(command[0]);
                if (receivedPlayer is null)
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>UnturnedChat.Say(caller, Uconomy.Instance.Translate("command_pay_error_player_not_found")));
                    return;
                }
                // Try to pay
                try
                {
                    Uconomy.Instance.Database.AddBalance(receivedPlayer.Id, decimal.Parse(command[1]));
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => UnturnedChat.Say(caller, Uconomy.Instance.Translate("command_pay_private", command[1], receivedPlayer.DisplayName)));
                    // Inform received player
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => UnturnedChat.Say(receivedPlayer, Uconomy.Instance.Translate("command_pay_console", command[1], caller.DisplayName)));
                }
                catch (Exception)
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>UnturnedChat.Say(caller, Uconomy.Instance.Translate("command_pay_error_invalid_amount")));
                    return;
                }
            });
        }
    }
}
