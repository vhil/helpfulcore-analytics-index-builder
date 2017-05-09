namespace Helpfulcore.AnalyticsIndexBuilder.sitecore.admin
{
    using System;
    using System.Threading;
    using Sitecore.Configuration;
    using System.Web.Script.Serialization;

    using Sitecore.sitecore.admin;
    using Logging;

    using ContentSearch;

    public partial class AnalyticsIndexBuilderPage : AdminPage
    {
        private static readonly object InitializationSyncRoot = new object();

        protected static IAnalyticsIndexBuilder AnalyticsIndexBuilder;
        protected IAnalyticsSearchService AnalyticsSearchService;
        protected ProcessQueueLoggingProvider LogQueue;
        protected AnalyticsEntryFacetResult AnalyticsIndexFacets;
         
        public AnalyticsIndexBuilderPage()
        {
            var pageLogger = (ILoggingService)Factory.CreateObject("helpfulcore/analytics.index.builder/logging/pageLoggingService", true);
            this.AnalyticsSearchService = (IAnalyticsSearchService)Factory.CreateObject("helpfulcore/analytics.index.builder/analyticsSearchService", true);
            this.LogQueue = (ProcessQueueLoggingProvider)Factory.CreateObject("helpfulcore/analytics.index.builder/logging/providers/indexingQueueLog", true);

            if (AnalyticsIndexBuilder == null)
            {
                lock (InitializationSyncRoot)
                {
                    if (AnalyticsIndexBuilder == null)
                    {
                        // need to create specific static instance so it uses page logger.
                        AnalyticsIndexBuilder = (IAnalyticsIndexBuilder)Factory.CreateObject("helpfulcore/analytics.index.builder/analyticsIndexBuilder", true);
                        AnalyticsIndexBuilder.ChangeLogger(pageLogger);
                    }
                }
            }

            this.AnalyticsIndexFacets = new AnalyticsEntryFacetResult();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.CheckSecurity();

            var task = this.Request.QueryString["task"];

            if (!string.IsNullOrWhiteSpace(task))
            {
                this.Response.Clear();

                if (task == "GetLogProgress")
                {
                    this.GetLogProgress();
                }

                if (task == "GetFacets")
                {
                    this.GetFacets();
                }

                if (task == "RebuildContacts")
                {
                    ThreadPool.QueueUserWorkItem(i =>
                    {
                        AnalyticsIndexBuilder.RebuildContactEntriesIndex();
                        this.LogQueue.EndLogging();
                    });
                }

                if (task == "RebuildAddresses")
                {
                    ThreadPool.QueueUserWorkItem(i =>
                    {
                        AnalyticsIndexBuilder.RebuildAddressEntriesIndex();
                        this.LogQueue.EndLogging();
                    });
                }

                if (task == "RebuildContactTags")
                {
                    ThreadPool.QueueUserWorkItem(i =>
                    {
                        AnalyticsIndexBuilder.RebuildContactTagEntriesIndex();
                        this.LogQueue.EndLogging();
                    });
                }

                this.Response.End();

                return;
            }

            this.AnalyticsIndexFacets = this.AnalyticsSearchService.GetAnalyticsIndexFacets();
        }

        private void GetFacets()
        {
            this.Response.ContentType = "text/javascript";

            var facets = this.AnalyticsSearchService.GetAnalyticsIndexFacets();
            var response = new JavaScriptSerializer().Serialize(facets);

            this.Response.Write(response);
        }

        protected void GetLogProgress()
        {
            this.Response.ContentType = "text/javascript";

            var messages = this.LogQueue.GetLogMessages();
            var response = new JavaScriptSerializer().Serialize(messages);

            this.Response.Write(response);
        }
    }
}