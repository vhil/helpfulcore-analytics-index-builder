<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AnalyticsIndexBuilder.aspx.cs" Inherits="Helpfulcore.AnalyticsIndexBuilder.sitecore.admin.AnalyticsIndexBuilderPage" %>

<%@ Import Namespace="Helpfulcore.Logging" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Analytics index builder</title>
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css" integrity="sha384-BVYiiSIFeK1dGmJRAkycuHAHRg32OmUcww7on3RYdg4Va+PmSTsz/K68vbdEjh4u" crossorigin="anonymous">
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap-theme.min.css" integrity="sha384-rHyoN1iRsVXV4nD0JutlnGaslCJuC7uwjduW9SVrLvRYooPp2bWYgmgJQIXwl/Sp" crossorigin="anonymous">
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.12.4/jquery.min.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js" integrity="sha384-Tc5IQib027qvyjSMfHjOMaLkfuWVxZxUPnCJA7l2mCWNIpG9mGCD8wGNIcPD7Txa" crossorigin="anonymous"></script>
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <style>
        .row {
            margin-bottom: 10px;
        }

            .row .row {
                margin-top: 10px;
                margin-bottom: 0;
            }

        .block {
            background-color: rgba(245,245,245,0.70);
        }

        .col-lg-1, .col-lg-10, .col-lg-11, .col-lg-12, .col-lg-2, .col-lg-3, .col-lg-4, .col-lg-5, .col-lg-6, .col-lg-7, .col-lg-8, .col-lg-9, .col-md-1, .col-md-10, .col-md-11, .col-md-12, .col-md-2, .col-md-3, .col-md-4, .col-md-5, .col-md-6, .col-md-7, .col-md-8, .col-md-9, .col-sm-1, .col-sm-10, .col-sm-11, .col-sm-12, .col-sm-2, .col-sm-3, .col-sm-4, .col-sm-5, .col-sm-6, .col-sm-7, .col-sm-8, .col-sm-9, .col-xs-1, .col-xs-10, .col-xs-11, .col-xs-12, .col-xs-2, .col-xs-3, .col-xs-4, .col-xs-5, .col-xs-6, .col-xs-7, .col-xs-8, .col-xs-9 {
            padding-right: 7px;
            padding-left: 7px;
        }

        @media (min-width: 1200px) {
            .dl-horizontal dt {
                float: left;
                width: 295px;
                overflow: hidden;
                clear: left;
                text-align: left;
                text-overflow: ellipsis;
                white-space: nowrap;
            }
            .dl-horizontal dd {
                margin-left: 295px;
            }
        }
    </style>
</head>
<body>
    <div class="jumbotron">
        <div class="container">
            <div class="row">
                <div class="col-md-12">
                    <h1>Sitecore Analytics Index Builder</h1>
                </div>
            </div>
        </div>
    </div>
    
    <div class="container">
        <div class="row">
            <div class="col-md-12">
                <div class="col-md-12 text-right">
                    <button type="button" data-apply-filter="true" data-action="rebuild" data-type="" class="btn btn-success btn-active">Rebuild all filtered indexables</button>
                    <button type="button" data-action="rebuild" data-type="" class="btn btn-primary btn-active">Rebuild all indexables</button>
                    <button type="button" data-action="delete" data-type="" class="btn btn-danger btn-active">Reset analytics index</button>
                </div>
            </div>
        </div>
    </div>
    <div class="container">
        <div class="row">
            <div class="col-md-12">
                <div class="col-md-12">
                    <h3>Analytics index overview</h3>
                    <div class="table-responsive">
                        <table class="table">
                            <thead>
                                <tr>
                                    <th>Indexable type</th>
                                    <th>Count of indexables</th>
                                    <th width="120"></th>
                                    <th>Actions</th>
                                    <th></th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                                <%foreach (var facet in this.AnalyticsIndexFacets.Facets)
                                    { %>
                                <tr class="facet" data-facet-type="<%=facet.Type%>">
                                    <td class="facet-name"><%=facet.Type %></td>
                                    <td class="facet-count"><%=facet.Count %></td>
                                    <td class="action-status text-success"></td>
                                    <td>
                                        <% if (facet.ActionsAvailable)
                                            { %>
                                        <button type="button" data-apply-filter="true" data-action="rebuild" data-type="<%=facet.Type%>" class="btn btn-success btn-xs btn-active">Rebuild filtered indexables of this type</button>
                                        <%} %>
                                    </td>
                                    <td>
                                        <% if (facet.ActionsAvailable)
                                            { %>
                                        <button type="button" data-apply-filter="false" data-action="rebuild" data-type="<%=facet.Type%>" class="btn btn-primary btn-xs btn-active">Rebuild all indexables of this type</button>
                                        <%} %>
                                    </td>
                                    <td>
                                        <% if (facet.ActionsAvailable)
                                            { %>
                                        <button type="button" data-action="delete" data-type="<%=facet.Type%>" class="btn btn-danger btn-xs btn-active">Delete all indexables of this type</button>
                                        <%} %>
                                    </td>
                                </tr>
                                <%} %>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>
    <div class="container block">
        <div class="row">
            <div class="col-md-12">
                <div class="col-md-12">
                    <h3>Execution log <small>(last 100 lines)</small></h3>
                    <div class="form-group">
                        <textarea class="form-control input-sm" id="tbxLog" rows="15" readonly="readonly"></textarea>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <br />
