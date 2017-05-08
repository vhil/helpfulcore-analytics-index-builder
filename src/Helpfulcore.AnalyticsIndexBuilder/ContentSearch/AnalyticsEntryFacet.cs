namespace Helpfulcore.AnalyticsIndexBuilder.ContentSearch
{
    using System;

    public class AnalyticsEntryFacet
    {
        public AnalyticsEntryFacet(string type, int count)
        {
            this.Type = type;
            this.Count = count;
            this.ActionsAvailable = 
                type.Equals("contact", StringComparison.CurrentCultureIgnoreCase)
                || type.Equals("address", StringComparison.CurrentCultureIgnoreCase);
        }

        public string Type { get; protected set; }
        public int Count { get; protected set; }
        public bool ActionsAvailable { get; protected set; }
    }
}
