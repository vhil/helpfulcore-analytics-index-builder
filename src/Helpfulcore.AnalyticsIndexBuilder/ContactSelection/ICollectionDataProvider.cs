namespace Helpfulcore.AnalyticsIndexBuilder.ContactSelection
{
    using System;
    using System.Collections.Generic;

    using Sitecore.Analytics.Aggregation.Data.Model;
    using Sitecore.Analytics.Model.Entities;

    /// <summary>
    /// Abstraction for retrieving contact ids from collection database to re-index.
    /// </summary>
    public interface ICollectionDataProvider : ILoggerChangeable
    {
        /// <summary>
        /// Retrieves contact ids to re-index.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Guid> GetFilteredContactIdsToReindex();
        IEnumerable<Guid> GetAllContactIdsToReindex();
        IEnumerable<IContact> GetContacts();
        IEnumerable<IContact> GetContacts(IEnumerable<Guid> contactIds);
        IEnumerable<IVisitAggregationContext> GetVisits();
        IEnumerable<IVisitAggregationContext> GetVisits(IEnumerable<Guid> contactIds);
    }
}
