namespace Helpfulcore.AnalyticsIndexBuilder.ContactSelection
{
    using System;

    public interface IContactSelectionFilter
    {
        Func<ContactIdentifiersData, bool> GetFilter();
    }
}
