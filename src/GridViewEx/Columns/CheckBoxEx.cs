using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace GridViewEx.Columns
{
    /// <summary>
    /// Extended checkbox column used by GridViewEx
    /// </summary>
    /// <remarks>
    /// [{"Author": "Vicent Climent";
    /// "Created Date": "08/03/2013"}]
    /// </remarks>
    public class CheckBoxEx : DataControlField
    {
        #region VARIABLES
        /// <summary>
        /// Stores the row index
        /// </summary>
        internal int RowIndex { get; set; }

        /// <summary>
        /// Stores the header tooltip. If not defined then use the HeaderText
        /// </summary>
        public string HeaderToolTip { get; set; }

        /// <summary>
        /// Stores if the indeterminate state is allowed or not as NULL value
        /// </summary>
        public bool AllowIndeterminateState { get; set; }

        /// <summary>
        /// Stores if the checkbox is editable or not whenever the condition is meet
        /// </summary>
        public CheckboxDisabledModesEnum DisableStateMode { get; set; }

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
        /// Stores the data field for the id
        /// </summary>
        public string DataFieldID
        {
            get
            {
                object value = ViewState["DataFieldID"];

                if (value != null)
                    return value.ToString();

                return string.Empty;
            }

            set
            {
                ViewState["DataFieldID"] = value;
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
            return new CheckBoxField();
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
            RowIndex = rowIndex;

            base.InitializeCell(cell, cellType, rowState, rowIndex);

            if (cellType == DataControlCellType.Header)
            {
                cell.Controls.Add(new LinkButton
                {
                    Text = HeaderText,
                    ToolTip = String.IsNullOrWhiteSpace(HeaderToolTip) ? HeaderText : HeaderToolTip,
                    CommandName = "Sort",
                    CommandArgument = DataField
                });

                cell.Controls.Add(CreateFilterCheckBoxControl(DataField));
            }
            else if (cellType == DataControlCellType.DataCell)
                cell.DataBinding += new EventHandler(cell_DataBinding);
        }
        #endregion

        #region CONTROL CREATION
        /// <summary>
        /// Create the filter checkbox control
        /// </summary>
        /// <param name="dataField">Data field (Column name)</param>
        private Control CreateFilterCheckBoxControl(string dataField)
        {
            var controlClientID = this.Control.ClientID;
            var controlClientIDDataField = controlClientID + DataField;

            var divFilter = new HtmlGenericControl("div");
            divFilter.Attributes.Add("class", controlClientID + "Filters " + this.ItemStyle.CssClass);
            divFilter.Attributes.Add("style", "display: none;");
            
            var filters = new List<FilterExpression>();
            if (Control.Page.Session[this.Control.ID + "_Filters"] != null)
                filters = Control.Page.Session[this.Control.ID + "_Filters"] as List<FilterExpression>;

            var checkBox = new CheckBox
            {
                ID = "cb" + controlClientIDDataField,
                ClientIDMode = ClientIDMode.Static,
                AutoPostBack = true,
                Checked = filters.Exists(x => x.Column == dataField
                    && x.Expression == Extensions.GetExpressionType("=")
                    && x.ExpressionShortName == "="
                    && x.Text == "True")
            };

            checkBox.CheckedChanged += new EventHandler(checkBox_CheckedChanged);
            divFilter.Controls.Add(checkBox);

            return divFilter;
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
            // Do all the work only if the control is visible
            var cell = sender as TableCell;
            if (cell != null
                && Visible)
            {
                var controlClientIDDataField = this.Control.ClientID + DataField;

                var dataItem = DataBinder.GetDataItem(cell.NamingContainer);
                var dataValue = DataBinder.GetPropertyValue(dataItem, DataField);
                bool? value = dataValue != null ? Convert.ToBoolean(dataValue) : (bool?)null;

                var cb = new HtmlInputCheckBox { Checked = value != null ? (bool)value : false };
                cb.Attributes.Add("class", this.Control.ClientID + DataField);

                // Add a tooltip if any
                string tooltip = String.Empty;
                if (!String.IsNullOrEmpty(DataFieldToolTip))
                    tooltip = DataBinder.GetPropertyValue(dataItem, DataFieldToolTip).ToString();

                if (!String.IsNullOrWhiteSpace(tooltip))
                    cb.Attributes.Add("title", tooltip);

                // Add an attribute data-id if any
                string dataID = String.Empty;
                if (!String.IsNullOrEmpty(DataFieldID))
                {
                    var propertyValue = DataBinder.GetPropertyValue(dataItem, DataFieldID);
                    if (propertyValue != null)
                        dataID = propertyValue.ToString();
                }

                if (!String.IsNullOrWhiteSpace(dataID))
                    cb.Attributes.Add("data-id", dataID);

                cell.Controls.Add(cb);

                // Allow 3rd state checkbox
                if (AllowIndeterminateState
                    && value == null)
                {
                    ((GridViewEx)this.Control).JSScriptEndRequestHandler += @"GVEXCheckboxIndeterminate('" + cb.ClientID + @"');";
                    ((GridViewEx)this.Control).JSScriptDocumentReady += @"GVEXCheckboxIndeterminate('" + cb.ClientID + @"');";
                }

                // Check if the checkbox is editable or not
                if (DisableStateMode != CheckboxDisabledModesEnum.None)
                {
                    // Add this script only on first row (only one per column)
                    if (RowIndex == 0)
                    {
                        switch (DisableStateMode)
                        {
                            case CheckboxDisabledModesEnum.All:
                                ((GridViewEx)this.Control).JSScriptEndRequestHandlerDelayed += @"GVEXColumnCheckboxesDisable('" + controlClientIDDataField + @"', 'all');";
                                ((GridViewEx)this.Control).JSScriptDocumentReadyDelayed += @"GVEXColumnCheckboxesDisable('" + controlClientIDDataField + @"', 'all');";
                                break;
                            case CheckboxDisabledModesEnum.Unchecked:
                                ((GridViewEx)this.Control).JSScriptEndRequestHandlerDelayed += @"GVEXColumnCheckboxesDisable('" + controlClientIDDataField + @"', 'unchecked');";
                                ((GridViewEx)this.Control).JSScriptDocumentReadyDelayed += @"GVEXColumnCheckboxesDisable('" + controlClientIDDataField + @"', 'unchecked');";
                                break;
                            case CheckboxDisabledModesEnum.Indeterminate:
                                ((GridViewEx)this.Control).JSScriptEndRequestHandlerDelayed += @"GVEXColumnCheckboxesDisable('" + controlClientIDDataField + @"', 'null');";
                                ((GridViewEx)this.Control).JSScriptDocumentReadyDelayed += @"GVEXColumnCheckboxesDisable('" + controlClientIDDataField + @"', 'null');";
                                break;
                            case CheckboxDisabledModesEnum.Checked:
                                ((GridViewEx)this.Control).JSScriptEndRequestHandlerDelayed += @"GVEXColumnCheckboxesDisable('" + controlClientIDDataField + @"', 'checked');";
                                ((GridViewEx)this.Control).JSScriptDocumentReadyDelayed += @"GVEXColumnCheckboxesDisable('" + controlClientIDDataField + @"', 'checked');";
                                break;
                            case CheckboxDisabledModesEnum.CheckedOrIndeterminate:
                                ((GridViewEx)this.Control).JSScriptEndRequestHandlerDelayed += @"GVEXColumnCheckboxesDisable('" + controlClientIDDataField + @"', 'checkedOrNull');";
                                ((GridViewEx)this.Control).JSScriptDocumentReadyDelayed += @"GVEXColumnCheckboxesDisable('" + controlClientIDDataField + @"', 'checkedOrNull');";
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Filter textbox applied
        /// </summary>
        /// <param name="sender">Object which has raised the event</param>
        /// <param name="e">Contains additional information about the event</param>
        protected void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            var cb = sender as CheckBox;
            if (cb != null)
            {
                var filters = new List<FilterExpression>();
                if (Control.Page.Session[this.Control.ID + "_Filters"] != null)
                    filters = Control.Page.Session[this.Control.ID + "_Filters"] as List<FilterExpression>;

                var filterExpression = "=";

                var filterExp = new FilterExpression
                {
                    Expression = Extensions.GetExpressionType(filterExpression),
                    ExpressionShortName = filterExpression,
                    Column = DataField,
                    DisplayName = this.HeaderText,
                    Text = cb.Checked.ToString()
                };

                if (!filters.Exists(x => x.Column == filterExp.Column
                    && x.Expression == filterExp.Expression
                    && x.ExpressionShortName == filterExp.ExpressionShortName))
                    filters.Add(filterExp);
                else
                {
                    var filter = filters.SingleOrDefault(x => x.Column == filterExp.Column
                        && x.Expression == filterExp.Expression
                        && x.ExpressionShortName == filterExp.ExpressionShortName);
                    filter.Text = filterExp.Text;
                }

                Control.Page.Session[this.Control.ID + "_Filters"] = filters;
                Control.Page.Session[this.Control.ID + "_PageIndex"] = 0;

                if (FilterApplied != null)
                    FilterApplied(null, EventArgs.Empty);
            }
        }
        #endregion
    }
}