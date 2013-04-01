using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace GridViewEx
{
    /// <summary>
    /// Extended standard .NET GridView lot more functional and powerful than the original
    /// </summary>
    /// <remarks>
    /// [{"Author": "Vicent Climent";
    /// "Created Date": "08/03/2013"}]
    /// </remarks>
    public class GridViewEx : GridView
    {
        #region VARIABLES
        /// <summary>
        /// Stores the JS script for the whole table and print it only on one place at the end of the HTML table
        /// </summary>
        internal string JSScript { get; set; }
        /// <summary>
        /// Stores the JS function calls to add them inside the JS BeginRequestHandler
        /// </summary>
        internal string JSScriptBeginRequestHandler { get; set; }
        /// <summary>
        /// Stores the JS function calls to add them inside the JS EndRequestHandler
        /// </summary>
        internal string JSScriptEndRequestHandler { get; set; }
        /// <summary>
        /// Stores the JS function calls to add them inside the JS jQuery DocumentReady
        /// </summary>
        internal string JSScriptDocumentReady { get; set; }

        /// <summary>
        /// Used to title the table
        /// </summary>
        /// <remarks> 
        /// The title is added inside the 'legend' tag of the 'fieldset'
        /// </remarks> 
        public string Title { get; set; }
        /// <summary>
        /// Relative URL to a loading image on your project. If <see cref="LoadingImageUrl"/> is blank or NULL none image is used
        /// </summary>
        /// <example>  
        /// LoadingImageUrl="~/Images/ajax-loader.gif"
        /// </example> 
        public string LoadingImageUrl { get; set; }
        /// <summary>
        /// Used to set the default size of the table
        /// </summary>
        public bool IsCompact { get; set; }
        /// <summary>
        /// Used to set if by default the inline filters will be displayed
        /// </summary>
        public bool IsFilterShown { get; set; }
        /// <summary>
        /// Set if the table should be CSS striped
        /// </summary>
        public bool TableStriped { get; set; }
        /// <summary>
        /// Set if the table row should change the background while hovering
        /// </summary>
        public bool TableHover { get; set; }
        /// <summary>
        /// Set the pager available option of the drop down list. By default is set to "10,50,100"
        /// </summary>
        /// <example>  
        /// PagerSelectorOptions="5,10,20,40,80"
        /// </example>
        public string PagerSelectorOptions { get; set; }
        /// <summary>
        /// Set the default Sort Expressions of the table
        /// </summary>
        /// <example>  
        /// This sample shows how to call the <see cref="SortExpressions"/> method.
        /// <code> 
        /// gridViewEx.SortExpressions = new List&lt;SortExpression&gt;();
        /// gridViewEx.SortExpressions.Add(new SortExpression
        /// {
        ///     Column = "Name",
        ///     Direction = "DESC"
        /// });
        /// </code> 
        /// </example> 
        public List<SortExpression> SortExpressions { get; set; }
        #endregion

        #region EVENTS
        /// <summary>
        /// Event fired when the sorting is changed
        /// </summary>
        public event EventHandler SortingChanged;
        /// <summary>
        /// Event fired when all the filters are deleted
        /// </summary>
        public event EventHandler FilterDeleted;
        /// <summary>
        /// Event fired when the page index or page size is changed
        /// </summary>
        public event EventHandler PageChanged;
        /// <summary>
        /// Event fired when the column is moved or when the columns are shown/hide
        /// </summary>
        public event EventHandler ColumnSelectionChanged;
        /// <summary>
        /// Event fired when export the table to excel
        /// </summary>
        public event EventHandler ExcelExport;
        /// <summary>
        /// Event fired when the view is changed or when saving a new one
        /// </summary>
        public event EventHandler ViewChanged;
        #endregion

        #region OVERRIDE FUNCTIONS
        /// <summary>
        /// Override the Init function to add some default options
        /// </summary>
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
            PageSize = PageSize == 0 ? 10 : PageSize;
            GridLines = GridLines.None;
            AllowSorting = true;
            AutoGenerateColumns = false;
            ShowHeaderWhenEmpty = true;

            if (String.IsNullOrWhiteSpace(EmptyDataText))
                EmptyDataText = "No data to display";
            if (String.IsNullOrWhiteSpace(PagerSelectorOptions))
                PagerSelectorOptions = "10,50,100";
        }

        /// <summary>
        /// Override the Init function to call the <see cref="InitControls()"/> function
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InitControls();
        }

        ///// <summary>
        ///// Override the DataBinding function
        ///// </summary>
        //protected override void OnDataBinding(EventArgs e)
        //{
        //    //DataSource = GridViewExDataSource((IQueryable<object>)DataSource);
        //    base.OnDataBinding(e);
        //}

        /// <summary>
        /// Override the DataBound function to show the HTML table header as TH instead of TR
        /// </summary>
        protected override void OnDataBound(EventArgs e)
        {
            base.OnDataBound(e);
            if (base.HeaderRow != null)
                base.HeaderRow.TableSection = TableRowSection.TableHeader;
        }

        /// <summary>
        /// Override the RowCreated function to show an icon next to the Header title when there's a sorting applied for that column
        /// </summary>
        protected override void OnRowCreated(GridViewRowEventArgs e)
        {
            base.OnRowCreated(e);

            // Check if we have filters to apply
            var sortExpressions = Context.Session[ID + "_SortExpressions"] as List<SortExpression>;
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

        /// <summary>
        /// Override the Sorting function to allow the multi column sorting feature
        /// </summary>
        protected override void OnSorting(GridViewSortEventArgs e)
        {
            // Get the defined sort expressions
            var sortExpressions = Context.Session[ID + "_SortExpressions"] as List<SortExpression>;
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
            Context.Session[ID + "_SortExpressions"] = sortExpressions;
            base.OnSorting(e);

            InitControls();
        }

        /// <summary>
        /// Override the Render function to add all the new functions
        /// </summary>
        protected override void Render(HtmlTextWriter writer)
        {
            var hlCompactTable = new HyperLink
            {
                ID = "hl" + ClientID + "CompactTable",
                NavigateUrl = "#",
                CssClass = "btn pull-right",
                ToolTip = "Compact Table",
                Text = "<i class=\"icon-resize-small\"></i>"
            };
            hlCompactTable.Attributes.Add("onclick", ClientID + "CompactTable(true);");
            hlCompactTable.Attributes.Add("style", "margin-right: 10px;");

            var hlExpandTable = new HyperLink
            {
                ID = "hl" + ClientID + "ExpandTable",
                NavigateUrl = "#",
                CssClass = "btn hide pull-right",
                ToolTip = "Expand Table",
                Text = "<i class=\"icon-resize-full\"></i>"
            };
            hlExpandTable.Attributes.Add("onclick", ClientID + "CompactTable(false);");
            hlExpandTable.Attributes.Add("style", "margin-right: 10px;");

            // Render the table wrap and the overlay which is shown on the AJAX calls
            writer.Write("<div id=\"" + ClientID + "grid-view\" class=\"grid-view\"><div class=\"overlay\">");

            // Check if the loading image is null, if so not add it
            if (!String.IsNullOrWhiteSpace(LoadingImageUrl))
            {
                var imgLoader = new Image { ImageUrl = LoadingImageUrl };
                imgLoader.RenderControl(writer);
            }

            // Render the close tag for overlay and the title
            writer.Write("</div><!-- .overlay --><fieldset><legend>" + Title);

            // Render filter icon and compact/expand icons
            writer.Write("<a href=\"#\" class=\"btn pull-right\" title=\"Filter\" onclick=\"" + ClientID + "ToggleInlineFilter();\"><i class=\"icon-search\"></i></a>");
            hlCompactTable.RenderControl(writer);
            hlExpandTable.RenderControl(writer);

            if (base.Controls.Count == 7)
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

                // Render management views control
                base.Controls[5].RenderControl(writer);
                base.Controls[5].Visible = false;

                base.Controls[6].Visible = false; // Hide because it should go after the table

                writer.Write("</legend></fieldset><div id=\"" + ClientID + "GridViewTable\" class=\"grid-view-table\">");
                base.Render(writer);
                writer.Write("</div><!-- .grid-view-table -->");

                // Render pager control
                base.Controls[6].Visible = true;
                base.Controls[6].RenderControl(writer);
                base.Controls[6].Visible = false;
            }
            else
            {
                writer.Write("</legend></fieldset><div id=\"" + ClientID + "GridViewTable\" class=\"grid-view-table\">");
                base.Controls.Clear();
                base.Render(writer);
                writer.Write("</div><!-- .grid-view-table -->");
            }

            writer.Write(@"</div><!-- .grid-view -->
            <script type='text/javascript'>
                var " + ClientID + @"SizeCompact = " + IsCompact.ToString().ToLower() + @";
                var " + ClientID + @"IsFilterShown = " + IsFilterShown.ToString().ToLower() + @";

                Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(" + ClientID + @"BeginRequestHandler);
                Sys.WebForms.PageRequestManager.getInstance().add_endRequest(" + ClientID + @"EndRequestHandler);

                function " + ClientID + @"ToggleInlineFilter(isVisible) {
                    if (typeof isVisible != 'undefined') {
                        " + ClientID + @"IsFilterShown = isVisible;
                        if (isVisible) {
                            $('." + ClientID + @"Filters').show();
                        }
                        else {
                            $('." + ClientID + @"Filters').hide();
                        }
                    }
                    else {
                        " + ClientID + @"IsFilterShown = !" + ClientID + @"IsFilterShown;
                        $('." + ClientID + @"Filters').toggle();
                    }
                    $('#" + ClientID + @"Scrollbar')
                        .children('div')
                        .width($('#" + ClientID + @"GridViewTable')
                            .children('div')
                            .children('table')[0]
                            .scrollWidth);
                }
                
                function " + ClientID + @"CompactTable(isCompact) {
                    " + ClientID + @"SizeCompact = isCompact;
                    if (isCompact) {
                        $('#" + ClientID + @"').addClass('table-condensed');
                        $('#" + hlExpandTable.ClientID + @"').show();
                        $('#" + hlCompactTable.ClientID + @"').hide();
                    }
                    else {
                        $('#" + ClientID + @"').removeClass('table-condensed');
                        $('#" + hlExpandTable.ClientID + @"').hide();
                        $('#" + hlCompactTable.ClientID + @"').show();
                    }
                    $('#" + ClientID + @"Scrollbar')
                        .children('div')
                        .width($('#" + ClientID + @"GridViewTable')
                            .children('div')
                            .children('table')[0]
                            .scrollWidth);
                }

                function " + ClientID + @"CreateTopScrollbar() {
                    var element = $('#" + ClientID + @"GridViewTable');
                    var scrollbar = $('<div></div>')
                        .attr('id','" + ClientID + @"Scrollbar')
                        .css('overflow-x', 'auto')
                        .css('overflow-y', 'hidden')
                        .append($('<div></div>')
                            .width(element
                                .children('div')
                                .children('table')[0]
                                .scrollWidth)
                            .css('padding-top', '1px')
                            .append('\xA0'));
                    scrollbar.scroll(function() {
                        element.scrollLeft(scrollbar.scrollLeft());
                    });
                    element.scroll(function() {
                        scrollbar.scrollLeft(element.scrollLeft());
                    });
                    element.before(scrollbar);
                }

                function " + ClientID + @"Popover(hoverElementID, divElementID) {
                    $(hoverElementID).popover({
                        html: true,
                        trigger: 'manual',
                        placement: 'bottom',
                        content: function () {
                            return $(divElementID).html();
                        }
                    });

                    var timer, popover_parent;

                    $(hoverElementID).hover(function () {
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

                function " + ClientID + @"SaveSearchExp(hfID, focusID, value) {
                    $('#' + hfID).val(value);
                    $('#' + focusID).focus();
                }
                " + JSScript + @"

                function " + ClientID + @"BeginRequestHandler(sender, args) {
                    $('#" + ClientID + @"grid-view .overlay')
                        .width($('#" + ClientID + @"grid-view').width())
                        .height($('#" + ClientID + @"grid-view').height())
                        .show();
                    " + JSScriptBeginRequestHandler + @"
                }

                function " + ClientID + @"EndRequestHandler(sender, args) {
                    $('#" + ClientID + @"grid-view .overlay').hide();

                    if (" + ClientID + @"SizeCompact)
                        " + ClientID + @"CompactTable(true);

                    if (" + ClientID + @"IsFilterShown)
                        " + ClientID + @"ToggleInlineFilter(true);

                    " + ClientID + @"CreateTopScrollbar()
                    " + JSScriptEndRequestHandler + @"
                }

                $(document).ready(function () {
                    if (" + ClientID + @"SizeCompact)
                        " + ClientID + @"CompactTable(true);

                    if (" + ClientID + @"IsFilterShown)
                        " + ClientID + @"ToggleInlineFilter(true);

                    " + ClientID + @"CreateTopScrollbar();
                    " + JSScriptDocumentReady + @"
                });
            </script>");
        }
        #endregion

        /// <summary>
        /// Create all the different table management like the sorting management, paging, export...
        /// </summary>
        public void InitControls()
        {
            // Add export control
            Controls.Add(CreateExportControl());

            // Add sorting management control
            Controls.Add(CreateSortingManagementControl());

            // Add filter management control
            Controls.Add(CreateFilterManagementControl());

            // Add column management control
            Controls.Add(CreateColumnManagementControl());

            // Add view management control
            Controls.Add(CreateViewManagementControl());

            // Add pager control
            Controls.Add(CreatePagerControl());
        }

        /// <summary>
        /// Set the <paramref name="view"/> as the current
        /// </summary>
        /// <param name="view">ViewExpression with the sortings/filters to apply</param>
        public void SetView(ViewExpression view)
        {
            if (view != null)
            {
                // TODO: Check than the column on the ColumnExpressions/FilterExpressions/SortExpressions exist or not to remove it from it before add it to the session
                Context.Session[ID + "_Columns"] = view.ColumnExpressions;
                Context.Session[ID + "_Filters"] = view.FilterExpressions;
                Context.Session[ID + "_SortExpressions"] = view.SortExpressions;

                if (view.PageSize != 0)
                    Context.Session[ID + "_PageSize"] = view.PageSize;
            }
        }

        /// <summary>
        /// Load the <paramref name="views"/> into the list of available views
        /// </summary>
        /// <param name="views">List of ViewExpression with the sortings/filters to apply</param>
        public void LoadViews(List<ViewExpression> views)
        {
            Context.Session[ID + "_ViewExpressions"] = views;
        }

        /// <summary>
        /// Get the GridView DataSource with the current filters/sortings/columns applied to it. Also returns the specified page
        /// </summary>
        /// <param name="query">IQueryable with the query to send to DB</param>
        /// <param name="customOrder">Set to true if you want to custom order the list, so no SQL order is applied</param>
        /// <param name="isExport">Set to true so all results are returned</param>
        /// <returns>List with the selected data</returns>
        public List<dynamic> GridViewExDataSource<T>(IQueryable<T> query, bool customOrder = false, bool isExport = false)
        {
            // Reset the session when reload the grid unles ks is defined
            if (!Page.IsPostBack)
            {
                bool keepSession = false;
                bool.TryParse(Context.Request["ks"], out keepSession);

                if (!keepSession)
                {
                    Context.Session[ID + "_PageIndex"] = PageIndex;
                    Context.Session[ID + "_PageSize"] = PageSize;
                    Context.Session[ID + "_SortExpressions"] = SortExpressions != null ? SortExpressions : new List<SortExpression>();
                    Context.Session[ID + "_Filters"] = null;
                    Context.Session[ID + "_Columns"] = null;
                }
            }

            if (!isExport)
            {
                var sortExpressions = Context.Session[ID + "_SortExpressions"] as List<SortExpression>;

                // Column management
                if (Context.Session[ID + "_Columns"] == null)
                {
                    var columns = new List<ColumnExpression>();
                    var index = 0;
                    foreach (var column in Columns)
                    {
                        var gridViewExColumn = column as ColumnEx;
                        var boundColumn = column as BoundField;
                        var checkboxColumn = column as CheckBoxField;

                        if (gridViewExColumn != null)
                        {
                            columns.Add(new ColumnExpression
                            {
                                Column = gridViewExColumn.DataField,
                                DisplayName = gridViewExColumn.HeaderText,
                                Visible = gridViewExColumn.Visible,
                                Type = "ColumnEx",
                                Index = index,
                                DataFormat = gridViewExColumn.DataFormat,
                                DataFormatExpression = gridViewExColumn.DataFormatExpression
                            });

                            // Add the display name into the sort expressions
                            if (sortExpressions != null)
                            {
                                var sortExpr = sortExpressions.SingleOrDefault(x => x.Column == gridViewExColumn.DataField);
                                if (sortExpr != null)
                                    sortExpr.DisplayName = gridViewExColumn.HeaderText;
                            }
                        }
                        else if (boundColumn != null)
                        {
                            columns.Add(new ColumnExpression
                            {
                                Column = boundColumn.DataField,
                                DisplayName = boundColumn.HeaderText,
                                Visible = boundColumn.Visible,
                                Type = "BoundField",
                                Index = index,
                                DataFormat = DataFormatEnum.Text,
                                DataFormatExpression = String.Empty
                            });

                            // Add the display name into the sort expressions
                            if (sortExpressions != null)
                            {
                                var sortExpr = sortExpressions.SingleOrDefault(x => x.Column == boundColumn.DataField);
                                if (sortExpr != null)
                                    sortExpr.DisplayName = boundColumn.HeaderText;
                            }
                        }
                        else if (checkboxColumn != null)
                        {
                            columns.Add(new ColumnExpression
                            {
                                Column = checkboxColumn.DataField,
                                DisplayName = checkboxColumn.HeaderText,
                                Visible = checkboxColumn.Visible,
                                Type = "CheckBoxField",
                                Index = index,
                                DataFormat = DataFormatEnum.Text,
                                DataFormatExpression = String.Empty
                            });

                            // Add the display name into the sort expressions
                            if (sortExpressions != null)
                            {
                                var sortExpr = sortExpressions.SingleOrDefault(x => x.Column == checkboxColumn.DataField);
                                if (sortExpr != null)
                                    sortExpr.DisplayName = checkboxColumn.HeaderText;
                            }
                        }

                        index++;
                    }

                    Context.Session[ID + "_Columns"] = columns;
                }
                else
                {
                    foreach (var sessionColumn in (List<ColumnExpression>)Context.Session[ID + "_Columns"])
                    {
                        var index = 0;
                        foreach (var column in Columns)
                        {
                            var flag = false;
                            switch (sessionColumn.Type)
                            {
                                case "ColumnEx":
                                    var gridViewExColumn = column as ColumnEx;
                                    if (gridViewExColumn != null
                                        && gridViewExColumn.DataField == sessionColumn.Column)
                                    {
                                        gridViewExColumn.Visible = sessionColumn.Visible;

                                        if (sessionColumn.Index != index)
                                        {
                                            var col = Columns[index];
                                            Columns.Remove(col);
                                            Columns.Insert(sessionColumn.Index, col);
                                            flag = true;
                                        }

                                        // Add the display name into the sort expressions
                                        if (sortExpressions != null)
                                        {
                                            var sortExpr = sortExpressions.SingleOrDefault(x => x.Column == gridViewExColumn.DataField);
                                            if (sortExpr != null)
                                                sortExpr.DisplayName = gridViewExColumn.HeaderText;
                                        }
                                    }
                                    break;
                                case "BoundField":
                                    var boundColumn = column as BoundColumn;
                                    if (boundColumn != null
                                        && boundColumn.DataField == sessionColumn.Column)
                                    {
                                        boundColumn.Visible = sessionColumn.Visible;

                                        if (sessionColumn.Index != index)
                                        {
                                            var col = Columns[index];
                                            Columns.Remove(col);
                                            Columns.Insert(sessionColumn.Index, col);
                                            flag = true;
                                        }

                                        // Add the display name into the sort expressions
                                        if (sortExpressions != null)
                                        {
                                            var sortExpr = sortExpressions.SingleOrDefault(x => x.Column == boundColumn.DataField);
                                            if (sortExpr != null)
                                                sortExpr.DisplayName = boundColumn.HeaderText;
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

                Context.Session[ID + "_SortExpressions"] = sortExpressions;

                int pageSize = Context.Session[ID + "_PageSize"] != null
                    ? Convert.ToInt32(Context.Session[ID + "_PageSize"])
                    : PageSize;
                int pageIndex = Context.Session[ID + "_PageIndex"] != null
                    ? Convert.ToInt32(Context.Session[ID + "_PageIndex"])
                    : PageIndex;

                Context.Session[ID + "_PageIndex"] = pageIndex;
                Context.Session[ID + "_PageSize"] = pageSize;

                int startRow = pageIndex * pageSize;

                // Save filters data for later access
                foreach (var column in Columns)
                {
                    var gridViewExColumn = column as ColumnEx;
                    if (gridViewExColumn != null
                        && gridViewExColumn.SearchType == SearchTypeEnum.DropDownList
                        && gridViewExColumn.DropDownDataSource == null)
                    {
                        gridViewExColumn.DropDownDataSource = query.GetDropDownDataSource(gridViewExColumn.DataField, gridViewExColumn.DataFormat, gridViewExColumn.DataFormatExpression);
                    }
                }

                query = query.Filter((List<FilterExpression>)Context.Session[ID + "_Filters"]); // Apply filters

                // Count total records
                Context.Session[ID + "_Records"] = query.Count();

                if (!customOrder)
                {
                    // Sort the rows
                    var sourceIQueryable = (IQueryable<dynamic>)query.Order((List<SortExpression>)Context.Session[ID + "_SortExpressions"]);

                    // Page the query if necessary
                    sourceIQueryable = sourceIQueryable.Skip(startRow).Take(pageSize);

                    // Finally return list
                    return sourceIQueryable.ToList();
                }
                else
                {
                    // Page the query if necessary
                    query = query.Skip(startRow).Take(pageSize);

                    // Finally return list
                    return ((IQueryable<dynamic>)query).ToList();
                }
            }
            else
            {
                query = query.Filter((List<FilterExpression>)Context.Session[ID + "_Filters"]); // Apply filters
                return (!customOrder)
                    ? ((IQueryable<dynamic>)query.Order((List<SortExpression>)Context.Session[ID + "_SortExpressions"])).ToList()
                    : ((IQueryable<dynamic>)query).ToList();
            }
        }

        /// <summary>
        /// Change the sorting index
        /// </summary>
        /// <param name="sender">Object which has raised the event</param>
        /// <param name="e">Contains additional information about the event</param>
        /// <param name="indexUp">Set true if the index is moved up, oterwhise set to false</param>
        private void SortingChangeIndex(object sender, EventArgs e, bool indexUp)
        {
            var btn = sender as LinkButton;
            if (btn != null)
            {
                var sortingSelection = (List<SortExpression>)Context.Session[ID + "_SortExpressions"];
                if (sortingSelection != null)
                {
                    var sortExpression = sortingSelection.SingleOrDefault(x => x.Column == btn.CommandArgument);
                    if (sortExpression != null)
                    {
                        int oldIndex = sortingSelection.IndexOf(sortExpression);
                        sortingSelection.Remove(sortExpression);

                        sortingSelection.Insert(indexUp ? (oldIndex - 1) : (oldIndex + 1), sortExpression);

                        Context.Session[ID + "_SortExpressions"] = sortingSelection;
                    }

                    //ucGridViewEx_DataBind();
                    if (SortingChanged != null)
                        SortingChanged(null, EventArgs.Empty);

                    InitControls();
                }
            }
        }

        #region CONTROL CREATION
        private Control CreateExportControl()
        {
            var phControl = new PlaceHolder();

            var lbExport = new LinkButton
            {
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
            var filters = (List<FilterExpression>)Context.Session[ID + "_Filters"] != null
                ? (List<FilterExpression>)Context.Session[ID + "_Filters"]
                : new List<FilterExpression>();

            var phControl = new PlaceHolder();

            var hlFilterSelection = new HtmlAnchor
            {
                ID = "hl" + ID + "FilterSelection",
                ClientIDMode = ClientIDMode.Static,
                HRef = "#",
                Title = "Filter Management",
                InnerHtml = "<i class=\"icon-filter\"></i>"
            };
            hlFilterSelection.Attributes.Add("class", "btn pull-right");
            hlFilterSelection.Attributes.Add("style", "margin-right: 10px;");
            if (filters.Count >= 1)
                hlFilterSelection.Attributes["style"] += "background-color: #B2C6E0;background-image: linear-gradient(to bottom, #FFFFFF, #B2C6E0);";

            phControl.Controls.Add(hlFilterSelection);

            var divFilterSelection = new HtmlGenericControl("div");
            divFilterSelection.ID = "div" + ID + "FilterSelection";
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

            JSScriptEndRequestHandler += ClientID + @"Popover('#" + hlFilterSelection.ClientID + "','#" + divFilterSelection.ClientID + "');";
            JSScriptDocumentReady += ClientID + @"Popover('#" + hlFilterSelection.ClientID + "','#" + divFilterSelection.ClientID + "');";

            return phControl;
        }

        private Control CreateSortingManagementControl()
        {
            var sortings = (List<SortExpression>)Context.Session[ID + "_SortExpressions"] != null
                ? (List<SortExpression>)Context.Session[ID + "_SortExpressions"]
                : new List<SortExpression>();

            var phControl = new PlaceHolder();

            var hlSortSelection = new HtmlAnchor
            {
                ID = "hl" + ID + "SortSelection",
                ClientIDMode = ClientIDMode.Static,
                HRef = "#",
                Title = "Sorting Management",
                InnerHtml = "<i class=\"icon-list-alt\"></i>",
            };
            hlSortSelection.Attributes.Add("class", "btn pull-right");
            hlSortSelection.Attributes.Add("style", "margin-right: 10px;");
            if (sortings.Count >= 1)
                hlSortSelection.Attributes["style"] += "background-color: #B2C6E0;background-image: linear-gradient(to bottom, #FFFFFF, #B2C6E0);";

            phControl.Controls.Add(hlSortSelection);

            var divSortSelection = new HtmlGenericControl("div");
            divSortSelection.ID = "div" + ID + "SortSelection";
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

                    liSort.Controls.Add(new Literal { Text = String.Format("{0} - {1}", !String.IsNullOrWhiteSpace(sortExpression.DisplayName) ? sortExpression.DisplayName : sortExpression.Column, sortExpression.Direction) });

                    ol.Controls.Add(liSort);
                }

                divSortSelection.Controls.Add(ol);
            }
            else
                divSortSelection.Controls.Add(new Label { Text = "No sortings" });

            phControl.Controls.Add(divSortSelection);

            JSScriptEndRequestHandler += ClientID + @"Popover('#" + hlSortSelection.ClientID + "','#" + divSortSelection.ClientID + "');";
            JSScriptDocumentReady += ClientID + @"Popover('#" + hlSortSelection.ClientID + "','#" + divSortSelection.ClientID + "');";

            return phControl;
        }

        private Control CreateColumnManagementControl()
        {
            var columns = (List<ColumnExpression>)Context.Session[ID + "_Columns"] != null
                ? (List<ColumnExpression>)Context.Session[ID + "_Columns"]
                : new List<ColumnExpression>();

            var phControl = new PlaceHolder();

            var hlColumnSelection = new HtmlAnchor
            {
                ID = "hl" + ID + "ColumnSelection",
                ClientIDMode = ClientIDMode.Static,
                HRef = "#",
                Title = "Column Management",
                InnerHtml = "<i class=\"icon-calendar\"></i>"
            };
            hlColumnSelection.Attributes.Add("class", "btn pull-right");
            hlColumnSelection.Attributes.Add("style", "margin-right: 10px;");
            phControl.Controls.Add(hlColumnSelection);

            var js = new JavaScriptSerializer();

            var hfColumnsSelected = new HiddenField
            {
                ID = "hf" + ID + "ColumnsSelected",
                ClientIDMode = ClientIDMode.Static,
                Value = js.Serialize(columns)
            };
            phControl.Controls.Add(hfColumnsSelected);

            var divColumnSelection = new HtmlGenericControl("div");
            divColumnSelection.ID = "div" + ID + "ColumnSelection";
            divColumnSelection.ClientIDMode = ClientIDMode.Static;
            divColumnSelection.Attributes.Add("style", "display: none;");

            var hlHideAll = new HtmlAnchor
            {
                ID = "hl" + ID + "HideAll",
                ClientIDMode = ClientIDMode.Static,
                HRef = "#",
                InnerText = "Hide All",
                Title = "Hide all columns",
                Visible = columns.Count >= 1
            };
            hlHideAll.Attributes.Add("onclick", "ColumnSelectionShowAll(this, false);");

            var hlShowAll = new HtmlAnchor
            {
                ID = "hl" + ID + "ShowAll",
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
                ID = "lb" + ID + "Apply",
                ClientIDMode = ClientIDMode.Static,
                Text = "Apply",
                ToolTip = "Apply changes",
                Visible = columns.Count >= 1
            };
            lbApply.Attributes.Add("style", "display: none;float: right;");
            lbApply.Click += lbColumnApply_Click;
            divColumnSelection.Controls.Add(lbApply);

            if (columns.Count >= 1)
            {
                var ol = new HtmlGenericControl("ol");

                foreach (var column in columns)
                {
                    var liColumn = new HtmlGenericControl("li");

                    var cbVisible = new HtmlInputCheckBox
                    {
                        Checked = column.Visible
                    };
                    cbVisible.Attributes.Add("title", "Check to make it visible");
                    cbVisible.Attributes.Add("onclick", "ColumnSelectionChanged(this);");
                    cbVisible.Attributes.Add("data-field", column.Column);
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
                function ColumnSelectionShowAll(link, showAll) {
                    var arr = JSON.parse($('#" + hfColumnsSelected.ClientID + @"').val());
                    
                    $(link).parent().children('ol').children().each(function () {
                        $(this).children('input:checkbox').prop('checked', showAll);
                    });

                    $.map(arr, function (elementOfArray, indexInArray) {
                        elementOfArray.Visible = showAll;
                    });

                    $('#" + hfColumnsSelected.ClientID + @"').val(JSON.stringify(arr));
                    document.cookie = '" + ID + @"_ColumnsSelected' + '=' + escape(JSON.stringify(arr)) + '; ';
                    $('#" + hlHideAll.ClientID + @", #" + hlShowAll.ClientID + @"').toggle();
                    $('#" + lbApply.ClientID + @"').show();
                }

                function ColumnSelectionChanged(cb) {
                    var arr = JSON.parse($('#" + hfColumnsSelected.ClientID + @"').val());

                    $.map(arr, function (elementOfArray, indexInArray) {
                        if (elementOfArray.Column == $(cb).attr('data-field')) {
                            elementOfArray.Visible = $(cb).is(':checked');
                        }
                    });

                    $('#" + hfColumnsSelected.ClientID + @"').val(JSON.stringify(arr));
                    document.cookie = '" + ID + @"_ColumnsSelected' + '=' + escape(JSON.stringify(arr)) + '; ';
                    $('#" + lbApply.ClientID + @"').show();
                }

                function ColumnIndexChanged(link, index) {
                    var arr = JSON.parse($('#" + hfColumnsSelected.ClientID + @"').val());
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

                    $('#" + hfColumnsSelected.ClientID + @"').val(JSON.stringify(arr));
                    document.cookie = '" + ID + @"_ColumnsSelected' + '=' + escape(JSON.stringify(arr)) + '; ';
                    $('#" + lbApply.ClientID + @"').show();
                }";

            JSScriptEndRequestHandler += ClientID + @"Popover('#" + hlColumnSelection.ClientID + "', '#" + divColumnSelection.ClientID + "');";
            JSScriptDocumentReady += ClientID + @"Popover('#" + hlColumnSelection.ClientID + "', '#" + divColumnSelection.ClientID + "');";

            return phControl;
        }

        private Control CreateViewManagementControl()
        {
            var views = (List<ViewExpression>)Context.Session[ID + "_ViewExpressions"] != null
                ? (List<ViewExpression>)Context.Session[ID + "_ViewExpressions"]
                : new List<ViewExpression>();

            var phControl = new PlaceHolder();

            if (views.Count >= 1)
            {
                var divAppend = new HtmlGenericControl("div");
                divAppend.ID = "divViewExpressionList";
                divAppend.ClientIDMode = ClientIDMode.Static;
                divAppend.Attributes.Add("class", "pull-right input-append");
                divAppend.Attributes.Add("style", "margin-right: 10px;");

                var ddlViews = new DropDownList
                {
                    ID = "ddl" + ID + "ViewExpressionList",
                    ClientIDMode = ClientIDMode.Static,
                    AutoPostBack = true,
                    CssClass = "span1"
                };
                ddlViews.SelectedIndexChanged += ddlViews_SelectedIndexChanged;

                ddlViews.Items.Add(new ListItem("", "-1"));
                ddlViews.Items.AddRange(views.Select(x => new ListItem
                {
                    Text = x.Name,
                    Value = x.ID.ToString()
                }).ToArray());

                divAppend.Controls.Add(ddlViews);

                var hlViewExpression = new HtmlAnchor
                {
                    ID = "hl" + ID + "ViewExpression",
                    ClientIDMode = ClientIDMode.Static,
                    HRef = "#div" + ID + "ViewExpression",
                    Title = "View Management",
                    InnerHtml = "<i class=\"icon-eye-open\"></i>",
                };
                hlViewExpression.Attributes.Add("data-toggle", "modal");
                hlViewExpression.Attributes.Add("role", "button");
                hlViewExpression.Attributes.Add("style", "display: inline-block;");
                hlViewExpression.Attributes.Add("class", "btn add-on");

                divAppend.Controls.Add(hlViewExpression);
                phControl.Controls.Add(divAppend);

                //                JSScript += @"
                //                    function " + ClientID + @"ViewsManagement() {
                //                        $('#" + divAppend.ClientID + @"').hover(function() {
                //                            $('#" + hlViewExpression.ClientID + @"').delay(500).queue(function(next){
                //                                $(this).addClass('add-on');
                //                                $('#" + ddlViews.ClientID + @"').show();
                //                                next();
                //                            });
                //                        }, function() { 
                //                            $('#" + hlViewExpression.ClientID + @"').removeClass('add-on');
                //                            $('#" + ddlViews.ClientID + @"').hide();
                //                        });
                //                    }
                //                ";

                //                JSScriptEndRequestHandler += ClientID + @"ViewsManagement();";
                //                JSScriptDocumentReady += ClientID + @"ViewsManagement();";
            }
            else
            {
                var hlViewExpression = new HtmlAnchor
                {
                    ID = "hl" + ID + "ViewExpression",
                    ClientIDMode = ClientIDMode.Static,
                    HRef = "#div" + ID + "ViewExpression",
                    Title = "View Management",
                    InnerHtml = "<i class=\"icon-eye-open\"></i>",
                };
                hlViewExpression.Attributes.Add("data-toggle", "modal");
                hlViewExpression.Attributes.Add("role", "button");
                hlViewExpression.Attributes.Add("style", "display: inline-block;margin-right: 10px;");
                hlViewExpression.Attributes.Add("class", "btn pull-right");

                phControl.Controls.Add(hlViewExpression);
            }

            var divViewExpression = new HtmlGenericControl("div");
            divViewExpression.ID = "div" + ID + "ViewExpression";
            divViewExpression.ClientIDMode = ClientIDMode.Static;
            divViewExpression.Attributes.Add("class", "modal hide fade");
            divViewExpression.Attributes.Add("tabindex", "-1");
            divViewExpression.Attributes.Add("role", "dialog");
            divViewExpression.Attributes.Add("aria-labelledby", "h3" + ID + "ViewExpressionLabel");
            divViewExpression.Attributes.Add("aria-hidden", "true");

            /* START MODAL HEADER */
            var divModalHeader = new HtmlGenericControl("div");
            divModalHeader.Attributes.Add("class", "modal-header");

            var btnCloseModal = new HtmlButton { InnerText = "x" };
            btnCloseModal.Attributes.Add("type", "button");
            btnCloseModal.Attributes.Add("class", "close");
            btnCloseModal.Attributes.Add("data-dismiss", "modal");
            btnCloseModal.Attributes.Add("aria-hidden", "true");
            divModalHeader.Controls.Add(btnCloseModal);

            var h3ModalLabel = new HtmlGenericControl("h3");
            h3ModalLabel.ID = "h3" + ID + "ViewExpressionLabel";
            h3ModalLabel.ClientIDMode = ClientIDMode.Static;
            h3ModalLabel.InnerText = "View Management";
            divModalHeader.Controls.Add(h3ModalLabel);

            divViewExpression.Controls.Add(divModalHeader);
            /* END MODAL HEADER */

            /* START MODAL BODY */
            var divModalBody = new HtmlGenericControl("div");
            divModalBody.Attributes.Add("class", "modal-body");

            var divModalBodyRow = new HtmlGenericControl("div");
            divModalBodyRow.Attributes.Add("class", "row-fluid");

            var divModalBodyRowSpan1 = new HtmlGenericControl("div");
            divModalBodyRowSpan1.Attributes.Add("class", "span6");

            var txtViewName = new TextBox
            {
                ID = "txt" + ID + "ViewExpressionTitle",
                ClientIDMode = ClientIDMode.Static
            };
            txtViewName.Attributes.Add("placeholder", "View name...");
            divModalBodyRowSpan1.Controls.Add(txtViewName);

            var cbSaveFilterLabel = new HtmlGenericControl("label");
            cbSaveFilterLabel.Attributes.Add("class", "checkbox");
            cbSaveFilterLabel.Controls.Add(new CheckBox
            {
                Checked = true,
                ID = "cb" + ID + "ViewExpressionFilters",
                ClientIDMode = ClientIDMode.Static
            });
            cbSaveFilterLabel.Controls.Add(new Literal { Text = " Save Filters" });
            divModalBodyRowSpan1.Controls.Add(cbSaveFilterLabel);

            var cbSaveSortingLabel = new HtmlGenericControl("label");
            cbSaveSortingLabel.Attributes.Add("class", "checkbox");
            cbSaveSortingLabel.Controls.Add(new CheckBox
            {
                Checked = true,
                ID = "cb" + ID + "ViewExpressionSortings"
            });
            cbSaveSortingLabel.Controls.Add(new Literal { Text = " Save Sortings" });
            divModalBodyRowSpan1.Controls.Add(cbSaveSortingLabel);

            var cbSaveColumnLabel = new HtmlGenericControl("label");
            cbSaveColumnLabel.Attributes.Add("class", "checkbox");
            cbSaveColumnLabel.Controls.Add(new CheckBox
            {
                Checked = true,
                ID = "cb" + ID + "ViewExpressionColumns"
            });
            cbSaveColumnLabel.Controls.Add(new Literal { Text = " Save Columns" });
            divModalBodyRowSpan1.Controls.Add(cbSaveColumnLabel);

            var cbSavePageSizeLabel = new HtmlGenericControl("label");
            cbSavePageSizeLabel.Attributes.Add("class", "checkbox");
            cbSavePageSizeLabel.Controls.Add(new CheckBox
            {
                Checked = false,
                ID = "cb" + ID + "ViewExpressionPageSize"
            });
            cbSavePageSizeLabel.Controls.Add(new Literal { Text = " Save Page Size" });
            divModalBodyRowSpan1.Controls.Add(cbSavePageSizeLabel);

            divModalBodyRow.Controls.Add(divModalBodyRowSpan1);

            var divModalBodyRowSpan2 = new HtmlGenericControl("div");
            divModalBodyRowSpan2.Attributes.Add("class", "span6");

            var divSavedFilters = new HtmlGenericControl("div");
            divSavedFilters.Attributes.Add("style", "overflow-y:auto; width:100%; min-height:100px; padding:5px; max-height:200px; border:1px solid whiteSmoke; font-size:14px; line-height:20px;");

            if (views.Count >= 1)
            {
                //var hlSelectAll = new HtmlAnchor
                //{
                //    HRef = "#",
                //    InnerText = "Select All",
                //    Title = "Select all columns"
                //};
                ////hlSelectAll.Attributes.Add("onclick", "ColumnSelectionShowAll(this, false);");
                //divSavedFilters.Controls.Add(hlSelectAll);

                var ol = new HtmlGenericControl("ol");

                foreach (var view in views)
                {
                    var li = new HtmlGenericControl("li");

                    var lblVisible = new HtmlGenericControl("label");
                    lblVisible.Attributes.Add("class", "checkbox");

                    //var cbVisible = new CheckBox();
                    ////cbVisible.Attributes.Add("onclick", "ColumnSelectionChanged(this);");
                    //cbVisible.Attributes.Add("data-index", view.ID.ToString());

                    //lblVisible.Controls.Add(cbVisible);
                    lblVisible.Controls.Add(new Literal { Text = view.Name });

                    li.Controls.Add(lblVisible);
                    ol.Controls.Add(li);
                }

                divSavedFilters.Controls.Add(ol);
            }
            else
                divSavedFilters.Controls.Add(new Label { Text = "No views" });


            divModalBodyRowSpan2.Controls.Add(divSavedFilters);
            divModalBodyRow.Controls.Add(divModalBodyRowSpan2);

            divModalBody.Controls.Add(divModalBodyRow);
            divViewExpression.Controls.Add(divModalBody);
            /* END MODAL BODY */

            /* START MODAL FOOTER */
            var divModalFooter = new HtmlGenericControl("div");
            divModalFooter.Attributes.Add("class", "modal-footer");

            var btnCloseModalFooter = new HtmlButton { InnerText = "Close" };
            btnCloseModalFooter.Attributes.Add("class", "btn");
            btnCloseModalFooter.Attributes.Add("data-dismiss", "modal");
            btnCloseModalFooter.Attributes.Add("aria-hidden", "true");
            divModalFooter.Controls.Add(btnCloseModalFooter);

            var btnSaveViewManagement = new LinkButton { Text = "Save" };
            btnSaveViewManagement.Click += btnSaveViewManagement_Click;
            btnSaveViewManagement.Attributes.Add("class", "btn btn-primary");
            divModalFooter.Controls.Add(btnSaveViewManagement);

            divViewExpression.Controls.Add(divModalFooter);
            /* END MODAL FOOTER */

            phControl.Controls.Add(divViewExpression);

            return phControl;
        }

        private Control CreatePagerControl()
        {
            int pageIndex = (int)Context.Session[ID + "_PageIndex"];
            int pageSize = (int)Context.Session[ID + "_PageSize"];
            int records = (int)Context.Session[ID + "_Records"];

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
        /// <summary>
        /// Excel export
        /// </summary>
        /// <param name="sender">Object which has raised the event</param>
        /// <param name="e">Contains additional information about the event</param>
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

        /// <summary>
        /// Filter management remove single filter expression
        /// </summary>
        /// <param name="sender">Object which has raised the event</param>
        /// <param name="e">Contains additional information about the event</param>
        protected void lbFilterRemove_Click(object sender, EventArgs e)
        {
            var btn = sender as LinkButton;
            if (btn != null)
            {
                var filterSelection = (List<FilterExpression>)Context.Session[ID + "_Filters"];
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

                    Context.Session[ID + "_Filters"] = filterSelection;
                    Context.Session[ID + "_PageIndex"] = 0;

                    //ucGridViewEx_DataBind();
                    if (FilterDeleted != null)
                        FilterDeleted(null, EventArgs.Empty);

                    InitControls();
                }
            }
        }

        /// <summary>
        /// Filter management remove all filter expressions
        /// </summary>
        /// <param name="sender">Object which has raised the event</param>
        /// <param name="e">Contains additional information about the event</param>
        protected void lbFilterRemoveAll_Click(object sender, EventArgs e)
        {
            var btn = sender as LinkButton;
            if (btn != null)
            {
                Context.Session[ID + "_Filters"] = new List<FilterExpression>();
                Context.Session[ID + "_PageIndex"] = 0;

                //ucGridViewEx_DataBind();
                if (FilterDeleted != null)
                    FilterDeleted(null, EventArgs.Empty);

                InitControls();
            }
        }

        /// <summary>
        /// Sort management remove single sort expression
        /// </summary>
        /// <param name="sender">Object which has raised the event</param>
        /// <param name="e">Contains additional information about the event</param>
        protected void lbSortingRemove_Click(object sender, EventArgs e)
        {
            var btn = sender as LinkButton;
            if (btn != null)
            {
                var sortingSelection = (List<SortExpression>)Context.Session[ID + "_SortExpressions"];
                if (sortingSelection != null)
                {
                    var sortExpression = sortingSelection.SingleOrDefault(x => x.Column == btn.CommandArgument);
                    if (sortExpression != null)
                        sortingSelection.Remove(sortExpression);

                    Context.Session[ID + "_SortExpressions"] = sortingSelection;

                    //ucGridViewEx_DataBind();
                    if (SortingChanged != null)
                        SortingChanged(null, EventArgs.Empty);

                    InitControls();
                }
            }
        }

        /// <summary>
        /// Sort management remove all sort expressions
        /// </summary>
        /// <param name="sender">Object which has raised the event</param>
        /// <param name="e">Contains additional information about the event</param>
        protected void lbSortingRemoveAll_Click(object sender, EventArgs e)
        {
            var btn = sender as LinkButton;
            if (btn != null)
            {
                Context.Session[ID + "_SortExpressions"] = new List<SortExpression>();

                //ucGridViewEx_DataBind();
                if (SortingChanged != null)
                    SortingChanged(null, EventArgs.Empty);

                InitControls();
            }
        }

        /// <summary>
        /// Sort management change sort expression index up
        /// </summary>
        /// <param name="sender">Object which has raised the event</param>
        /// <param name="e">Contains additional information about the event</param>
        protected void lbSortingChangeIndexUp_Click(object sender, EventArgs e)
        {
            SortingChangeIndex(sender, e, true);
        }

        /// <summary>
        /// Sort management change sort expression index down
        /// </summary>
        /// <param name="sender">Object which has raised the event</param>
        /// <param name="e">Contains additional information about the event</param>
        protected void lbSortingChangeIndexDown_Click(object sender, EventArgs e)
        {
            SortingChangeIndex(sender, e, false);
        }

        /// <summary>
        /// Column management changed
        /// </summary>
        /// <param name="sender">Object which has raised the event</param>
        /// <param name="e">Contains additional information about the event</param>
        protected void lbColumnApply_Click(object sender, EventArgs e)
        {
            var btn = sender as LinkButton;
            if (btn != null)
            {
                var cookie = Page.Request.Cookies[ID + "_ColumnsSelected"];
                if (cookie != null)
                {
                    var js = new JavaScriptSerializer();

                    var columnSelection = js.Deserialize<List<ColumnExpression>>(HttpUtility.UrlDecode(cookie.Value));
                    Context.Session[ID + "_Columns"] = columnSelection.OrderBy(x => x.Index).ToList();

                    if (ColumnSelectionChanged != null)
                        ColumnSelectionChanged(null, EventArgs.Empty);
                }
            }

            InitControls();
        }

        /// <summary>
        /// Page index changed
        /// </summary>
        /// <param name="sender">Object which has raised the event</param>
        /// <param name="e">Contains additional information about the event</param>
        protected void lbPage_Click(object sender, EventArgs e)
        {
            var btn = sender as LinkButton;
            if (btn != null)
            {
                Context.Session[ID + "_PageIndex"] = Convert.ToInt32(btn.CommandArgument) - 1;

                if (PageChanged != null)
                    PageChanged(null, EventArgs.Empty);
            }

            InitControls();
        }

        /// <summary>
        /// Page size changed
        /// </summary>
        /// <param name="sender">Object which has raised the event</param>
        /// <param name="e">Contains additional information about the event</param>
        protected void ddlPageRecords_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ddl = sender as DropDownList;
            if (ddl != null)
            {
                Context.Session[ID + "_PageIndex"] = 0;
                Context.Session[ID + "_PageSize"] = Convert.ToInt32(ddl.SelectedValue);

                if (PageChanged != null)
                    PageChanged(null, EventArgs.Empty);
            }

            InitControls();
        }

        /// <summary>
        /// View management save view
        /// </summary>
        /// <param name="sender">Object which has raised the event</param>
        /// <param name="e">Contains additional information about the event</param>
        protected void btnSaveViewManagement_Click(object sender, EventArgs e)
        {
            var btn = sender as LinkButton;
            if (btn != null
                && ViewChanged != null)
            {
                var txtName = btn.NamingContainer.FindControl("txt" + ID + "ViewExpressionTitle") as TextBox;
                var cbFilters = btn.NamingContainer.FindControl("cb" + ID + "ViewExpressionFilters") as CheckBox;
                var cbSortings = btn.NamingContainer.FindControl("cb" + ID + "ViewExpressionSortings") as CheckBox;
                var cbColumns = btn.NamingContainer.FindControl("cb" + ID + "ViewExpressionColumns") as CheckBox;
                var cbPageSize = btn.NamingContainer.FindControl("cb" + ID + "ViewExpressionPageSize") as CheckBox;
                if (txtName != null
                    && cbFilters != null
                    && cbSortings != null
                    && cbColumns != null
                    && cbPageSize != null)
                {
                    var view = new ViewExpression
                    {
                        ColumnExpressions = cbColumns.Checked ? Context.Session[ID + "_Columns"] as List<ColumnExpression> : null,
                        FilterExpressions = cbFilters.Checked ? Context.Session[ID + "_Filters"] as List<FilterExpression> : null,
                        SortExpressions = cbSortings.Checked ? Context.Session[ID + "_SortExpressions"] as List<SortExpression> : null,
                        Name = txtName.Text,
                        PageSize = cbPageSize.Checked ? Convert.ToInt32(Context.Session[ID + "_PageSize"]) : 0
                    };

                    ViewChanged(view, EventArgs.Empty);
                }
            }

            InitControls();
        }

        /// <summary>
        /// View management change view
        /// </summary>
        /// <param name="sender">Object which has raised the event</param>
        /// <param name="e">Contains additional information about the event</param>
        protected void ddlViews_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ddl = sender as DropDownList;
            if (ddl != null
                && ViewChanged != null)
            {
                int viewID = Convert.ToInt32(ddl.SelectedValue);
                ViewChanged(viewID, EventArgs.Empty);
            }

            InitControls();
        }
        #endregion
    }
}