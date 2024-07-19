using System;
using System.Collections.Generic;
using System.IO;
using ItemFilterLibrary;
using Newtonsoft.Json;

namespace AutoStash;

public class Parser
{
    public static List<CustomFilter> Load(string filePath)
    {
        var allFilters = new List<CustomFilter>();
        try
        {
            var fileContents = File.ReadAllText(filePath);
            var filterConfig = JsonConvert.DeserializeObject<FilterConfiguration>(fileContents);

            if (filterConfig == null)
            {
                Log.Error("Deserialized Filter Configuration is NULL.");
                return allFilters;
            }

            foreach (var category in filterConfig.Categories)
            {
                var newCategory = new CustomFilter
                {
                    Name = category.Name,
                    Filters = new List<CustomFilter.Filter>()
                };

                foreach (var filter in category.Filters)
                {
                    var rawQuery = string.Join("", filter.RawQuery).Replace("\n", "");
                    var compiledQuery = ItemQuery.Load(rawQuery);

                    if (compiledQuery.FailedToCompile)
                    {
                        Log.Error($"Unable to Parse JSON. Category: {category.Name}. Filter: {filter.Name}");
                        continue;
                    }

                    newCategory.Filters.Add(new CustomFilter.Filter
                    {
                        Name = filter.Name,
                        Shifting = filter.Shifting ?? false,
                        Affinity = filter.Affinity ?? false,
                        Stackable = filter.Stackable ?? false,
                        RawQuery = rawQuery,
                        CompiledQuery = compiledQuery
                    });
                }

                if (newCategory.Filters.Count > 0) allFilters.Add(newCategory);
            }
        }
        catch (Exception e)
        {
            Log.Error($"Failed to Load Filter Configuration:\n{e.Message}");
        }

        return allFilters;
    }

    public class FilterConfiguration
    {
        public Category[] Categories { get; set; }
    }

    public class Category
    {
        public string Name { get; set; }
        public List<Filter> Filters { get; set; }
    }

    public class Filter
    {
        public string Name { get; set; }
        public string[] RawQuery { get; set; }
        public bool? Shifting { get; set; }
        public bool? Affinity { get; set; }
        public bool? Stackable { get; set; }
    }
}