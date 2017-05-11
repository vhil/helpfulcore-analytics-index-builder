namespace Helpfulcore.AnalyticsIndexBuilder.ContentSearch
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using Sitecore.Configuration;
    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.Linq;
    using Sitecore.Reflection;

    using Lucene.Net.Index;
    using Logging;

    public class AnalyticsSearchService : IAnalyticsSearchService
    {
        protected string AnalyticsIndexName => Settings.GetSetting("ContentSearch.Analytics.IndexName", "sitecore_analytics_index"); 
        protected ILoggingService Logger;

        public AnalyticsSearchService(ILoggingService logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
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

        public virtual void UpdateIndexables(IEnumerable<AbstractIndexable> indexablesToUpdate)
        {
            var indexables = indexablesToUpdate as ICollection<AbstractIndexable> ?? indexablesToUpdate.ToList();

            this.SafeExecution($"Updating {indexables.Count} indexables in", () =>
            {
                using (var context = ContentSearchManager.GetIndex(this.AnalyticsIndexName).CreateUpdateContext())
                {
                    foreach (var indexable in indexables)
                    {
                        var updateTerm = new Term("_uniqueid", indexable.UniqueId.Value.ToString());
                        var executionContext = indexable.Culture != null ? new CultureExecutionContext(indexable.Culture) : null;
                        var document = this.BuildIndexableDocument(indexable, context);

                        context.UpdateDocument(document, updateTerm, executionContext);
                    }

                    context.Commit();
                }
            });
        }

        public virtual void DeleteIndexablesByType(string indexableType)
        {
            indexableType = indexableType.Trim().ToLower();
            var existingIndexablesIds = this.GetAllUniqueIdsByType(indexableType);

            this.SafeExecution($"Deleting all indexables of type [{indexableType}] from", () =>
            {
                using (var context = ContentSearchManager.GetIndex(this.AnalyticsIndexName).CreateDeleteContext())
                {
                    foreach (var indexableId in existingIndexablesIds)
                    {
                        context.Delete(indexableId);
                    }

                    context.Commit();
                }
            }, true);
        }

        public virtual void ResetIndex()
        {
            this.SafeExecution($"Erasing everything from ", () =>
            {
                using (var index = ContentSearchManager.GetIndex(this.AnalyticsIndexName))
                {
                    index.Reset();
                }
            }, true);
        }

        protected virtual IEnumerable<IIndexableUniqueId> GetAllUniqueIdsByType(string indexableType)
        {
            using (var context = ContentSearchManager.GetIndex(this.AnalyticsIndexName).CreateSearchContext())
            {
                return context.GetQueryable<AnalyticsEntry>().Where(x => x.Type == indexableType).ToList().Select(x => x.UniqueId);
            }
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

        protected virtual void SafeExecution(string actionDescription, Action action, bool info = false)
        {
            if (info)
            {
                this.Logger.Info($"{actionDescription} '{this.AnalyticsIndexName}' content search index...", this);
            }
            else
            {
                this.Logger.Debug($"{actionDescription} '{this.AnalyticsIndexName}' content search index...", this);
            }

            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                this.Logger.Error($"Error while {actionDescription} '{this.AnalyticsIndexName}'. {ex.Message}", this, ex);
            }
            finally
            {
                if (info)
                {
                    this.Logger.Info($"DONE {actionDescription} '{this.AnalyticsIndexName}' content search index.", this);
                }
                else
                {
                    this.Logger.Debug($"DONE {actionDescription} '{this.AnalyticsIndexName}' content search index.", this);
                }
            }
        }

        public virtual void ChangeLogger(ILoggingService logger)
        {
            if (logger != null)
            {
                this.Logger = logger;
            }
        }
    }
}
