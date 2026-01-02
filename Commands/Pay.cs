using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;

namespace fr34kyn01535.Uconomy.Commands
{
    public class Pay : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "pay";

        public string Help => "Transfer your currency to a player";

        public string Syntax => "/pay [PlayerName] [Amount]";

        public List<string> Aliases => new System.Collections.Generic.List<string> { "payplayer", "pagar" };

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "uconomy.pay" };
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
                // Get paying player
                UnturnedPlayer payingPlayer = caller as UnturnedPlayer ?? null;
                if (payingPlayer is null)
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>UnturnedChat.Say(caller, Uconomy.Instance.Translate("commnad_error_null")));
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
                bool success;
                try
                {
                    success = Uconomy.Instance.Database.PlayerPayPlayer(payingPlayer.Id, receivedPlayer.Id, decimal.Parse(command[1]));
                }
                catch (Exception)
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>UnturnedChat.Say(payingPlayer, Uconomy.Instance.Translate("command_pay_error_invalid_amount")));
                    return;
                }

                // Check if player has sufficient balance
                if (success)
                {
                    // Inform paying player
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>UnturnedChat.Say(payingPlayer, Uconomy.Instance.Translate("command_pay_private", command[1], receivedPlayer.DisplayName)));
                    // Inform received player
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>UnturnedChat.Say(receivedPlayer, Uconomy.Instance.Translate("command_pay_console", command[1], payingPlayer.DisplayName)));
                }
                else
                {
                    // Inform error
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>UnturnedChat.Say(payingPlayer, Uconomy.Instance.Translate("command_pay_error_cant_afford")));
                }
            });
        }
    }
}
