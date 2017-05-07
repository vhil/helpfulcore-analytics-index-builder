namespace Helpfulcore.AnalyticsIndexBuilder.ContactSelection
{
    using System;
    using System.Collections.Generic;

    public interface IContactsSelectorProvider
    {
        IEnumerable<Guid> GetContactIdsToReindex();
    }
}
