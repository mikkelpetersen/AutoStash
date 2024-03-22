using System.Collections.Generic;
using System.Windows.Forms;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace AutoStash;

public class AutoStashSettings : ISettings
{
    public ListNode FilterFile { get; set; } = new();

    public HotkeyNode RunHotkey { get; set; } = new(Keys.F9);

    public HotkeyNode CancelHotkey { get; set; } = new(Keys.F10);

    public int[,] IgnoredCells { get; set; } =
    {
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
    };

    public ListNode LogLevel { get; set; } = new()
    {
        Values = ["None", "Debug", "Error"],
        Value = "Error"
    };

    public Dictionary<string, FilterNode> FilterSettings { get; set; } = new();
    public ToggleNode Enable { get; set; } = new(false);
}