namespace Helpfulcore.AnalyticsIndexBuilder.ContentSearch
{
    using System;
    using Sitecore.ContentSearch;

    [Serializable]
    public class AnalyticsEntry
    {
        [IndexField("type")]
        public virtual string Type { get; set; }

        [IndexField("_uniqueId")]
        public virtual IIndexableUniqueId UniqueId { get; set; }
    }
}