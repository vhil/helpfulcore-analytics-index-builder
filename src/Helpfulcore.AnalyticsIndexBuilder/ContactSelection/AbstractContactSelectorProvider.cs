using System.Collections;
using Sitecore.Exceptions;

namespace Helpfulcore.AnalyticsIndexBuilder.ContactSelection
{
    using Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class AbstractContactSelectorProvider : IContactsSelectorProvider
    {
        protected readonly ILoggingService Logger;
        public ArrayList Filters { get; protected set; }

        protected AbstractContactSelectorProvider(ILoggingService logger)
        {
            this.Logger = logger;
            this.Filters = new ArrayList();
        }

        public virtual IEnumerable<Guid> GetContactIdsToReindex()
        {
            return this.SafeExecution($"getting contact ids to re-index", () =>
            {
                var contactIds = this.GetContactIds();

                if (this.Filters.Count > 0)
                {
                    foreach (var filter in this.Filters)
                    {
                        this.Logger.Info($"Applying '{filter.GetType().Name}' contact selection filter to retrieved contacts.", this);

                        var selectionFilter = filter as IContactSelectionFilter;

                        if (selectionFilter == null)
                        {
                            throw new ConfigurationException($"'{filter.GetType().Name}' can't be casted to IContactSelectionFilter. Please review your configuration.");
                        }

                        contactIds = contactIds.Where(selectionFilter.GetFilter());
                    }
                }

                var result = contactIds.Select(x => x._id).Distinct().ToList();

                this.Logger.Info($"Returning {result.Count} contact ids to re-index.", this);

                return result;
            }) ?? Enumerable.Empty<Guid>();
        }

        protected abstract IEnumerable<ContactIdentifiersData> GetContactIds();

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
    }
}
