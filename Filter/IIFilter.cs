using ItemFilterLibrary;

namespace AutoStash;

public interface IIFilter
{
    bool CompareItem(ItemData itemData, ItemQuery filterData);
}