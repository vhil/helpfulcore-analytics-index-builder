namespace Helpfulcore.AnalyticsIndexBuilder.Data
{
    using System;

    [Serializable]
    public class ContactIdentifiersData
    {
        public ContactIdentifiersData()
        {
            this.Identifiers = new ContactIdentifiers();
        }

        public virtual Guid _id { get; set; }
        public virtual ContactIdentifiers Identifiers { get; set; }
    }
}