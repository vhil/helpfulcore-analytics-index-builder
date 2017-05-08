namespace Helpfulcore.AnalyticsIndexBuilder.ContactSelection
{
    using System;

    /// <summary>
    /// Abstraction for filters collection of IContactsSelectorProvider types.
    /// Applies enumerable filter for retrieved contacts from collection database before reindexing.
    /// </summary>
    public interface IContactSelectionFilter
    {
        /// <summary>
        /// Gets the implemented filter for this instance.
        /// </summary>
        /// <returns></returns>
        Func<ContactIdentifiersData, bool> GetFilter();
    }
}
