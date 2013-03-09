using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace GridViewEx
{
    public class GridViewEx : GridView
    {
        internal string JSScript { get; set; }
        internal string JSScriptBeginRequestHandler { get; set; }
        internal string JSScriptEndRequestHandler { get; set; }
        internal string JSScriptDocumentReady { get; set; }

        public string Title { get; set; }
        public string LoadingImageUrl { get; set; }
        public bool IsCompact { get; set; }
        public bool TableStriped { get; set; }
        public bool TableHover { get; set; }
        public string PagerSelectorOptions { get; set; }
        //public bool CustomOrder { get; set; }

        public event EventHandler SortingChanged;
        public event EventHandler FilterDeleted;
        //public event EventHandler FilterApplied;
        public event EventHandler PageChanged;
        public event EventHandler ColumnSelectionChanged;
        public event EventHandler ExcelExport;

        // Init with our default options
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var css = CssClass.Split(' ').ToList();
            css.Add("table");
            if (TableStriped)
                css.Add("table-striped");
            if (TableHover)
                css.Add("table-hover");

            CssClass = String.Join(" ", css.Distinct().ToArray());
            GridLines = GridLines.None;
            AllowSorting = true;
            AutoGenerateColumns = false;
            ShowHeaderWhenEmpty = true;
            if (String.IsNullOrWhiteSpace(EmptyDataText))
                EmptyDataText = "No data to display";
            if (String.IsNullOrWhiteSpace(PagerSelectorOptions))
                PagerSelectorOptions = "10,50,100";
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InitControls();
        }

        public void InitControls()
        {
            // Add export control
            this.Controls.Add(CreateExportControl());

            // Add management sorting control
            this.Controls.Add(CreateSortingManagementControl());

            // Add management filters control
            this.Controls.Add(CreateFilterManagementControl());

            // Add management columns control
            this.Controls.Add(CreateColumnManagementControl());

            // Add pager control
            this.Controls.Add(CreatePagerControl());
        }

        protected override void Render(HtmlTextWriter writer)
        {
            var hlFilter = new HyperLink
            {
                NavigateUrl = "#",
                CssClass = "btn pull-right",
                ToolTip = "Filter",
                Text = "<i class=\"icon-search\"></i>"
            };
            var hlCompactTable = new HyperLink
            {
                ID = "hl" + this.ClientID + "CompactTable",
                ClientIDMode = ClientIDMode.Static,
                NavigateUrl = "#",
                CssClass = "btn pull-right",
                ToolTip = "Compact Table",
                Text = "<i class=\"icon-resize-small\"></i>"
            };
            var hlExpandTable = new HyperLink
            {
                ID = "hl" + this.ClientID + "ExpandTable",
                ClientIDMode = ClientIDMode.Static,
                NavigateUrl = "#",
                CssClass = "btn hide pull-right",
                ToolTip = "Expand Table",
                Text = "<i class=\"icon-resize-full\"></i>"
            };

            hlFilter.Attributes.Add("onclick", "$('." + this.ClientID + "Filters').toggle();$('#" + this.ClientID + "Scrollbar').children('div').width($('#" + this.ClientID + @"GridViewTable').children('div').children('table')[0].scrollWidth);");

            hlCompactTable.Attributes.Add("onclick", this.ClientID + "CompactTable(true);");
            hlCompactTable.Attributes.Add("style", "margin-right: 10px;");

            hlExpandTable.Attributes.Add("onclick", this.ClientID + "CompactTable(false);");
            hlExpandTable.Attributes.Add("style", "margin-right: 10px;");

            writer.Write("<div id=\"" + this.ClientID + "grid-view\" class=\"grid-view\">");
            writer.Write("<div class=\"overlay\">");

            // Check if the loading image is null, if so not add it
            if (!String.IsNullOrWhiteSpace(LoadingImageUrl))
            {
                var imgLoader = new Image { ImageUrl = LoadingImageUrl };
                imgLoader.RenderControl(writer);
            }

            writer.Write("</div>"); // End of .overlay

            writer.Write("<fieldset><legend>" + Title);
            hlFilter.RenderControl(writer);
            hlCompactTable.RenderControl(writer);
            hlExpandTable.RenderControl(writer);

            if (base.Controls.Count == 6)
            {
                // Render export control
                base.Controls[1].RenderControl(writer);
                base.Controls[1].Visible = false;

                // Render management sorting control
                base.Controls[2].RenderControl(writer);
                base.Controls[2].Visible = false;

                // Render management filters control
                base.Controls[3].RenderControl(writer);
                base.Controls[3].Visible = false;

                // Render management columns control
                base.Controls[4].RenderControl(writer);
                base.Controls[4].Visible = false;

                base.Controls[5].Visible = false; // Hide because it should go after the table

                writer.Write("</legend></fieldset><div id=\"" + this.ClientID + "GridViewTable\" class=\"grid-view-table\">");
                base.Render(writer);
                writer.Write("</div>"); // End of .grid-view-table

                // Render pager control
                base.Controls[5].Visible = true;
                base.Controls[5].RenderControl(writer);
                base.Controls[5].Visible = false;
            }
            else
            {
                writer.Write("</legend></fieldset><div class=\"grid-view-table\">");
                base.Controls.Clear();
                base.Render(writer);
                writer.Write("</div>"); // End of .grid-view-table
            }

            writer.Write("</div>"); // End of .grid-view
            writer.Write(@"<script type='text/javascript'>
                var " + this.ClientID + @"SizeCompact = " + IsCompact.ToString().ToLower() + @";

                Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(" + this.ClientID + @"BeginRequestHandler);
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(" + this.ClientID + @"EndRequestHandler);
                
                function " + this.ClientID + @"CompactTable(isCompact) {
                    " + this.ClientID + @"SizeCompact = isCompact;
                    if (isCompact) {
                        $('#" + this.ClientID + @"').addClass('table-condensed');
                        $('#" + hlExpandTable.ClientID + @"').show();
                        $('#" + hlCompactTable.ClientID + @"').hide();
                    }
                    else {
                        $('#" + this.ClientID + @"').removeClass('table-condensed');
                        $('#" + hlExpandTable.ClientID + @"').hide();
                        $('#" + hlCompactTable.ClientID + @"').show();
                    }
                    $('#" + this.ClientID + "Scrollbar').children('div').width($('#" + this.ClientID + @"GridViewTable').children('div').children('table')[0].scrollWidth);
                }

                function " + this.ClientID + @"SaveSearchExp(hfID, focusID, value) {
                    $('#' + hfID).val(value);
                    $('#' + focusID).focus();
                }
                " + JSScript + @"

                function " + this.ClientID + @"BeginRequestHandler(sender, args) {
                    $('#" + this.ClientID + @"grid-view .overlay')
                        .width($('#" + this.ClientID + @"grid-view').width())
                        .height($('#" + this.ClientID + @"grid-view').height())
                        .show();
                    " + JSScriptBeginRequestHandler + @"
                }

                function " + this.ClientID + @"EndRequestHandler(sender, args) {
                    $('#" + this.ClientID + @"grid-view .overlay').hide();
                    if (" + this.ClientID + @"SizeCompact)
                        " + this.ClientID + @"CompactTable(true);
                    " + this.ClientID + @"CreateTopScrollbar()
                    " + JSScriptEndRequestHandler + @"
                }

                function " + this.ClientID + @"CreateTopScrollbar() {
                    var element = $('#" + this.ClientID + @"GridViewTable');
                    var scrollbar = $('<div></div>')
                        .attr('id','" + this.ClientID + @"Scrollbar')
                        
                        .css('overflow-x', 'auto')
                        .css('overflow-y', 'hidden')
                        .append($('<div></div>')
                            .width(element.children('div').children('table')[0].scrollWidth)
                            .css('padding-top', '1px')
                            .append($('\xA0')));
                    scrollbar.scroll(function() {
                        element.scrollLeft(scrollbar.scrollLeft());
                    });
                    element.scroll(function() {
                        scrollbar.scrollLeft(element.scrollLeft());
                    });
                    element.before(scrollbar);
                }

                $(document).ready(function () {
                    if (" + this.ClientID + @"SizeCompact)
                        " + this.ClientID + @"CompactTable(true);

                    " + this.ClientID + @"CreateTopScrollbar();
                    " + JSScriptDocumentReady + @"
                });
            </script>");
        }

        // Show an icon next to the Header title when there's a sorting applied for that column
        protected override void OnRowCreated(GridViewRowEventArgs e)
        {
            base.OnRowCreated(e);

            // Check if we have filters to apply
            var sortExpressions = ((List<SortExpression>)Context.Session[this.ID + "_SortExpressions"]);
            if (sortExpressions != null)
            {
                // Use the RowType property to determine whether the row being created is the header row. 
                if (e.Row.RowType == DataControlRowType.Header)
                {
                    // Loop against the header cells
                    foreach (TableCell cell in e.Row.Cells)
                    {
                        // If the cell doesn't have controls means the sort is not enabled for that cell
                        if (!cell.HasControls())
                            continue;

                        // Loop against the controls to find our sort LinkButton
                        foreach (var control in cell.Controls)
                        {
                            // The button must have the Sort CommandName
                            var button = control as LinkButton;
                            if (button == null || button.CommandName != "Sort")
                                continue;

                            // Add by Default ToolTip if none defined
                            if (string.IsNullOrWhiteSpace(button.ToolTip))
                                button.ToolTip = button.Text;

                            // Add rel="tooltip" for Bootstrap nicer tooltips
                            button.Attributes.Add("rel", "tooltip");

                            // Check if the header have a sortExpression
                            var sortExpression = sortExpressions.SingleOrDefault(x => x.Column == button.CommandArgument);
                            if (sortExpression != null)
                            {
                                // Create the sorting image based on the sort direction.
                                var sortImage = new Literal();
                                if (sortExpression.Direction == SortDirection.Ascending.ToSQLString())
                                {
                                    button.Text += " <i class=\"icon-arrow-up\"></i>";
                                    button.ToolTip += " - ASC Order";
                                }
                                else
                                {
                                    button.Text += " <i class=\"icon-arrow-down\"></i>";
                                    button.ToolTip += " - DESC Order";
                                }

                                // Add the image to the appropriate header cell.
                                cell.Controls.Add(sortImage);
                                break;
                            }
                        }
                    }
                }
            }
        }

        protected override void OnDataBinding(EventArgs e)
        {
            //DataSource = GridViewExDataSource((IQueryable<object>)DataSource);
            base.OnDataBinding(e);
        }

        // Show the header as TH instead of TR
        protected override void OnDataBound(EventArgs e)
        {
            base.OnDataBound(e);
            if (base.HeaderRow != null)
                base.HeaderRow.TableSection = TableRowSection.TableHeader;
        }

        protected override void OnSorting(GridViewSortEventArgs e)
        {
            // Get the defined sort expressions
            var sortExpressions = Context.Session[this.ID + "_SortExpressions"] as List<SortExpression>;
            if (sortExpressions != null)
            {
                // Check if we have defined this already. If have it assigned, change the direction else add the new one
                var sortExpression = sortExpressions.SingleOrDefault(x => x.Column == e.SortExpression);
                if (sortExpression != null)
                {
                    // If have it assigned, change the direction
                    if (sortExpression.PreviousDirection == null)
                    {
                        sortExpression.PreviousDirection = sortExpression.Direction;
                        sortExpression.Direction = (sortExpression.Direction == SortDirection.Ascending.ToSQLString())
                            ? SortDirection.Descending.ToSQLString()
                            : SortDirection.Ascending.ToSQLString();
                    }
                    else
                        sortExpressions.Remove(sortExpression);
                }
                else
                    sortExpressions.Add(new SortExpression
                    {
                        Column = e.SortExpression,
                        Direction = e.SortDirection.ToSQLString()
                    });
            }
            else // If not defined create a new one
            {
                sortExpressions = new List<SortExpression>();
                sortExpressions.Add(new SortExpression
                {
                    Column = e.SortExpression,
                    Direction = e.SortDirection.ToSQLString()
                });
            }

            // Update the result into the Session
            Context.Session[this.ID + "_SortExpressions"] = sortExpressions;
            base.OnSorting(e);

            InitControls();
        }

        public List<dynamic> GridViewExDataSource<T>(IQueryable<T> query, bool customOrder = false, bool isExport = false)
        {
            if (!isExport)
            {
                // Column management
                if (Context.Session[this.ID + "_Columns"] == null)
                {
                    var columns = new List<ColumnExpression>();
                    var index = 0;
                    foreach (var column in this.Columns)
                    {
                        var gridViewExColumn = column as ColumnEx;
                        var boundColumn = column as BoundColumn;

                        if (gridViewExColumn != null)
                            columns.Add(new ColumnExpression
                            {
                                ColumnName = gridViewExColumn.DataField,
                                DisplayName = gridViewExColumn.HeaderText,
                                Visible = gridViewExColumn.Visible,
                                Type = "ColumnEx",
                                Index = index,
                                DataFormat = gridViewExColumn.DataFormat,
                                DataFormatExpression = gridViewExColumn.DataFormatExpression
                            });
                        else if (boundColumn != null)
                            columns.Add(new ColumnExpression
                            {
                                ColumnName = boundColumn.DataField,
                                DisplayName = boundColumn.HeaderText,
                                Visible = boundColumn.Visible,
                                Type = "BoundField",
                                Index = index,
                                DataFormat = DataFormatEnum.Text,
                                DataFormatExpression = String.Empty
                            });

                        index++;
                    }

                    Context.Session[this.ID + "_Columns"] = columns;
                }
                else
                {
                    foreach (var sessionColumn in (List<ColumnExpression>)Context.Session[this.ID + "_Columns"])
                    {
                        var index = 0;
                        foreach (var column in this.Columns)
                        {
                            var flag = false;
                            switch (sessionColumn.Type)
                            {
                                case "ColumnEx":
                                    var gridViewExColumn = column as ColumnEx;
                                    if (gridViewExColumn != null
                                        && gridViewExColumn.DataField == sessionColumn.ColumnName)
                                    {
                                        gridViewExColumn.Visible = sessionColumn.Visible;

                                        if (sessionColumn.Index != index)
                                        {
                                            var col = this.Columns[index];
                                            this.Columns.Remove(col);
                                            this.Columns.Insert(sessionColumn.Index, col);
                                            flag = true;
                                        }
                                    }
                                    break;
                                case "BoundField":
                                    var boundColumn = column as BoundColumn;
                                    if (boundColumn != null
                                        && boundColumn.DataField == sessionColumn.ColumnName)
                                    {
                                        boundColumn.Visible = sessionColumn.Visible;

                                        if (sessionColumn.Index != index)
                                        {
                                            var col = this.Columns[index];
                                            this.Columns.Remove(col);
                                            this.Columns.Insert(sessionColumn.Index, col);
                                            flag = true;
                                        }
                                    }
                                    break;
                            }

                            index++;
                            if (flag)
                                break;
                        }
                    }
                }

                int pageSize = Context.Session[this.ID + "_PageSize"] != null
                    ? Convert.ToInt32(Context.Session[this.ID + "_PageSize"])
                    : PageSize;
                int pageIndex = Context.Session[this.ID + "_PageIndex"] != null
                    ? Convert.ToInt32(Context.Session[this.ID + "_PageIndex"])
                    : PageIndex;

                Context.Session[this.ID + "_PageIndex"] = pageIndex;
                Context.Session[this.ID + "_PageSize"] = pageSize;

                int startRow = pageIndex * pageSize;

                // Save filters data for later access
                foreach (var column in this.Columns)
                {
                    var gridViewExColumn = column as ColumnEx;
                    if (gridViewExColumn != null
                        && gridViewExColumn.SearchType == SearchTypeEnum.DropDownList
                        && gridViewExColumn.DropDownDataSource == null)
                    {
                        gridViewExColumn.DropDownDataSource = query.GetDropDownDataSource(gridViewExColumn.DataField, gridViewExColumn.DataFormat, gridViewExColumn.DataFormatExpression);
                    }
                }

                query = query.Filter((List<FilterExpression>)Context.Session[this.ID + "_Filters"]); // Apply filters

                // Count total records
                Context.Session[this.ID + "_Records"] = query.Count();

                if (!customOrder)
                {
                    // Sort the rows
                    var sourceIQueryable = (IQueryable<dynamic>)query.Order((List<SortExpression>)Context.Session[this.ID + "_SortExpressions"]);

                    // Page the query if necessary
                    sourceIQueryable = sourceIQueryable.Skip(startRow).Take(pageSize);

                    // Finally return list
                    return sourceIQueryable.ToList<dynamic>();
                }
                else
                {
                    // Page the query if necessary
                    query = query.Skip(startRow).Take(pageSize);

                    // Finally return list
                    return ((IQueryable<dynamic>)query).ToList<dynamic>();
                }
            }
            else
            {
                query = query.Filter((List<FilterExpression>)Context.Session[this.ID + "_Filters"]); // Apply filters
                return (!customOrder)
                    ? ((IQueryable<dynamic>)query.Order((List<SortExpression>)Context.Session[this.ID + "_SortExpressions"])).ToList<dynamic>()
                    : ((IQueryable<dynamic>)query).ToList<dynamic>();
            }
        }

        #region CONTROL CREATION
        private Control CreateExportControl()
        {
            var phControl = new PlaceHolder();

            var lbExport = new LinkButton
            {
                ID = "hl" + this.ClientID + "Export",
                ClientIDMode = ClientIDMode.Static,
                CssClass = "btn pull-right",
                ToolTip = "Export to Excel",
                Text = "<i class=\"icon-download\"></i>"
            };
            lbExport.Click += lbExport_Click;
            lbExport.Attributes.Add("style", "margin-right: 10px;");

            phControl.Controls.Add(lbExport);

            // In case user use UpdatePanel register a normal postback if not it fails
            ScriptManager.GetCurrent(Page).RegisterPostBackControl(lbExport);
            return phControl;
        }

        private Control CreateFilterManagementControl()
        {
            var filters = (List<FilterExpression>)Context.Session[this.ID + "_Filters"] != null
                ? (List<FilterExpression>)Context.Session[this.ID + "_Filters"]
                : new List<FilterExpression>();

            var phControl = new PlaceHolder();

            var hlFilterSelection = new HtmlAnchor
            {
                ID = "hl" + this.ID + "FilterSelection",
                ClientIDMode = ClientIDMode.Static,
                HRef = "#",
                Title = "Filter Management",
                InnerHtml = "<i class=\"icon-filter\"></i>"
            };
            hlFilterSelection.Attributes.Add("class", "btn pull-right");
            hlFilterSelection.Attributes.Add("style", "margin-right: 10px;");
            if (filters.Count >= 1)
                hlFilterSelection.Attributes["style"] += "background-color: #B2C6E0;";

            phControl.Controls.Add(hlFilterSelection);

            var divFilterSelection = new HtmlGenericControl("div");
            divFilterSelection.ID = "div" + this.ID + "FilterSelection";
            divFilterSelection.ClientIDMode = ClientIDMode.Static;
            divFilterSelection.Attributes.Add("style", "display: none;");

            var lbRemoveAll = new LinkButton
            {
                Text = "Remove All",
                ToolTip = "Remove all sortings",
                Visible = filters.Count >= 1
            };
            lbRemoveAll.Click += lbFilterRemoveAll_Click;
            divFilterSelection.Controls.Add(lbRemoveAll);

            var ul = new HtmlGenericControl("ul");

            if (filters.Count >= 1)
            {
                foreach (var filterExpression in filters)
                {
                    var r = new Regex(@"
                    (?<=[A-Z])(?=[A-Z][a-z]) |
                    (?<=[^A-Z])(?=[A-Z]) |
                    (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

                    var liFilter = new HtmlGenericControl("li");

                    var lbRemove = new LinkButton
                    {
                        Text = "<i class=\"icon-remove\"></i>",
                        ToolTip = "Remove",
                        CommandArgument = filterExpression.Column + "|" + filterExpression.ExpressionShortName + "|" + filterExpression.Text
                    };
                    lbRemove.Click += lbFilterRemove_Click;
                    liFilter.Controls.Add(lbRemove);

                    liFilter.Controls.Add(new Literal
                    {
                        Text = String.Format("{0} {1} '{2}'",
                            filterExpression.DisplayName,
                            r.Replace(filterExpression.Expression, " ").ToLower(),
                            filterExpression.Text)
                    });

                    ul.Controls.Add(liFilter);
                }

                divFilterSelection.Controls.Add(ul);
            }
            else
                divFilterSelection.Controls.Add(new Label { Text = "No filters" });

            phControl.Controls.Add(divFilterSelection);

            JSScript += @"
                function " + this.ClientID + @"FilterPopover() {
                    $('#" + hlFilterSelection.ClientID + @"').popover({
                        html: true,
                        trigger: 'manual',
                        placement: 'bottom',
                        content: function () {
                            return $('#div" + this.ID + @"FilterSelection').html();
                        }
                    });

                    var timer, popover_parent;

                    $('#" + hlFilterSelection.ClientID + @"').hover(function () {
                        clearTimeout(timer);
                        $('.popover').hide(); //Hide any open popovers on other elements.
                        popover_parent = this
                        $(this).popover('show');
                    },
                        function () {
                            timer = setTimeout(function () { $(this).popover('hide'); }, 300);
                        });

                    $('.popover').live({
                        mouseover: function () {
                            clearTimeout(timer);
                        },
                        mouseleave: function () {
                            timer = setTimeout(function () { $(popover_parent).popover('hide'); }, 300);
                        }
                    });
                }";

            JSScriptEndRequestHandler += this.ClientID + @"FilterPopover();";
            JSScriptDocumentReady += this.ClientID + @"FilterPopover();";

            return phControl;
        }

        private Control CreateSortingManagementControl()
        {
            var sortings = (List<SortExpression>)Context.Session[this.ID + "_SortExpressions"] != null
                ? (List<SortExpression>)Context.Session[this.ID + "_SortExpressions"]
                : new List<SortExpression>();

            var phControl = new PlaceHolder();

            var hlSortSelection = new HtmlAnchor
            {
                ID = "hl" + this.ID + "SortSelection",
                ClientIDMode = ClientIDMode.Static,
                HRef = "#",
                Title = "Sorting Management",
                InnerHtml = "<i class=\"icon-list-alt\"></i>",
            };
            hlSortSelection.Attributes.Add("class", "btn pull-right");
            hlSortSelection.Attributes.Add("style", "margin-right: 10px;");
            if (sortings.Count >= 1)
                hlSortSelection.Attributes["style"] += "background-color: #B2C6E0;";

            phControl.Controls.Add(hlSortSelection);

            var divSortSelection = new HtmlGenericControl("div");
            divSortSelection.ID = "div" + this.ID + "SortSelection";
            divSortSelection.ClientIDMode = ClientIDMode.Static;
            divSortSelection.Attributes.Add("style", "display: none;");

            var lbRemoveAll = new LinkButton
            {
                Text = "Remove All",
                ToolTip = "Remove all sortings",
                Visible = sortings.Count >= 1
            };
            lbRemoveAll.Click += lbSortingRemoveAll_Click;
            divSortSelection.Controls.Add(lbRemoveAll);

            var ol = new HtmlGenericControl("ol");

            if (sortings.Count >= 1)
            {
                foreach (var sortExpression in sortings)
                {
                    var liSort = new HtmlGenericControl("li");

                    var lbRemove = new LinkButton
                    {
                        Text = "<i class=\"icon-remove\"></i>",
                        ToolTip = "Remove",
                        CommandArgument = sortExpression.Column
                    };
                    lbRemove.Click += lbSortingRemove_Click;
                    liSort.Controls.Add(lbRemove);

                    var lbChangeIndexUp = new LinkButton
                    {
                        Text = "<i class=\"icon-arrow-up\"></i>",
                        ToolTip = "Up",
                        CommandArgument = sortExpression.Column,
                        Visible = sortings.Count > 1
                            && sortings.IndexOf(sortExpression) != 0
                    };
                    lbChangeIndexUp.Click += lbSortingChangeIndexUp_Click;
                    liSort.Controls.Add(lbChangeIndexUp);

                    var lbChangeIndexDown = new LinkButton
                    {
                        Text = "<i class=\"icon-arrow-down\"></i>",
                        ToolTip = "Down",
                        CommandArgument = sortExpression.Column,
                        Visible = sortings.Count > 1
                            && sortings.IndexOf(sortExpression) != sortings.Count - 1
                    };
                    lbChangeIndexDown.Click += lbSortingChangeIndexDown_Click;
                    liSort.Controls.Add(lbChangeIndexDown);

                    liSort.Controls.Add(new Literal { Text = String.Format("{0} - {1}", sortExpression.Column, sortExpression.Direction) });

                    ol.Controls.Add(liSort);
                }

                divSortSelection.Controls.Add(ol);
            }
            else
                divSortSelection.Controls.Add(new Label { Text = "No sortings" });

            phControl.Controls.Add(divSortSelection);

            JSScript += @"
                function " + this.ClientID + @"SortPopover() {
                    $('#" + hlSortSelection.ClientID + @"').popover({
                        html: true,
                        trigger: 'manual',
                        placement: 'bottom',
                        content: function () {
                            return $('#div" + this.ID + @"SortSelection').html();
                        }
                    });

                    var timer, popover_parent;

                    $('#" + hlSortSelection.ClientID + @"').hover(function () {
                        clearTimeout(timer);
                        $('.popover').hide(); //Hide any open popovers on other elements.
                        popover_parent = this
                        $(this).popover('show');
                    },
                        function () {
                            timer = setTimeout(function () { $(this).popover('hide'); }, 300);
                        });

                    $('.popover').live({
                        mouseover: function () {
                            clearTimeout(timer);
                        },
                        mouseleave: function () {
                            timer = setTimeout(function () { $(popover_parent).popover('hide'); }, 300);
                        }
                    });
                }";

            JSScriptEndRequestHandler += this.ClientID + @"SortPopover();";
            JSScriptDocumentReady += this.ClientID + @"SortPopover();";

            return phControl;
        }

        private Control CreateColumnManagementControl()
        {
            var columns = (List<ColumnExpression>)Context.Session[this.ID + "_Columns"] != null
                ? (List<ColumnExpression>)Context.Session[this.ID + "_Columns"]
                : new List<ColumnExpression>();

            var phControl = new PlaceHolder();

            var hlColumnSelection = new HtmlAnchor
            {
                ID = "hl" + this.ID + "ColumnSelection",
                ClientIDMode = ClientIDMode.Static,
                HRef = "#",
                Title = "Column Management",
                InnerHtml = "<i class=\"icon-calendar\"></i>"
            };
            hlColumnSelection.Attributes.Add("class", "btn pull-right");
            hlColumnSelection.Attributes.Add("style", "margin-right: 10px;");
            phControl.Controls.Add(hlColumnSelection);

            var hfColumnsSelected = new HiddenField
            {
                ID = "hf" + this.ID + "ColumnsSelected",
                ClientIDMode = ClientIDMode.Static,
                Value = JsonConvert.SerializeObject(columns)
            };
            phControl.Controls.Add(hfColumnsSelected);

            var divColumnSelection = new HtmlGenericControl("div");
            divColumnSelection.ID = "div" + this.ID + "ColumnSelection";
            divColumnSelection.ClientIDMode = ClientIDMode.Static;
            divColumnSelection.Attributes.Add("style", "display: none;");

            var hlHideAll = new HtmlAnchor
            {
                ID = "hl" + this.ID + "HideAll",
                ClientIDMode = ClientIDMode.Static,
                HRef = "#",
                InnerText = "Hide All",
                Title = "Hide all columns",
                Visible = columns.Count >= 1
            };
            hlHideAll.Attributes.Add("onclick", "ColumnSelectionShowAll(this, false);");

            var hlShowAll = new HtmlAnchor
            {
                ID = "hl" + this.ID + "ShowAll",
                ClientIDMode = ClientIDMode.Static,
                HRef = "#",
                InnerText = "Show All",
                Title = "Show all columns",
                Visible = columns.Count >= 1
            };
            hlShowAll.Attributes.Add("onclick", "ColumnSelectionShowAll(this, true);");

            var isAllVisible = true;
            foreach (var column in columns)
                if (!column.Visible)
                {
                    isAllVisible = false;
                    break;
                }

            if (isAllVisible)
                hlShowAll.Attributes.Add("style", "display: none;");
            else
                hlHideAll.Attributes.Add("style", "display: none;");

            divColumnSelection.Controls.Add(hlHideAll);
            divColumnSelection.Controls.Add(hlShowAll);

            var lbApply = new LinkButton
            {
                ID = "lb" + this.ID + "Apply",
                ClientIDMode = ClientIDMode.Static,
                Text = "Apply",
                ToolTip = "Apply changes",
                Visible = columns.Count >= 1
            };
            lbApply.Attributes.Add("style", "display: none;float: right;");
            lbApply.Click += lbColumnApply_Click;
            divColumnSelection.Controls.Add(lbApply);

            var ol = new HtmlGenericControl("ol");

            if (columns.Count >= 1)
            {
                foreach (var column in columns)
                {
                    var liColumn = new HtmlGenericControl("li");

                    var cbVisible = new HtmlInputCheckBox
                    {
                        Checked = column.Visible
                    };
                    cbVisible.Attributes.Add("title", "Check to make it visible");
                    cbVisible.Attributes.Add("onclick", "ColumnSelectionChanged(this);");
                    cbVisible.Attributes.Add("data-field", column.ColumnName);
                    cbVisible.Attributes.Add("data-index", column.Index.ToString());
                    liColumn.Controls.Add(cbVisible);

                    var lbChangeIndexUp = new HtmlAnchor
                    {
                        InnerHtml = "<i class=\"icon-arrow-up\"></i>",
                        Title = "Up"
                    };

                    if (columns.Count <= 1
                        || columns.IndexOf(column) == 0)
                        lbChangeIndexUp.Attributes.Add("style", "visibility: hidden;");

                    lbChangeIndexUp.Attributes.Add("data-action", "up");
                    lbChangeIndexUp.Attributes.Add("onclick", "ColumnIndexChanged(this, parseInt($(this).parent().children().closest('input:checkbox').attr('data-index')) - 1);");
                    liColumn.Controls.Add(lbChangeIndexUp);

                    var lbChangeIndexDown = new HtmlAnchor
                    {
                        InnerHtml = "<i class=\"icon-arrow-down\"></i>",
                        Title = "Down"
                    };
                    if (columns.Count <= 1
                        || columns.IndexOf(column) == columns.Count - 1)
                        lbChangeIndexDown.Attributes.Add("style", "visibility: hidden;");

                    lbChangeIndexDown.Attributes.Add("data-action", "down");
                    lbChangeIndexDown.Attributes.Add("onclick", "ColumnIndexChanged(this, parseInt($(this).parent().children().closest('input:checkbox').attr('data-index')) + 1);");
                    liColumn.Controls.Add(lbChangeIndexDown);

                    liColumn.Controls.Add(new Literal { Text = column.DisplayName });

                    ol.Controls.Add(liColumn);
                }

                divColumnSelection.Controls.Add(ol);
            }
            else
                divColumnSelection.Controls.Add(new Label { Text = "No columns" });

            phControl.Controls.Add(divColumnSelection);

            JSScript += @"
                function " + this.ClientID + @"ColumnPopover() {
                    $('#" + hlColumnSelection.ClientID + @"').popover({
                        html: true,
                        trigger: 'manual',
                        placement: 'bottom',
                        content: function () {
                            return $('#div" + this.ID + @"ColumnSelection').html();
                        }
                    });

                    var timer, popover_parent;

                    $('#" + hlColumnSelection.ClientID + @"').hover(function () {
                        clearTimeout(timer);
                        $('.popover').hide(); //Hide any open popovers on other elements.
                        popover_parent = this
                        $(this).popover('show');
                    },
                        function () {
                            timer = setTimeout(function () { $(this).popover('hide'); }, 300);
                        });

                    $('.popover').live({
                        mouseover: function () {
                            clearTimeout(timer);
                        },
                        mouseleave: function () {
                            timer = setTimeout(function () { $(popover_parent).popover('hide'); }, 300);
                        }
                    });
                }

                function ColumnSelectionShowAll(link, showAll) {
                    var arr = JSON.parse($('#hf" + this.ID + @"ColumnsSelected').val());
                    
                    $(link).parent().children('ol').children().each(function () {
                        $(this).children('input:checkbox').prop('checked', showAll);
                    });

                    $.map(arr, function (elementOfArray, indexInArray) {
                        elementOfArray.Visible = showAll;
                    });

                    $('#hf" + this.ID + @"ColumnsSelected').val(JSON.stringify(arr));
                    document.cookie = '" + this.ID + @"_ColumnsSelected' + '=' + escape(JSON.stringify(arr)) + '; ';
                    $('#" + hlHideAll.ClientID + @", #" + hlShowAll.ClientID + @"').toggle();
                    $('#" + lbApply.ClientID + @"').show();
                }

                function ColumnSelectionChanged(cb) {
                    var arr = JSON.parse($('#hf" + this.ID + @"ColumnsSelected').val());

                    $.map(arr, function (elementOfArray, indexInArray) {
                        if (elementOfArray.ColumnName == $(cb).attr('data-field')) {
                            elementOfArray.Visible = $(cb).is(':checked');
                        }
                    });

                    $('#hf" + this.ID + @"ColumnsSelected').val(JSON.stringify(arr));
                    document.cookie = '" + this.ID + @"_ColumnsSelected' + '=' + escape(JSON.stringify(arr)) + '; ';
                    $('#" + lbApply.ClientID + @"').show();
                }

                function ColumnIndexChanged(link, index) {
                    var arr = JSON.parse($('#hf" + this.ID + @"ColumnsSelected').val());
                    var li = $(link).parent();
                    var liPrev = li.prev();
                    var liNext = li.next();
                    var liChildren = li.children();
                    var oldIndex = parseInt(liChildren.closest('input:checkbox').attr('data-index'));

                    arr.move(oldIndex, index);
                    $.map(arr, function (elementOfArray, indexInArray) {
                        elementOfArray.Index = indexInArray;
                    });
                    
                    // Move the item on the list & handle the sort actions depending on the position
                    if (index == 0) { // Second moved up to first place
                        li.insertBefore(liPrev);
                        liPrev.children().closest('input:checkbox').attr('data-index', index + 1);
                        
                        liChildren.closest('[data-action=""up""]').css('visibility', 'hidden');
                        liPrev.children().closest('[data-action=""up""], [data-action=""down""]').css('visibility', 'visible');
                    }
                    else if (index == 1
                        && index > oldIndex) { // First moved down to second place
                        li.insertAfter(liNext);
                        liNext.children().closest('input:checkbox').attr('data-index', index - 1);
                        
                        liNext.children().closest('[data-action=""up""]').css('visibility', 'hidden');
                        liChildren.closest('[data-action=""up""], [data-action=""down""]').css('visibility', 'visible');
                    }
                    else if (index == li.parent().children().length - 2
                        && oldIndex > index) { // Last moved up to penultimate place
                        li.insertBefore(liPrev);
                        liPrev.children().closest('input:checkbox').attr('data-index', index + 1);
                        
                        liPrev.children().closest('[data-action=""down""]').css('visibility', 'hidden');
                        liChildren.closest('[data-action=""up""], [data-action=""down""]').css('visibility', 'visible');
                    }
                    else if (index == li.parent().children().length - 1) { // Penultimate moved down to last place
                        li.insertAfter(liNext);
                        liNext.children().closest('input:checkbox').attr('data-index', index - 1);
                        
                        liChildren.closest('[data-action=""down""]').css('visibility', 'hidden');
                        liNext.children().closest('[data-action=""up""], [data-action=""down""]').css('visibility', 'visible');
                    }
                    else {
                        if (oldIndex < index) { // Moved Down
                            li.insertAfter(liNext);
                            liNext.children().closest('input:checkbox').attr('data-index', index - 1);
                        }
                        else { // Moved Up
                            li.insertBefore(liPrev);
                            liPrev.children().closest('input:checkbox').attr('data-index', index + 1);
                        }
                    }
                    liChildren.closest('input:checkbox').attr('data-index', index);

                    $('#hf" + this.ID + @"ColumnsSelected').val(JSON.stringify(arr));
                    document.cookie = '" + this.ID + @"_ColumnsSelected' + '=' + escape(JSON.stringify(arr)) + '; ';
                    $('#" + lbApply.ClientID + @"').show();
                }";

            JSScriptEndRequestHandler += this.ClientID + @"ColumnPopover();";
            JSScriptDocumentReady += this.ClientID + @"ColumnPopover();";

            return phControl;
        }

        private Control CreatePagerControl()
        {
            int pageIndex = (int)Context.Session[this.ID + "_PageIndex"];
            int pageSize = (int)Context.Session[this.ID + "_PageSize"];
            int records = (int)Context.Session[this.ID + "_Records"];

            var divControl = new HtmlGenericControl("div");
            divControl.Attributes.Add("class", "pagination pagination-centered");

            var ul = new HtmlGenericControl("ul");

            if (records > pageSize)
            {
                var pages = Extensions.FillPager(records, pageIndex, pageSize);
                foreach (var page in pages)
                {
                    var liPage = new HtmlGenericControl("li");

                    var lbPage = new LinkButton
                    {
                        Text = page.Text,
                        CommandArgument = page.Value,
                        Enabled = page.Enabled
                    };

                    if (!page.Enabled)
                        liPage.Attributes["class"] = (!page.Text.Equals("FIRST") && !page.Text.Equals("LAST"))
                            ? "active"
                            : "disabled";

                    lbPage.Click += lbPage_Click;
                    liPage.Controls.Add(lbPage);

                    ul.Controls.Add(liPage);
                }

                divControl.Controls.Add(ul);
            }

            var divRecordsCount = new HtmlGenericControl("div");
            divRecordsCount.Attributes.Add("class", "records-count");

            // Calculate total of pages
            double pagesCount = (double)records / (double)pageSize;
            if (pagesCount < 1)
                pagesCount = 1;
            else if (pagesCount > (int)pagesCount)
                pagesCount++;

            var litPagerRecords = new Literal
            {
                Text = String.Format("Page {0} of {1} ({2} records)",
                    pageIndex + 1,
                    (int)pagesCount,
                    records)
            };
            divRecordsCount.Controls.Add(litPagerRecords);
            divControl.Controls.Add(divRecordsCount);

            // Show the recods selector to show if the total records is bigger than the minimun of records to show
            if (records >= 10)
            {
                var divRecordsSelector = new HtmlGenericControl("div");
                divRecordsSelector.Attributes.Add("class", "records-select");

                var lblPageRecords = new Label
                {
                    Text = "Nº Records: ",
                    AssociatedControlID = "ddlPageRecords",
                    ToolTip = "Select number of records to display per page"
                };
                divRecordsSelector.Controls.Add(lblPageRecords);

                var ddlPageRecords = new DropDownList
                {
                    ID = "ddlPageRecords",
                    AutoPostBack = true,
                    CssClass = "span1",
                    ToolTip = "Select number of records to display per page"
                };
                ddlPageRecords.SelectedIndexChanged += ddlPageRecords_SelectedIndexChanged;
                ddlPageRecords.FillPageRecordsSelector(PagerSelectorOptions.Split(','), pageSize);
                divRecordsSelector.Controls.Add(ddlPageRecords);
                divControl.Controls.Add(divRecordsSelector);
            }

            var divClearFix = new HtmlGenericControl("div");
            divClearFix.Attributes.Add("class", "clearfix");
            divControl.Controls.Add(divClearFix);

            return divControl;
        }
        #endregion

        #region ELEMENT EVENTS
        protected void lbExport_Click(object sender, EventArgs e)
        {
            var btn = sender as LinkButton;
            if (btn != null)
            {
                if (ExcelExport != null)
                    ExcelExport(null, EventArgs.Empty);

                InitControls();
            }
        }

        protected void lbFilterRemove_Click(object sender, EventArgs e)
        {
            var btn = sender as LinkButton;
            if (btn != null)
            {
                var filterSelection = (List<FilterExpression>)Context.Session[this.ID + "_Filters"];
                if (filterSelection != null)
                {
                    var param = btn.CommandArgument.ToString().Split('|');

                    string column = param[0];
                    string expressionShortName = param[1];
                    string text = param[2];

                    var filterExpression = filterSelection.SingleOrDefault(x => x.Column == column
                        && x.ExpressionShortName == expressionShortName
                        && x.Text == text);
                    if (filterExpression != null)
                        filterSelection.Remove(filterExpression);

                    Context.Session[this.ID + "_Filters"] = filterSelection;
                    Context.Session[this.ID + "_PageIndex"] = 0;

                    //ucGridViewEx_DataBind();
                    if (FilterDeleted != null)
                        FilterDeleted(null, EventArgs.Empty);

                    InitControls();
                }
            }
        }

        protected void lbFilterRemoveAll_Click(object sender, EventArgs e)
        {
            var btn = sender as LinkButton;
            if (btn != null)
            {
                Context.Session[this.ID + "_Filters"] = new List<FilterExpression>();
                Context.Session[this.ID + "_PageIndex"] = 0;

                //ucGridViewEx_DataBind();
                if (FilterDeleted != null)
                    FilterDeleted(null, EventArgs.Empty);

                InitControls();
            }
        }

        protected void lbSortingRemove_Click(object sender, EventArgs e)
        {
            var btn = sender as LinkButton;
            if (btn != null)
            {
                var sortingSelection = (List<SortExpression>)Context.Session[this.ID + "_SortExpressions"];
                if (sortingSelection != null)
                {
                    var sortExpression = sortingSelection.SingleOrDefault(x => x.Column == btn.CommandArgument);
                    if (sortExpression != null)
                        sortingSelection.Remove(sortExpression);

                    Context.Session[this.ID + "_SortExpressions"] = sortingSelection;

                    //ucGridViewEx_DataBind();
                    if (SortingChanged != null)
                        SortingChanged(null, EventArgs.Empty);

                    InitControls();
                }
            }
        }

        protected void lbSortingRemoveAll_Click(object sender, EventArgs e)
        {
            var btn = sender as LinkButton;
            if (btn != null)
            {
                Context.Session[this.ID + "_SortExpressions"] = new List<SortExpression>();

                //ucGridViewEx_DataBind();
                if (SortingChanged != null)
                    SortingChanged(null, EventArgs.Empty);

                InitControls();
            }
        }

        protected void lbSortingChangeIndexUp_Click(object sender, EventArgs e)
        {
            var btn = sender as LinkButton;
            if (btn != null)
            {
                var sortingSelection = (List<SortExpression>)Context.Session[this.ID + "_SortExpressions"];
                if (sortingSelection != null)
                {
                    var sortExpression = sortingSelection.SingleOrDefault(x => x.Column == btn.CommandArgument);
                    if (sortExpression != null)
                    {
                        int oldIndex = sortingSelection.IndexOf(sortExpression);
                        sortingSelection.Remove(sortExpression);
                        sortingSelection.Insert(oldIndex - 1, sortExpression);

                        Context.Session[this.ID + "_SortExpressions"] = sortingSelection;
                    }

                    //ucGridViewEx_DataBind();
                    if (SortingChanged != null)
                        SortingChanged(null, EventArgs.Empty);

                    InitControls();
                }
            }
        }

        protected void lbSortingChangeIndexDown_Click(object sender, EventArgs e)
        {
            var btn = sender as LinkButton;
            if (btn != null)
            {
                var sortingSelection = (List<SortExpression>)Context.Session[this.ID + "_SortExpressions"];
                if (sortingSelection != null)
                {
                    var sortExpression = sortingSelection.SingleOrDefault(x => x.Column == btn.CommandArgument);
                    if (sortExpression != null)
                    {
                        int oldIndex = sortingSelection.IndexOf(sortExpression);
                        sortingSelection.Remove(sortExpression);
                        sortingSelection.Insert(oldIndex + 1, sortExpression);

                        Context.Session[this.ID + "_SortExpressions"] = sortingSelection;
                    }

                    //ucGridViewEx_DataBind();
                    if (SortingChanged != null)
                        SortingChanged(null, EventArgs.Empty);

                    InitControls();
                }
            }
        }

        protected void lbColumnApply_Click(object sender, EventArgs e)
        {
            var btn = sender as LinkButton;
            if (btn != null)
            {
                var cookie = Page.Request.Cookies[this.ID + "_ColumnsSelected"];
                if (cookie != null)
                {
                    var columnSelection = JsonConvert.DeserializeObject<List<ColumnExpression>>(HttpUtility.UrlDecode(cookie.Value));
                    Context.Session[this.ID + "_Columns"] = columnSelection.OrderBy(x => x.Index).ToList();

                    if (ColumnSelectionChanged != null)
                        ColumnSelectionChanged(null, EventArgs.Empty);
                }
            }

            InitControls();
        }

        protected void lbPage_Click(object sender, EventArgs e)
        {
            var btn = sender as LinkButton;
            if (btn != null)
            {
                Context.Session[this.ID + "_PageIndex"] = Convert.ToInt32(btn.CommandArgument) - 1;

                if (PageChanged != null)
                    PageChanged(null, EventArgs.Empty);
            }

            InitControls();
        }

        protected void ddlPageRecords_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ddl = sender as DropDownList;
            if (ddl != null)
            {
                Context.Session[this.ID + "_PageIndex"] = 0;
                Context.Session[this.ID + "_PageSize"] = Convert.ToInt32(ddl.SelectedValue);

                if (PageChanged != null)
                    PageChanged(null, EventArgs.Empty);
            }

            InitControls();
        }
        #endregion
    }
}