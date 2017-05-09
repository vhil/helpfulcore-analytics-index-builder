namespace Helpfulcore.AnalyticsIndexBuilder.ContentSearch
{
    public class AnalyticsEntryFacet
    {
        public AnalyticsEntryFacet(string type, int count, bool actionsAvailable = false)
        {
            this.Type = type;
            this.Count = count;
            this.ActionsAvailable = actionsAvailable;
        }

        public string Type { get; protected set; }
        public int Count { get; set; }
        public bool ActionsAvailable { get; protected set; }
    }
}
