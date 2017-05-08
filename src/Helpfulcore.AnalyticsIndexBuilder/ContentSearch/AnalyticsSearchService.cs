namespace Helpfulcore.AnalyticsIndexBuilder.ContentSearch
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using Sitecore.Configuration;
    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.Analytics.Models;
    using Sitecore.ContentSearch.Linq;
    using Sitecore.Reflection;

    using Lucene.Net.Index;
    using Helpfulcore.Logging;

    public class AnalyticsSearchService : IAnalyticsSearchService
    {
        protected string AnalyticsIndexName => Settings.GetSetting("ContentSearch.Analytics.IndexName", "sitecore_analytics_index"); 
        protected ILoggingService Logger;

        public AnalyticsSearchService(ILoggingService logger)
        {
            this.Logger = logger;
        }

        public virtual AnalyticsEntryFacetResult GetAnalyticsIndexFacets()
        {
            using (var context = ContentSearchManager.GetIndex(this.AnalyticsIndexName).CreateSearchContext())
            {
                // this includes query filter for type:contact
                var facets = context.GetQueryable<AnalyticsEntry>().FacetOn(x => x.Type).GetFacets();
                return new AnalyticsEntryFacetResult(facets);
            }
        }

        public virtual IEnumerable<IndexedContact> GetIndexedContacts(IEnumerable<Guid> contactIds = null)
        {
            if (contactIds == null)
            {
                contactIds = new List<Guid>();
            }

            var contactsIdsDictionary = contactIds.Distinct().ToDictionary(k => k, v => v);

            using (var context = ContentSearchManager.GetIndex(this.AnalyticsIndexName).CreateSearchContext())
            {
                // this includes filter for type:contact
                var allContacts = context.GetQueryable<IndexedContact>().ToList();

                if (contactsIdsDictionary.Any())
                {
                    return allContacts.Where(contact => contactsIdsDictionary.ContainsKey(contact.ContactId)).ToList();
                }

                return allContacts;
            }
        }

        public virtual void UpdateContactsInIndex(IEnumerable<ContactIndexable> contacts)
        {
            var indexables = contacts as ICollection<ContactIndexable> ?? contacts.ToList();

            this.SafeExecution($"Updating {indexables.Count} indexed contacts in", () =>
            {
                using (var context = ContentSearchManager.GetIndex(this.AnalyticsIndexName).CreateUpdateContext())
                {
                    foreach (var contact in indexables)
                    {
                        var updateTerm = new Term("_uniqueid", contact.UniqueId.Value.ToString());
                        var executionContext = contact.Culture != null ? new CultureExecutionContext(contact.Culture) : null;
                        var document = this.BuildIndexableDocument(contact, context);

                        context.UpdateDocument(document, updateTerm, executionContext);
                    }

                    context.Commit();
                }
            });
        }

        public virtual void RemoveContactsFromIndex(IEnumerable<IndexedContact> contacts)
        {
            var indexables = contacts as ICollection<IndexedContact> ?? contacts.ToList();

            this.SafeExecution($"Removing {indexables.Count} indexed contacts from", () =>
            {
                using (var context = ContentSearchManager.GetIndex(this.AnalyticsIndexName).CreateDeleteContext())
                {
                    foreach (var contact in indexables)
                    {
                        context.Delete(contact.UniqueId);
                    }

                    context.Commit();
                }
            });
        }

        protected virtual Dictionary<string, object> BuildIndexableDocument(IIndexable indexable, IProviderUpdateContext context)
        {
            var sitecoreIndexableItem = indexable as SitecoreIndexableItem;
            if (sitecoreIndexableItem != null)
            {
                sitecoreIndexableItem.IndexFieldStorageValueFormatter = context.Index.Configuration.IndexFieldStorageValueFormatter;
            }

            var builderObject = ReflectionUtil.CreateObject(
                context.Index.Configuration.DocumentBuilderType,
                new[] { indexable, context as object });

            var documentBuilder = (AbstractDocumentBuilder<ConcurrentDictionary<string, object>>)builderObject;

            if (documentBuilder == null)
            {
                throw new InvalidOperationException("Unable to create AbstractDocumentBuilder.");
            }

            documentBuilder.AddSpecialFields();
            documentBuilder.AddItemFields();
            documentBuilder.AddComputedIndexFields();
            documentBuilder.AddBoost();

            return documentBuilder.Document.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        protected virtual void SafeExecution(string actionDescription, Action action)
        {
            this.Logger.Info($"{actionDescription} '{this.AnalyticsIndexName}' content search index..", this);

            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                this.Logger.Error($"Error while {actionDescription}. {ex.Message}", this, ex);
            }
        }

        public void ChangeLogger(ILoggingService logger)
        {
            if (logger != null)
            {
                this.Logger = logger;
            }
        }
    }
}
