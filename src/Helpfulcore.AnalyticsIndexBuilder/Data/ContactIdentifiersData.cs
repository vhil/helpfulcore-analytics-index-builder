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

        public Guid _id { get; set; }
        public ContactIdentifiers Identifiers { get; set; }
    }
}