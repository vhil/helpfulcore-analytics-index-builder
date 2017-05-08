namespace Helpfulcore.AnalyticsIndexBuilder.ContactSelection
{
    using System;

    public class KnownContactsFilter : IContactSelectionFilter
    {
        public virtual Func<ContactIdentifiersData, bool> GetFilter()
        {
            return data => !string.IsNullOrEmpty(data?.Identifiers?.Identifier);
        }
    }
}
