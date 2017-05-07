namespace Helpfulcore.AnalyticsIndexBuilder.ContactSelection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class AbstractContactSelectorProvider : IContactsSelectorProvider
    {
        public ICollection<IContactSelectionFilter> Filters;

        protected AbstractContactSelectorProvider()
        {
            this.Filters = new List<IContactSelectionFilter>();
        }

        public IEnumerable<Guid> GetContactIdsToReindex()
        {
            var contactIds = this.GetContactIds();

            if (this.Filters.Any())
            {
                foreach (var filter in this.Filters)
                {
                    contactIds = contactIds.Where(filter.GetFilter());
                }
            }

            return contactIds.Select(x => x._id).Distinct();
        }

        protected abstract IEnumerable<ContactIdentifiersData> GetContactIds();
    }
}
