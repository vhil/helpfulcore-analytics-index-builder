namespace Helpfulcore.AnalyticsIndexBuilder.Batches
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Sitecore.Analytics.Model.Entities;
    using Sitecore.Configuration;
    using Sitecore.ContentSearch;

    using ContentSearch;
    using Logging;

    public abstract class BatchedEntryIndexUpdater<TSourceEntry, TIndexable> where TIndexable : AbstractIndexable
    {
        public readonly string IndexableType;
        protected readonly IAnalyticsSearchService AnalyticsSearchService;
        protected readonly IContactFactory ContactFactory;
        protected readonly ILoggingService Logger;
        protected readonly int BatchSize;
        protected readonly int ConcurrentThreads;

        protected BatchedEntryIndexUpdater(
            string indexableType,
            IAnalyticsSearchService analyticsSearchService,
            ILoggingService logger,
            int batchSize,
            int concurrentThreads)
        {
            this.IndexableType = indexableType;
            this.AnalyticsSearchService = analyticsSearchService;
            this.Logger = logger;
            this.BatchSize = batchSize;
            this.ConcurrentThreads = concurrentThreads;
            this.ContactFactory = Factory.CreateObject("model/entities/contact/factory", true) as IContactFactory;
        }


        public void ProcessInBatches(IEnumerable<Guid> contactIds)
        {
            var contacts = contactIds?.Distinct().ToArray() ?? new Guid[0];

            long count = 0;
            long updated = 0;
            long failed = 0;

            var sourceEntries = this.GetAllSourceEntries(contacts);
            this.Logger.Info($"Found {sourceEntries.Count} {this.IndexableType} indexables to update.", this);

            this.Logger.Info($"Updating {this.IndexableType} indexables progress: {count} of {sourceEntries.Count} (0.00%). Updated: {updated}, Failed: {failed}...", this);

            var sourceList = new List<TSourceEntry>();
            foreach (var address in sourceEntries)
            {
                sourceList.Add(address);
                count++;

                if (count % this.BatchSize == 0 || count == sourceEntries.Count)
                {
                    try
                    {
                        var indexables = this.LoadIndexablesFields(sourceList);
                        this.AnalyticsSearchService.UpdateIndexables(indexables);

                        updated += sourceList.Count;
                    }
                    catch (Exception ex)
                    {
                        failed += sourceList.Count;
                        this.Logger.Error($"Error while updating batch of {sourceList.Count} {this.IndexableType} indexables. {ex.Message}", this);
                    }
                    finally
                    {
                        var percentage = 100 * count / (decimal)contacts.Length;
                        this.Logger.Info($"Updating {this.IndexableType} indexables progress: {count} of {sourceEntries.Count} ({percentage:#0.00}%). Updated: {updated}, Failed: {failed}", this);

                        sourceList.Clear();
                    }
                }
            }
        }

        protected virtual IEnumerable<TIndexable> LoadIndexablesFields(IEnumerable<TSourceEntry> sourceEntries)
        {
            // loading  indexables could be a heavy operation 
            // so executing it in multiple threads for performance
            var indexables = new ConcurrentBag<TIndexable>();

            var options = new ParallelOptions { MaxDegreeOfParallelism = this.ConcurrentThreads };
            Parallel.ForEach(sourceEntries.Distinct(), options, source =>
            {
                // this will execute "[indexable-name].loadfields" pipeline to load field values;
                var indexable = this.ConstructIndexable(source);

                indexables.Add(indexable);
            });

            return indexables.ToArray();
        }

        protected abstract ICollection<TSourceEntry> GetAllSourceEntries(ICollection<Guid> contactIds);
        protected abstract TIndexable ConstructIndexable(TSourceEntry source);
    }
}
