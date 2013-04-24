using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// Stores the header tooltip. If not defined then use the HeaderText
        /// </summary>
        public string HeaderToolTip { get; set; }

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
            divFilter.Attributes.Add("class", controlClientID + "Filters");
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
            var cell = sender as TableCell;
            if (cell != null)
            {
                var dataItem = DataBinder.GetDataItem(cell.NamingContainer);
                var dataValue = DataBinder.GetPropertyValue(dataItem, DataField);
                bool value = dataValue != null ? Convert.ToBoolean(dataValue) : false;

                var cb = new HtmlInputCheckBox { Checked = value };
                cb.Attributes.Add("class", this.Control.ClientID + DataField);

                cell.Controls.Add(cb);
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