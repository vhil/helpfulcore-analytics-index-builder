namespace Helpfulcore.AnalyticsIndexBuilder.Collections
{
    using Sitecore.Analytics.Model.Entities;

    public class LazyContact : ILazyUniqueItem<IContact>
    {
        private readonly IContact contact;

        public LazyContact(IContact contact)
        {
            this.contact = contact;
        }

        public string UniqueId => this.contact.Id.Guid.ToString();
        public IContact Value => this.contact;
    }
}