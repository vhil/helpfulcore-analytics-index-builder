namespace Helpfulcore.AnalyticsIndexBuilder.ContentSearch
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.Analytics.Models;
    using Sitecore.ContentSearch.Linq;
    using Sitecore.Reflection;

    using Lucene.Net.Index;

    public class AnalyticsSearchService : IAnalyticsSearchService
    {
        protected readonly string AnalyticsIndexName;

        public AnalyticsSearchService(string analyticsIndexName)
        {
            this.AnalyticsIndexName = analyticsIndexName;
        }

        public AnalyticsEntryFacetResult GetAnalyticsIndexFacets()
        {
            using (var context = ContentSearchManager.GetIndex(this.AnalyticsIndexName).CreateSearchContext())
            {
                // this includes filter for type:contact
                var facets = context.GetQueryable<AnalyticsEntry>().FacetOn(x => x.Type).GetFacets();
                return new AnalyticsEntryFacetResult(facets);
            }
        }

        public IEnumerable<IndexedContact> GetIndexedContacts(IEnumerable<Guid> contactIds = null)
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

        public void UpdateContactsInIndex(IEnumerable<ContactIndexable> contacts)
        {
            using (var context = ContentSearchManager.GetIndex(this.AnalyticsIndexName).CreateUpdateContext())
            {
                foreach (var contact in contacts)
                {
                    var updateTerm = new Term("_uniqueid", contact.UniqueId.Value.ToString());
                    var executionContext = contact.Culture != null ? new CultureExecutionContext(contact.Culture) : null;
                    var document = this.BuildIndexableDocument(contact, context);

                    context.UpdateDocument(document, updateTerm, executionContext);
                }

                context.Commit();
            }
        }

        public void RemoveContactsFromIndex(IEnumerable<IndexedContact> contacts)
        {
            using (var context = ContentSearchManager.GetIndex(this.AnalyticsIndexName).CreateDeleteContext())
            {
                foreach (var contact in contacts)
                {
                    context.Delete(contact.UniqueId);
                }

                context.Commit();
            }
        }

        protected Dictionary<string, object> BuildIndexableDocument(IIndexable indexable, IProviderUpdateContext context)
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
    }
}
