using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Cache;
using ImGuiNET;

namespace AutoStash;

public class AutoStash : BaseSettingsPlugin<AutoStashSettings>
{
    internal static AutoStash Instance;

    private readonly Scheduler _scheduler = new();

    private readonly TimeCache<List<string>> _tabNames;
    public List<CustomFilter> CurrentFilter = [];

    public AutoStash()
    {
        Name = "AutoStash";
        _tabNames = new TimeCache<List<string>>(() => Stash.TabNames, 1000);
    }

    public override bool Initialise()
    {
        Instance ??= this;

        ExileCore.Input.RegisterKey(Settings.RunHotkey);
        Settings.RunHotkey.OnValueChanged += () => { ExileCore.Input.RegisterKey(Settings.RunHotkey); };
        ExileCore.Input.RegisterKey(Settings.CancelHotkey);
        Settings.CancelHotkey.OnValueChanged += () => { ExileCore.Input.RegisterKey(Settings.CancelHotkey); };

        LoadFilters();
        Settings.FilterFile.OnValueSelected = _ => LoadFilters();

        return true;
    }

    public override void AreaChange(AreaInstance area)
    {
    }

    public override Job Tick()
    {
        if (!Stash.IsVisible || !Inventory.IsVisible)
            if (_scheduler.CurrentTask != null)
                Stop();

        if (Settings.CancelHotkey.PressedOnce()) Stop();

        if (Settings.RunHotkey.PressedOnce()) _scheduler.AddTask(Run(), "Run");

        return null;
    }

    public override void Render()
    {
        _scheduler.Run();
    }

    public override void DrawSettings()
    {
        if (ImGui.Button("Copy Inventory")) SaveIgnoredSlotsFromInventory();

        var number = 1;
        for (var i = 0; i < 5; i++)
        for (var j = 0; j < 12; j++)
        {
            var isEnabled = Convert.ToBoolean(Settings.IgnoredCells[i, j]);
            if (ImGui.Checkbox($"##{number}IgnoredInventoryCells", ref isEnabled)) Settings.IgnoredCells[i, j] ^= 1;

            if ((number - 1) % 12 < 11) ImGui.SameLine();

            number += 1;
        }

        ImGui.Separator();

        ImGui.NewLine();

        base.DrawSettings();

        ImGui.NewLine();

        foreach (var customFilter in CurrentFilter)
        {
            ImGui.TextColored(new Vector4(0f, 1f, 0.025f, 1f), customFilter.Name);

            ImGui.Separator();

            foreach (var filter in customFilter.Filters)
                if (Settings.FilterSettings.TryGetValue($"{customFilter.Name} - {filter.Name}",
                        out var filterSettings))
                {
                    var formattedString = $"{filter.Name}##{customFilter.Name + filter.Name}";
                    ImGui.Columns(2, formattedString, false);

                    var isEnabled = filterSettings.Enabled;

                    if (ImGui.Checkbox($"{filter.Name}##{customFilter.Name + filter.Name}", ref isEnabled))
                        filterSettings.Enabled = isEnabled;

                    ImGui.SameLine();
                    ImGui.NextColumn();

                    var stashItem = filterSettings.Index;

                    if (ImGui.Combo($"##{customFilter.Name + filter.Name}", ref stashItem,
                            _tabNames.Value.ToArray(), _tabNames.Value.Count))
                    {
                        filterSettings.Value = _tabNames.Value[stashItem];
                        filterSettings.Index = stashItem;
                    }

                    ImGui.NextColumn();
                }
                else
                {
                    filterSettings = new FilterNode { Value = "Ignore", Index = -1, Enabled = true };
                }

            ImGui.Separator();
        }
    }

    public override void EntityAdded(Entity entity)
    {
    }

    private void Stop()
    {
        _scheduler.Stop();
        _scheduler.Clear();
        _scheduler.AddTask(Cleanup(), "Cleanup");
    }

    private static async SyncTask<bool> Cleanup()
    {
        Input.LockController = false;

        await Input.KeyUp(Keys.LControlKey);
        await Input.KeyUp(Keys.ShiftKey);

        return true;
    }

