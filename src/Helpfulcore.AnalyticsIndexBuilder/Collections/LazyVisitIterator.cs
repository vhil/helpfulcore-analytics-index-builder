namespace Helpfulcore.AnalyticsIndexBuilder.Collections
{
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Sitecore.Analytics.Aggregation.Data.Model;

    public class LazyVisitIterator : IEnumerable<IVisitAggregationContext>
    {
        private readonly IEnumerable<IVisitAggregationContext> contactsEnumerable;
        private readonly ConcurrentDictionary<string, IVisitAggregationContext> cache;

        public LazyVisitIterator(IEnumerable<IVisitAggregationContext> contactsEnumerable)
        {
            this.contactsEnumerable = contactsEnumerable;
            this.cache = new ConcurrentDictionary<string, IVisitAggregationContext>();
        }

        public IEnumerator<IVisitAggregationContext> GetEnumerator()
        {
            foreach (var item in this.contactsEnumerable)
            {
                var uniqueId = $"{item.Contact.Id.Guid}_{item.Visit.InteractionId}";

                if (this.cache.ContainsKey(uniqueId))
                {
                    yield return this.cache[uniqueId];
                }
                else
                {
                    this.cache.TryAdd(uniqueId, item);
                    yield return item;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}