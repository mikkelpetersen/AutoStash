using Vector2 = System.Numerics.Vector2;

namespace AutoStash;

public class FilterResult(CustomFilter.Filter filter, Vector2 clickPosition, string? itemName)
{
    public int StashIndex { get; } = filter.FilterSettings.Index;
    public string ItemName { get; } = itemName ?? string.Empty;
    public Vector2 ClickPosition { get; } = clickPosition;
    public bool Affinity { get; } = filter.Affinity ?? false;
    public bool Shifting { get; } = filter.Shifting ?? false;
    public bool Stackable { get; } = filter.Stackable ?? false;
}