using ItemFilterLibrary;

namespace AutoStash;

public class BaseFilter : IIFilter
{
    public bool CompareItem(ItemData itemData, ItemQuery itemFilter)
    {
        return itemFilter.Matches(itemData);
    }
}