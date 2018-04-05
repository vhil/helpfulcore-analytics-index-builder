namespace Helpfulcore.AnalyticsIndexBuilder.Data
{
    using System;
    using MongoDB.Bson.Serialization.Attributes;

    [Serializable]
    [BsonIgnoreExtraElements]
    public class ContactIdentifiers
    {
        public virtual bool IsEmpty { get; set; }
        public virtual string Identifier { get; set; }
    }
}