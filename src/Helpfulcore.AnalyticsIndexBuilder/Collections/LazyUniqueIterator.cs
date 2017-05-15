namespace Helpfulcore.AnalyticsIndexBuilder.Collections
{
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class LazyUniqueIterator<TLazyItem, TItem> : IEnumerable<TItem>
        where TLazyItem : ILazyUniqueItem<TItem>
    {
        private readonly IEnumerable<TLazyItem> contactsEnumerable;
        private readonly ConcurrentDictionary<string, TItem> cache;

        public LazyUniqueIterator(IEnumerable<TLazyItem> contactsEnumerable)
        {
            this.contactsEnumerable = contactsEnumerable;
            this.cache = new ConcurrentDictionary<string, TItem>();
        }

        public virtual IEnumerator<TItem> GetEnumerator()
        {
            foreach (var item in this.contactsEnumerable)
            {
                if (this.cache.ContainsKey(item.UniqueId))
                {
                    yield return this.cache[item.UniqueId];
                }
                else
                {
                    this.cache.TryAdd(item.UniqueId, item.Value);
                    yield return item.Value;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}