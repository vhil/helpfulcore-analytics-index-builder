namespace Helpfulcore.AnalyticsIndexBuilder.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Sitecore.Exceptions;
    using Sitecore.Analytics.Aggregation.Data.Model;
    using Sitecore.Analytics.Model;
    using Sitecore.Analytics.Model.Entities;

    using Collections;
    using Logging;

    public abstract class CollectionDataProvider : ICollectionDataProvider
    {
        protected ILoggingService Logger;
        public ArrayList Filters { get; protected set; }

        protected CollectionDataProvider(ILoggingService logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            this.Logger = logger;
            this.Filters = new ArrayList();
        }

        public virtual IEnumerable<Guid> GetFilteredContactIdsToReindex()
        {
            return this.SafeExecution($"getting contact ids to re-index", () =>
            {
                var contactIds = this.GetContactIdentifiers().Where(x => !x._id.Equals(default(Guid)));

                if (this.Filters.Count > 0)
                {
                    foreach (var filter in this.Filters)
                    {
                        this.Logger.Info($"Applying '{filter.GetType().Name}' contact selection filter to retrieved contacts.", this);

                        var selectionFilter = filter as IContactSelectionFilter;

                        if (selectionFilter == null)
                        {
                            throw new ConfigurationException($"'{filter.GetType().FullName}' can't be casted to IContactSelectionFilter. Please review your configuration.");
                        }

                        contactIds = contactIds.Where(selectionFilter.GetFilter());
                    }
                }

                var result = contactIds.Select(x => x._id).Distinct().ToList();

                this.Logger.Info($"Returning {result.Count} contact ids to re-index.", this);

                return result;
            }) ?? Enumerable.Empty<Guid>();
        }

        public virtual void ChangeLogger(ILoggingService logger)
        {
            if (logger != null)
            {
                this.Logger = logger;
            }
        }

        protected virtual TEntry SafeExecution<TEntry>(string actionDescription, Func<TEntry> action)
        {
            this.Logger.Info($"Start {actionDescription}...", this);

            try
            {
                return action.Invoke();
            }
            catch (Exception ex)
            {
                this.Logger.Error($"Error while {actionDescription}. {ex.Message}", this, ex);
            }

            return default(TEntry);
        }

        protected IEnumerable<IVisitAggregationContext> ToLazyIterator(IEnumerable<IVisitAggregationContext> visits)
        {
            return new LazyUniqueIterator<LazyVisit, IVisitAggregationContext>(visits.Select(v => new LazyVisit(v)));
        }

        protected IEnumerable<IContact> ToLazyIterator(IEnumerable<IContact> visits)
        {
            return new LazyUniqueIterator<LazyContact, IContact>(visits.Select(v => new LazyContact(v)));
        }

        protected abstract IEnumerable<ContactIdentifiersData> GetContactIdentifiers();
        public abstract IEnumerable<Guid> GetAllContactIdsToReindex();
        public abstract IEnumerable<VisitData> GetVisitDataToReindex();
        public abstract IEnumerable<VisitData> GetVisitDataToReindex(IEnumerable<Guid> contactIds);
        public abstract IEnumerable<IContact> GetContacts();
        public abstract IEnumerable<IContact> GetContacts(IEnumerable<Guid> contactIds);
        public abstract IEnumerable<IVisitAggregationContext> GetVisits();
        public abstract IEnumerable<IVisitAggregationContext> GetVisits(IEnumerable<Guid> contactIds);
    }
}
