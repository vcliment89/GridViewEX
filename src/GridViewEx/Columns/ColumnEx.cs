using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace GridViewEx.Columns
{
    /// <summary>
    /// Extended column used by GridViewEx
    /// </summary>
    /// <remarks>
    /// [{"Author": "Vicent Climent";
    /// "Created Date": "08/03/2013"}]
    /// </remarks>
    public class ColumnEx : DataControlField
    {
        #region VARIABLES
        /// <summary>
        /// Stores the header tooltip. If not defined then use the HeaderText
        /// </summary>
        public string HeaderToolTip { get; set; }

        /// <summary>
        /// Stores the search type. None by default
        /// </summary>
        public SearchTypeEnum SearchType { get; set; }

        /// <summary>
        /// Stores the data format. Text by default
        /// </summary>
        public DataFormatEnum DataFormat { get; set; }

        /// <summary>
        /// Stores the data format expression. If DataFormat is Expression then you need to pass an expression here
        /// </summary>
        public string DataFormatExpression { get; set; }

        /// <summary>
        /// Stores the text than is displayed when there's no data. Blank by default
        /// </summary>
        public string NullDisplayText { get; set; }

        /// <summary>
        /// Stores the color of the text than is displayed when there's no data
        /// </summary>
        public Color NullDisplayColor { get; set; }

        /// <summary>
        /// Stores if bold the text than is displayed when there's no data
        /// </summary>
        public bool NullDisplayBold { get; set; }

        /// <summary>
        /// Stores the URL of the link. If this is filled it means the column data will have links
        /// </summary>
        public string NavigateUrl { get; set; }

        /// <summary>
        /// Stores the list of items to fill the dropdown with if SearchType is set to DropDownList
        /// </summary>
        public List<ListItem> DropDownDataSource { get; set; }

        /// <summary>
        /// Stores the data field
        /// </summary>
        public string DataField
        {
            get
            {
                object value = ViewState["DataField"];

                if (value != null)
                    return value.ToString();

                return string.Empty;
            }

            set
            {
                ViewState["DataField"] = value;
                OnFieldChanged();
            }
        }

        /// <summary>
        /// Stores the data field for the tool tip
        /// </summary>
        public string DataFieldToolTip
        {
            get
            {
                object value = ViewState["DataFieldToolTip"];

                if (value != null)
                    return value.ToString();

                return string.Empty;
            }

            set
            {
                ViewState["DataFieldToolTip"] = value;
                OnFieldChanged();
            }
        }

        /// <summary>
        /// Stores the data field for the currency symbol
        /// </summary>
        public string DataFieldCurrencySymbol
        {
            get
            {
                object value = ViewState["DataFieldCurrencySymbol"];

                if (value != null)
                    return value.ToString();

                return string.Empty;
            }

            set
            {
                ViewState["DataFieldCurrencySymbol"] = value;
                OnFieldChanged();
            }
        }
        #endregion

        #region EVENTS
        /// <summary>
        /// Event fired when the filter is applied
        /// </summary>
        public event EventHandler FilterApplied;
        #endregion

        #region OVERRIDE FUNCTIONS
        /// <summary>
        /// Override the CreateField function
        /// </summary>
        protected override DataControlField CreateField()
        {
            return new BoundField();
        }

        /// <summary>
        /// Override the Initialize function
        /// </summary>
        public override bool Initialize(bool sortingEnabled, Control control)
        {
            return base.Initialize(sortingEnabled, control);
        }

        /// <summary>
        /// Override the InitializeCell function to add the filters and header tooltips
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="cellType"></param>
        /// <param name="rowState"></param>
        /// <param name="rowIndex"></param>
        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType, DataControlRowState rowState, int rowIndex)
        {
            base.InitializeCell(cell, cellType, rowState, rowIndex);

            if (cellType == DataControlCellType.Header)
            {
                cell.Controls.Add(new LinkButton
                {
                    Text = HeaderText,
                    ToolTip = String.IsNullOrWhiteSpace(HeaderToolTip)
                        ? HeaderText
                        : HeaderToolTip,
                    CommandName = "Sort",
                    CommandArgument = DataField
                });

                if (SearchType != SearchTypeEnum.None)
                {
                    // Add filter control
                    switch (SearchType)
                    {
                        case SearchTypeEnum.TextBox:
                            cell.Controls.Add(CreateFilterTextBoxControl());
                            break;
                        case SearchTypeEnum.DropDownList:
                            cell.Controls.Add(CreateFilterDropDownListControl());
                            break;
                    }
                }
            }
            else if (cellType == DataControlCellType.DataCell)
                cell.DataBinding += new EventHandler(cell_DataBinding);
        }
        #endregion

        #region CONTROL CREATION
        /// <summary>
        /// Create the filter textbox control
        /// </summary>
        private Control CreateFilterTextBoxControl()
        {
            // Do all the work only if the control is visible
            if (Visible)
            {
                var controlClientID = this.Control.ClientID;
                var controlClientIDDataField = controlClientID + DataField;

                var divFilter = new HtmlGenericControl("div");
                divFilter.Attributes.Add("class", controlClientID + "Filters");
                divFilter.Attributes.Add("style", "display: none;");

                var inputPrependClass = ((GridViewEx)this.Control).BootstrapVersion >= 3
                    ? "input-group input-group-sm"
                    : "input-prepend";

                var divInputPrepend = new HtmlGenericControl("div");
                divInputPrepend.Attributes.Add("class", inputPrependClass);

                var divBtnGroup = new HtmlGenericControl("div");
                divBtnGroup.Attributes.Add("class", ((GridViewEx)this.Control).BootstrapVersion >= 3
                    ? "input-group-btn"
                    : "btn-group");

                var icon = ((GridViewEx)this.Control).BootstrapVersion >= 3
                    ? "glyphicon glyphicon-filter"
                    : "icon-filter";

                var btnClass = ((GridViewEx)this.Control).BootstrapVersion >= 3
                    ? "btn btn-default"
                    : "btn";

                var btnFilter = new HtmlButton();
                btnFilter.Attributes.Add("class", btnClass + " dropdown-toggle");
                btnFilter.Attributes.Add("title", "Filter By");
                btnFilter.Attributes.Add("data-toggle", "dropdown");
                btnFilter.InnerHtml = "<i class=\"" + icon + "\"></i>";
                divBtnGroup.Controls.Add(btnFilter);

                var ulFilter = new HtmlGenericControl("ul");
                ulFilter.Attributes.Add("class", "dropdown-menu");

                var txtBox = new TextBox
                {
                    ID = "txt" + controlClientIDDataField,
                    ClientIDMode = ClientIDMode.Static,
                    AutoPostBack = true,
                    CssClass = ((GridViewEx)this.Control).BootstrapVersion >= 3
                        ? "form-control col-md-1"
                        : "span1"
                };

                var hiddenField = new HiddenField
                {
                    ID = "hf" + controlClientIDDataField,
                    ClientIDMode = ClientIDMode.Static
                };

                var liFilter = new HtmlGenericControl("li");
                liFilter.InnerHtml = "<a href=\"javascript:GVEXSaveSearchExp('" + hiddenField.ClientID + "', '" + txtBox.ClientID + "', '= ');\">Is equal to</a>";
                ulFilter.Controls.Add(liFilter);

                liFilter = new HtmlGenericControl("li");
                liFilter.InnerHtml = "<a href=\"javascript:GVEXSaveSearchExp('" + hiddenField.ClientID + "', '" + txtBox.ClientID + "', '!= ');\">Is not equal to</a>";
                ulFilter.Controls.Add(liFilter);

                // Check the data format to add the correct filter expressions
                if (DataFormat == DataFormatEnum.Number
                    || DataFormat == DataFormatEnum.Currency
                    || DataFormat == DataFormatEnum.Hour
                    || DataFormat == DataFormatEnum.Percentage
                    || DataFormat == DataFormatEnum.Date
                    || DataFormat == DataFormatEnum.ShortDate)
                {
                    liFilter = new HtmlGenericControl("li");
                    liFilter.InnerHtml = "<a href=\"javascript:GVEXSaveSearchExp('" + hiddenField.ClientID + "', '" + txtBox.ClientID + "', '> ');\">Is greater than</a>";
                    ulFilter.Controls.Add(liFilter);

                    liFilter = new HtmlGenericControl("li");
                    liFilter.InnerHtml = "<a href=\"javascript:GVEXSaveSearchExp('" + hiddenField.ClientID + "', '" + txtBox.ClientID + "', '>= ');\">Is greater than or equal to</a>";
                    ulFilter.Controls.Add(liFilter);

                    liFilter = new HtmlGenericControl("li");
                    liFilter.InnerHtml = "<a href=\"javascript:GVEXSaveSearchExp('" + hiddenField.ClientID + "', '" + txtBox.ClientID + "', '< ');\">Is less than</a>";
                    ulFilter.Controls.Add(liFilter);

                    liFilter = new HtmlGenericControl("li");
                    liFilter.InnerHtml = "<a href=\"javascript:GVEXSaveSearchExp('" + hiddenField.ClientID + "', '" + txtBox.ClientID + "', '<= ');\">Is less than or equal to</a>";
                    ulFilter.Controls.Add(liFilter);
                }
                else
                {
                    liFilter = new HtmlGenericControl("li");
                    liFilter.InnerHtml = "<a href=\"javascript:GVEXSaveSearchExp('" + hiddenField.ClientID + "', '" + txtBox.ClientID + "', '* ');\">Contains</a>";
                    ulFilter.Controls.Add(liFilter);

                    liFilter = new HtmlGenericControl("li");
                    liFilter.InnerHtml = "<a href=\"javascript:GVEXSaveSearchExp('" + hiddenField.ClientID + "', '" + txtBox.ClientID + "', '!* ');\">Not contains</a>";
                    ulFilter.Controls.Add(liFilter);

                    liFilter = new HtmlGenericControl("li");
                    liFilter.InnerHtml = "<a href=\"javascript:GVEXSaveSearchExp('" + hiddenField.ClientID + "', '" + txtBox.ClientID + "', '˄ ');\">Starts with</a>";
                    ulFilter.Controls.Add(liFilter);

                    liFilter = new HtmlGenericControl("li");
                    liFilter.InnerHtml = "<a href=\"javascript:GVEXSaveSearchExp('" + hiddenField.ClientID + "', '" + txtBox.ClientID + "', '˅ ');\">Ends with</a>";
                    ulFilter.Controls.Add(liFilter);

                    liFilter = new HtmlGenericControl("li");
                    liFilter.InnerHtml = "<a href=\"javascript:GVEXSaveSearchExp('" + hiddenField.ClientID + "', '" + txtBox.ClientID + "', '!˄ ');\">Not starts with</a>";
                    ulFilter.Controls.Add(liFilter);

                    liFilter = new HtmlGenericControl("li");
                    liFilter.InnerHtml = "<a href=\"javascript:GVEXSaveSearchExp('" + hiddenField.ClientID + "', '" + txtBox.ClientID + "', '!˅ ');\">Not ends with</a>";
                    ulFilter.Controls.Add(liFilter);
                }

                divBtnGroup.Controls.Add(ulFilter);
                divInputPrepend.Controls.Add(divBtnGroup);

                txtBox.TextChanged += new EventHandler(txtBox_TextChanged);
                divInputPrepend.Controls.Add(txtBox);
                divInputPrepend.Controls.Add(hiddenField);


                ((GridViewEx)this.Control).JSScript += (DataFormat == DataFormatEnum.Date || DataFormat == DataFormatEnum.ShortDate)
                    ? @"
                    function " + controlClientIDDataField + @"Function() {
                        $('#" + txtBox.ClientID + @"').datepicker().on('changeDate', function (ev) {
                            var date = new Date(ev.date);
                            if ($('#" + hiddenField.ClientID + @"').val().indexOf(date.format('" + ((GridViewEx)this.Control).CultureInfo.DateTimeFormat.FullDateTimePattern + @"')) === -1) {
                                $('#" + hiddenField.ClientID + @"').val($('#" + hiddenField.ClientID + @"').val() + date.format('" + ((GridViewEx)this.Control).CultureInfo.DateTimeFormat.FullDateTimePattern + @"'));

                                $('#" + txtBox.ClientID + @"').datepicker('hide');
                                $('#" + txtBox.ClientID + @"').change();
                            }
                        });
                    }"
                    : @"
                    function " + controlClientIDDataField + @"Function() {
                        $('#" + txtBox.ClientID + @"').removeAttr('onkeypress');
                        var oldOnChange = $('#" + txtBox.ClientID + @"').attr('onchange');
                        if (typeof oldOnChange != 'undefined') {
                            var onChange = '$(\'#" + hiddenField.ClientID + @"\').val($(\'#" + hiddenField.ClientID + @"\').val() + $(\'#" + txtBox.ClientID + @"\').val());';
                            $('#" + txtBox.ClientID + @"').attr('onchange', onChange + oldOnChange.substring(oldOnChange.indexOf('setTimeout')));
                        }
                    }";

                ((GridViewEx)this.Control).JSScriptEndRequestHandler += controlClientIDDataField + @"Function();";
                ((GridViewEx)this.Control).JSScriptDocumentReady += controlClientIDDataField + @"Function();";

                divFilter.Controls.Add(divInputPrepend);

                return divFilter;
            }
            else
                return new Control();
        }

        /// <summary>
        /// Create the filter dropdown list control
        /// </summary>
        private Control CreateFilterDropDownListControl()
        {
            // Do all the work only if the control is visible
            if (Visible)
            {
                var controlClientID = this.Control.ClientID;
                var controlClientIDDataField = controlClientID + DataField;

                var divFilter = new HtmlGenericControl("div");
                divFilter.Attributes.Add("class", controlClientID + "Filters");
                divFilter.Attributes.Add("style", "display: none;");

                var inputPrependClass = ((GridViewEx)this.Control).BootstrapVersion >= 3
                    ? "input-group input-group-sm"
                    : "input-prepend";

                var divInputPrepend = new HtmlGenericControl("div");
                divInputPrepend.Attributes.Add("class", inputPrependClass);

                var divBtnGroup = new HtmlGenericControl("div");
                divBtnGroup.Attributes.Add("class", ((GridViewEx)this.Control).BootstrapVersion >= 3
                    ? "input-group-btn"
                    : "btn-group");

                var icon = ((GridViewEx)this.Control).BootstrapVersion >= 3
                    ? "glyphicon glyphicon-filter"
                    : "icon-filter";

                var btnClass = ((GridViewEx)this.Control).BootstrapVersion >= 3
                    ? "btn btn-default"
                    : "btn";

                var btnFilter = new HtmlButton();
                btnFilter.Attributes.Add("class", btnClass + " dropdown-toggle");
                btnFilter.Attributes.Add("title", "Filter By");
                btnFilter.Attributes.Add("data-toggle", "dropdown");
                btnFilter.InnerHtml = "<i class=\"" + icon + "\"></i>";
                divBtnGroup.Controls.Add(btnFilter);

                var ulFilter = new HtmlGenericControl("ul");
                ulFilter.Attributes.Add("class", "dropdown-menu");

                var ddlDropDownList = new DropDownList
                {
                    ID = "ddl" + controlClientID + DataField,
                    ClientIDMode = ClientIDMode.Static,
                    AutoPostBack = true,
                    CssClass = ((GridViewEx)this.Control).BootstrapVersion >= 3
                        ? "form-control col-md-1"
                        : "span1"
                };
                ddlDropDownList.SelectedIndexChanged += new EventHandler(ddlDropDownList_SelectedIndexChanged);

                if (DropDownDataSource != null)
                    ddlDropDownList.DataSource = DropDownDataSource;

                var hiddenField = new HiddenField
                {
                    ID = "hf" + controlClientIDDataField,
                    ClientIDMode = ClientIDMode.Static
                };

                var liFilter = new HtmlGenericControl("li");
                liFilter.InnerHtml = "<a href=\"javascript:GVEXSaveSearchExp('" + hiddenField.ClientID + "', '" + ddlDropDownList.ClientID + "', '= ');\">Is equal to</a>";
                ulFilter.Controls.Add(liFilter);

                liFilter = new HtmlGenericControl("li");
                liFilter.InnerHtml = "<a href=\"javascript:GVEXSaveSearchExp('" + hiddenField.ClientID + "', '" + ddlDropDownList.ClientID + "', '!= ');\">Is not equal to</a>";
                ulFilter.Controls.Add(liFilter);

                // Check the data format to add the correct filter expressions
                if (DataFormat == DataFormatEnum.Number
                    || DataFormat == DataFormatEnum.Currency
                    || DataFormat == DataFormatEnum.Hour
                    || DataFormat == DataFormatEnum.Percentage
                    || DataFormat == DataFormatEnum.Date
                    || DataFormat == DataFormatEnum.ShortDate)
                {
                    liFilter = new HtmlGenericControl("li");
                    liFilter.InnerHtml = "<a href=\"javascript:GVEXSaveSearchExp('" + hiddenField.ClientID + "', '" + ddlDropDownList.ClientID + "', '> ');\">Is greater than</a>";
                    ulFilter.Controls.Add(liFilter);

                    liFilter = new HtmlGenericControl("li");
                    liFilter.InnerHtml = "<a href=\"javascript:GVEXSaveSearchExp('" + hiddenField.ClientID + "', '" + ddlDropDownList.ClientID + "', '>= ');\">Is greater than or equal to</a>";
                    ulFilter.Controls.Add(liFilter);

                    liFilter = new HtmlGenericControl("li");
                    liFilter.InnerHtml = "<a href=\"javascript:GVEXSaveSearchExp('" + hiddenField.ClientID + "', '" + ddlDropDownList.ClientID + "', '< ');\">Is less than</a>";
                    ulFilter.Controls.Add(liFilter);

                    liFilter = new HtmlGenericControl("li");
                    liFilter.InnerHtml = "<a href=\"javascript:GVEXSaveSearchExp('" + hiddenField.ClientID + "', '" + ddlDropDownList.ClientID + "', '<= ');\">Is less than or equal to</a>";
                    ulFilter.Controls.Add(liFilter);
                }

                divBtnGroup.Controls.Add(ulFilter);
                divInputPrepend.Controls.Add(divBtnGroup);

                divInputPrepend.Controls.Add(ddlDropDownList);
                divInputPrepend.Controls.Add(hiddenField);

                ((GridViewEx)this.Control).JSScript += @"
                    function " + this.Control.ClientID + DataField + @"Function() {
                        var oldOnChange = $('#" + ddlDropDownList.ClientID + @"').attr('onchange');
                        if (typeof oldOnChange != 'undefined') {
                            var onChange = '$(\'#" + hiddenField.ClientID + @"\').val($(\'#" + hiddenField.ClientID + @"\').val() + $(\'#" + ddlDropDownList.ClientID + @" option:selected\').val());';
                            $('#" + ddlDropDownList.ClientID + @"').attr('onchange', onChange + oldOnChange.substring(oldOnChange.indexOf('setTimeout')));
                        }
                    }";

                ((GridViewEx)this.Control).JSScriptEndRequestHandler += controlClientIDDataField + @"Function();";
                ((GridViewEx)this.Control).JSScriptDocumentReady += controlClientIDDataField + @"Function();";

                divFilter.Controls.Add(divInputPrepend);

                return divFilter;
            }
            else
                return new Control();
        }
        #endregion

        #region ELEMENT EVENTS
        /// <summary>
        /// Bind the data into the row
        /// </summary>
        /// <param name="sender">Object which has raised the event</param>
        /// <param name="e">Contains additional information about the event</param>
        protected void cell_DataBinding(object sender, EventArgs e)
        {
            var cell = sender as TableCell;
            if (cell != null)
            {
                var dataItem = DataBinder.GetDataItem(cell.NamingContainer);
                var dataValue = DataBinder.GetPropertyValue(dataItem, DataField);
                var culture = (CultureInfo)((GridViewEx)this.Control).CultureInfo.Clone();

                string tooltip = String.Empty;
                if (!String.IsNullOrEmpty(DataFieldToolTip))
                    tooltip = DataBinder.GetPropertyValue(dataItem, DataFieldToolTip).ToString();

                if (!String.IsNullOrEmpty(DataFieldCurrencySymbol))
                    culture.NumberFormat.CurrencySymbol = DataBinder.GetPropertyValue(dataItem, DataFieldCurrencySymbol).ToString();

                string value = dataValue != null ? dataValue.ToString() : "";
                if (!String.IsNullOrWhiteSpace(value))
                    switch (DataFormat)
                    {
                        case DataFormatEnum.Percentage:
                            Decimal pValue;
                            if (Decimal.TryParse(value, out pValue))
                            {
                                var pText = pValue % 1 == 0 ? String.Format(culture, "{0:0%}", pValue) : String.Format(culture, "{0:0.00%}", pValue);

                                if (!String.IsNullOrWhiteSpace(NavigateUrl))
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new HyperLink { Text = pText, NavigateUrl = NavigateUrl, ToolTip = tooltip });
                                    else
                                        cell.Controls.Add(new HyperLink { Text = pText, NavigateUrl = NavigateUrl, ToolTip = pText });
                                else
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new Label { Text = pText, ToolTip = tooltip });
                                    else
                                        cell.Text = pText;
                            }
                            else
                            {
                                if (!String.IsNullOrWhiteSpace(NavigateUrl))
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new HyperLink { Text = value, NavigateUrl = NavigateUrl, ToolTip = tooltip });
                                    else
                                        cell.Controls.Add(new HyperLink { Text = value, NavigateUrl = NavigateUrl, ToolTip = value });
                                else
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new Label { Text = value, ToolTip = tooltip });
                                    else
                                        cell.Text = value;
                            }
                            break;
                        case DataFormatEnum.Currency:
                            Decimal cValue;
                            if (Decimal.TryParse(value, out cValue))
                            {
                                var cText = cValue % 1 == 0 ? String.Format(culture, "{0:C0}", cValue) : String.Format(culture, "{0:C}", cValue);

                                if (!String.IsNullOrWhiteSpace(NavigateUrl))
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new HyperLink { Text = cText, NavigateUrl = NavigateUrl, ToolTip = tooltip });
                                    else
                                        cell.Controls.Add(new HyperLink { Text = cText, NavigateUrl = NavigateUrl, ToolTip = cText });
                                else
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new Label { Text = cText, ToolTip = tooltip });
                                    else
                                        cell.Text = cText;
                            }
                            else
                            {
                                if (!String.IsNullOrWhiteSpace(NavigateUrl))
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new HyperLink { Text = value, NavigateUrl = NavigateUrl, ToolTip = tooltip });
                                    else
                                        cell.Controls.Add(new HyperLink { Text = value, NavigateUrl = NavigateUrl, ToolTip = value });
                                else
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new Label { Text = value, ToolTip = tooltip });
                                    else
                                        cell.Text = value;
                            }
                            break;
                        case DataFormatEnum.Date:
                            DateTime dValue;
                            if (DateTime.TryParse(value, out dValue))
                            {
                                var dText = dValue.ToShortDateString();

                                if (!String.IsNullOrWhiteSpace(NavigateUrl))
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new HyperLink { Text = dText, NavigateUrl = NavigateUrl, ToolTip = tooltip });
                                    else
                                        cell.Controls.Add(new HyperLink { Text = dText, NavigateUrl = NavigateUrl, ToolTip = dText });
                                else
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new Label { Text = dText, ToolTip = tooltip });
                                    else
                                        cell.Text = dText;
                            }
                            else
                            {
                                if (!String.IsNullOrWhiteSpace(NavigateUrl))
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new HyperLink { Text = value, NavigateUrl = NavigateUrl, ToolTip = tooltip });
                                    else
                                        cell.Controls.Add(new HyperLink { Text = value, NavigateUrl = NavigateUrl, ToolTip = value });
                                else
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new Label { Text = value, ToolTip = tooltip });
                                    else
                                        cell.Text = value;
                            }
                            break;
                        case DataFormatEnum.ShortDate:
                            DateTime sdValue;
                            if (DateTime.TryParse(value, out sdValue))
                            {
                                var sdText = String.Format("{0:MM/dd}", sdValue);

                                if (!String.IsNullOrWhiteSpace(NavigateUrl))
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new HyperLink { Text = sdText, NavigateUrl = NavigateUrl, ToolTip = tooltip });
                                    else
                                        cell.Controls.Add(new HyperLink { Text = sdText, NavigateUrl = NavigateUrl, ToolTip = sdText });
                                else
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new Label { Text = sdText, ToolTip = tooltip });
                                    else
                                        cell.Text = sdText;
                            }
                            else
                            {
                                if (!String.IsNullOrWhiteSpace(NavigateUrl))
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new HyperLink { Text = value, NavigateUrl = NavigateUrl, ToolTip = tooltip });
                                    else
                                        cell.Controls.Add(new HyperLink { Text = value, NavigateUrl = NavigateUrl, ToolTip = value });
                                else
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new Label { Text = value, ToolTip = tooltip });
                                    else
                                        cell.Text = value;
                            }
                            break;
                        case DataFormatEnum.Hour:
                            Decimal hValue;
                            if (Decimal.TryParse(value, out hValue))
                            {
                                var hText = hValue % 1 == 0 ? String.Format(culture, "{0:0 H}", hValue) : String.Format(culture, "{0:0.00 H}", hValue);

                                if (!String.IsNullOrWhiteSpace(NavigateUrl))
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new HyperLink { Text = hText, NavigateUrl = NavigateUrl, ToolTip = tooltip });
                                    else
                                        cell.Controls.Add(new HyperLink { Text = hText, NavigateUrl = NavigateUrl, ToolTip = hText });
                                else
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new Label { Text = hText, ToolTip = tooltip });
                                    else
                                        cell.Text = hText;
                            }
                            else
                            {
                                if (!String.IsNullOrWhiteSpace(NavigateUrl))
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new HyperLink { Text = value, NavigateUrl = NavigateUrl, ToolTip = tooltip });
                                    else
                                        cell.Controls.Add(new HyperLink { Text = value, NavigateUrl = NavigateUrl, ToolTip = value });
                                else
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new Label { Text = value, ToolTip = tooltip });
                                    else
                                        cell.Text = value;
                            }
                            break;
                        case DataFormatEnum.Expression:
                            if (!String.IsNullOrWhiteSpace(DataFormatExpression))
                            {
                                if (!String.IsNullOrWhiteSpace(NavigateUrl))
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new HyperLink { Text = String.Format(DataFormatExpression, value), NavigateUrl = NavigateUrl, ToolTip = tooltip });
                                    else
                                        cell.Controls.Add(new HyperLink { Text = String.Format(DataFormatExpression, value), NavigateUrl = NavigateUrl, ToolTip = String.Format(DataFormatExpression, value) });
                                else
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new Label { Text = String.Format(DataFormatExpression, value), ToolTip = tooltip });
                                    else
                                        cell.Text = String.Format(DataFormatExpression, value);
                            }
                            else
                            {
                                if (!String.IsNullOrWhiteSpace(NavigateUrl))
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new HyperLink { Text = value, NavigateUrl = NavigateUrl, ToolTip = tooltip });
                                    else
                                        cell.Controls.Add(new HyperLink { Text = value, NavigateUrl = NavigateUrl, ToolTip = value });
                                else
                                    if (!String.IsNullOrWhiteSpace(tooltip))
                                        cell.Controls.Add(new Label { Text = value, ToolTip = tooltip });
                                    else
                                        cell.Text = value;
                            }
                            break;
                        default:
                            if (!String.IsNullOrWhiteSpace(NavigateUrl))
                                cell.Controls.Add(new HyperLink { Text = value, NavigateUrl = NavigateUrl, ToolTip = value });
                            else
                                if (!String.IsNullOrWhiteSpace(tooltip))
                                    cell.Controls.Add(new Label { Text = value, ToolTip = tooltip });
                                else
                                    cell.Text = value;
                            break;
                    }
                else
                {
                    cell.Text = NullDisplayText;
                    cell.ForeColor = NullDisplayColor;
                    cell.Font.Bold = NullDisplayBold;
                }
            }
        }

        /// <summary>
        /// Filter textbox applied
        /// </summary>
        /// <param name="sender">Object which has raised the event</param>
        /// <param name="e">Contains additional information about the event</param>
        protected void txtBox_TextChanged(object sender, EventArgs e)
        {
            var txt = sender as TextBox;
            if (txt != null)
            {
                var hiddenField = txt.NamingContainer.FindControl("hf" + this.Control.ClientID + DataField) as HiddenField;
                if (hiddenField != null)
                    ApplyFilter(hiddenField.Value); // TODO: For currency should include the symbol somewhere
            }
        }

        /// <summary>
        /// Filter dropdownlist applied
        /// </summary>
        /// <param name="sender">Object which has raised the event</param>
        /// <param name="e">Contains additional information about the event</param>
        protected void ddlDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ddl = (DropDownList)sender;
            if (ddl != null)
            {
                var hiddenField = ddl.NamingContainer.FindControl("hf" + this.Control.ClientID + DataField) as HiddenField;
                if (hiddenField != null)
                    ApplyFilter(hiddenField.Value); // TODO: For currency should include the symbol somewhere
            }
        }

        /// <summary>
        /// Apply the filter
        /// </summary>
        /// <param name="fullFilterExpression">string as came from the user with the filter expression in it</param>
        private void ApplyFilter(string fullFilterExpression)
        {
            var filter = new List<string>(fullFilterExpression.Trim().Split(' '));

            // Default values of the filter expression depending on the data format
            string filterExpression = String.Empty;
            switch (DataFormat)
            {
                case DataFormatEnum.Number:
                    filterExpression = "=";
                    break;
                case DataFormatEnum.Currency:
                    filterExpression = "=";
                    break;
                case DataFormatEnum.Date:
                    filterExpression = "=";
                    break;
                case DataFormatEnum.Hour:
                    filterExpression = "=";
                    break;
                case DataFormatEnum.Percentage:
                    filterExpression = "=";
                    break;
                case DataFormatEnum.ShortDate:
                    filterExpression = "=";
                    break;
                default:
                    filterExpression = SearchType == SearchTypeEnum.DropDownList
                        ? "="
                        : "˄";
                    break;
            }

            string filterText = String.Empty;

            if (Extensions.IsValidExpressionType(filter[0]))
            {
                filterExpression = filter[0];
                filter.Remove(filter[0]); // Remove filterExpression from the list, rest is text
                filterText = String.Join(" ", filter);
            }
            else
                filterText = String.Join(" ", filter);

            // Convert the number if needed
            switch (DataFormat)
            {
                case DataFormatEnum.Percentage:
                    filterText = (Decimal.Parse(filterText.Split('%')[0]) / 100M).ToString();
                    break;
                case DataFormatEnum.Currency:
                    Decimal cValue;
                    if (Decimal.TryParse(filterText, NumberStyles.Currency, ((GridViewEx)this.Control).CultureInfo, out cValue))
                        filterText = cValue.ToString();
                    break;
                case DataFormatEnum.Date:
                    DateTime dValue;
                    if (DateTime.TryParse(filterText, out dValue))
                        filterText = dValue.ToString();
                    break;
                case DataFormatEnum.ShortDate:
                    DateTime sdValue;
                    if (DateTime.TryParse(filterText, out sdValue))
                        filterText = sdValue.ToString();
                    break;
                case DataFormatEnum.Hour:
                    filterText = (Decimal.Parse(filterText.Split('H')[0])).ToString();
                    break;
                //case DataFormatEnum.Expression:
                //    text = (!String.IsNullOrWhiteSpace(dataFormatExpression))
                //        ? String.Format(dataFormatExpression, item.ToString())
                //        : item.ToString();
                //    break;
                //default:
                //    text = item.ToString();
                //    break;
            }

            if (!string.IsNullOrWhiteSpace(filterText))
            {
                var filters = new List<FilterExpression>();
                if (Control.Page.Session[this.Control.ID + "_Filters"] != null)
                    filters = Control.Page.Session[this.Control.ID + "_Filters"] as List<FilterExpression>;

                var filterExp = new FilterExpression
                {
                    Expression = Extensions.GetExpressionType(filterExpression),
                    ExpressionShortName = filterExpression,
                    Column = DataField,
                    DisplayName = this.HeaderText,
                    Text = filterText
                };
                if (!filters.Exists(x => x.Column == filterExp.Column
                    && x.Expression == filterExp.Expression
                    && x.ExpressionShortName == filterExp.ExpressionShortName
                    && x.Text == filterExp.Text))
                    filters.Add(filterExp);

                Control.Page.Session[this.Control.ID + "_Filters"] = filters;
                Control.Page.Session[this.Control.ID + "_PageIndex"] = 0;

                if (FilterApplied != null)
                    FilterApplied(null, EventArgs.Empty);
            }
        }
        #endregion
    }
}