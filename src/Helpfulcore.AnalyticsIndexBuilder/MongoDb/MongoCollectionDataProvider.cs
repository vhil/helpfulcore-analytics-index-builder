namespace Helpfulcore.AnalyticsIndexBuilder.MongoDb
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    using Sitecore.Data;
    using Sitecore.Analytics.Model;
    using Sitecore.Analytics.Data.DataAccess.MongoDb;
    using Sitecore.Analytics.Aggregation;
    using Sitecore.Analytics.Aggregation.Data.DataAccess;
    using Sitecore.Analytics.Aggregation.Data.Model;
    using Sitecore.Analytics.Data.DataAccess;
    using Sitecore.Analytics.Model.Entities;

    using Collections;
    using Data;
    using Logging;

    public class MongoCollectionDataProvider : CollectionDataProvider
    {
        protected readonly string AnalyticsMongoConnectionString;
        protected readonly IContactFactory ContactFactory;
        protected readonly ICollectionDataProvider2 CollectionDataProvider;

        public MongoCollectionDataProvider(string analyticsConnectionString, ILoggingService logger, IContactFactory contactFactory)
            :base(logger)
        {
            if (string.IsNullOrEmpty(analyticsConnectionString)) throw new ArgumentNullException(nameof(analyticsConnectionString));

            this.AnalyticsMongoConnectionString = analyticsConnectionString;
            this.ContactFactory = contactFactory;
            this.CollectionDataProvider = new MongoDbCollectionDataProvider(analyticsConnectionString);
        }

        protected override IEnumerable<ContactIdentifiersData> GetContactIdentifiers()
        {
            this.Logger.Info($"Retrieveing all contact id's from '{this.AnalyticsMongoConnectionString}' collection database.", this);

            var driver = MongoDbDriver.FromConnectionString(this.AnalyticsMongoConnectionString);
            return driver.Contacts.FindAllAs<ContactIdentifiersData>();
        }

        public override IEnumerable<Guid> GetAllContactIdsToReindex()
        {
            var contactIds = this.GetContactIdentifiers().Select(x => x._id).Distinct().ToArray();

            this.Logger.Info($"Returning {contactIds.Length} contact ids to reindex.", this);

            return contactIds;
        }

        public override IEnumerable<VisitData> GetVisitDataToReindex()
        {
            return this.SafeExecution($"Retrieveing all visit data from '{this.AnalyticsMongoConnectionString}' collection database.", () =>
            {
                var driver = MongoDbDriver.FromConnectionString(this.AnalyticsMongoConnectionString);
                return driver.Interactions.FindAllAs<VisitData>();
            });
        }

        public override IEnumerable<VisitData> GetVisitDataToReindex(IEnumerable<Guid> contactIds)
        {
            var map = contactIds.Distinct().ToDictionary(k => k, v => v);
            return this.GetVisitDataToReindex().Where(v => map.ContainsKey(v.ContactId));
        }

        public override IEnumerable<IEnumerable<IContact>> GetContacts()
        {
            return this.GetContacts(this.GetAllContactIdsToReindex());
        }

        public override IEnumerable<IEnumerable<IContact>> GetContacts(IEnumerable<Guid> contactIds)
        {
            var contacts = contactIds
                .Select(id => DataAdapterManager.Provider.LoadContactReadOnly(new ID(id), this.ContactFactory));

            return new BatchedCollection<IContact>(this.BatchSize, contacts);
        }

        public override IEnumerable<IEnumerable<IVisitAggregationContext>> GetVisits()
        {
            var visits = this.GetVisitDataToReindex()
                .Select(data => new InteractionKey(data.ContactId, data.InteractionId))
                .Select(key => this.CollectionDataProvider.CreateContextForInteraction(key));

            return new BatchedCollection<IVisitAggregationContext>(this.BatchSize, visits);
        }

        public override IEnumerable<IEnumerable<IVisitAggregationContext>> GetVisits(IEnumerable<Guid> contactIds)
        {
            var visits = this.GetVisitDataToReindex(contactIds)
                .Select(data => new InteractionKey(data.ContactId, data.InteractionId))
                .Select(key => this.CollectionDataProvider.CreateContextForInteraction(key));

            return new BatchedCollection<IVisitAggregationContext>(this.BatchSize, visits);
        }
    }
}