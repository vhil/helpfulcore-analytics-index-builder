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
                <div class="col-md-9 text-right">
                    <button type="button" class="btn btn-success btn-xs btn-active btn-rebuild-all-known">Rebuild all filtered indexables</button>
                    <button type="button" class="btn btn-primary btn-xs btn-active btn-rebuild-all">Rebuild all indexables</button>
                </div>
            </div>
        </div>
    </div>
    <div class="container">
        <div class="row">
            <div class="col-md-12">
                <div class="col-md-9">
                    <h3>Analytics index overview</h3>
                    <div class="table-responsive">
                        <table class="table">
                            <thead>
                                <tr>
                                    <th>Indexable type</th>
                                    <th>Count of entries</th>
                                    <th>Actions</th>
                                    <th width="0"></th>
                                    <th></th>
                                    <th width="120"></th>
                                </tr>
                            </thead>
                            <tbody>
                                <%foreach (var facet in this.AnalyticsIndexFacets.Facets)
                                    { %>
                                <tr class="facet" data-facet-type="<%=facet.Type%>">
                                    <td class="facet-name"><%=facet.Type %></td>
                                    <td class="facet-count"><%=facet.Count %></td>
                                    <td>
                                        <% if (facet.ActionsAvailable)
                                        { %>
                                        <button type="button" class="pull-right btn btn-success btn-xs btn-active btn-rebuild-<%=facet.Type%>">Rebuild filtered indexables of this type</button>
                                        <%} %>
                                    </td>
                                    <td></td>
                                     <td>
                                        <% if (facet.ActionsAvailable)
                                        { %>
                                        <button type="button" class="pull-left btn btn-primary btn-xs btn-active btn-rebuild-all-<%=facet.Type%>">Rebuild all indexables of this type</button>
                                        <%} %>
                                    </td>
                                    <td class="action-status text-success"></td>
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
    <br />
    <br />
    <script>
        $(function () {

            $(".btn-rebuild-all").on("click", function (e) {
                disableAllButtons(this);
                rebuildAll();
                getProgress();
            });

            $(".btn-rebuild-all-known").on("click", function (e) {
                disableAllButtons(this);
                rebuildAllKnown();
                getProgress();
            });

            $(".btn-rebuild-contact").on("click", function (e) {
                disableAllButtons(this);
                rebuildContacts(false);
                getProgress();
            });

            $(".btn-rebuild-address").on("click", function (e) {
                disableAllButtons(this);
                rebuildAddresses(false);
                getProgress();
            });

            $(".btn-rebuild-contactTag").on("click", function (e) {
                disableAllButtons(this);
                rebuildContactTags(false);
                getProgress();
            });

            $(".btn-rebuild-visit").on("click", function (e) {
                disableAllButtons(this);
                rebuildVisits(false);
                getProgress();
            });

            $(".btn-rebuild-visitPage").on("click", function (e) {
                disableAllButtons(this);
                rebuildVisitPages(false);
                getProgress();
            });

            $(".btn-rebuild-visitPageEvent").on("click", function (e) {
                disableAllButtons(this);
                rebuildVisitPageEvents(false);
                getProgress();
            });
            
            $(".btn-rebuild-all-contact").on("click", function (e) {
                disableAllButtons(this);
                rebuildContacts(true);
                getProgress();
            });

            $(".btn-rebuild-all-address").on("click", function (e) {
                disableAllButtons(this);
                rebuildAddresses(true);
                getProgress();
            });

            $(".btn-rebuild-all-contactTag").on("click", function (e) {
                disableAllButtons(this);
                rebuildContactTags(true);
                getProgress();
            });

            $(".btn-rebuild-all-visit").on("click", function (e) {
                disableAllButtons(this);
                rebuildVisits(true);
                getProgress();
            });

            $(".btn-rebuild-all-visitPage").on("click", function (e) {
                disableAllButtons(this);
                rebuildVisitPages(true);
                getProgress();
            });

            $(".btn-rebuild-all-visitPageEvent").on("click", function (e) {
                disableAllButtons(this);
                rebuildVisitPageEvents(true);
                getProgress();
            });

            if (<%=AnalyticsIndexBuilder.IsBusy.ToString().ToLower()%>)
            {
                getProgress();
            }

        });

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
            
        function rebuildAllKnown() {
            var url = window.location.href + getJoiner() + "task=RebuildAllKnown";
            $.get(url, function (data) {});
        }

        function rebuildAll() {
            var url = window.location.href + getJoiner() + "task=RebuildAll";
            $.get(url, function (data) {});
        }

        function rebuildContacts(all) {
            var url = window.location.href + getJoiner() + "task=RebuildContacts";
            if (all) {url += "All";}
            $.get(url, function (data) {});
        }

        function rebuildAddresses(all) {
            var url = window.location.href + getJoiner() + "task=RebuildAddresses";
            if (all) {url += "All";}
            $.get(url, function (data) {});
        }

        function rebuildContactTags(all) {
            var url = window.location.href + getJoiner() + "task=RebuildContactTags";
            if (all) {url += "All";}
            $.get(url, function (data) {});
        }
        
        function rebuildVisits(all) {
            var url = window.location.href + getJoiner() + "task=RebuildVisits";
            if (all) {url += "All";}
            $.get(url, function (data) {});
        }

        function rebuildVisitPages(all) {
            var url = window.location.href + getJoiner() + "task=RebuildVisitPages";
            if (all) {url += "All";}
            $.get(url, function (data) {});
        }

        function rebuildVisitPageEvents(all) {
            var url = window.location.href + getJoiner() + "task=RebuildVisitPageEvents";
            if (all) {url += "All";}
            $.get(url, function (data) {});
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
