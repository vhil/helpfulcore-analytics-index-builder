namespace Helpfulcore.AnalyticsIndexBuilder.MongoDb
{
    using System.Collections.Generic;
    using Sitecore.Analytics.Data.DataAccess.MongoDb;

    using ContactSelection;
    using Helpfulcore.Logging;

    public class MongoDbContactSelectorProvider : AbstractContactSelectorProvider
    {
        protected readonly string AnalyticsMongoConnectionString;

        public MongoDbContactSelectorProvider(string analyticsConnectionString, ILoggingService logger)
            :base(logger)
        {
            this.AnalyticsMongoConnectionString = analyticsConnectionString;
        }

        protected override IEnumerable<ContactIdentifiersData> GetContactIds()
        {
            this.Logger.Info($"Retrieveing all contact id's from '{this.AnalyticsMongoConnectionString}' collection database.", this);

            var driver = MongoDbDriver.FromConnectionString(this.AnalyticsMongoConnectionString);
            return driver.Contacts.FindAllAs<ContactIdentifiersData>();
        }
    }
}