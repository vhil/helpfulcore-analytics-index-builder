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
	using Sitecore.ContentSearch.LuceneProvider;
    using System.Threading.Tasks;

	/// <summary>
	/// Analytics search service. Implementation of <see cref="IAnalyticsSearchService"/>. Performs operations with sitecore analytics content search index.
	/// </summary>
	public class AnalyticsSearchService : IAnalyticsSearchService
    {
        protected string AnalyticsIndexName => Settings.GetSetting("ContentSearch.Analytics.IndexName", "sitecore_analytics_index");
	    protected int SearchMaxResults => Settings.GetIntSetting("ContentSearch.SearchMaxResults", 1024);

		protected ILoggingService Logger;

        public AnalyticsSearchService(ILoggingService logger)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            this.Logger = logger;
        }

        /// <summary>
        /// Executes faceted search query to analytics content search index on indexable type.
        /// </summary>
        /// <returns>
        /// Returns aggregated facet search result as <see cref="AnalyticsIndexablesFacetResult"/>
        /// </returns>
        public virtual AnalyticsIndexablesFacetResult GetAnalyticsIndexFacets()
        {
            using (var context = ContentSearchManager.GetIndex(this.AnalyticsIndexName).CreateSearchContext())
            {
                // this includes query filter for type:contact
                var facets = context.GetQueryable<AnalyticsIndexable>().FacetOn(x => x.Type).GetFacets();
                return new AnalyticsIndexablesFacetResult(facets);
            }
        }

        /// <summary>
        /// Builds indexables into content search documents and submits them to the analytics index.
        /// </summary>
        /// <param name="indexables">
        /// IEnumerable of indexables to submit to analytics index.
        /// </param>
        public virtual void UpdateIndexables(IEnumerable<AbstractIndexable> indexables)
        {
            var indexablesToUpdate = indexables as ICollection<AbstractIndexable> ?? indexables.ToList();

            this.SafeExecution($"Updating {indexablesToUpdate.Count} indexables in", () =>
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

        /// <summary>
        /// Deletes all indexables from analytics index by specified indexable type.
        /// </summary>
        /// <param name="indexableType">
        /// Indexable type all entries of which are going to be deleted from index. By default next types are available : 'contact', 'contactTag', 'address', 'visit', 'visitPage', 'visitPageEvent'
        /// </param>
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

        /// <summary>
        /// Resets the sitecore analytics content search index. This action erases all data in the index.
        /// </summary>
        public virtual void ResetIndex()
        {
            this.SafeExecution($"Reseting ", () =>
            {
	            var index = ContentSearchManager.GetIndex(this.AnalyticsIndexName);

				index.Reset();
                index.Initialize();

            }, true);
        }

        protected virtual IEnumerable<IIndexableUniqueId> GetAllUniqueIdsByType(string indexableType)
        {
	        int totalCount;
	        var pageSize = this.SearchMaxResults;

			using (var context = ContentSearchManager.GetIndex(this.AnalyticsIndexName).CreateSearchContext())
			{
				var queryable = context.GetQueryable<AnalyticsIndexable>()
					.Where(x => x.Type == indexableType);

				var facets = queryable.FacetOn(x => x.Type).GetFacets();
	            var typeCategory = facets.Categories.First();
	            var contactsCategory = typeCategory.Values.First(x => x.Name == indexableType);
	            totalCount = contactsCategory.AggregateCount;
            }

	        var dic = new ConcurrentDictionary<string, IIndexableUniqueId>();
			var lastPage = totalCount / pageSize + 1;
			var options = new ParallelOptions { MaxDegreeOfParallelism = 4 };

	        Parallel.For(0, lastPage, options, pageNumber =>
	        {
		        using (var context = ContentSearchManager.GetIndex(this.AnalyticsIndexName).CreateSearchContext())
		        {
			        var queryable = context.GetQueryable<AnalyticsIndexable>()
					    .Where(x => x.Type == indexableType)
				        .Skip(pageNumber * pageSize)
				        .Take(pageSize);

			        var results = queryable.GetResults();

			        foreach (var indexable in results.Select(x => x.Document))
			        {
				        if (!dic.ContainsKey(indexable.UniqueId.ToString()))
				        {
					        dic.TryAdd(indexable.UniqueId.ToString(), indexable.UniqueId);
				        }
			        }
		        }
	        });

	        return dic.Values;
		}

        protected virtual object BuildIndexableDocument(IIndexable indexable, IProviderUpdateContext context)
        {
            var sitecoreIndexableItem = indexable as SitecoreIndexableItem;
            if (sitecoreIndexableItem != null)
            {
                sitecoreIndexableItem.IndexFieldStorageValueFormatter = context.Index.Configuration.IndexFieldStorageValueFormatter;
            }

            var builderObject = ReflectionUtil.CreateObject(
                context.Index.Configuration.DocumentBuilderType,
                new[] { indexable, context as object });

	        var document = this.BuildSolrDocument(builderObject) ?? this.BuildLuceneDocument(builderObject, indexable);

	        if (document == null)
	        {
				throw new InvalidCastException("Unable to cast builder object.");
	        }

	        return document;
        }

	    protected virtual object BuildLuceneDocument(object builderObject, IIndexable indexable)
	    {
		    var documentBuilder = builderObject as LuceneDocumentBuilder;

		    if (documentBuilder == null)
		    {
			    return null;
		    }

		    documentBuilder.AddSpecialFields();
		    documentBuilder.AddItemFields();
		    documentBuilder.AddComputedIndexFields();
		    documentBuilder.AddBoost();

		    return new IndexData(ContentSearchManager.GetIndex(this.AnalyticsIndexName), indexable, documentBuilder).BuildDocument();
	    }

	    protected virtual object BuildSolrDocument(object builderObject)
		{
			var documentBuilder = builderObject as AbstractDocumentBuilder<ConcurrentDictionary<string, object>>;

			if (documentBuilder == null)
			{
				return null;
			}

			documentBuilder.AddSpecialFields();
			documentBuilder.AddItemFields();
			documentBuilder.AddComputedIndexFields();
			documentBuilder.AddBoost();

			return documentBuilder.Document.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
		}

	    protected virtual void SafeExecution(string actionDescription, Action action, bool info = false)
        {
            var startMessage = $"{actionDescription} '{this.AnalyticsIndexName}' content search index...";

            if (info) this.Logger.Info(startMessage, this);
            else this.Logger.Debug(startMessage, this);

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
                var endMessage = $"DONE {actionDescription} '{this.AnalyticsIndexName}' content search index.";

                if (info) this.Logger.Info(endMessage, this);
                else this.Logger.Debug(endMessage, this);
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
