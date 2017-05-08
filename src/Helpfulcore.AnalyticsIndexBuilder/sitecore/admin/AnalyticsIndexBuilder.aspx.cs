namespace Helpfulcore.AnalyticsIndexBuilder.sitecore.admin
{
    using System;
    using System.Threading;
    using Sitecore.Configuration;
    using Logging;
    using ContentSearch;
    using System.Web.Script.Serialization;
    using Sitecore.sitecore.admin;

    public partial class AnalyticsIndexBuilderPage : AdminPage
    {
        private static readonly object InitializationSyncRoot = new object();

        protected static IAnalyticsIndexBuilder AnalyticsIndexService;
        protected IAnalyticsSearchService AnalyticsSearchService;
        protected ProcessQueueLoggingProvider LogQueue;
        protected AnalyticsEntryFacetResult AnalyticsIndexFacets;
         
        public AnalyticsIndexBuilderPage()
        {
            this.AnalyticsIndexFacets = new AnalyticsEntryFacetResult();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.CheckSecurity();
            this.Initialize();

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

                if (task == "RebuildAll")
                {
                    ThreadPool.QueueUserWorkItem(i =>
                    {
                        AnalyticsIndexService.RebuildContactEntriesIndex();
                        this.LogQueue.EndLogging();
                    });
                }

                this.Response.End();

                return;
            }

            this.AnalyticsIndexFacets = this.AnalyticsSearchService.GetAnalyticsIndexFacets();
        }

        private void Initialize()
        {
            if (AnalyticsIndexService == null)
            {
                lock (InitializationSyncRoot)
                {
                    if (AnalyticsIndexService == null)
                    {
                        AnalyticsIndexService = Factory.CreateObject("helpfulcore/analytics.index.builder/analyticsIndexBuilder", true) as IAnalyticsIndexBuilder;
                    }
                }
            }

            this.AnalyticsSearchService = Factory.CreateObject("helpfulcore/analytics.index.builder/analyticsSearchService", true) as IAnalyticsSearchService;
            this.LogQueue = Factory.CreateObject("helpfulcore/analytics.index.builder/logging/providers/indexingQueueLog", true) as ProcessQueueLoggingProvider;
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