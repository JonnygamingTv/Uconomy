using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace ZaupShop.Commands
{
    public class Cost : IRocketCommand
    {
        public string Name => "cost";

        public string Syntax => "[v] <name or id>";

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Help => "Tells you the cost of a selected item.";

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "zaupshop.cost" };
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
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>UnturnedChat.Say(caller, ZaupShop.instance.Translate("commnad_error_null")));
                    return;
                }

                if (command.Length == 0 || command.Length == 1 && (command[0].Trim() == string.Empty || command[0].Trim() == "v"))
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>UnturnedChat.Say(caller, ZaupShop.instance.Translate("cost_command_usage")));
                    return;
                }
                if (command.Length == 2 && (command[0] != "v" || command[1].Trim() == string.Empty))
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>UnturnedChat.Say(caller, ZaupShop.instance.Translate("cost_command_usage")));
                    return;
                }

                ushort id;
                switch (command[0])
                {
                    case "v":
                        string name = null;
                        ushort vehicleId;
                        if (command[1] != null)
                        {
                            vehicleId = ushort.Parse(command[1].ToString());
                        }
                        else
                        {
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>UnturnedChat.Say(caller, ZaupShop.instance.Translate("cost_command_usage")));
                            return;
                        }

                        foreach (VehicleAsset vAsset in Assets.find(EAssetType.VEHICLE).Cast<VehicleAsset>())
                        {
                            if (vAsset != null && vAsset.vehicleName != null && vAsset.vehicleName.ToLower().Contains(command[1].ToLower()))
                            {
                                vehicleId = vAsset.id;
                                name = vAsset.vehicleName;
                                break;
                            }
                        }
                        if (Assets.find(EAssetType.VEHICLE, vehicleId) == null)
                        {
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>UnturnedChat.Say(caller, ZaupShop.instance.Translate("could_not_find", command[1])));
                            return;
                        }
                        else if (name == null && vehicleId != 0)
                        {
                            name = ((VehicleAsset)Assets.find(EAssetType.VEHICLE, vehicleId)).vehicleName;
                        }
                        decimal cost = ZaupShop.instance.Database.GetVehicleCost(vehicleId);
                        if (cost <= 0m)
                        {
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>UnturnedChat.Say(caller, ZaupShop.instance.Translate("error_getting_cost", name)));
                        }
                        else
                        {
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>UnturnedChat.Say(caller, ZaupShop.instance.Translate("vehicle_cost_msg", name, cost.ToString(CultureInfo.CurrentCulture), ZaupShop.instance.Configuration.Instance.UconomyCurrencyName)));
                        }
                        break;
                    default:
                        name = null;
                        if (!ushort.TryParse(command[0], out id))
                        {
                            Asset[] array = Assets.find(EAssetType.ITEM);
                            Asset[] array2 = array;
                            for (int i = 0; i < array2.Length; i++)
                            {
                                ItemAsset iAsset = (ItemAsset)array2[i];
                                if (iAsset != null && iAsset.itemName != null && iAsset.itemName.ToLower().Contains(command[0].ToLower()))
                                {
                                    id = iAsset.id;
                                    name = iAsset.itemName;
                                    break;
                                }
                            }
                        }
                        if (Assets.find(EAssetType.ITEM, id) == null)
                        {
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>UnturnedChat.Say(caller, ZaupShop.instance.Translate("could_not_find", command[0])));
                            return;
                        }
                        else if (name == null && id != 0)
                        {
                            name = ((ItemAsset)Assets.find(EAssetType.ITEM, id)).itemName;
                        }
                        cost = ZaupShop.instance.Database.GetItemCost(id);
                        decimal bbp = ZaupShop.instance.Database.GetItemBuyPrice(id);
                        if (cost <= 0m)
                        {
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>UnturnedChat.Say(caller, ZaupShop.instance.Translate("error_getting_cost", name)));
                        }
                        else
                        {
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>UnturnedChat.Say(caller, ZaupShop.instance.Translate("item_cost_msg", name, cost.ToString(CultureInfo.CurrentCulture), ZaupShop.instance.Configuration.Instance.UconomyTableName, bbp.ToString(CultureInfo.CurrentCulture), ZaupShop.instance.Configuration.Instance.UconomyTableName)));
                        }
                        break;
                }
            });
        }
    }
}