<div class="container">
    <div class="row">
        <div class="col-md-12">
            <div class="col-md-12">
                <h4>This page lists all indexables available in the <code>sitecore_analytics_index</code> ContentSearch index and their amounts and provides next available actions:</h4>
                <br />
                <dl class="dl-horizontal">
                    <dt>Rebuild all filtered indexables</dt>
                    <dd><p>This action requests all contacts from collection database and applies contact filters declared in <code>/App_Config/Include/Helpfulcore/Helpfulcore.AnalyticsIndexBuilder.config</code> file under the <code>helpfulcore/analytics.index.builder/collectionDataProvider/Filters</code> configuration node to them and then finally re-builds index for retrieved contact IDs. By default there is a filter for contacts that have not empty identifiers. You are welcome to implement your own filters if required.</p></dd>
                    <dt>Rebuild all indexables</dt>
                    <dd><p>This action gets all indexables from collection database (Contacts or Interactions, depending on selected indexable type) and rebuilds index for them.</p></dd>
                    <dt>Reset analytics index</dt>
                    <dd><p>This action erases all data which currently exists in the <code>sitecore_analytics_index</code> ContentSearch index.</p></dd>
                    <dt>Delete all indexables of this type</dt>
                    <dd><p>Deletes indexed data of selected type from the <code>sitecore_analytics_index</code> ContentSearch index.</p></dd>
                    <dt>Rebuild filtered indexables of this type</dt>
                    <dd><p>This action does same as <code>Rebuild filtered indexables</code> but only for indexables of selected type.</p></dd>
                    <dt>Rebuild all indexables of this type</dt>
                    <dd><p>This action does same as <code>Rebuild all indexables</code> but only for indexables of selected type.</p></dd>
                </dl>
                <br/>
                <blockquote class="blockquote-reverse">
                    <p>For more information please have a look at <a href="https://vladimirhil.com/2017/05/16/rebuild-sitecore-analytics-index-without-re-building-reporting-database" target="_blank">this blog post</a></p>
                    <footer>Sourse code can be found at <a href="https://github.com/vhil/helpfulcore-analytics-index-builder" target="_blank">GitHub</a></footer>
                </blockquote>
            </div>
        </div>
    </div>
</div>
    <br />
    <br />
    <script>
        $(function () {

            $(".btn").on("click", function (e) {
                var applyFilters = $(this).attr("data-apply-filter") === "true";
                var action = $(this).attr("data-action");
                var type = $(this).attr("data-type");

                if (action === "undefined") {
                    action = "";
                }

                if (type === "undefined") {
                    type = "";
                }

                if (confirm(getConfirmMessage(action, type, applyFilters))) {
                    
                    disableAllButtons(this);

                    var command = action;

                    if (type !== "") {
                        command += "-" + type;
                    }

                    if (applyFilters) {
                        command += "-filtered";
                    }

                    var url = window.location.href + getJoiner() + "task=" + command;
                    $.get(url, function() {});

                    getProgress();
                }
            });

            if (<%=AnalyticsIndexBuilder.IsBusy.ToString().ToLower()%>)
            {
                getProgress();
        }

        });

        function getConfirmMessage(action, type, filtered) {
            if (type === "" && filtered === false && action === "delete") {
                return "Are you sure you want to reset the analytics index? This will erase all indexed data.";
            }

            var message = "You are about to " + action + " all ";

            if (filtered) {
                message += "filtered ";
            }

            message += "indexables ";

            if (type !== "") {
                message += "of type '" + type + "' ";
            }

            if (action === "delete") {
                message += "from ";
            } else {
                message += "in ";
            }

            message += "the analytics index. Do you confirm?";

            return message;
        }

        function updateStats() {
            var url = window.location.href + getJoiner() + "task=GetFacets";

            $.getJSON(url, function( data ) {
                var facets = data.Facets;
                for (var i = 0; i < facets.length; i++) {
                    var facet = facets[i];
                    var facetEl = $("body").find("tr[data-facet-type='" + facet.Type + "']");
                    facetEl.find(".facet-name").text(facet.Type);
                    facetEl.find(".facet-count").text(facet.Count);
                }
            });
        }

        function getProgress() {
            var url = window.location.href + getJoiner() + "task=GetLogProgress";
            $.get(url, function (data) {
                var json = JSON.parse(data);
                var log = $("#tbxLog");
                var complete = false;
                var messages = "";
                for (var i = 0; i < json.length; i++) {
                    if (json[i] === "<%=ProcessQueueLoggingProvider.CompletedKeyword%>") {
                        complete = true;
                    } else {
                        messages += json[i] + "\n";
                    }
                }
            
                appendAndScroll(log, messages);

                updateStats();

                if (!complete) {
                    disableAllButtons();
                    setTimeout(function () { getProgress() }, 1000);
                }
                else {
                    enableAllButtons();
                }
            });
        }

        function appendAndScroll(log, messages) {
            var maxLines = 100;
            log.append(messages);

            setTimeout(function() {
                log.animate({
                    scrollTop: log[0].scrollHeight - log.height()
                }, 500);
            }, 100);

            var lines = log.text().split("\n");
            if (lines.length > maxLines) {
                setTimeout(function() {
                    var newLines = [];
                    var start = lines.length - maxLines - 1;
                    for (var i = start; i < lines.length; i++) {
                        newLines.push(lines[i]);
                    }

                    log.text(newLines.join("\n"));
                }, 300);
            }
        }
            
        function disableAllButtons(button) {
            var facetEl = $(button).parent().parent();
            facetEl.find(".action-status").html("<strong>In progress...</strong>");
            $(".btn-active").attr("disabled", "disabled");
        }

        function enableAllButtons() {
            $("body").find(".action-status").html("");
            $(".btn-active").removeAttr("disabled");
        }

        function getJoiner() {
            var joiner = "?";
            if (window.location.href.indexOf("?") > -1) {
                joiner = "&";
            }

            return joiner;
        }
    </script>
</body>
</html>
