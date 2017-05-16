### Helpfulcore - helpful features for Sitecore
# Helpfulcore Analytics Index Builder

## Installation
To install `Helpfulcore.AnalyticsIndexBuilder` run this command in the Nuget Package Manager console on your Sitecore website project:

> install-package Helpfulcore.AnalyticsIndexBuilder

Or you can find a Sitecore module on the Sitecore Market Place with name `Helpfulcore.AnalyticsIndexBuilder`.

## Compatibility
- Built and tested on Sitecore CMS 8.2 rev 160729 (initial release) and SOLR content search integration.
- Supposed to work on any Sitecore CMS 8.x and later as well as on both SOLR and Lucene content search providers.


## Functionality

The module installs new Sitecore admin page which can be found by path `<host_name>/sitecore/admin/analyticsindexbuilder.aspx`. The module provides functionality for re-building `sitecore_analytics_index` using data in the collection database. It also includes methods to clean the index if nesessary

The module uses native Sitecore content search API for updating the index so both `SOLR` and `Lucene` content search providers should be supported. Also it uses native pipelines for building indexed records such as:
- `<contacttagindexable.loadfields>`
- `<contactaddressindexable.loadfields>`
- `<contactindexable.loadfields>`
- `<visitindexable.loadfields>`
- `<visitpageindexable.loadfields>`
- `<visitpageeventindexable.loadfields>`

The new admin page `<host_name>/sitecore/admin/analyticsindexbuilder.aspx` shows the analytics index content overview with count of each indexable type currently present in the index. And provides actions to 
- Delete indexables of specific indexable type
- Rebuild index for specific indexable type
- Reset whole analytics index
- Rebuild whole analytics index

There are options for re-building indexables only for required contacts. This was names as `filterable` funstion. By default, there is a filter for this action set to use only known contacts (contacts that have not empty identifier). Filtering can be extended or replaced using include configuration file.

The new admin page `<host_name>/sitecore/admin/analyticsindexbuilder.aspx` display real time log in the `executoin log` field. As well as logs all actions in separate log file `$(dataFolder)/logs/Helpfulcore.AnalyticsIndexBuilder.log.${date:format=yyyyMMdd}.txt`

There is a brief functionality legend at the bottom of new admin page `<host_name>/sitecore/admin/analyticsindexbuilder.aspx`.

## API

There is an option to use the API provided by the module in your code. The primary object is `AnalyticsIndexBuilder` class.
Here how you can get an instance of it:

```
using Sitecore.Configuration;
using Helpfulcore.AnalyticsIndexBuilder;
...

var analyticsIndexBuilder = (IAnalyticsIndexBuilder)Factory.CreateObject("helpfulcore/analytics.index.builder/analyticsIndexBuilder", true)
```

Here is the interface which it provides:

```
public interface IAnalyticsIndexBuilder
{
	bool IsBusy { get; }
	void RebuildAllIndexables(bool applyFilters);
	void RebuildContactIndexableTypes(bool applyFilters);
	void RebuildContactIndexableTypes(IEnumerable<Guid> contactIds);
	void RebuildVisitIndexableTypes(bool applyFilters);
	void RebuildVisitIndexableTypes(IEnumerable<Guid> contactIds);
	void RebuildContactIndexables(bool applyFilters);
	void RebuildContactIndexables(IEnumerable<Guid> contactIds);
	void RebuildAddressIndexables(bool applyFilters);
	void RebuildAddressIndexables(IEnumerable<Guid> contactIds);
	void RebuildContactTagIndexables(bool applyFilters);
	void RebuildContactTagIndexables(IEnumerable<Guid> contactIds);
	void RebuildVisitIndexables(bool applyFilters);
	void RebuildVisitIndexables(IEnumerable<Guid> contactIds);
	void RebuildVisitPageIndexables(bool applyFilters);
	void RebuildVisitPageIndexables(IEnumerable<Guid> contactIds);
	void RebuildVisitPageEventIndexables(bool applyFilters);
	void RebuildVisitPageEventIndexables(IEnumerable<Guid> contactIds);
}
```