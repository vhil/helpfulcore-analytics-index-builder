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

            this.AnalyticsSearchService.ChangeLogger(pageLogger);
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

                if (task == "GetLogProgress") this.GetLogProgress();
                if (task == "GetFacets") this.GetFacets(); 

                if (task == "rebuild-filtered") this.StartAsyncAction(() => { AnalyticsIndexBuilder.RebuildAllIndexables(true); });
                if (task == "rebuild-contact-filtered") this.StartAsyncAction(() => { AnalyticsIndexBuilder.RebuildContactIndexables(true); });
                if (task == "rebuild-address-filtered") this.StartAsyncAction(() => { AnalyticsIndexBuilder.RebuildAddressIndexables(true); });
                if (task == "rebuild-contactTag-filtered") this.StartAsyncAction(() => { AnalyticsIndexBuilder.RebuildContactTagIndexables(true); });
                if (task == "rebuild-visit-filtered") this.StartAsyncAction(() => { AnalyticsIndexBuilder.RebuildVisitIndexables(true); });
                if (task == "rebuild-visitPage-filtered") this.StartAsyncAction(() => { AnalyticsIndexBuilder.RebuildVisitPageIndexables(true); });
                if (task == "rebuild-visitPageEvent-filtered") this.StartAsyncAction(() => { AnalyticsIndexBuilder.RebuildVisitPageEventIndexables(true); });

                if (task == "rebuild") this.StartAsyncAction(() => { AnalyticsIndexBuilder.RebuildAllIndexables(false); });
                if (task == "rebuild-contact") this.StartAsyncAction(() => { AnalyticsIndexBuilder.RebuildContactIndexables(false); });
                if (task == "rebuild-address") this.StartAsyncAction(() => { AnalyticsIndexBuilder.RebuildAddressIndexables(false); });
                if (task == "rebuild-contactTag") this.StartAsyncAction(() => { AnalyticsIndexBuilder.RebuildContactTagIndexables(false);});
                if (task == "rebuild-visit") this.StartAsyncAction(() => { AnalyticsIndexBuilder.RebuildVisitIndexables(false); });
                if (task == "rebuild-visitPage") this.StartAsyncAction(() => { AnalyticsIndexBuilder.RebuildVisitPageIndexables(false); });
                if (task == "rebuild-visitPageEvent") this.StartAsyncAction(() => { AnalyticsIndexBuilder.RebuildVisitPageEventIndexables(false); });
            
                if (task == "delete") this.StartAsyncAction(() => { this.AnalyticsSearchService.ResetIndex(); });
                if (task == "delete-contact") this.StartAsyncAction(() => { this.AnalyticsSearchService.DeleteIndexablesByType("contact"); });
                if (task == "delete-contactTag") this.StartAsyncAction(() => { this.AnalyticsSearchService.DeleteIndexablesByType("contacttag"); });
                if (task == "delete-address") this.StartAsyncAction(() => { this.AnalyticsSearchService.DeleteIndexablesByType("address"); });
                if (task == "delete-visit") this.StartAsyncAction(() => { this.AnalyticsSearchService.DeleteIndexablesByType("visit"); });
                if (task == "delete-visitPage") this.StartAsyncAction(() => { this.AnalyticsSearchService.DeleteIndexablesByType("visitpage"); });
                if (task == "delete-visitPageEvent") this.StartAsyncAction(() => { this.AnalyticsSearchService.DeleteIndexablesByType("visitpageevent"); });

                this.Response.End();

                return;
            }

            this.AnalyticsIndexFacets = this.AnalyticsSearchService.GetAnalyticsIndexFacets();
        }

        private void StartAsyncAction(Action action)
        {
            ThreadPool.QueueUserWorkItem(i =>
            {
                action.Invoke();
                this.LogQueue.EndLogging();
            });
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