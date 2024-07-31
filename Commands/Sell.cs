using System.Collections.Generic;
using System.Linq;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace ZaupShop.Commands
{
    public class Sell : IRocketCommand
    {
        public string Name => "sell";

        public string Syntax => "<name or id> [amount]";

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Help => "Allows you to sell items to the shop from your inventory.";

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "zaupshop.sell" };
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

                //#region tarned
                //// In tarned context you can sell only in certain locations
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

                if (command.Length == 0 || command.Length > 0 && command[0].Trim() == string.Empty)
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>UnturnedChat.Say(caller, ZaupShop.instance.Translate("sell_command_usage")));
                    return;
                }
                byte amttosell = 1;
                if (command.Length > 1)
                {
                    if (!byte.TryParse(command[1], out amttosell))
                    {
                        Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>UnturnedChat.Say(caller, ZaupShop.instance.Translate("invalid_amt")));
                        return;
                    }
                }
                byte amt = amttosell;
                if (!ZaupShop.instance.Configuration.Instance.CanSellItems)
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>UnturnedChat.Say(caller, ZaupShop.instance.Translate("sell_items_off")));
                    return;
                }

                ItemAsset asset = null;
                ushort itemId;
                if (command[0] != null)
                {
                    itemId = ushort.Parse(command[1].ToString());
                }
                else
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>UnturnedChat.Say(caller, ZaupShop.instance.Translate("sell_command_usage")));
                    return;
                }

                foreach (ItemAsset vAsset in Assets.find(EAssetType.ITEM).Cast<ItemAsset>())
                {
                    if (vAsset?.itemName != null && vAsset.itemName.ToLower().Contains(command[0].ToLower()))
                    {
                        asset = vAsset;
                        itemId = vAsset.id;
                        break;
                    }
                }
                if (Assets.find(EAssetType.ITEM, itemId) == null)
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>UnturnedChat.Say(caller, ZaupShop.instance.Translate("could_not_find", command[0])));
                    return;
                }

                asset = (ItemAsset)Assets.find(EAssetType.ITEM, itemId);

                if (player.Inventory.has(itemId) == null || asset == null)
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>UnturnedChat.Say(caller, ZaupShop.instance.Translate("not_have_item_sell", asset.itemName)));
                    return;
                }
                List<InventorySearch> list = player.Inventory.search(itemId, true, true);
                if (list.Count == 0 || asset.amount == 1 && list.Count < amttosell)
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>UnturnedChat.Say(caller, ZaupShop.instance.Translate("not_enough_items_sell", amttosell.ToString(), asset.itemName)));
                    return;
                }
                if (asset.amount > 1)
                {
                    int ammomagamt = 0;
                    foreach (InventorySearch ins in list)
                    {
                        ammomagamt += ins.jar.item.amount;
                    }
                    if (ammomagamt < amttosell)
                    {
                        Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>UnturnedChat.Say(caller, ZaupShop.instance.Translate("not_enough_ammo_sell", asset.itemName)));
                        return;
                    }
                }

                // We got this far, so let's buy back the items and give them money.
                // Get cost per item.  This will be whatever is set for most items, but changes for ammo and magazines.
                decimal price = ZaupShop.instance.Database.GetItemBuyPrice(itemId);
                if (price <= 0.00m)
                {
                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>UnturnedChat.Say(caller, ZaupShop.instance.Translate("no_sell_price_set", asset.itemName)));
                    return;
                }
                byte quality = 100;
                decimal addmoney = 0;
                decimal peritemprice;
                switch (asset.amount)
                {
                    case 1:
                        // These are single items, not ammo or magazines
                        while (amttosell > 0)
                        {
                            if (ZaupShop.instance.Configuration.Instance.QualityCounts)
                                quality = list[0].jar.item.durability;
                            peritemprice = decimal.Round(price * (quality / 100.0m), 2);
                            addmoney += peritemprice;
                            Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => player.Inventory.removeItem(list[0].page, player.Inventory.getIndex(list[0].page, list[0].jar.x, list[0].jar.y)));
                            list.RemoveAt(0);
                            amttosell--;
                        }
                        break;
                    default:
                        // This is ammo or magazines
                        byte amttosell1 = amttosell;
                        while (amttosell > 0)
                        {
                            if (list[0].jar.item.amount >= amttosell)
                            {
                                byte left = (byte)(list[0].jar.item.amount - amttosell);
                                list[0].jar.item.amount = left;
                                Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => player.Inventory.sendUpdateAmount(list[0].page, list[0].jar.x, list[0].jar.y, left));
                                amttosell = 0;
                                if (left == 0)
                                {
                                    Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() => player.Inventory.removeItem(list[0].page, player.Inventory.getIndex(list[0].page, list[0].jar.x, list[0].jar.y)));
                                    list.RemoveAt(0);
                                }
                            }
                            else
                            {
                                amttosell -= list[0].jar.item.amount;
                                Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>
                                {
                                    player.Inventory.sendUpdateAmount(list[0].page, list[0].jar.x, list[0].jar.y, 0);
                                    player.Inventory.removeItem(list[0].page, player.Inventory.getIndex(list[0].page, list[0].jar.x, list[0].jar.y));
                                });
                                list.RemoveAt(0);
                            }
                        }
                        peritemprice = decimal.Round(price * (amttosell1 / (decimal)asset.amount), 2);
                        addmoney += peritemprice;
                        break;
                }

                ZaupShop.instance.Database.AddBalance(player.Id, addmoney);
                decimal balance = ZaupShop.instance.Database.GetBalance(player.Id);
                Rocket.Core.Utils.TaskDispatcher.QueueOnMainThread(() =>UnturnedChat.Say(caller, ZaupShop.instance.Translate("sold_items", amt, asset.itemName, addmoney, ZaupShop.instance.Database.GetBalance(player.Id), balance, ZaupShop.instance.Database.GetBalance(player.Id))));
            });
        }
    }
}