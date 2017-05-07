namespace Helpfulcore.AnalyticsIndexBuilder.ContactSelection
{
    using System;

    public class KnownContactsFilter : IContactSelectionFilter
    {
        public Func<ContactIdentifiersData, bool> GetFilter()
        {
            return data => !string.IsNullOrEmpty(data?.Identifiers?.Identifier);
        }
    }
}
