using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;

namespace Uconomy.Commands
{
    public class Pay : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "pay";

        public string Help => "Transfer to a player your currency";

        public string Syntax => "/pay [PlayerName] [Amount]";

        public List<string> Aliases => new();

        public List<string> Permissions => new();

        public async void Execute(IRocketPlayer caller, string[] command)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                if (command.Length != 2)
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>UnturnedChat.Say(caller, UconomyPlugin.instance.Translate("command_pay_invalid")));
                    return;
                }
                // Get paying player
                UnturnedPlayer payingPlayer = caller as UnturnedPlayer ?? null;
                if (payingPlayer is null)
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>UnturnedChat.Say(caller, UconomyPlugin.instance.Translate("commnad_error_null")));
                    return;
                }
                // Get Received player
                UnturnedPlayer receivedPlayer = UnturnedPlayer.FromName(command[0]);
                if (receivedPlayer is null)
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>UnturnedChat.Say(caller, UconomyPlugin.instance.Translate("command_pay_error_player_not_found")));
                    return;
                }
                // Try to pay
                bool success;
                try
                {
                    success = UconomyPlugin.instance.Database.PlayerPayPlayer(payingPlayer.Id, receivedPlayer.Id, decimal.Parse(command[1]));
                }
                catch (Exception)
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>UnturnedChat.Say(payingPlayer, UconomyPlugin.instance.Translate("command_pay_error_invalid_amount")));
                    return;
                }

                // Check if player has sufficient balance
                if (success)
                {
                    // Inform paying player
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>UnturnedChat.Say(payingPlayer, UconomyPlugin.instance.Translate("command_pay_private", command[1], command[0])));
                    // Inform received player
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>UnturnedChat.Say(receivedPlayer, UconomyPlugin.instance.Translate("command_pay_console", command[1], payingPlayer.DisplayName)));
                }
                else
                {
                    // Inform error
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(()=>UnturnedChat.Say(payingPlayer, UconomyPlugin.instance.Translate("command_pay_error_cant_afford")));
                }
            });
        }
    }
}
