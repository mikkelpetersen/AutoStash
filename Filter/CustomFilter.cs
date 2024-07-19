using System.Collections.Generic;
using ItemFilterLibrary;

namespace AutoStash;

public class CustomFilter : BaseFilter
{
    public string Name { get; init; }
    public List<Filter> Filters { get; init; } = [];

    public class Filter
    {
        public string Name { get; init; }
        public bool? Shifting { get; init; }
        public bool? Affinity { get; init; }
        public bool? Stackable { get; init; }
        public string RawQuery { get; set; }
        public ItemQuery CompiledQuery { get; init; }
        public FilterNode FilterSettings { get; set; }
        public bool AllowProcess => FilterSettings.Enabled && FilterSettings.Index != -1;
    }
}