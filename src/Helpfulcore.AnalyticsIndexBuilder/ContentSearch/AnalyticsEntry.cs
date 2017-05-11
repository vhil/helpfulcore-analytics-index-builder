namespace Helpfulcore.AnalyticsIndexBuilder.ContentSearch
{
    using System;
    using Sitecore.ContentSearch;

    [Serializable]
    public class AnalyticsEntry
    {
        [IndexField("type")]
        public string Type { get; set; }

        [IndexField("_uniqueId")]
        public IIndexableUniqueId UniqueId { get; set; }
    }
}