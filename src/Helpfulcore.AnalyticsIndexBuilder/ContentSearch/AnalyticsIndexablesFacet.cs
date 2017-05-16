namespace Helpfulcore.AnalyticsIndexBuilder.ContentSearch
{
    public class AnalyticsIndexablesFacet
    {
        public AnalyticsIndexablesFacet(string type, int count, bool actionsAvailable = false)
        {
            this.Type = type;
            this.Count = count;
            this.ActionsAvailable = actionsAvailable;
        }

        public virtual string Type { get; protected set; }
        public virtual int Count { get; set; }
        public virtual bool ActionsAvailable { get; protected set; }
    }
}
