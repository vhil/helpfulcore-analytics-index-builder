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

    public abstract class BatchedEntryIndexUpdater<TSourceEntry, TParentObject, TIndexable> : IBatchedEntryIndexUpdater
        where TIndexable : AbstractIndexable
    {
        protected readonly IAnalyticsSearchService AnalyticsSearchService;
        protected readonly IContactFactory ContactFactory;
        protected ILoggingService Logger;
        protected readonly int BatchSize;
        protected readonly int ConcurrentThreads;

        public string IndexableType { get; private set; }

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
        
        public void ProcessInBatches(IEnumerable<object> parentObjects)
        {
            if (parentObjects == null) throw new ArgumentNullException(nameof(parentObjects));

            this.ProcessInBatches(this.LoadSourceEntries(parentObjects as IEnumerable<TParentObject>));
        }

        public void ProcessInBatches(IEnumerable<TSourceEntry> sourceEntries)
        {
            if (sourceEntries == null) throw new ArgumentNullException(nameof(sourceEntries));

            long count = 0;

            var sourceList = new List<TSourceEntry>();
            foreach (var address in sourceEntries)
            {
                sourceList.Add(address);
                count++;

                if (count%this.BatchSize == 0)
                {
                    this.SubmitBatch(sourceList);
                    sourceList.Clear();
                }
            }

            if (sourceList.Any())
            {
                this.SubmitBatch(sourceList);
            }
        }

        protected virtual void SubmitBatch(ICollection<TSourceEntry> sourceEntries)
        {
            if (sourceEntries == null) throw new ArgumentNullException(nameof(sourceEntries));

            try
            {
                var indexables = this.LoadIndexablesFields(sourceEntries);
                this.AnalyticsSearchService.UpdateIndexables(indexables);
            }
            catch (Exception ex)
            {
                this.Logger.Error($"Error while updating batch of {sourceEntries.Count} {this.IndexableType} indexables. {ex.Message}", this);
            }
            finally
            {
                this.Logger.Info($"Batch of {sourceEntries.Count} {this.IndexableType} indexables rebuilt and submitted to index.", this);
            }
        }

        protected virtual IEnumerable<TIndexable> LoadIndexablesFields(IEnumerable<TSourceEntry> sourceEntries)
        {
            if (sourceEntries == null) throw new ArgumentNullException(nameof(sourceEntries));

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

        public void ChangeLogger(ILoggingService logger)
        {
            if (logger != null)
            {
                this.Logger = logger;
            }
        }

        protected abstract TIndexable ConstructIndexable(TSourceEntry source);
        protected abstract IEnumerable<TSourceEntry> LoadSourceEntries(IEnumerable<TParentObject> sources);
    }
}