    private static async SyncTask<bool> Run()
    {
        Input.LockController = true;

        if (!Stash.IsVisible)
            return false;

        var parsedInventoryItems = Inventory.ParseInventory()
            .OrderBy(x => x.Affinity || x.StashIndex == Stash.StashElement.IndexVisibleStash ? 0 : 1)
            .ThenBy(x => x.StashIndex).ToList();

        if (parsedInventoryItems.Count == 0)
            return true;

        await Input.KeyDown(Keys.LControlKey);

        foreach (var inventoryItem in parsedInventoryItems)
        {
            if (!inventoryItem.Affinity) await Stash.ClickTab(inventoryItem.StashIndex);

            // If the inventoryItem should be stashed while holding Shift, or if it's Stackable, then hold Shift.
            switch (inventoryItem.Shifting || inventoryItem.Stackable)
            {
                case true when !ExileCore.Input.IsKeyDown(Keys.ShiftKey):
                    await Input.KeyDown(Keys.ShiftKey);
                    break;
                case false when ExileCore.Input.IsKeyDown(Keys.ShiftKey):
                    await Input.KeyUp(Keys.ShiftKey);
                    break;
            }

            await Input.Click(inventoryItem.ClickPosition);
        }

        if (ExileCore.Input.IsKeyDown(Keys.ShiftKey)) await Input.KeyUp(Keys.ShiftKey);

        await Input.KeyUp(Keys.LControlKey);

        await Cleanup();

        return true;
    }

    private void SaveIgnoredSlotsFromInventory()
    {
        Settings.IgnoredCells = new[,]
        {
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        try
        {
            foreach (var inventoryItem in Inventory.InventoryItems)
            {
                var itemBase = inventoryItem.Item?.GetComponent<Base>();

                if (itemBase == null)
                    continue;

                var itemX = itemBase.ItemCellsSizeX;
                var itemY = itemBase.ItemCellsSizeY;
                var itemPosX = inventoryItem.PosX;
                var itemPosY = inventoryItem.PosY;

                for (var i = 0; i < itemY; i++)
                for (var j = 0; j < itemX; j++)
                    Settings.IgnoredCells[itemPosY + i, itemPosX + j] = 1;
            }
        }
        catch (Exception e)
        {
            Log.Error($"{e.Message}");
        }
    }

    private void LoadFilters()
    {
        var configDirectory = Path.Combine(ConfigDirectory);

        if (!Directory.Exists(configDirectory))
        {
            Directory.CreateDirectory(configDirectory);
            return;
        }

        var directoryInfo = new DirectoryInfo(configDirectory);
        Settings.FilterFile.Values = directoryInfo.GetFiles("*.ifl")
            .Select(x => Path.GetFileNameWithoutExtension(x.Name)).ToList();

        if (Settings.FilterFile.Values.Count != 0 && !Settings.FilterFile.Values.Contains(Settings.FilterFile.Value))
            Settings.FilterFile.Value = Settings.FilterFile.Values.First();

        if (!string.IsNullOrWhiteSpace(Settings.FilterFile.Value))
        {
            var filePath = Path.Combine(configDirectory, $"{Settings.FilterFile.Value}.ifl");
            if (File.Exists(filePath))
            {
                CurrentFilter = Parser.Load(filePath);

                foreach (var customFilter in CurrentFilter)
                foreach (var filter in customFilter.Filters)
                {
                    if (!Settings.FilterSettings.TryGetValue($"{customFilter.Name} - {filter.Name}",
                            out var filterSettings))
                    {
                        filterSettings = new FilterNode { Value = "Ignore", Index = -1, Enabled = true };
                        Settings.FilterSettings.Add($"{customFilter.Name} - {filter.Name}", filterSettings);
                    }

                    filter.FilterSettings = filterSettings;
                }
            }
            else
            {
                Log.Error($"Filter File {Settings.FilterFile.Value}.ifl does not exist.");
            }
        }
    }
}