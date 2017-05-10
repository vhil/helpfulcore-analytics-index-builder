namespace Helpfulcore.AnalyticsIndexBuilder.ContactSelection
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Abstraction for retrieving contact ids from collection database to re-index.
    /// </summary>
    public interface IContactsSelectorProvider : ILoggerChangeable
    {
        /// <summary>
        /// Retrieves contact ids to re-index.
        /// </summary>
        /// <returns></returns>
        IEnumerable<Guid> GetFilteredContactIdsToReindex();

        IEnumerable<Guid> GetAllContactIdsToReindex();
    }
}
