namespace Helpfulcore.AnalyticsIndexBuilder.Data
{
    using System;

    [Serializable]
    public class ContactIdentifiers
    {
        public virtual bool IsEmpty { get; set; }
        public virtual string Identifier { get; set; }
    }
}