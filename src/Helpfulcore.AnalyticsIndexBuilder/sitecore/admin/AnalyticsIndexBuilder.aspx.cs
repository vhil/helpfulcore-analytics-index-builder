namespace Helpfulcore.AnalyticsIndexBuilder.sitecore.admin
{
    using System;
    using System.Threading;
    using Sitecore.Configuration;
    using Logging;
    using ContentSearch;
    using System.Web.Script.Serialization;
    using Sitecore.sitecore.admin;

    public partial class AnalyticsIndexBuilder : AdminPage
    {
        protected IAnalyticsIndexBuilder AnalyticsIndexService;
        protected IAnalyticsSearchService AnalyticsSearchService;
        protected ProcessQueueLoggingProvider LogQueue;
        protected AnalyticsEntryFacetResult AnalyticsIndexFacets;
         
        public AnalyticsIndexBuilder()
        {
            this.AnalyticsIndexService = Factory.CreateObject("helpfulcore/analytics.index.builder/analyticsIndexBuilder", true) as IAnalyticsIndexBuilder;
            this.AnalyticsSearchService = Factory.CreateObject("helpfulcore/analytics.index.builder/analyticsSearchService", true) as IAnalyticsSearchService;
            this.LogQueue = Factory.CreateObject("helpfulcore/analytics.index.builder/logging/providers/indexingQueueLog", true) as ProcessQueueLoggingProvider;
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

                if (task == "RebuildAll")
                {
                    ThreadPool.QueueUserWorkItem(i =>
                    {
                        this.AnalyticsIndexService.RebuildContactEntriesIndex();
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