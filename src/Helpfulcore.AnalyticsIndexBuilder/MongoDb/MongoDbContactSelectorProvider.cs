namespace Helpfulcore.AnalyticsIndexBuilder.MongoDb
{
    using System.Collections.Generic;
    using ContactSelection;
    using Logging;
    using Sitecore.Analytics.Data.DataAccess.MongoDb;

    public class MongoDbContactSelectorProvider : AbstractContactSelectorProvider
    {
        protected readonly string AnalyticsMongoConnectionStringName;

        public MongoDbContactSelectorProvider(string analyticsMongoConnectionStringName,  ILoggingService logger)
            :base(logger)
        {
            this.AnalyticsMongoConnectionStringName = analyticsMongoConnectionStringName;
        }

        protected override IEnumerable<ContactIdentifiersData> GetContactIds()
        {
            this.Logger.Info($"Retrieving contact ids from '{this.AnalyticsMongoConnectionStringName}' mongo database...", this);

            var driver = MongoDbDriver.FromConnectionString(this.AnalyticsMongoConnectionStringName);
            return driver.Contacts.FindAllAs<ContactIdentifiersData>();
        }
    }
}