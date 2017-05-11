namespace Helpfulcore.AnalyticsIndexBuilder.Batches
{
    using System;
    using System.Threading;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Sitecore.Analytics.Model.Entities;
    using Sitecore.Configuration;
    using Sitecore.ContentSearch;

    using ContentSearch;
    using Logging;

    public abstract class BatchedEntryIndexUpdater<TSourceEntry, TParentObject, TIndexable> : BatchedEntryIndexUpdater
        where TIndexable : AbstractIndexable
    {
        protected readonly IAnalyticsSearchService AnalyticsSearchService;
        protected readonly IContactFactory ContactFactory;
        protected readonly int BatchSize;
        protected readonly int ConcurrentThreads;

        protected BatchedEntryIndexUpdater(
            string indexableType,
            IAnalyticsSearchService analyticsSearchService,
            ILoggingService logger,
            int batchSize,
            int concurrentThreads) : base(indexableType, logger)
        {
            this.AnalyticsSearchService = analyticsSearchService;
            this.BatchSize = batchSize;
            this.ConcurrentThreads = concurrentThreads;
            this.ContactFactory = Factory.CreateObject("model/entities/contact/factory", true) as IContactFactory;
        }

        public override void ProcessInBatches(IEnumerable<object> parentObjects)
        {
            if (parentObjects == null) throw new ArgumentNullException(nameof(parentObjects));

            this.ProcessInBatches(this.LoadSourceEntries(parentObjects as IEnumerable<TParentObject>));
        }

        public void ProcessInBatches(IEnumerable<TSourceEntry> sourceEntries)
        {
            if (sourceEntries == null) throw new ArgumentNullException(nameof(sourceEntries));

            this.updated = 0;
            this.failed = 0;
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
                sourceList.Clear();
            }
        }

        protected virtual void SubmitBatch(ICollection<TSourceEntry> sourceEntries)
        {
            if (sourceEntries == null) throw new ArgumentNullException(nameof(sourceEntries));

            try
            {
                var indexables = this.LoadIndexablesFields(sourceEntries);
                this.AnalyticsSearchService.UpdateIndexables(indexables);
                this.updated += sourceEntries.Count;
            }
            catch (Exception ex)
            {
                this.failed += sourceEntries.Count;
                this.Logger.Error($"Error while updating batch of {sourceEntries.Count} {this.IndexableType} indexables. {ex.Message}", this);
            }
            finally
            {
                this.Logger.Debug($"Batch of {sourceEntries.Count} {this.IndexableType} indexables rebuilt and submitted to index.", this);
                this.RaiseStatusChangedEvent();
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

        protected abstract TIndexable ConstructIndexable(TSourceEntry source);
        protected abstract IEnumerable<TSourceEntry> LoadSourceEntries(IEnumerable<TParentObject> sources);
    }

    public abstract class BatchedEntryIndexUpdater : IBatchedEntryIndexUpdater
    {
        protected ILoggingService Logger;

        protected BatchedEntryIndexUpdater(string indexableType, ILoggingService logger)
        {
            this.IndexableType = indexableType;
            this.Logger = logger;
            this.updated = 0;
            this.failed = 0;
        }

        protected long updated;
        protected long failed;

        public long Updated => Interlocked.Read(ref this.updated);
        public long Failed => Interlocked.Read(ref this.failed);
        public string IndexableType { get; private set; }
        public event Action StatusChanged;

        public void ChangeLogger(ILoggingService logger)
        {
            if (logger != null)
            {
                this.Logger = logger;
            }
        }

        protected void RaiseStatusChangedEvent()
        {
            this.StatusChanged?.Invoke();
        }

        public void ResetStats()
        {
            this.updated = 0;
            this.failed = 0;
        }

        public abstract void ProcessInBatches(IEnumerable<object> parentObjects);
    }
}
