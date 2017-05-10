namespace Helpfulcore.AnalyticsIndexBuilder.Collections
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Sitecore.Analytics.Model.Entities;

    public class LazyContactIterator : IEnumerable<IContact>
    {
        private readonly IEnumerable<IContact> contactsEnumerable;
        private readonly ConcurrentDictionary<Guid, IContact> cache;

        public LazyContactIterator(IEnumerable<IContact> contactsEnumerable)
        {
            this.contactsEnumerable = contactsEnumerable;
            this.cache = new ConcurrentDictionary<Guid, IContact>();
        }

        public IEnumerator<IContact> GetEnumerator()
        {
            foreach (var item in this.contactsEnumerable)
            {
                if (this.cache.ContainsKey(item.Id.Guid))
                {
                    yield return this.cache[item.Id.Guid];
                }
                else
                {
                    this.cache.TryAdd(item.Id.Guid, item);
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