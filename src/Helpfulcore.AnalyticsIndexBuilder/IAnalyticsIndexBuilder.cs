namespace Helpfulcore.AnalyticsIndexBuilder
{
    using System;
    using System.Collections.Generic;
    using ContentSearch;
    using Data;

    /// <summary>
    /// The AnalyticsIndexBuilder abstraction. Provides list of methods for building Sitecore analytics index.
    /// <para>
    /// Uses multithreading for parallel indexables building and submits them to the index in batches.
    /// </para>
    /// </summary>
    public interface IAnalyticsIndexBuilder : ILoggerChangeable
    {
        /// <summary>
        /// Indicates if given instance is currenty processing any operation.
        /// </summary>
        bool IsBusy { get; }

        /// <summary>
        /// Rebuilds and submits to index all data taken from Contacts and Interactions collections using the 
        /// <see cref="Helpfulcore.AnalyticsIndexBuilder.ContactSelection.ICollectionDataProvider"/> as a source.
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
        void RebuildAllIndexables(bool applyFilters);

        /// <summary>
        /// Rebuilds and submits to index all data taken from Contacts collection using the 
        /// <see cref="Helpfulcore.AnalyticsIndexBuilder.ContactSelection.ICollectionDataProvider"/> as a source.
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
        void RebuildContactIndexableTypes(bool applyFilters);

        /// <summary>
        /// Rebuilds and submits to index all data taken from Contacts collection for specified contacts using the 
        /// <see cref="Helpfulcore.AnalyticsIndexBuilder.ContactSelection.ICollectionDataProvider"/> as a source.
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
        void RebuildContactIndexableTypes(IEnumerable<Guid> contactIds);

        /// <summary>
        /// Rebuilds and submits to index 'visit' indexables taken from Interactions collection using the 
        /// <see cref="Helpfulcore.AnalyticsIndexBuilder.ContactSelection.ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// This includes building of 'visit' indexable type
        /// </para>
        /// </summary>
        /// <param name="applyFilters">
        /// Specifies whether to apply contact filtering that is configured in Helpfulcore.AnalyticsIndexBuilder.config in helpfulcore/analytics.index.builder/collectionDataProvider Filters collection
        /// </param>
        void RebuildVisitIndexableTypes(bool applyFilters);

        /// <summary>
        /// Rebuilds and submits to index 'visit' indexables taken from Contacts collection using 
        /// <see cref="Helpfulcore.AnalyticsIndexBuilder.ContactSelection.ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// Builds only visits of contacts which contact ID's specified in the input parameter.
        /// </para>
        /// </summary>
        /// <param name="contactIds">
        /// Specify contact ID's you want to rebuild 'visit' indexables for.
        /// </param>
        void RebuildVisitIndexableTypes(IEnumerable<Guid> contactIds);

        /// <summary>
        /// Rebuilds and submits to index 'contact' indexables taken from Contacts collection using the 
        /// <see cref="Helpfulcore.AnalyticsIndexBuilder.ContactSelection.ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// This includes building of 'contact' indexable type
        /// </para>
        /// </summary>
        /// <param name="applyFilters">
        /// Specifies whether to apply contact filtering that is configured in Helpfulcore.AnalyticsIndexBuilder.config in helpfulcore/analytics.index.builder/collectionDataProvider Filters collection
        /// </param>
        void RebuildContactIndexables(bool applyFilters);

        /// <summary>
        /// Rebuilds and submits to index 'contact' indexables taken from Contacts collection using 
        /// <see cref="Helpfulcore.AnalyticsIndexBuilder.ContactSelection.ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// Builds only contacts which contact ID's specified in the input parameter.
        /// </para>
        /// </summary>
        /// <param name="contactIds">
        /// Specify contact ID's you want to rebuild 'contact' indexables for.
        /// </param>
        void RebuildContactIndexables(IEnumerable<Guid> contactIds);

        /// <summary>
        /// Rebuilds and submits to index 'address' indexables taken from Contacts collection using the 
        /// <see cref="Helpfulcore.AnalyticsIndexBuilder.ContactSelection.ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// This includes building of 'address' indexable type
        /// </para>
        /// </summary>
        /// <param name="applyFilters">
        /// Specifies whether to apply contact filtering that is configured in Helpfulcore.AnalyticsIndexBuilder.config in helpfulcore/analytics.index.builder/collectionDataProvider Filters collection
        /// </param>
        void RebuildAddressIndexables(bool applyFilters);

        /// <summary>
        /// Rebuilds and submits to index 'contact' indexables taken from Contacts collection using 
        /// <see cref="Helpfulcore.AnalyticsIndexBuilder.ContactSelection.ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// Builds only addresses of contacts which contact ID's specified in the input parameter.
        /// </para>
        /// </summary>
        /// <param name="contactIds">
        /// Specify contact ID's you want to rebuild 'address' indexables for.
        /// </param>
        void RebuildAddressIndexables(IEnumerable<Guid> contactIds);

        /// <summary>
        /// Rebuilds and submits to index 'contactTag' indexables taken from Contacts collection using the 
        /// <see cref="Helpfulcore.AnalyticsIndexBuilder.ContactSelection.ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// This includes building of 'contactTag' indexable type
        /// </para>
        /// </summary>
        /// <param name="applyFilters">
        /// Specifies whether to apply contact filtering that is configured in Helpfulcore.AnalyticsIndexBuilder.config in helpfulcore/analytics.index.builder/collectionDataProvider Filters collection
        /// </param>
        void RebuildContactTagIndexables(bool applyFilters);

        /// <summary>
        /// Rebuilds and submits to index 'contactTag' indexables taken from Contacts collection database using 
        /// <see cref="Helpfulcore.AnalyticsIndexBuilder.ContactSelection.ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// Builds only contactTags of contacts which contact ID's specified in the input parameter.
        /// </para>
        /// </summary>
        /// <param name="contactIds">
        /// Specify contact ID's you want to rebuild 'contactTag' indexables for.
        /// </param>
        void RebuildContactTagIndexables(IEnumerable<Guid> contactIds);

        /// <summary>
        /// Rebuilds and submits to index 'visit' indexables taken from Interactions collection database using the 
        /// <see cref="Helpfulcore.AnalyticsIndexBuilder.ContactSelection.ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// This includes building of 'visit' indexable type
        /// </para>
        /// </summary>
        /// <param name="applyFilters">
        /// Specifies whether to apply contact filtering that is configured in Helpfulcore.AnalyticsIndexBuilder.config in helpfulcore/analytics.index.builder/collectionDataProvider Filters collection
        /// </param>
        void RebuildVisitIndexables(bool applyFilters);

        /// <summary>
        /// Rebuilds and submits to index 'visit' indexables taken from Interactions collection database using 
        /// <see cref="Helpfulcore.AnalyticsIndexBuilder.ContactSelection.ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// Builds only visits of contacts which contact ID's specified in the input parameter.
        /// </para>
        /// </summary>
        /// <param name="contactIds">
        /// Specify contact ID's you want to rebuild 'visit' indexables for.
        /// </param>
        void RebuildVisitIndexables(IEnumerable<Guid> contactIds);

        /// <summary>
        /// Rebuilds and submits to index 'visitPage' indexables taken from Interactions collection database using the 
        /// <see cref="Helpfulcore.AnalyticsIndexBuilder.ContactSelection.ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// This includes building of 'visitPage' indexable type
        /// </para>
        /// </summary>
        /// <param name="applyFilters">
        /// Specifies whether to apply contact filtering that is configured in Helpfulcore.AnalyticsIndexBuilder.config in helpfulcore/analytics.index.builder/collectionDataProvider Filters collection
        /// </param>
        void RebuildVisitPageIndexables(bool applyFilters);

        /// <summary>
        /// Rebuilds and submits to index 'visitPage' indexables taken from Interactions collection database using 
        /// <see cref="Helpfulcore.AnalyticsIndexBuilder.ContactSelection.ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// Builds only visitsPages of contacts which contact ID's specified in the input parameter.
        /// </para>
        /// </summary>
        /// <param name="contactIds">
        /// Specify contact ID's you want to rebuild 'visitPage' indexables for.
        /// </param>
        void RebuildVisitPageIndexables(IEnumerable<Guid> contactIds);

        /// <summary>
        /// Rebuilds and submits to index 'visitPageEvent' indexables taken from Interactions collection database using the 
        /// <see cref="Helpfulcore.AnalyticsIndexBuilder.ContactSelection.ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// This includes building of 'visitPageEvent' indexable type
        /// </para>
        /// </summary>
        /// <param name="applyFilters">
        /// Specifies whether to apply contact filtering that is configured in Helpfulcore.AnalyticsIndexBuilder.config in helpfulcore/analytics.index.builder/collectionDataProvider Filters collection
        /// </param>
        void RebuildVisitPageEventIndexables(bool applyFilters);

        /// <summary>
        /// Rebuilds and submits to index 'visitPageEvent' indexables taken from Interactions collection database using 
        /// <see cref="Helpfulcore.AnalyticsIndexBuilder.ContactSelection.ICollectionDataProvider"/> as a source.
        /// and submits them to the index using <see cref="IAnalyticsSearchService"/>
        /// <para>
        /// Builds only visitsPageEvents of contacts which contact ID's specified in the input parameter.
        /// </para>
        /// </summary>
        /// <param name="contactIds">
        /// Specify contact ID's you want to rebuild 'visitPageEvent' indexables for.
        /// </param>
        void RebuildVisitPageEventIndexables(IEnumerable<Guid> contactIds);
    }
}