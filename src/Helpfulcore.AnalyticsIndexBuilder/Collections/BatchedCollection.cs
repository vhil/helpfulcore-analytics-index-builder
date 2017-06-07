namespace Helpfulcore.AnalyticsIndexBuilder.Collections
{
    using System.Collections;
    using System.Collections.Generic;

    public class BatchedCollection<TItem> : IEnumerable<IEnumerable<TItem>>
    {
        private readonly int batchSize;
        private readonly IEnumerable<TItem> items;

        public BatchedCollection(int batchSize, IEnumerable<TItem> items)
        {
            this.batchSize = batchSize;
            this.items = items;
        }

        public IEnumerable<IEnumerable<TItem>> YieldBatches()
        {
            var batch = new List<TItem>();

            var count = 0;
            foreach (var item in this.items)
            {
                if (count == 0)
                {
                    batch.Clear();
                }

                batch.Add(item);
                count++;

                if (count == this.batchSize)
                {
                    count = 0;
                    yield return batch;
                }
            }

            yield return batch;
        }

        public IEnumerator<IEnumerable<TItem>> GetEnumerator()
        {
            return this.YieldBatches().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
