using System;
using System.Collections.Generic;
using System.Linq;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ItemFilterLibrary;
using static ExileCore.PoEMemory.MemoryObjects.ServerInventory;

namespace AutoStash;

public class Inventory
{
    private static readonly AutoStash Instance = AutoStash.Instance;

    public static bool IsVisible => InventoryElement is { IsVisible: true };

    public static IList<InventSlotItem> InventoryItems => ServerInventory.InventorySlotItems;

    private static ServerInventory ServerInventory => Instance.GameController.IngameState.Data.ServerData
        .PlayerInventories[(int)InventorySlotE.MainInventory1].Inventory;

    private static InventoryElement InventoryElement =>
        Instance.GameController.Game.IngameState.IngameUi.InventoryPanel;

    public static List<FilterResult> ParseInventory()
    {
        var parsedItems = new List<FilterResult>();

        if (!IsVisible)
            return parsedItems;

        var sortedInventoryItems = InventoryItems
            .OrderBy(item => item.PosX)
            .ThenBy(item => item.PosY)
            .ToList();

        foreach (var inventoryItem in sortedInventoryItems)
        {
            if (inventoryItem.Item == null || inventoryItem.Address == 0)
                continue;

            if (Instance.Settings.IgnoredCells[inventoryItem.PosY, inventoryItem.PosX] == 1)
                continue;

            var testItem = new ItemData(inventoryItem.Item, Instance.GameController);

            foreach (var customFilter in Instance.CurrentFilter)
            foreach (var filter in customFilter.Filters)
                try
                {
                    if (!filter.AllowProcess)
                        continue;

                    if (customFilter.CompareItem(testItem, filter.CompiledQuery))
                        parsedItems.Add(new FilterResult(filter,
                            inventoryItem.GetClientRect().Center.ToVector2Num()));
                }
                catch (Exception e)
                {
                    Log.Error($"{e.Message}");
                }
        }

        return parsedItems;
    }
}