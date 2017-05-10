namespace Helpfulcore.AnalyticsIndexBuilder.ContentSearch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sitecore.ContentSearch.Linq;

    [Serializable]
    public class AnalyticsEntryFacetResult
    {
        public ICollection<AnalyticsEntryFacet> Facets { get; protected set; }

        public AnalyticsEntryFacetResult()
        {
            this.Facets = new List<AnalyticsEntryFacet>
            {
                new AnalyticsEntryFacet("contact", 0, true),
                new AnalyticsEntryFacet("address", 0, true),
                new AnalyticsEntryFacet("contactTag", 0, true),
                new AnalyticsEntryFacet("visit", 0, true),
                new AnalyticsEntryFacet("visitPage", 0, true),
                new AnalyticsEntryFacet("visitPageEvent", 0, true)
            };

        }

        public AnalyticsEntryFacetResult(FacetResults facetResults)
            : this()
        {
            var byType = facetResults?.Categories?.FirstOrDefault(x => x.Name.ToLower() == "type");

            if (byType != null)
            {
                var nonEmpty = byType.Values.Where(x => x.AggregateCount > 0);
                var facets = nonEmpty.Select(x => new AnalyticsEntryFacet(x.Name.ToLower(), x.AggregateCount));

                foreach (var facet in facets.OrderByDescending(c => c.ActionsAvailable).ThenBy(c => c.Type))
                {
                    var existingFacet = this.Facets.FirstOrDefault(x => 
                        x.Type.Equals(facet.Type, StringComparison.CurrentCultureIgnoreCase));

                    if (existingFacet != null)
                    {
                        existingFacet.Count = facet.Count;
                    }
                    else
                    {
                        this.Facets.Add(facet);
                    }
                }
            }
        }
    }
}
