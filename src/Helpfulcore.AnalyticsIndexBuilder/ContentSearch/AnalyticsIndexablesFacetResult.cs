namespace Helpfulcore.AnalyticsIndexBuilder.ContentSearch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Sitecore.ContentSearch.Linq;

    [Serializable]
    public class AnalyticsIndexablesFacetResult
    {
        public ICollection<AnalyticsIndexablesFacet> Facets { get; protected set; }

        public AnalyticsIndexablesFacetResult()
        {
            this.Facets = new List<AnalyticsIndexablesFacet>
            {
                new AnalyticsIndexablesFacet("contact", 0, true),
                new AnalyticsIndexablesFacet("address", 0, true),
                new AnalyticsIndexablesFacet("contactTag", 0, true),
                new AnalyticsIndexablesFacet("visit", 0, true),
                new AnalyticsIndexablesFacet("visitPage", 0, true),
                new AnalyticsIndexablesFacet("visitPageEvent", 0, true)
            };

        }

        public AnalyticsIndexablesFacetResult(FacetResults facetResults)
            : this()
        {
            var byType = facetResults?.Categories?.FirstOrDefault(x => x.Name.ToLower() == "type");

            if (byType != null)
            {
                var nonEmpty = byType.Values.Where(x => x.AggregateCount > 0);
                var facets = nonEmpty.Select(x => new AnalyticsIndexablesFacet(x.Name.ToLower(), x.AggregateCount));

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
