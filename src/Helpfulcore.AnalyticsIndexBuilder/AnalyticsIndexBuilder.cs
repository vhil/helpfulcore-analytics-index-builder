namespace Helpfulcore.AnalyticsIndexBuilder
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Sitecore.Configuration;

    using Data;
    using ContentSearch;
    using Updaters;
    using Logging;

    /// <summary>
    /// Provides list of methods for building Sitecore analytics index. The implementation of <see cref="IAnalyticsIndexBuilder"/> abstraction.
    /// <para>
    /// Uses multithreading for parallel indexables building and submits them to the index in batches.
    /// </para>
    /// </summary>
    public class AnalyticsIndexBuilder : IAnalyticsIndexBuilder
    {
        protected readonly int ConcurrentThreads;
        protected readonly IAnalyticsSearchService AnalyticsSearchService;
        protected readonly ICollectionDataProvider CollectionDataProvider;
        protected ILoggingService Logger;

        private readonly BatchedIndexableUpdater addressUpdater;
        private readonly BatchedIndexableUpdater contactUpdater;
        private readonly BatchedIndexableUpdater contactTagUpdater;
        private readonly BatchedIndexableUpdater visitUpdater;
        private readonly BatchedIndexableUpdater visitPageUpdater;
        private readonly BatchedIndexableUpdater visitPageEventUpdater;


        public AnalyticsIndexBuilder(
            IAnalyticsSearchService analyticsSearchService, 
            ICollectionDataProvider contactSelector, 
            ILoggingService logger,
            string indexSubmitBatchSize = "1000",
            string concurrentThreads = "4"): this(
                analyticsSearchService, 
                contactSelector, 
                logger, 
                int.Parse(indexSubmitBatchSize),
                int.Parse(concurrentThreads))
        {
        }

        public AnalyticsIndexBuilder(
            IAnalyticsSearchService analyticsSearchService,
            ICollectionDataProvider contactSelector,
            ILoggingService logger,
            int indexSubmitBatchSize = 1000,
            int concurrentThreads = 4)
        {
            if (analyticsSearchService == null) throw new ArgumentNullException(nameof(analyticsSearchService));
            if (contactSelector == null) throw new ArgumentNullException(nameof(contactSelector));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            this.AnalyticsSearchService = analyticsSearchService;
            this.CollectionDataProvider = contactSelector;
            this.Logger = logger;
            this.ConcurrentThreads = concurrentThreads;

            this.addressUpdater        =        new AddressIndexableUpdater(this.AnalyticsSearchService, this.Logger, indexSubmitBatchSize, this.ConcurrentThreads);
            this.contactUpdater        =        new ContactIndexableUpdater(this.AnalyticsSearchService, this.Logger, indexSubmitBatchSize, this.ConcurrentThreads);
            this.contactTagUpdater     =     new ContactTagIndexableUpdater(this.AnalyticsSearchService, this.Logger, indexSubmitBatchSize, this.ConcurrentThreads);
            this.visitUpdater          =          new VisitIndexableUpdater(this.AnalyticsSearchService, this.Logger, indexSubmitBatchSize, this.ConcurrentThreads);
            this.visitPageUpdater      =      new VisitPageIndexableUpdater(this.AnalyticsSearchService, this.Logger, indexSubmitBatchSize, this.ConcurrentThreads);
            this.visitPageEventUpdater = new VisitPageEventIndexableUpdater(this.AnalyticsSearchService, this.Logger, indexSubmitBatchSize, this.ConcurrentThreads);

            this.addressUpdater.StatusChanged += this.OnUpdatersStatusChanged;
            this.contactUpdater.StatusChanged += this.OnUpdatersStatusChanged;
            this.contactTagUpdater.StatusChanged += this.OnUpdatersStatusChanged;
            this.visitUpdater.StatusChanged += this.OnUpdatersStatusChanged;
            this.visitPageUpdater.StatusChanged += this.OnUpdatersStatusChanged;
            this.visitPageEventUpdater.StatusChanged += this.OnUpdatersStatusChanged;
        }

        /// <summary>
        /// Indicates if given instance is currenty processing any operation.
        /// </summary>
        public virtual bool IsBusy { get; protected set; }

        protected virtual bool XdbEnabled => Settings.GetBoolSetting("Xdb.Enabled", false);
        protected virtual int BatchSize => Settings.GetIntSetting("Helpfulcore.AnalyticsIndexBuilder.BatchSize", 1000);

        /// <summary>
        /// Rebuilds and submits to index all data taken from Contacts and Interactions collections using the 
        /// <see cref="ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// This method is optimized performance-wise so it does from collection database not load data more than once.
        /// </para>
        /// <para>
        /// This includes building of all known indexable types:
        /// <list type="bullet">
        /// <item>
        /// <term>'contact' </term>
        /// </item>
        /// <item>
        /// <description>'contactTag' </description>
        /// </item>
        /// <item>
        /// <description>'address' </description>
        /// </item>
        /// <item>
        /// <description>'visit' </description>
        /// </item>
        /// <item>
        /// <description>'visitPage' </description>
        /// </item>
        /// <item>
        /// <description>'visitPageEvent' </description>
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="applyFilters">
        /// Specifies whether to apply contact filtering that is configured in Helpfulcore.AnalyticsIndexBuilder.config in helpfulcore/analytics.index.builder/collectionDataProvider Filters collection
        /// </param>
        public virtual void RebuildAllIndexables(bool applyFilters)
        {
            this.SafeExecution($"rebuilding all {(applyFilters ? "filtered " : "")}indexables", () =>
            {
                var contactIds = this.LoadContactIds(applyFilters);

                var contactBatches = this.CollectionDataProvider.GetContacts(contactIds);

                var visitBatches = applyFilters
                    ? this.CollectionDataProvider.GetVisits(contactIds)
                    : this.CollectionDataProvider.GetVisits();

                var batchedTasks = new Action[]
                {
                    () => {
                        foreach (var contactBatch in contactBatches)
                        {
                            var updateTasks = new Action[]
                            {
                                () => { this.contactUpdater.ProcessInBatches(contactBatch); },
                                () => { this.addressUpdater.ProcessInBatches(contactBatch); },
                                () => { this.contactTagUpdater.ProcessInBatches(contactBatch); },
                            };

                            var options = new ParallelOptions { MaxDegreeOfParallelism = updateTasks.Length };
                            Parallel.ForEach(updateTasks, options, task => { task.Invoke(); });
                            GC.Collect();
                        }},

                    () => {
                        foreach (var visitBatch in visitBatches)
                        {
                            var updateTasks = new Action[]
                            {
                                () => { this.visitUpdater.ProcessInBatches(visitBatch); },
                                () => { this.visitPageUpdater.ProcessInBatches(visitBatch); },
                                () => { this.visitPageEventUpdater.ProcessInBatches(visitBatch); },
                            };

                            var options = new ParallelOptions { MaxDegreeOfParallelism = updateTasks.Length };
                            Parallel.ForEach(updateTasks, options, task => { task.Invoke(); });
                            GC.Collect();
                        }},
                };

                Parallel.ForEach(
                    batchedTasks, 
                    new ParallelOptions { MaxDegreeOfParallelism = batchedTasks.Length }, 
                    task => { task.Invoke(); });
            });
        }

        /// <summary>
        /// Rebuilds and submits to index all data taken from Contacts collection using the 
        /// <see cref="ICollectionDataProvider"/> as a source
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// This method is optimized performance-wise so it does from collection database not load data more than once.
        /// </para>
        /// <para>
        /// This includes building of next indexable types:
        /// <list type="bullet">
        /// <item>
        /// <term>'contact' </term>
        /// </item>
        /// <item>
        /// <description>'contactTag' </description>
        /// </item>
        /// <item>
        /// <description>'address' </description>
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="applyFilters">
        /// Specifies whether to apply contact filtering that is configured in Helpfulcore.AnalyticsIndexBuilder.config in helpfulcore/analytics.index.builder/collectionDataProvider Filters collection
        /// </param>
        public virtual void RebuildContactIndexableTypes(bool applyFilters)
        {
            this.RebuildContactIndexableTypes(this.LoadContactIds(applyFilters));
        }

        /// <summary>
        /// Rebuilds and submits to index all data taken from Contacts collection for specified contacts using the 
        /// <see cref="ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// This method is optimized performance-wise so it does from collection database not load data more than once.
        /// </para>
        /// <para>
        /// This includes building of next indexable types:
        /// <list type="bullet">
        /// <item>
        /// <term>'contact' </term>
        /// </item>
        /// <item>
        /// <description>'contactTag' </description>
        /// </item>
        /// <item>
        /// <description>'address' </description>
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="contactIds">
        /// Specify contact ID's you want to rebuild 'contact', 'address' and 'contactTag' indexables for.
        /// </param>
        public virtual void RebuildContactIndexableTypes(IEnumerable<Guid> contactIds)
        {
            var ids = contactIds as ICollection<Guid> ?? contactIds.Distinct().ToList();

            this.SafeExecution($"rebuilding [{this.contactUpdater.IndexableType}, {this.addressUpdater.IndexableType}, {this.contactTagUpdater.IndexableType}] indexables for {ids.Count} contacts", () =>
            {
                var contacts = this.CollectionDataProvider.GetContacts(ids);

                foreach (var batch in contacts)
                {
                    var updateTasks = new Action[]
                    {
                        () => { this.addressUpdater.ProcessInBatches(batch); },
                        () => { this.contactUpdater.ProcessInBatches(batch); },
                        () => { this.contactTagUpdater.ProcessInBatches(batch); },
                    };

                    var options = new ParallelOptions { MaxDegreeOfParallelism = updateTasks.Length };
                    Parallel.ForEach(updateTasks, options, task => { task.Invoke(); });
                    GC.Collect();
                }
            });
        }

        /// <summary>
        /// Rebuilds and submits to index all indexables created from Interactions collection using the 
        /// <see cref="ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// This includes building of 'visit', 'visitPage' and 'visitPageEvent' indexable types
        /// </para>
        /// </summary>
        /// <param name="applyFilters">
        /// Specifies whether to apply contact filtering that is configured in Helpfulcore.AnalyticsIndexBuilder.config in helpfulcore/analytics.index.builder/collectionDataProvider Filters collection
        /// </param>
        public virtual void RebuildVisitIndexableTypes(bool applyFilters)
        {
            this.RebuildVisitIndexableTypes(applyFilters ? this.LoadContactIds(true) : null);
        }

        /// <summary>
        /// Rebuilds and submits to index all indexables created from Interactions collection using the 
        /// <see cref="ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// This includes building of 'visit', 'visitPage' and 'visitPageEvent' indexable types
        /// </para>
        /// </summary>
        /// <param name="contactIds">
        /// Specify contact ID's you want to rebuild interaction indexables for.
        /// </param>
        public virtual void RebuildVisitIndexableTypes(IEnumerable<Guid> contactIds)
        {
            var all = contactIds == null;
            var ids = contactIds as ICollection<Guid> ?? contactIds?.Distinct().ToList();

            this.SafeExecution($"rebuilding [{this.visitUpdater.IndexableType}, {this.visitPageUpdater.IndexableType}, {this.visitPageEventUpdater.IndexableType}] indexables {(all ? "for all visits in collection database" : $"for {ids.Count} contacts")}", () =>
            {
                var visits = all
                    ? this.CollectionDataProvider.GetVisits()
                    : this.CollectionDataProvider.GetVisits(ids);

                foreach (var batch in visits)
                {
                    var updateTasks = new Action[]
                    {
                        () => { this.visitUpdater.ProcessInBatches(batch); },
                        () => { this.visitPageUpdater.ProcessInBatches(batch); },
                        () => { this.visitPageEventUpdater.ProcessInBatches(batch); },
                    };

                    var options = new ParallelOptions { MaxDegreeOfParallelism = updateTasks.Length };
                    Parallel.ForEach(updateTasks, options, task => { task.Invoke(); });
                    GC.Collect();
                }
            });
        }

        /// <summary>
        /// Rebuilds and submits to index 'contact' indexables taken from Contacts collection using the 
        /// <see cref="ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// This includes building of 'contact' indexable type
        /// </para>
        /// </summary>
        /// <param name="applyFilters">
        /// Specifies whether to apply contact filtering that is configured in Helpfulcore.AnalyticsIndexBuilder.config in helpfulcore/analytics.index.builder/collectionDataProvider Filters collection
        /// </param>
        public virtual void RebuildContactIndexables(bool applyFilters)
        {
            this.SafeRebuildContactBasedIndex(this.contactUpdater, applyFilters);
        }

        /// <summary>
        /// Rebuilds and submits to index 'contact' indexables taken from Contacts collection using 
        /// <see cref="ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// Builds only contacts which contact ID's specified in the input parameter.
        /// </para>
        /// </summary>
        /// <param name="contactIds">
        /// Specify contact ID's you want to rebuild 'contact' indexables for.
        /// </param>
        public virtual void RebuildContactIndexables(IEnumerable<Guid> contactIds)
        {
            this.SafeRebuildContactBasedIndex(this.contactUpdater, contactIds);
        }

        /// <summary>
        /// Rebuilds and submits to index 'address' indexables taken from Contacts collection using the 
        /// <see cref="ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// This includes building of 'address' indexable type
        /// </para>
        /// </summary>
        /// <param name="applyFilters">
        /// Specifies whether to apply contact filtering that is configured in Helpfulcore.AnalyticsIndexBuilder.config in helpfulcore/analytics.index.builder/collectionDataProvider Filters collection
        /// </param>
        public virtual void RebuildAddressIndexables(bool applyFilters)
        {
            this.SafeRebuildContactBasedIndex(this.addressUpdater, applyFilters);
        }

        /// <summary>
        /// Rebuilds and submits to index 'contact' indexables taken from Contacts collection using 
        /// <see cref="ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// Builds only addresses of contacts which contact ID's specified in the input parameter.
        /// </para>
        /// </summary>
        /// <param name="contactIds">
        /// Specify contact ID's you want to rebuild 'address' indexables for.
        /// </param>
        public virtual void RebuildAddressIndexables(IEnumerable<Guid> contactIds)
        {
            this.SafeRebuildContactBasedIndex(this.addressUpdater, contactIds);
        }

        /// <summary>
        /// Rebuilds and submits to index 'contactTag' indexables taken from Contacts collection using the 
        /// <see cref="ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// This includes building of 'contactTag' indexable type
        /// </para>
        /// </summary>
        /// <param name="applyFilters">
        /// Specifies whether to apply contact filtering that is configured in Helpfulcore.AnalyticsIndexBuilder.config in helpfulcore/analytics.index.builder/collectionDataProvider Filters collection
        /// </param>
        public virtual void RebuildContactTagIndexables(bool applyFilters)
        {
            this.SafeRebuildContactBasedIndex(this.contactTagUpdater, applyFilters);
        }

        /// <summary>
        /// Rebuilds and submits to index 'contactTag' indexables taken from Contacts collection database using 
        /// <see cref="ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// Builds only contactTags of contacts which contact ID's specified in the input parameter.
        /// </para>
        /// </summary>
        /// <param name="contactIds">
        /// Specify contact ID's you want to rebuild 'contactTag' indexables for.
        /// </param>
        public virtual void RebuildContactTagIndexables(IEnumerable<Guid> contactIds)
        {
            this.SafeRebuildContactBasedIndex(this.contactTagUpdater, contactIds);
        }

        /// <summary>
        /// Rebuilds and submits to index 'visit' indexables taken from Interactions collection database using the 
        /// <see cref="ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// This includes building of 'visit' indexable type
        /// </para>
        /// </summary>
        /// <param name="applyFilters">
        /// Specifies whether to apply contact filtering that is configured in Helpfulcore.AnalyticsIndexBuilder.config in helpfulcore/analytics.index.builder/collectionDataProvider Filters collection
        /// </param>
        public virtual void RebuildVisitIndexables(bool applyFilters)
        {
            this.SafeRebuildVisitBasedIndex(this.visitUpdater, applyFilters);
        }

        /// <summary>
        /// Rebuilds and submits to index 'visit' indexables taken from Interactions collection database using 
        /// <see cref="ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// Builds only visits of contacts which contact ID's specified in the input parameter.
        /// </para>
        /// </summary>
        /// <param name="contactIds">
        /// Specify contact ID's you want to rebuild 'visit' indexables for.
        /// </param>
        public virtual void RebuildVisitIndexables(IEnumerable<Guid> contactIds)
        {
            this.SafeRebuildVisitBasedIndex(this.visitUpdater, contactIds);
        }

        /// <summary>
        /// Rebuilds and submits to index 'visitPage' indexables taken from Interactions collection database using the 
        /// <see cref="ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// This includes building of 'visitPage' indexable type
        /// </para>
        /// </summary>
        /// <param name="applyFilters">
        /// Specifies whether to apply contact filtering that is configured in Helpfulcore.AnalyticsIndexBuilder.config in helpfulcore/analytics.index.builder/collectionDataProvider Filters collection
        /// </param>
        public virtual void RebuildVisitPageIndexables(bool applyFilters)
        {
            this.SafeRebuildVisitBasedIndex(this.visitPageUpdater, applyFilters);
        }

        /// <summary>
        /// Rebuilds and submits to index 'visitPage' indexables taken from Interactions collection database using 
        /// <see cref="ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// Builds only visitsPages of contacts which contact ID's specified in the input parameter.
        /// </para>
        /// </summary>
        /// <param name="contactIds">
        /// Specify contact ID's you want to rebuild 'visitPage' indexables for.
        /// </param>
        public virtual void RebuildVisitPageIndexables(IEnumerable<Guid> contactIds)
        {
            this.SafeRebuildVisitBasedIndex(this.visitPageUpdater, contactIds);
        }

        /// <summary>
        /// Rebuilds and submits to index 'visitPageEvent' indexables taken from Interactions collection database using the 
        /// <see cref="ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// This includes building of 'visitPageEvent' indexable type
        /// </para>
        /// </summary>
        /// <param name="applyFilters">
        /// Specifies whether to apply contact filtering that is configured in Helpfulcore.AnalyticsIndexBuilder.config in helpfulcore/analytics.index.builder/collectionDataProvider Filters collection
        /// </param>
        public virtual void RebuildVisitPageEventIndexables(bool applyFilters)
        {
            this.SafeRebuildVisitBasedIndex(this.visitPageEventUpdater, applyFilters);
        }

        /// <summary>
        /// Rebuilds and submits to index 'visitPageEvent' indexables taken from Interactions collection database using 
        /// <see cref="ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// Builds only visitsPageEvents of contacts which contact ID's specified in the input parameter.
        /// </para>
        /// </summary>
        /// <param name="contactIds">
        /// Specify contact ID's you want to rebuild 'visitPageEvent' indexables for.
        /// </param>
        public virtual void RebuildVisitPageEventIndexables(IEnumerable<Guid> contactIds)
        {
            this.SafeRebuildVisitBasedIndex(this.visitPageEventUpdater, contactIds);
        }

        #region infrastructure

        protected virtual void ResetAllStats()
        {
            this.contactUpdater.ResetStats();
            this.addressUpdater.ResetStats();
            this.contactTagUpdater.ResetStats();
            this.visitUpdater.ResetStats();
            this.visitPageUpdater.ResetStats();
            this.visitPageEventUpdater.ResetStats();
        }

        protected virtual void OnUpdatersStatusChanged()
        {
            var failed = new StringBuilder();
            var updated = new StringBuilder();

            updated.Append($"{this.contactUpdater.IndexableType}: {this.contactUpdater.Updated}, ");
            updated.Append($"{this.addressUpdater.IndexableType}: {this.addressUpdater.Updated}, ");
            updated.Append($"{this.contactTagUpdater.IndexableType}: {this.contactTagUpdater.Updated}, ");
            updated.Append($"{this.visitUpdater.IndexableType}: {this.visitUpdater.Updated}, ");
            updated.Append($"{this.visitPageUpdater.IndexableType}: {this.visitPageUpdater.Updated}, ");
            updated.Append($"{this.visitPageEventUpdater.IndexableType}: {this.visitPageEventUpdater.Updated}");

            this.Logger.Info($"Updated by indexable type: [ {updated.ToString().Replace("type:", "")} ]", this);

            if (this.contactUpdater.Failed > 0
                || this.addressUpdater.Failed > 0
                || this.contactTagUpdater.Failed > 0
                || this.visitUpdater.Failed > 0
                || this.visitPageUpdater.Failed > 0
                || this.visitPageEventUpdater.Failed > 0)
            {
                failed.Append($"{this.contactUpdater.IndexableType}: {this.contactUpdater.Failed},");
                failed.Append($"{this.addressUpdater.IndexableType}: {this.addressUpdater.Failed},");
                failed.Append($"{this.contactTagUpdater.IndexableType}: {this.contactTagUpdater.Failed},");
                failed.Append($"{this.visitUpdater.IndexableType}: {this.visitUpdater.Failed},");
                failed.Append($"{this.visitPageUpdater.IndexableType}: {this.visitPageUpdater.Failed},");
                failed.Append($"{this.visitPageEventUpdater.IndexableType}: {this.visitPageEventUpdater.Failed}");

                this.Logger.Info($"Failed by indexable type: [ {failed.ToString().Replace("type:", "")} ]", this);
            }
        }

        protected virtual void SafeRebuildContactBasedIndex(IBatchedIndexableUpdater updater, bool applyFilters)
        {
            this.SafeExecution($"rebuilding {(applyFilters ? "filtered " : "")}[{updater.IndexableType}] indexables index", () =>
            {
                var contactIds = this.LoadContactIds(applyFilters);
                var contactBatches = this.CollectionDataProvider.GetContacts(contactIds);

                foreach (var contactBatch in contactBatches)
                {
                    updater.ProcessInBatches(contactBatch);
                    GC.Collect();
                }
            });
        }

        protected virtual void SafeRebuildContactBasedIndex(IBatchedIndexableUpdater updater, IEnumerable<Guid> contactIds)
        {
            var ids = contactIds as ICollection<Guid> ?? contactIds.ToArray();

            this.SafeExecution($"rebuilding [{updater.IndexableType}] indexables index for {ids.Count} contacts", () =>
            {
                var contactBatches = this.CollectionDataProvider.GetContacts(ids);
                foreach (var contactBatch in contactBatches)
                {
                    updater.ProcessInBatches(contactBatch);
                    GC.Collect();
                }
            });
        }

        protected virtual void SafeRebuildVisitBasedIndex(IBatchedIndexableUpdater updater, bool applyFilters)
        {
            this.SafeExecution($"rebuilding {(applyFilters ? "filtered " : "")}[{updater.IndexableType}] indexables index", () =>
            {
                var visitBatches = applyFilters
                    ? this.CollectionDataProvider.GetVisits(this.LoadContactIds(true))
                    : this.CollectionDataProvider.GetVisits();

                foreach (var visitBatch in visitBatches)
                {
                    updater.ProcessInBatches(visitBatch);
                    GC.Collect();
                }
            });
        }

        protected virtual void SafeRebuildVisitBasedIndex(IBatchedIndexableUpdater updater, IEnumerable<Guid> contactIds)
        {
            var ids = contactIds as ICollection<Guid> ?? contactIds.ToArray();

            this.SafeExecution($"rebuilding [{updater.IndexableType}] indexables index for {ids.Count} contacts", () =>
            {
                var visitBatches = this.CollectionDataProvider.GetVisits(ids);

                foreach (var visitBatch in visitBatches)
                {
                    updater.ProcessInBatches(visitBatch);
                    GC.Collect();
                }
            });
        }

        protected virtual ICollection<Guid> LoadContactIds(bool applyFilters)
        {
            var contactIds = applyFilters
                ? this.CollectionDataProvider.GetFilteredContactIdsToReindex()
                : this.CollectionDataProvider.GetAllContactIdsToReindex();

            return contactIds as ICollection<Guid> ?? contactIds.ToArray();
        }

        protected virtual void SafeExecution(string actionDescription, Action action)
        {
            if (this.IsBusy)
            {
                this.Logger.Warn($"Unable to execute {actionDescription}. AnalyticsIndexBuilder is busy at the moment with another operation.", this);
                return;
            }

            try
            {
                this.ResetAllStats();

                if (!this.XdbEnabled)
                {
                    throw new NotSupportedException("Sitecore Xdb.Enabled setting is set to false. Unable to rebuild analytics index.");
                }

                this.Logger.Info($"Start {actionDescription}. Batch size is set to {this.BatchSize}...", this);


                this.IsBusy = true;
                action.Invoke();
            }
            catch (Exception ex)
            {
                this.Logger.Error($"Error while {actionDescription}. {ex.Message}", this, ex);
            }
            finally
            {
                this.IsBusy = false;
                this.Logger.Info($"DONE {actionDescription}.", this);
                GC.Collect();
            }
        }

        public virtual void ChangeLogger(ILoggingService logger)
        {
            if (logger != null)
            {
                this.Logger = logger;
                this.AnalyticsSearchService.ChangeLogger(logger);
                this.CollectionDataProvider.ChangeLogger(logger);
                this.contactUpdater.ChangeLogger(logger);
                this.contactTagUpdater.ChangeLogger(logger);
                this.addressUpdater.ChangeLogger(logger);
                this.visitUpdater.ChangeLogger(logger);
                this.visitPageUpdater.ChangeLogger(logger);
                this.visitPageEventUpdater.ChangeLogger(logger);
            }
        }

        #endregion
    }
}
