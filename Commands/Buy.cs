using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;

namespace ZaupShop.Commands
{
    public class Buy : IRocketCommand
    {
        public string Name => "buy";

        public string Syntax => "[v.]<name or id> [amount] [25 | 50 | 75 | 100]";

        public string Permission => null;

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Help => "Buy something";

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "zaupshop.buy" };
            }
        }

        List<string> IRocketCommand.Aliases
        {
            get { return new List<string>(); }
        }

        public async void Execute(IRocketPlayer caller, string[] command)
        {
            await System.Threading.Tasks.Task.Run(() =>
            {
                UnturnedPlayer player = caller as UnturnedPlayer ?? null;
                if (player is null)
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => UnturnedChat.Say(caller, ZaupShop.instance.Translate("commnad_error_null")));
                    return;
                }
                //#region tarned
                //// In tarned context you can buy only in certain locations
                //var position = player.Position;
                //// Verify X position
                //if (position.x < 431.41 || position.x > 440.92)
                //{
                //    UnturnedChat.Say(caller, ZaupShop.instance.Translate("buy_not_safezone"));
                //    return;
                //}
                //// Verify Y position
                //else if (position.y < 51.00 || position.y > 54.00)
                //{
                //    UnturnedChat.Say(caller, ZaupShop.instance.Translate("buy_not_safezone"));
                //    return;
                //}
                //// Verifiy Z position
                //else if (position.z < 437.25 || position.z > 447.05)
                //{
                //    UnturnedChat.Say(caller, ZaupShop.instance.Translate("buy_not_safezone"));
                //    return;
                //}
                //#endregion

                if (command.Length == 0)
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => UnturnedChat.Say(caller, ZaupShop.instance.Translate("buy_command_usage")));
                    return;
                }

                byte amttobuy = 1;
                if (command.Length > 1)
                {
                    if (!byte.TryParse(command[1], out amttobuy))
                    {
                        Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => UnturnedChat.Say(caller, ZaupShop.instance.Translate("invalid_amt")));
                        return;
                    }
                }

                string[] components = Parser.getComponentsFromSerial(command[0], '.');
                if (components.Length == 2 && components[0].Trim() != "v" || components.Length == 1 && components[0].Trim() == "v" || components.Length > 2 || command[0].Trim() == string.Empty)
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => UnturnedChat.Say(caller, ZaupShop.instance.Translate("buy_command_usage")));
                    return;
                }
                switch (components[0])
                {
                    case "v":
                        if (!ZaupShop.instance.Configuration.Instance.CanBuyVehicles)
                        {
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => UnturnedChat.Say(caller, ZaupShop.instance.Translate("buy_vehicles_off")));
                            return;
                        }
                        string name = null;
                        ushort vehicleId;
                        if (command[1] != null)
                        {
                            vehicleId = ushort.Parse(command[1].ToString());
                        }
                        else
                        {
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => UnturnedChat.Say(caller, ZaupShop.instance.Translate("buy_command_usage")));
                            return;
                        }

                        foreach (VehicleAsset vAsset in Assets.find(EAssetType.VEHICLE).Cast<VehicleAsset>())
                        {
                            if (vAsset?.vehicleName != null && vAsset.vehicleName.ToLower().Contains(components[1].ToLower()))
                            {
                                vehicleId = vAsset.id;
                                name = vAsset.vehicleName;
                                break;
                            }
                        }

                        if (Assets.find(EAssetType.VEHICLE, vehicleId) == null)
                        {
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => UnturnedChat.Say(caller, ZaupShop.instance.Translate("could_not_find", components[1])));
                            return;
                        }
                        else if (name == null && vehicleId != 0)
                        {
                            name = ((VehicleAsset)Assets.find(EAssetType.VEHICLE, vehicleId)).vehicleName;
                        }
                        decimal cost = ZaupShop.instance.Database.GetVehicleCost(vehicleId);

                        decimal balance = ZaupShop.instance.Database.GetBalance(player.Id);

                        if (cost <= 0m)
                        {
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => UnturnedChat.Say(caller, ZaupShop.instance.Translate("vehicle_not_available", name)));
                            return;
                        }
                        if (balance < cost)
                        {
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => UnturnedChat.Say(caller, ZaupShop.instance.Translate("not_enough_currency_msg", ZaupShop.instance.Configuration.Instance.UconomyCurrencyName, "1", name)));
                            return;
                        }
                        Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>
                        {
                            if (!player.GiveVehicle(vehicleId))
                            {
                                UnturnedChat.Say(caller, ZaupShop.instance.Translate("error_giving_item", name));
                                return;
                            }
                        });

                        ZaupShop.instance.Database.RemoveBalance(player.Id, cost);
                        var newBal = ZaupShop.instance.Database.GetBalance(player.Id);

                        Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => UnturnedChat.Say(caller, ZaupShop.instance.Translate("vehicle_buy_msg", name, cost, ZaupShop.instance.Configuration.Instance.UconomyCurrencyName, newBal, ZaupShop.instance.Configuration.Instance.UconomyCurrencyName)));
                        return;
                    default:
                        if (!ZaupShop.instance.Configuration.Instance.CanBuyItems)
                        {
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => UnturnedChat.Say(caller, ZaupShop.instance.Translate("buy_items_off")));
                            return;
                        }
                        name = null;
                        ushort itemId;
                        if (command[0] != null)
                        {
                            itemId = ushort.Parse(command[1].ToString());
                        }
                        else
                        {
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => UnturnedChat.Say(caller, ZaupShop.instance.Translate("buy_command_usage")));
                            return;
                        }

                        foreach (ItemAsset vAsset in Assets.find(EAssetType.ITEM).Cast<ItemAsset>())
                        {
                            if (vAsset?.itemName != null && vAsset.itemName.ToLower().Contains(components[0].ToLower()))
                            {
                                itemId = vAsset.id;
                                name = vAsset.itemName;
                                break;
                            }
                        }
                        if (Assets.find(EAssetType.ITEM, itemId) == null)
                        {
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => UnturnedChat.Say(caller, ZaupShop.instance.Translate("could_not_find", components[0])));
                            return;
                        }
                        else if (name == null && itemId != 0)
                        {
                            name = ((ItemAsset)Assets.find(EAssetType.ITEM, itemId)).itemName;
                        }
                        cost = decimal.Round(ZaupShop.instance.Database.GetItemCost(itemId) * amttobuy, 2);
                        balance = ZaupShop.instance.Database.GetBalance(player.Id);

                        if (cost <= 0m)
                        {
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => UnturnedChat.Say(caller, ZaupShop.instance.Translate("item_not_available", name)));
                            return;
                        }
                        if (balance < cost)
                        {
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => UnturnedChat.Say(caller, ZaupShop.instance.Translate("not_enough_currency_msg", ZaupShop.instance.Configuration.Instance.UconomyCurrencyName, amttobuy, name)));
                            return;
                        }
                        Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>
                        {
                            if (!player.GiveItem(itemId, amttobuy))
                            {
                                UnturnedChat.Say(caller, ZaupShop.instance.Translate("error_giving_item", name));
                                return;
                            }
                        });
                        ZaupShop.instance.Database.RemoveBalance(player.Id, cost);
                        newBal = ZaupShop.instance.Database.GetBalance(player.Id);

                        Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => UnturnedChat.Say(caller, ZaupShop.instance.Translate("item_buy_msg", name, cost, ZaupShop.instance.Configuration.Instance.UconomyCurrencyName, newBal, ZaupShop.instance.Configuration.Instance.UconomyCurrencyName)));
                        return;
                }
            });
        }
    }
}