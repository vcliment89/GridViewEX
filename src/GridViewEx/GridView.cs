using GridViewEx.Columns;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
        /// Stores the JS function calls to add them inside the JS BeginRequestHandler (Delayed scripts)
        /// </summary>
        internal string JSScriptBeginRequestHandlerDelayed { get; set; }

        /// <summary>
        /// Stores the JS function calls to add them inside the JS EndRequestHandler
        /// </summary>
        internal string JSScriptEndRequestHandler { get; set; }

        /// <summary>
        /// Stores the JS function calls to add them inside the JS EndRequestHandler (Delayed scripts)
        /// </summary>
        internal string JSScriptEndRequestHandlerDelayed { get; set; }

        /// <summary>
        /// Stores the JS function calls to add them inside the JS jQuery DocumentReady
        /// </summary>
        internal string JSScriptDocumentReady { get; set; }

        /// <summary>
        /// Stores the JS function calls to add them inside the JS EndRequestHandler (Delayed scripts)
        /// </summary>
        internal string JSScriptDocumentReadyDelayed { get; set; }

        /// <summary>
        /// Stores the alert message CSS class, so it ca change for errors, warnings...
        /// </summary>
        internal string AlertMessageClass { get; set; }

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
        /// Used to set if the compact button will be displayed
        /// </summary>
        public bool IsCompactShown { get; set; }

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
        /// Set the pager available option of the drop down list. By default is set to "10,50,100,All"
        /// </summary>
        /// <example>  
        /// PagerSelectorOptions="5,10,20,40,80"
        /// </example>
        public string PagerSelectorOptions { get; set; }

        /// <summary>
        /// Set the version of bootstrap used
        /// </summary>
        /// <example>  
        /// 2.3
        /// </example>
        public double? BootstrapVersion { get; set; }

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

        /// <summary>
        /// Set the culture info used by the table. Default set to CultureInfo.InvariantCulture
        /// </summary>
        public CultureInfo CultureInfo { get; set; }
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
        /// <param name="e">Contains additional information about the event</param>
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

            // Set a default culture
            if (CultureInfo == null)
                CultureInfo = Thread.CurrentThread.CurrentCulture;

            if (String.IsNullOrWhiteSpace(EmptyDataText))
                EmptyDataText = "No data to display";
            if (String.IsNullOrWhiteSpace(PagerSelectorOptions))
                PagerSelectorOptions = "10,50,100,All";

            if (!String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["BootstrapVersion"]))
                BootstrapVersion = Convert.ToDouble(ConfigurationManager.AppSettings["BootstrapVersion"]);

            if (!BootstrapVersion.HasValue)
                BootstrapVersion = 2.3;
        }

        /// <summary>
        /// Override the Init function to call the <see cref="InitControls()"/> function
        /// </summary>
        /// <param name="e">Contains additional information about the event</param>
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
        /// <param name="e">Contains additional information about the event</param>
        protected override void OnDataBound(EventArgs e)
        {
            base.OnDataBound(e);
            if (base.HeaderRow != null)
                base.HeaderRow.TableSection = TableRowSection.TableHeader;
        }

        /// <summary>
        /// Override the RowCreated function to show an icon next to the Header title when there's a sorting applied for that column
        /// </summary>
        /// <param name="e">Contains additional information about the event</param>
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
                                    var icon = BootstrapVersion >= 3
                                        ? "glyphicon glyphicon-arrow-up"
                                        : "icon-arrow-up";
                                    button.Text += " <i class=\"" + icon + "\"></i>";
                                    button.ToolTip += " - ASC Order";
                                }
                                else
                                {
                                    var icon = BootstrapVersion >= 3
                                        ? "glyphicon glyphicon-arrow-down"
                                        : "icon-arrow-down";
                                    button.Text += " <i class=\"" + icon + "\"></i>";
                                    button.ToolTip += " - DESC Order";
                                }

                                // Add the image to the appropriate header cell.
                                cell.Controls.Add(sortImage);
                                break;
                            }
                        }
                    }
                }
                //else if (e.Row.RowType == DataControlRowType.DataRow)
                //{
                //    e.Row.Attributes.Add("data-id", "vicent");
                //}
            }
        }

        /// <summary>
        /// Override the Sorting function to allow the multi column sorting feature
        /// </summary>
        /// <param name="e">Contains additional information about the event</param>
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
            var icon = BootstrapVersion >= 3
                ? "glyphicon glyphicon-resize-small"
                : "icon-resize-small";

            var btnClass = BootstrapVersion >= 3
                ? "btn btn-default btn-sm"
                : "btn";

            var btnCompactTable = new HtmlButton
            {
                ID = "btn" + ClientID + "CompactTable",
                InnerHtml = "<i class=\"" + icon + "\"></i>"
            };

            icon = BootstrapVersion >= 3
               ? "glyphicon glyphicon-resize-full"
               : "icon-resize-full";

            var btnExpandTable = new HtmlButton
            {
                ID = "btn" + ClientID + "ExpandTable",
                InnerHtml = "<i class=\"" + icon + "\"></i>"
            };

            btnCompactTable.Attributes.Add("type", "button");
            btnCompactTable.Attributes.Add("onclick", "GVEXCompactTable('" + ClientID + "', true, '" + btnExpandTable.ClientID + "', '" + btnCompactTable.ClientID + "');");
            btnCompactTable.Attributes.Add("title", "Compact Table");
            btnCompactTable.Attributes.Add("class", btnClass + " pull-right");
            btnCompactTable.Attributes.Add("style", "margin-right: 10px;");
            
            btnExpandTable.Attributes.Add("type", "button");
            btnExpandTable.Attributes.Add("onclick", "GVEXCompactTable('" + ClientID + "', false, '" + btnExpandTable.ClientID + "', '" + btnCompactTable.ClientID + "');");
            btnExpandTable.Attributes.Add("title", "Expand Table");
            btnExpandTable.Attributes.Add("class", btnClass + " hide pull-right");
            btnExpandTable.Attributes.Add("style", "margin-right: 10px;");

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

            icon = BootstrapVersion >= 3
               ? "glyphicon glyphicon-search"
               : "icon-search";

            // Render filter icon. Disable the filter function if no records and no filters applied
            if (Rows.Count == 0
                && Context.Session[ID + "_Filters"] == null)
                writer.Write("<button id=\"" + ClientID + "btnFilter\" type=\"button\" class=\"" + btnClass + " pull-right disabled\" title=\"Filter\"><i class=\"" + icon + "\"></i></button>");
            else
                writer.Write("<button id=\"" + ClientID + "btnFilter\" type=\"button\" class=\"" + btnClass + " pull-right\" title=\"Filter\" onclick=\"GVEXToggleInlineFilter('" + ClientID + "', this);\"><i class=\"" + icon + "\"></i></button>");

            // Render compact/expand icons
            if (IsCompactShown)
            {
                btnCompactTable.RenderControl(writer);
                btnExpandTable.RenderControl(writer);
            }

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

                // Render message alerts
                var cookie = Page.Request.Cookies[ClientID + "_AlertMessage"];
                if (cookie != null)
                {
                    writer.Write("<div id=\"" + ClientID + "AlertMessage\" class=\"alert " + (String.IsNullOrWhiteSpace(AlertMessageClass) ? "alert-success" : AlertMessageClass) + " fade in\"><button type=\"button\" class=\"close\" data-dismiss=\"alert\">×</button>");
                    writer.Write(HttpUtility.UrlDecode(cookie.Value));
                    writer.Write("</div>");
                }
                else
                    writer.Write("<div id=\"" + ClientID + "AlertMessage\" class=\"alert fade in hide\"><button type=\"button\" class=\"close\" data-dismiss=\"alert\">×</button></div>");

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

            writer.Write("</div><!-- .grid-view -->");

            var jsScript = new StringBuilder();
            jsScript.AppendLine("<script type='text/javascript'>");
            jsScript.AppendLine("var " + ClientID + "SizeCompact = " + IsCompact.ToString().ToLower() + ";");
            jsScript.AppendLine("var " + ClientID + "IsFilterShown = " + IsFilterShown.ToString().ToLower() + ";");
            jsScript.AppendLine("Sys.WebForms.PageRequestManager.getInstance().add_beginRequest(" + ClientID + "BeginRequestHandler);");
            jsScript.AppendLine("Sys.WebForms.PageRequestManager.getInstance().add_endRequest(" + ClientID + "EndRequestHandler);");
            jsScript.AppendLine(JSScript);
            jsScript.AppendLine("function " + ClientID + "BeginRequestHandler(sender, args) {");
            jsScript.AppendLine("$('#" + ClientID + "grid-view .overlay')");
            jsScript.AppendLine(".width($('#" + ClientID + @"grid-view').width())");
            jsScript.AppendLine(".height($('#" + ClientID + @"grid-view').height())");
            jsScript.AppendLine(".show();");
            jsScript.AppendLine(JSScriptBeginRequestHandler);
            jsScript.AppendLine(JSScriptBeginRequestHandlerDelayed);
            jsScript.AppendLine("}");
            jsScript.AppendLine("function " + ClientID + "EndRequestHandler(sender, args) {");
            jsScript.AppendLine("$('#" + ClientID + "grid-view .overlay').hide();");
            jsScript.AppendLine("if (" + ClientID + "SizeCompact)");
            jsScript.AppendLine("GVEXCompactTable('" + ClientID + "', true, '" + btnExpandTable.ClientID + "', '" + btnCompactTable.ClientID + "');");
            jsScript.AppendLine("if (" + ClientID + "IsFilterShown)");
            jsScript.AppendLine("GVEXToggleInlineFilter('" + ClientID + "', $('#" + ClientID + "btnFilter')[0], true);");
            jsScript.AppendLine("GVEXDeleteAlert('" + ClientID + "');");
            jsScript.AppendLine(JSScriptEndRequestHandler);
            jsScript.AppendLine(JSScriptEndRequestHandlerDelayed);
            jsScript.AppendLine("}");
            jsScript.AppendLine("$(document).ready(function () {");
            jsScript.AppendLine("if (" + ClientID + "SizeCompact)");
            jsScript.AppendLine("GVEXCompactTable('" + ClientID + "', true, '" + btnExpandTable.ClientID + "', '" + btnCompactTable.ClientID + "');");
            jsScript.AppendLine("if (" + ClientID + "IsFilterShown)");
            jsScript.AppendLine("GVEXToggleInlineFilter('" + ClientID + "', $('#" + ClientID + "btnFilter')[0], true);");
            jsScript.AppendLine("$('body').on('click', function (e) {");
            jsScript.AppendLine("$('[data-toggle=\\'popover\\']').each(function () {");
            jsScript.AppendLine("if (!$(this).is(e.target) && $(this).has(e.target).length === 0 && $('.popover').has(e.target).length === 0)");
            jsScript.AppendLine("$(this).popover('hide');");
            jsScript.AppendLine("});");
            jsScript.AppendLine("});");
            jsScript.AppendLine("var cookies = document.cookie.split(';');");
            jsScript.AppendLine("for (var i = 0; i < cookies.length; i++) {");
            jsScript.AppendLine("var equals = cookies[i].indexOf('=');");
            jsScript.AppendLine("var name = equals > -1");
            jsScript.AppendLine("? cookies[i].substr(0, equals)");
            jsScript.AppendLine(": cookies[i];");
            jsScript.AppendLine("if (name.indexOf('_ColumnsSelected') != -1)");
            jsScript.AppendLine("document.cookie = name + '=;expires=Thu, 01 Jan 1970 00:00:00 GMT';");
            jsScript.AppendLine("}");
            jsScript.AppendLine("GVEXDeleteAlert('" + ClientID + "');");
            jsScript.AppendLine("GVEXCreateTopScrollbar('" + ClientID + "');");
            jsScript.AppendLine(JSScriptDocumentReady);
            jsScript.AppendLine(JSScriptDocumentReadyDelayed);
            jsScript.AppendLine("});");
            jsScript.AppendLine("</script>");

            ScriptManager.RegisterStartupScript(this, this.GetType(), ClientID + "JSScript", jsScript.ToString(), false);
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
            // Reset the session when reload the grid unless ks is defined
            if (!Page.IsPostBack)
            {
                bool keepSession = false;
                if (Context.Request["ks"] != null)
                    bool.TryParse(Context.Request["ks"], out keepSession);

                bool defaultView = Convert.ToBoolean(Context.Session[ID + "_DefaultView"]);

                if ((!keepSession
                        && !defaultView)
                    || Context.Session[ID + "_SortExpressions"] == null)
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
                    var displayIndex = 0;
                    foreach (var column in Columns)
                    {
                        var gridViewExColumn = column as ColumnEx;
                        var gridViewExCheckBox = column as CheckBoxEx;
                        var boundColumn = column as BoundField;
                        var checkboxColumn = column as CheckBoxField;
                        var templateFieldColumn = column as TemplateField;

                        if (gridViewExColumn != null)
                        {
                            columns.Add(new ColumnExpression
                            {
                                ID = index,
                                Column = gridViewExColumn.DataField,
                                DisplayName = gridViewExColumn.HeaderText,
                                Visible = gridViewExColumn.Visible,
                                Type = "ColumnEx",
                                Index = index,
                                DisplayIndex = displayIndex,
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

                            displayIndex++;
                        }
                        else if (gridViewExCheckBox != null)
                        {
                            columns.Add(new ColumnExpression
                            {
                                ID = index,
                                Column = gridViewExCheckBox.DataField,
                                DisplayName = gridViewExCheckBox.HeaderText,
                                Visible = gridViewExCheckBox.Visible,
                                Type = "CheckBoxEx",
                                Index = index,
                                DisplayIndex = displayIndex,
                                DataFormat = DataFormatEnum.Text,
                                DataFormatExpression = String.Empty
                            });

                            // Add the display name into the sort expressions
                            if (sortExpressions != null)
                            {
                                var sortExpr = sortExpressions.SingleOrDefault(x => x.Column == gridViewExCheckBox.DataField);
                                if (sortExpr != null)
                                    sortExpr.DisplayName = gridViewExCheckBox.HeaderText;
                            }

                            displayIndex++;
                        }
                        else if (boundColumn != null)
                        {
                            columns.Add(new ColumnExpression
                            {
                                ID = index,
                                Column = boundColumn.DataField,
                                DisplayName = boundColumn.HeaderText,
                                Visible = boundColumn.Visible,
                                Type = "BoundField",
                                Index = index,
                                DisplayIndex = displayIndex,
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

                            displayIndex++;
                        }
                        else if (checkboxColumn != null)
                        {
                            columns.Add(new ColumnExpression
                            {
                                ID = index,
                                Column = checkboxColumn.DataField,
                                DisplayName = checkboxColumn.HeaderText,
                                Visible = checkboxColumn.Visible,
                                Type = "CheckBoxField",
                                Index = index,
                                DisplayIndex = displayIndex,
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

                            displayIndex++;
                        }

                        else if (templateFieldColumn != null)
                        {
                            columns.Add(new ColumnExpression
                            {
                                ID = index,
                                Column = null,
                                DisplayName = templateFieldColumn.HeaderText,
                                Visible = templateFieldColumn.Visible,
                                Type = "TemplateField",
                                Index = index,
                                DisplayIndex = displayIndex,
                                DataFormat = DataFormatEnum.Text,
                                DataFormatExpression = String.Empty
                            });
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
                                case "CheckBoxEx":
                                    var gridViewExCheckBox = column as CheckBoxEx;
                                    if (gridViewExCheckBox != null
                                        && gridViewExCheckBox.DataField == sessionColumn.Column)
                                    {
                                        gridViewExCheckBox.Visible = sessionColumn.Visible;

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
                                            var sortExpr = sortExpressions.SingleOrDefault(x => x.Column == gridViewExCheckBox.DataField);
                                            if (sortExpr != null)
                                                sortExpr.DisplayName = gridViewExCheckBox.HeaderText;
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
                        gridViewExColumn.DropDownDataSource = query.GetDropDownDataSource(gridViewExColumn.DataField, gridViewExColumn.DataFormat, gridViewExColumn.DataFormatExpression, CultureInfo);
                        // TODO: Check, we might have a problem with the currency symbol
                    }
                }

                query = query.Filter((List<FilterExpression>)Context.Session[ID + "_Filters"]); // Apply filters

                // Count total records
                Context.Session[ID + "_Records"] = query.Count();

                if (!customOrder)
                {
                    // Sort the rows
                    var sourceIQueryable = ((List<SortExpression>)Context.Session[ID + "_SortExpressions"]).Count > 0
                        ? (IQueryable<dynamic>)query.Order((List<SortExpression>)Context.Session[ID + "_SortExpressions"])
                        : (IQueryable<dynamic>)query.OrderBy(x => x);

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
        /// <summary>
        /// Create the export to excel control
        /// </summary>
        private Control CreateExportControl()
        {
            var phControl = new PlaceHolder();

            if (ExcelExport != null)
            {
                var icon = BootstrapVersion >= 3
                    ? "glyphicon glyphicon-download"
                    : "icon-download";

                var btnClass = BootstrapVersion >= 3
                    ? "btn btn-default btn-sm"
                    : "btn";

                var lbExport = new LinkButton
                {
                    ID = "lbExport",
                    CssClass = btnClass + " pull-right",
                    ToolTip = "Export to Excel",
                    Text = "<i class=\"" + icon + "\"></i>"
                };
                lbExport.Click += lbExport_Click;
                lbExport.Attributes.Add("style", "margin-right: 10px;");
                lbExport.Attributes.Add("onclick", "$(this).addClass('disabled').delay(3000).queue(function(next) { $(this).removeClass('disabled'); next(); });");

                // Disable the export function if no records to export
                if (Rows.Count == 0)
                {
                    lbExport.CssClass += " disabled";
                    lbExport.Enabled = false;
                }

                phControl.Controls.Add(lbExport);

                // In case user use UpdatePanel register a normal postback if not it fails
                ScriptManager.GetCurrent(Page).RegisterPostBackControl(lbExport);
            }

            return phControl;
        }

        /// <summary>
        /// Create the filter management control
        /// </summary>
        private Control CreateFilterManagementControl()
        {
            var filters = (List<FilterExpression>)Context.Session[ID + "_Filters"] != null
                ? (List<FilterExpression>)Context.Session[ID + "_Filters"]
                : new List<FilterExpression>();

            var phControl = new PlaceHolder();

            var icon = BootstrapVersion >= 3
                ? "glyphicon glyphicon-filter"
                : "icon-filter";

            var btnClass = BootstrapVersion >= 3
                ? "btn btn-default btn-sm"
                : "btn";

            var btnFilterSelection = new HtmlButton
            {
                ID = "btn" + ID + "FilterSelection",
                ClientIDMode = ClientIDMode.Static,
                InnerHtml = "<i class=\"" + icon + "\"></i>"
            };

            btnFilterSelection.Attributes.Add("type", "button");
            btnFilterSelection.Attributes.Add("title", "Filter Management");
            btnFilterSelection.Attributes.Add("class", btnClass + " pull-right");
            btnFilterSelection.Attributes.Add("style", "margin-right: 10px;");
            btnFilterSelection.Attributes.Add("data-toggle", "popover");
            if (filters.Count >= 1)
                //hlFilterSelection.Attributes["style"] += "background-color: #B2C6E0;background-image: linear-gradient(to bottom, #FFFFFF, #B2C6E0);";
                btnFilterSelection.Attributes["class"] += " active";

            phControl.Controls.Add(btnFilterSelection);

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
                icon = BootstrapVersion >= 3
                    ? "glyphicon glyphicon-remove"
                    : "icon-remove";

                foreach (var filterExpression in filters)
                {
                    var r = new Regex(@"
                    (?<=[A-Z])(?=[A-Z][a-z]) |
                    (?<=[^A-Z])(?=[A-Z]) |
                    (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

                    var liFilter = new HtmlGenericControl("li");

                    var lbRemove = new LinkButton
                    {
                        Text = "<i class=\"" + icon + "\"></i>",
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

            JSScriptEndRequestHandler += @"GVEXPopover('#" + btnFilterSelection.ClientID + "','#" + divFilterSelection.ClientID + "');";
            JSScriptDocumentReady += @"GVEXPopover('#" + btnFilterSelection.ClientID + "','#" + divFilterSelection.ClientID + "');";

            return phControl;
        }

        /// <summary>
        /// Create the sorting management control
        /// </summary>
        private Control CreateSortingManagementControl()
        {
            var sortings = (List<SortExpression>)Context.Session[ID + "_SortExpressions"] != null
                ? (List<SortExpression>)Context.Session[ID + "_SortExpressions"]
                : new List<SortExpression>();

            var phControl = new PlaceHolder();

            var icon = BootstrapVersion >= 3
                ? "glyphicon glyphicon-list-alt"
                : "icon-list-alt";

            var btnClass = BootstrapVersion >= 3
                ? "btn btn-default btn-sm"
                : "btn";

            var btnSortSelection = new HtmlButton
            {
                ID = "btn" + ID + "SortSelection",
                ClientIDMode = ClientIDMode.Static,
                InnerHtml = "<i class=\"" + icon + "\"></i>",
            };
            btnSortSelection.Attributes.Add("type", "button");
            btnSortSelection.Attributes.Add("title", "Sorting Management");
            btnSortSelection.Attributes.Add("class", btnClass + " pull-right");
            btnSortSelection.Attributes.Add("style", "margin-right: 10px;");
            btnSortSelection.Attributes.Add("data-toggle", "popover");
            if (sortings.Count >= 1)
                //hlSortSelection.Attributes["style"] += "background-color: #B2C6E0;background-image: linear-gradient(to bottom, #FFFFFF, #B2C6E0);";
                btnSortSelection.Attributes["class"] += " active";

            phControl.Controls.Add(btnSortSelection);

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

                    icon = BootstrapVersion >= 3
                        ? "glyphicon glyphicon-remove"
                        : "icon-remove";

                    var lbRemove = new LinkButton
                    {
                        Text = "<i class=\"" + icon + "\"></i>",
                        ToolTip = "Remove",
                        CommandArgument = sortExpression.Column
                    };
                    lbRemove.Click += lbSortingRemove_Click;
                    liSort.Controls.Add(lbRemove);

                    icon = BootstrapVersion >= 3
                        ? "glyphicon glyphicon-arrow-up"
                        : "icon-arrow-up";

                    var lbChangeIndexUp = new LinkButton
                    {
                        Text = "<i class=\"" + icon + "\"></i>",
                        ToolTip = "Up",
                        CommandArgument = sortExpression.Column,
                        Visible = sortings.Count > 1
                            && sortings.IndexOf(sortExpression) != 0
                    };
                    lbChangeIndexUp.Click += lbSortingChangeIndexUp_Click;
                    liSort.Controls.Add(lbChangeIndexUp);

                    icon = BootstrapVersion >= 3
                        ? "glyphicon glyphicon-arrow-down"
                        : "icon-arrow-down";

                    var lbChangeIndexDown = new LinkButton
                    {
                        Text = "<i class=\"" + icon + "\"></i>",
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

            JSScriptEndRequestHandler += @"GVEXPopover('#" + btnSortSelection.ClientID + "','#" + divSortSelection.ClientID + "');";
            JSScriptDocumentReady += @"GVEXPopover('#" + btnSortSelection.ClientID + "','#" + divSortSelection.ClientID + "');";

            return phControl;
        }

        /// <summary>
        /// Create the column management control
        /// </summary>
        private Control CreateColumnManagementControl()
        {
            var phControl = new PlaceHolder();

            if (ColumnSelectionChanged != null)
            {
                var columns = (List<ColumnExpression>)Context.Session[ID + "_Columns"] != null
                    ? (List<ColumnExpression>)Context.Session[ID + "_Columns"]
                    : new List<ColumnExpression>();

                columns = columns.Where(x => x.Type != "TemplateField").ToList();

                var icon = BootstrapVersion >= 3
                    ? "glyphicon glyphicon-calendar"
                    : "icon-calendar";

                var btnClass = BootstrapVersion >= 3
                    ? "btn btn-default btn-sm"
                    : "btn";

                var btnColumnSelection = new HtmlButton
                {
                    ID = "btn" + ID + "ColumnSelection",
                    ClientIDMode = ClientIDMode.Static,
                    InnerHtml = "<i class=\"" + icon + "\"></i>"
                };
                btnColumnSelection.Attributes.Add("type", "button");
                btnColumnSelection.Attributes.Add("title", "Column Management");
                btnColumnSelection.Attributes.Add("class", btnClass + " pull-right");
                btnColumnSelection.Attributes.Add("style", "margin-right: 10px;");
                btnColumnSelection.Attributes.Add("data-toggle", "popover");
                phControl.Controls.Add(btnColumnSelection);

                var js = new JavaScriptSerializer();

                var hfColumnsSelected = new HiddenField
                {
                    ID = "hf" + ID + "ColumnsSelected",
                    ClientIDMode = ClientIDMode.Static,
                    Value = js.Serialize(columns.Select(x => new
                    {
                        ID = x.ID,
                        V = Convert.ToInt32(x.Visible),
                        I = x.DisplayIndex
                    }))
                };
                phControl.Controls.Add(hfColumnsSelected);

                var divColumnSelection = new HtmlGenericControl("div");
                divColumnSelection.ID = "div" + ID + "ColumnSelection";
                divColumnSelection.ClientIDMode = ClientIDMode.Static;
                divColumnSelection.Attributes.Add("style", "display: none;");

                var btnHideAll = new HtmlButton
                {
                    ID = "btn" + ID + "HideAll",
                    ClientIDMode = ClientIDMode.Static,
                    InnerText = "Hide All",
                    Visible = columns.Count >= 1
                };
                btnHideAll.Attributes.Add("type", "button");
                btnHideAll.Attributes.Add("class", "btn btn-link");
                btnHideAll.Attributes.Add("title", "Hide all columns");

                var btnShowAll = new HtmlButton
                {
                    ID = "hl" + ID + "ShowAll",
                    ClientIDMode = ClientIDMode.Static,
                    InnerText = "Show All",
                    Visible = columns.Count >= 1
                };
                btnShowAll.Attributes.Add("type", "button");
                btnShowAll.Attributes.Add("class", "btn btn-link");
                btnShowAll.Attributes.Add("title", "Show all columns");

                var lbApply = new LinkButton
                {
                    ID = "lb" + ID + "Apply",
                    ClientIDMode = ClientIDMode.Static,
                    Text = "Apply",
                    ToolTip = "Apply changes",
                    Visible = columns.Count >= 1
                };
                lbApply.Attributes.Add("class", "btn btn-link");
                lbApply.Attributes.Add("style", "display: none;float: right;");
                lbApply.Click += lbColumnApply_Click;

                btnHideAll.Attributes.Add("onclick", "GVEXColumnSelectionShowAll('" + ClientID + "', this, false, '#" + hfColumnsSelected.ClientID + "', '#" + lbApply.ClientID + "');$('#" + btnHideAll.ClientID + @", #" + btnShowAll.ClientID + @"').toggle();");
                btnShowAll.Attributes.Add("onclick", "GVEXColumnSelectionShowAll('" + ClientID + "', this, true, '#" + hfColumnsSelected.ClientID + "', '#" + lbApply.ClientID + "');$('#" + btnHideAll.ClientID + @", #" + btnShowAll.ClientID + @"').toggle();");

                var isAllVisible = true;
                foreach (var column in columns)
                    if (!column.Visible)
                    {
                        isAllVisible = false;
                        break;
                    }

                if (isAllVisible)
                    btnShowAll.Attributes.Add("style", "display: none;");
                else
                    btnHideAll.Attributes.Add("style", "display: none;");

                divColumnSelection.Controls.Add(btnHideAll);
                divColumnSelection.Controls.Add(btnShowAll);
                divColumnSelection.Controls.Add(lbApply);

                if (columns.Count >= 1)
                {
                    var ol = new HtmlGenericControl("ol");

                    for (int i = 0; i < columns.Count(); i++)
                    {
                        var column = columns[i];
                        var liColumn = new HtmlGenericControl("li");

                        var cbVisible = new CheckBox
                        {
                            Checked = column.Visible,
                            ID = "cb" + ID + "ColumnVisible" + i,
                            ClientIDMode = ClientIDMode.Static,
                        };
                        cbVisible.InputAttributes.Add("title", "Check to make it visible");
                        cbVisible.InputAttributes.Add("onclick", "GVEXColumnSelectionChanged('" + ClientID + "', this, '#" + hfColumnsSelected.ClientID + "', '#" + lbApply.ClientID + "');");
                        cbVisible.InputAttributes.Add("data-field", column.ID.ToString());
                        cbVisible.InputAttributes.Add("data-index", column.DisplayIndex.ToString());
                        liColumn.Controls.Add(cbVisible);

                        icon = BootstrapVersion >= 3
                            ? "glyphicon glyphicon-arrow-up"
                            : "icon-arrow-up";

                        var lbChangeIndexUp = new HtmlAnchor
                        {
                            InnerHtml = "<i class=\"" + icon + "\"></i>",
                            Title = "Up"
                        };

                        if (columns.Count <= 1
                            || columns.IndexOf(column) == 0)
                            lbChangeIndexUp.Attributes.Add("style", "visibility: hidden;");

                        lbChangeIndexUp.Attributes.Add("data-action", "up");
                        lbChangeIndexUp.Attributes.Add("onclick", "GVEXColumnIndexChanged('" + ClientID + "', this, parseInt($(this).parent().children().closest('input:checkbox').attr('data-index')) - 1, '#" + hfColumnsSelected.ClientID + "', '#" + lbApply.ClientID + "');");
                        liColumn.Controls.Add(lbChangeIndexUp);

                        icon = BootstrapVersion >= 3
                            ? "glyphicon glyphicon-arrow-down"
                            : "icon-arrow-down";

                        var lbChangeIndexDown = new HtmlAnchor
                        {
                            InnerHtml = "<i class=\"" + icon + "\"></i>",
                            Title = "Down"
                        };
                        if (columns.Count <= 1
                            || columns.IndexOf(column) == columns.Count - 1)
                            lbChangeIndexDown.Attributes.Add("style", "visibility: hidden;");

                        lbChangeIndexDown.Attributes.Add("data-action", "down");
                        lbChangeIndexDown.Attributes.Add("onclick", "GVEXColumnIndexChanged('" + ClientID + "', this, parseInt($(this).parent().children().closest('input:checkbox').attr('data-index')) + 1, '#" + hfColumnsSelected.ClientID + "', '#" + lbApply.ClientID + "');");
                        liColumn.Controls.Add(lbChangeIndexDown);

                        liColumn.Controls.Add(new Literal { Text = column.DisplayName });

                        ol.Controls.Add(liColumn);
                    }

                    divColumnSelection.Controls.Add(ol);
                }
                else
                    divColumnSelection.Controls.Add(new Label { Text = "No columns" });

                phControl.Controls.Add(divColumnSelection);

                JSScriptEndRequestHandler += @"GVEXPopover('#" + btnColumnSelection.ClientID + "', '#" + divColumnSelection.ClientID + "');";
                JSScriptDocumentReady += @"GVEXPopover('#" + btnColumnSelection.ClientID + "', '#" + divColumnSelection.ClientID + "');";
            }

            return phControl;
        }

        /// <summary>
        /// Create the view management control
        /// </summary>
        private Control CreateViewManagementControl()
        {
            var phControl = new PlaceHolder();

            var btnClass = BootstrapVersion >= 3
                ? "btn btn-default btn-sm"
                : "btn";

            if (ViewChanged != null)
            {
                var views = (List<ViewExpression>)Context.Session[ID + "_ViewExpressions"] != null
                    ? (List<ViewExpression>)Context.Session[ID + "_ViewExpressions"]
                    : new List<ViewExpression>();
                views = views.OrderBy(x => x.Name).ToList();

                if (views.Count >= 1)
                {
                    var inputAppendClass = BootstrapVersion >= 3
                        ? "input-group input-group-sm"
                        : "input-append";

                    var divAppend = new HtmlGenericControl("div");
                    divAppend.ID = "divViewExpressionList";
                    divAppend.ClientIDMode = ClientIDMode.Static;
                    divAppend.Attributes.Add("class", "pull-right " + inputAppendClass);
                    divAppend.Attributes.Add("style", "margin-right: 10px;");

                    var ddlViews = new DropDownList
                    {
                        ID = "ddl" + ID + "ViewExpressionList",
                        ClientIDMode = ClientIDMode.Static,
                        AutoPostBack = true,
                        CssClass = BootstrapVersion >= 3
                            ? "form-control col-md-1"
                            : "span1"
                    };
                    ddlViews.SelectedIndexChanged += ddlViews_SelectedIndexChanged;

                    ddlViews.Items.Add(new ListItem("", "-1"));
                    ddlViews.Items.AddRange(views.Select(x => new ListItem
                    {
                        Text = x.Name + (x.DefaultView ? " *" : ""),
                        Value = x.ID.ToString()
                    }).ToArray());

                    divAppend.Controls.Add(ddlViews);

                    if (BootstrapVersion >= 3)
                    {
                        var spanViewExpressoion = new HtmlGenericControl("span");
                        spanViewExpressoion.Attributes.Add("class", "input-group-btn");

                        var hlViewExpression = new HtmlAnchor
                        {
                            ID = "hl" + ID + "ViewExpression",
                            ClientIDMode = ClientIDMode.Static,
                            HRef = "#div" + ID + "ViewExpression",
                            Title = "View Management",
                            InnerHtml = "<i class=\"glyphicon glyphicon-eye-open\"></i>",
                        };
                        hlViewExpression.Attributes.Add("data-toggle", "modal");
                        hlViewExpression.Attributes.Add("role", "button");
                        hlViewExpression.Attributes.Add("style", "display: inline-block;");
                        hlViewExpression.Attributes.Add("class", btnClass);

                        spanViewExpressoion.Controls.Add(hlViewExpression);
                        divAppend.Controls.Add(spanViewExpressoion);
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
                        hlViewExpression.Attributes.Add("style", "display: inline-block;");
                        hlViewExpression.Attributes.Add("class", btnClass + " add-on");

                        divAppend.Controls.Add(hlViewExpression);
                    }

                    phControl.Controls.Add(divAppend);
                }
                else
                {
                    var icon = BootstrapVersion >= 3
                        ? "glyphicon glyphicon-eye-open"
                        : "icon-eye-open";

                    var hlViewExpression = new HtmlAnchor
                    {
                        ID = "hl" + ID + "ViewExpression",
                        ClientIDMode = ClientIDMode.Static,
                        HRef = "#div" + ID + "ViewExpression",
                        Title = "View Management",
                        InnerHtml = "<i class=\"" + icon + "\"></i>",
                    };
                    hlViewExpression.Attributes.Add("data-toggle", "modal");
                    hlViewExpression.Attributes.Add("role", "button");
                    hlViewExpression.Attributes.Add("style", "display: inline-block;margin-right: 10px;");
                    hlViewExpression.Attributes.Add("class", btnClass + " pull-right");

                    phControl.Controls.Add(hlViewExpression);
                }

                var divViewExpression = new HtmlGenericControl("div");
                divViewExpression.ID = "div" + ID + "ViewExpression";
                divViewExpression.ClientIDMode = ClientIDMode.Static;
                divViewExpression.Attributes.Add("class", BootstrapVersion >= 3 ? "modal fade" : "modal hide fade");
                divViewExpression.Attributes.Add("tabindex", "-1");
                divViewExpression.Attributes.Add("role", "dialog");
                //divViewExpression.Attributes.Add("aria-labelledby", "h3" + ID + "ViewExpressionLabel");
                divViewExpression.Attributes.Add("aria-hidden", "true");

                var divViewExpressionModalDialog = new HtmlGenericControl("div");
                divViewExpressionModalDialog.Attributes.Add("class", "modal-dialog");

                var divViewExpressionModalContent = new HtmlGenericControl("div");
                divViewExpressionModalContent.Attributes.Add("class", "modal-content");
                divViewExpressionModalDialog.Controls.Add(divViewExpressionModalContent);

                /* START MODAL HEADER */
                var divModalHeader = new HtmlGenericControl("div");
                divModalHeader.Attributes.Add("class", "modal-header");

                var btnCloseModal = new HtmlButton { InnerText = "x" };
                btnCloseModal.Attributes.Add("type", "button");
                btnCloseModal.Attributes.Add("class", "close");
                btnCloseModal.Attributes.Add("data-dismiss", "modal");
                btnCloseModal.Attributes.Add("aria-hidden", "true");
                divModalHeader.Controls.Add(btnCloseModal);

                if (BootstrapVersion >= 3)
                {
                    var h4ModalLabel = new HtmlGenericControl("h4");
                    h4ModalLabel.ID = "h4" + ID + "ViewExpressionLabel";
                    h4ModalLabel.ClientIDMode = ClientIDMode.Static;
                    h4ModalLabel.Attributes.Add("class", "modal-title");
                    h4ModalLabel.InnerText = "View Management";
                    divModalHeader.Controls.Add(h4ModalLabel);
                }
                else
                {
                    var h3ModalLabel = new HtmlGenericControl("h3");
                    h3ModalLabel.ID = "h3" + ID + "ViewExpressionLabel";
                    h3ModalLabel.ClientIDMode = ClientIDMode.Static;
                    h3ModalLabel.InnerText = "View Management";
                    divModalHeader.Controls.Add(h3ModalLabel);
                }

                if (BootstrapVersion >= 3)
                    divViewExpressionModalContent.Controls.Add(divModalHeader);
                else
                    divViewExpression.Controls.Add(divModalHeader);
                /* END MODAL HEADER */

                /* START MODAL BODY */
                var divModalBody = new HtmlGenericControl("div");
                divModalBody.Attributes.Add("class", "modal-body");

                var divModalBodyRow = new HtmlGenericControl("div");
                divModalBodyRow.Attributes.Add("class", BootstrapVersion >= 3
                    ? "row"
                    : "row-fluid");

                var divModalBodyRowSpan = new HtmlGenericControl("div");
                divModalBodyRowSpan.Attributes.Add("class", BootstrapVersion >= 3
                    ? "col-md-6"
                    : "span12");

                var txtViewName = new TextBox
                {
                    ID = "txt" + ID + "ViewExpressionTitle",
                    CssClass = BootstrapVersion >= 3 ? "form-control" : "",
                    ClientIDMode = ClientIDMode.Static
                };
                txtViewName.Attributes.Add("placeholder", "View name...");
                divModalBodyRowSpan.Controls.Add(txtViewName);

                divModalBodyRow.Controls.Add(divModalBodyRowSpan);
                divModalBody.Controls.Add(divModalBodyRow);

                var divModalBodyRow2 = new HtmlGenericControl("div");
                divModalBodyRow2.Attributes.Add("class", BootstrapVersion >= 3
                    ? "row"
                    : "row-fluid");

                var divModalBodyRowSpan1 = new HtmlGenericControl("div");
                divModalBodyRowSpan1.Attributes.Add("class", BootstrapVersion >= 3
                    ? "col-md-6"
                    : "span6");

                var divModalBodyRowSpan1Cb = new HtmlGenericControl("div");
                divModalBodyRowSpan1Cb.ID = "div" + ID + "ViewExpressionCheckBoxes";
                divModalBodyRowSpan1Cb.ClientIDMode = ClientIDMode.Static;

                if (BootstrapVersion >= 3)
                {
                    var divSaveFilterLabel = new HtmlGenericControl("div");
                    var cbSaveFilterLabel = new HtmlGenericControl("label");
                    divSaveFilterLabel.Attributes.Add("class", "checkbox");
                    cbSaveFilterLabel.Controls.Add(new CheckBox
                    {
                        Checked = true,
                        ID = "cb" + ID + "ViewExpressionFilters",
                        ClientIDMode = ClientIDMode.Static
                    });
                    cbSaveFilterLabel.Controls.Add(new Literal { Text = " Save Filters" });
                    divSaveFilterLabel.Controls.Add(cbSaveFilterLabel);
                    divModalBodyRowSpan1Cb.Controls.Add(divSaveFilterLabel);
                }
                else
                {
                    var cbSaveFilterLabel = new HtmlGenericControl("label");
                    cbSaveFilterLabel.Attributes.Add("class", "checkbox");
                    cbSaveFilterLabel.Controls.Add(new CheckBox
                    {
                        Checked = true,
                        ID = "cb" + ID + "ViewExpressionFilters",
                        ClientIDMode = ClientIDMode.Static
                    });
                    cbSaveFilterLabel.Controls.Add(new Literal { Text = " Save Filters" });
                    divModalBodyRowSpan1Cb.Controls.Add(cbSaveFilterLabel);
                }

                if (BootstrapVersion >= 3)
                {
                    var divSaveSortingLabel = new HtmlGenericControl("div");
                    var cbSaveSortingLabel = new HtmlGenericControl("label");
                    divSaveSortingLabel.Attributes.Add("class", "checkbox");
                    cbSaveSortingLabel.Controls.Add(new CheckBox
                    {
                        Checked = true,
                        ID = "cb" + ID + "ViewExpressionSortings"
                    });
                    cbSaveSortingLabel.Controls.Add(new Literal { Text = " Save Sortings" });
                    divSaveSortingLabel.Controls.Add(cbSaveSortingLabel);
                    divModalBodyRowSpan1Cb.Controls.Add(divSaveSortingLabel);
                }
                else
                {
                    var cbSaveSortingLabel = new HtmlGenericControl("label");
                    cbSaveSortingLabel.Attributes.Add("class", "checkbox");
                    cbSaveSortingLabel.Controls.Add(new CheckBox
                    {
                        Checked = true,
                        ID = "cb" + ID + "ViewExpressionSortings"
                    });
                    cbSaveSortingLabel.Controls.Add(new Literal { Text = " Save Sortings" });
                    divModalBodyRowSpan1Cb.Controls.Add(cbSaveSortingLabel);
                }

                if (BootstrapVersion >= 3)
                {
                    var divSaveColumnLabel = new HtmlGenericControl("div");
                    var cbSaveColumnLabel = new HtmlGenericControl("label");
                    divSaveColumnLabel.Attributes.Add("class", "checkbox");
                    cbSaveColumnLabel.Controls.Add(new CheckBox
                    {
                        Checked = true,
                        ID = "cb" + ID + "ViewExpressionColumns"
                    });
                    cbSaveColumnLabel.Controls.Add(new Literal { Text = " Save Columns" });
                    divSaveColumnLabel.Controls.Add(cbSaveColumnLabel);
                    divModalBodyRowSpan1Cb.Controls.Add(divSaveColumnLabel);
                }
                else
                {
                    var cbSaveColumnLabel = new HtmlGenericControl("label");
                    cbSaveColumnLabel.Attributes.Add("class", "checkbox");
                    cbSaveColumnLabel.Controls.Add(new CheckBox
                    {
                        Checked = true,
                        ID = "cb" + ID + "ViewExpressionColumns"
                    });
                    cbSaveColumnLabel.Controls.Add(new Literal { Text = " Save Columns" });
                    divModalBodyRowSpan1Cb.Controls.Add(cbSaveColumnLabel);
                }

                if (BootstrapVersion >= 3)
                {
                    var divSavePageSizeLabel = new HtmlGenericControl("div");
                    var cbSavePageSizeLabel = new HtmlGenericControl("label");
                    divSavePageSizeLabel.Attributes.Add("class", "checkbox");
                    cbSavePageSizeLabel.Controls.Add(new CheckBox
                    {
                        Checked = false,
                        ID = "cb" + ID + "ViewExpressionPageSize"
                    });
                    cbSavePageSizeLabel.Controls.Add(new Literal { Text = " Save Page Size" });
                    divSavePageSizeLabel.Controls.Add(cbSavePageSizeLabel);
                    divModalBodyRowSpan1Cb.Controls.Add(divSavePageSizeLabel);
                }
                else
                {
                    var cbSavePageSizeLabel = new HtmlGenericControl("label");
                    cbSavePageSizeLabel.Attributes.Add("class", "checkbox");
                    cbSavePageSizeLabel.Controls.Add(new CheckBox
                    {
                        Checked = false,
                        ID = "cb" + ID + "ViewExpressionPageSize"
                    });
                    cbSavePageSizeLabel.Controls.Add(new Literal { Text = " Save Page Size" });
                    divModalBodyRowSpan1Cb.Controls.Add(cbSavePageSizeLabel);
                }

                if (BootstrapVersion >= 3)
                {
                    var divDefaultViewLabel = new HtmlGenericControl("div");
                    var cbDefaultViewLabel = new HtmlGenericControl("label");
                    divDefaultViewLabel.Attributes.Add("class", "checkbox");
                    cbDefaultViewLabel.Controls.Add(new CheckBox
                    {
                        Checked = false,
                        ID = "cb" + ID + "ViewExpressionDefaultView"
                    });
                    cbDefaultViewLabel.Controls.Add(new Literal { Text = " Make Default View" });
                    divDefaultViewLabel.Controls.Add(cbDefaultViewLabel);
                    divModalBodyRowSpan1Cb.Controls.Add(divDefaultViewLabel);
                }
                else
                {
                    var cbDefaultViewLabel = new HtmlGenericControl("label");
                    cbDefaultViewLabel.Attributes.Add("class", "checkbox");
                    cbDefaultViewLabel.Controls.Add(new CheckBox
                    {
                        Checked = false,
                        ID = "cb" + ID + "ViewExpressionDefaultView"
                    });
                    cbDefaultViewLabel.Controls.Add(new Literal { Text = " Make Default View" });
                    divModalBodyRowSpan1Cb.Controls.Add(cbDefaultViewLabel);
                }

                divModalBodyRowSpan1.Controls.Add(divModalBodyRowSpan1Cb);

                /* START ALERT */
                var alertErrorClass = BootstrapVersion >= 3
                    ? "alert-danger"
                    : "alert-error";

                var divViewExpressionAlert = new HtmlGenericControl("div");
                divViewExpressionAlert.ID = "div" + ID + "ViewExpressionAlert";
                divViewExpressionAlert.ClientIDMode = ClientIDMode.Static;
                divViewExpressionAlert.Attributes.Add("class", "alert " + alertErrorClass + " fade in hide");
                divViewExpressionAlert.Attributes.Add("style", "font-size: 14px;line-height: 20px;");

                var btnViewExpressionAlert = new HtmlButton { InnerText = "x" };
                btnViewExpressionAlert.Attributes.Add("class", "close");
                btnViewExpressionAlert.Attributes.Add("data-dismiss", "alert");
                divViewExpressionAlert.Controls.Add(btnViewExpressionAlert);

                var divViewExpressionAlertText = new HtmlGenericControl("div");
                divViewExpressionAlert.Controls.Add(divViewExpressionAlertText);
                divModalBodyRowSpan1.Controls.Add(divViewExpressionAlert);
                /* END ALERT */

                divModalBodyRow2.Controls.Add(divModalBodyRowSpan1);

                var divModalBodyRowSpan2 = new HtmlGenericControl("div");
                divModalBodyRowSpan2.Attributes.Add("class", BootstrapVersion >= 3
                    ? "col-md-6"
                    : "span6");

                var divSavedFilters = new HtmlGenericControl("div");
                divSavedFilters.Attributes.Add("style", "overflow-y:auto; height:200px; padding:5px; border:1px solid whiteSmoke; font-size:14px; line-height:20px;");

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

                        //var lbMakeDefault = new LinkButton
                        //{
                        //    Text = "<i class=\"icon-ok-circle\"></i>",
                        //    ToolTip = "Default View"
                        //};
                        //li.Controls.Add(lbMakeDefault);

                        //var cbVisible = new CheckBox();
                        ////cbVisible.Attributes.Add("onclick", "ColumnSelectionChanged(this);");
                        //cbVisible.Attributes.Add("data-index", view.ID.ToString());

                        //li.Controls.Add(cbVisible);
                        var litViewName = new Literal { Text = " " + view.Name };
                        if (view.DefaultView)
                        {
                            var icon = BootstrapVersion >= 3
                                ? "glyphicon glyphicon-asterisk"
                                : "icon-asterisk";

                            litViewName.Text += " <i class=\"" + icon + "\"></i>";
                            li.Attributes.Add("title", "Default View");
                            li.Attributes.Add("style", "font-weight: bold;");
                        }

                        li.Controls.Add(litViewName);
                        ol.Controls.Add(li);
                    }

                    divSavedFilters.Controls.Add(ol);
                }
                else
                    divSavedFilters.Controls.Add(new Label { Text = "No views" });


                divModalBodyRowSpan2.Controls.Add(divSavedFilters);
                divModalBodyRow2.Controls.Add(divModalBodyRowSpan2);

                divModalBody.Controls.Add(divModalBodyRow2);

                if (BootstrapVersion >= 3)
                    divViewExpressionModalContent.Controls.Add(divModalBody);
                else
                    divViewExpression.Controls.Add(divModalBody);
                /* END MODAL BODY */

                /* START MODAL FOOTER */
                var divModalFooter = new HtmlGenericControl("div");
                divModalFooter.Attributes.Add("class", "modal-footer");

                var btnCloseModalFooter = new HtmlButton { InnerText = "Close" };
                btnCloseModalFooter.Attributes.Add("class", btnClass);
                btnCloseModalFooter.Attributes.Add("data-dismiss", "modal");
                btnCloseModalFooter.Attributes.Add("aria-hidden", "true");
                divModalFooter.Controls.Add(btnCloseModalFooter);

                var btnSaveViewManagement = new LinkButton { Text = "Save" };
                btnSaveViewManagement.Click += btnSaveViewManagement_Click;
                btnSaveViewManagement.Attributes.Add("class", "btn btn-primary");
                btnSaveViewManagement.Attributes.Add("onclick", "return GVEXSaveView('" + ClientID + "', '#" + divViewExpression.ClientID + "', '#" + txtViewName.ClientID + "', '#" + divModalBodyRowSpan1Cb.ClientID + "', '#" + divViewExpressionAlert.ClientID + "');");
                divModalFooter.Controls.Add(btnSaveViewManagement);

                if (BootstrapVersion >= 3)
                    divViewExpressionModalContent.Controls.Add(divModalFooter);
                else
                    divViewExpression.Controls.Add(divModalFooter);
                /* END MODAL FOOTER */

                if (BootstrapVersion >= 3)
                    divViewExpression.Controls.Add(divViewExpressionModalDialog);
                
                phControl.Controls.Add(divViewExpression);
            }

            return phControl;
        }

        /// <summary>
        /// Create the pager control
        /// </summary>
        private Control CreatePagerControl()
        {
            int pageIndex = (int)Context.Session[ID + "_PageIndex"];
            int pageSize = (int)Context.Session[ID + "_PageSize"];
            int records = (int)Context.Session[ID + "_Records"];

            var divControl = new HtmlGenericControl("div");
            var ul = new HtmlGenericControl("ul");

            if (BootstrapVersion >= 3)
            {
                ul.Attributes.Add("class", "pagination pagination-sm");
                divControl.Attributes.Add("class", "pagination-div");
            }
            else
                divControl.Attributes.Add("class", "pagination pagination-centered");

            if (records > pageSize)
            {
                var pages = Extensions.FillPager(records, pageIndex, pageSize, BootstrapVersion);
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

                if (BootstrapVersion < 3)
                {
                    var lblPageRecords = new Label
                    {
                        Text = "Nº Records: ",
                        AssociatedControlID = "ddlPageRecords",
                        ToolTip = "Select number of records to display per page"
                    };
                    divRecordsSelector.Controls.Add(lblPageRecords);
                }

                var ddlPageRecords = new DropDownList
                {
                    ID = "ddlPageRecords",
                    AutoPostBack = true,
                    CssClass = BootstrapVersion >= 3
                        ? "form-control col-md-1"
                        : "span1",
                    ToolTip = "Select number of records to display per page"
                };
                ddlPageRecords.SelectedIndexChanged += ddlPageRecords_SelectedIndexChanged;
                ddlPageRecords.FillPageRecordsSelector(PagerSelectorOptions.Split(','), pageSize, records);
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
                var cookie = Page.Request.Cookies[ClientID + "_ColumnsSelected"];
                if (cookie != null)
                {
                    var js = new JavaScriptSerializer();

                    var columnSelections = js.Deserialize<List<ColumnExpressionCookie>>(HttpUtility.UrlDecode(cookie.Value));
                    var oldColumnSelections = Context.Session[ID + "_Columns"] as List<ColumnExpression>;
                    foreach (var oldColumnSelection in oldColumnSelections)
                    {
                        var columnSelection = columnSelections.SingleOrDefault(x => x.ID == oldColumnSelection.ID);
                        if (columnSelection != null)
                        {
                            oldColumnSelection.Visible = Convert.ToBoolean(columnSelection.V);
                            oldColumnSelection.Index = columnSelection.I + (oldColumnSelection.Index - oldColumnSelection.DisplayIndex);
                            oldColumnSelection.DisplayIndex = columnSelection.I;
                        }
                    }

                    Context.Session[ID + "_Columns"] = oldColumnSelections.OrderBy(x => x.Index).ToList();
                }
                else
                {
                    Page.Response.Cookies.Add(new HttpCookie(ClientID + "_AlertMessage")
                    {
                        Value = "Cookie not found",
                        Path = "",
                        Expires = DateTime.Now.AddMinutes(5)
                    });

                    AlertMessageClass = BootstrapVersion >= 3 
                        ? "alert-danger"
                        : "alert-error";
                }

                if (ColumnSelectionChanged != null)
                    ColumnSelectionChanged(null, EventArgs.Empty);
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
                var cbDefaultView = btn.NamingContainer.FindControl("cb" + ID + "ViewExpressionDefaultView") as CheckBox;
                if (txtName != null
                    && cbFilters != null
                    && cbSortings != null
                    && cbColumns != null
                    && cbPageSize != null
                    && cbDefaultView != null)
                {
                    var view = new ViewExpression
                    {
                        ColumnExpressions = cbColumns.Checked ? Context.Session[ID + "_Columns"] as List<ColumnExpression> : null,
                        FilterExpressions = cbFilters.Checked ? Context.Session[ID + "_Filters"] as List<FilterExpression> : null,
                        SortExpressions = cbSortings.Checked ? Context.Session[ID + "_SortExpressions"] as List<SortExpression> : null,
                        Name = txtName.Text,
                        PageSize = cbPageSize.Checked ? Convert.ToInt32(Context.Session[ID + "_PageSize"]) : 0,
                        DefaultView = cbDefaultView.Checked
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