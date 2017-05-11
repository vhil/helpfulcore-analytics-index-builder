
namespace Helpfulcore.AnalyticsIndexBuilder.Collections
{
    using Sitecore.Analytics.Aggregation.Data.Model;

    public class LazyVisit : ILazyUniqueItem<IVisitAggregationContext>
    {
        private readonly IVisitAggregationContext visit;

        public LazyVisit(IVisitAggregationContext visit)
        {
            this.visit = visit;
        }

        public string UniqueId => $"{this.visit.Contact.Id}_{this.visit.Visit.InteractionId}";
        public IVisitAggregationContext Value => this.visit;
    }
}
