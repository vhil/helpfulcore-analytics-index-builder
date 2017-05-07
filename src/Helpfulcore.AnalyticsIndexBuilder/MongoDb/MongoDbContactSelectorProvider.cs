namespace Helpfulcore.AnalyticsIndexBuilder.MongoDb
{
    using System.Collections.Generic;
    using ContactSelection;
    using Logging;
    using Sitecore.Analytics.Data.DataAccess.MongoDb;

    public class MongoDbContactSelectorProvider : AbstractContactSelectorProvider
    {
        protected readonly ILoggingService Logger;

        public MongoDbContactSelectorProvider(ILoggingService logger)
        {
            this.Logger = logger;
        }

        protected override IEnumerable<ContactIdentifiersData> GetContactIds()
        {
            var driver = MongoDbDriver.FromConnectionString("analytics");
            return driver.Contacts.FindAllAs<ContactIdentifiersData>();
        }
    }
}