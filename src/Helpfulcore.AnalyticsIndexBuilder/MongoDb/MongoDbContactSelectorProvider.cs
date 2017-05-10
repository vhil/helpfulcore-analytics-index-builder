namespace Helpfulcore.AnalyticsIndexBuilder.MongoDb
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Sitecore.Analytics.Data.DataAccess.MongoDb;

    using ContactSelection;
    using Logging;

    public class MongoDbContactSelectorProvider : AbstractContactSelectorProvider
    {
        protected readonly string AnalyticsMongoConnectionString;

        public MongoDbContactSelectorProvider(string analyticsConnectionString, ILoggingService logger)
            :base(logger)
        {
            if (string.IsNullOrEmpty(analyticsConnectionString)) throw new ArgumentNullException(nameof(analyticsConnectionString));
            this.AnalyticsMongoConnectionString = analyticsConnectionString;
        }

        protected override IEnumerable<ContactIdentifiersData> GetContactIds()
        {
            this.Logger.Info($"Retrieveing all contact id's from '{this.AnalyticsMongoConnectionString}' collection database.", this);

            var driver = MongoDbDriver.FromConnectionString(this.AnalyticsMongoConnectionString);
            return driver.Contacts.FindAllAs<ContactIdentifiersData>();
        }

        public override IEnumerable<Guid> GetAllContactIdsToReindex()
        {
            return this.GetContactIds().Select(x => x._id);
        }
    }
}