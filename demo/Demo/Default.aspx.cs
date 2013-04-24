using GridViewEx;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Demo
{
    public partial class Default : System.Web.UI.Page
    {
        private AdventureWorksDataContext dl = new AdventureWorksDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                bool keepSession = false;
                bool.TryParse(Request["ks"], out keepSession);

                if (!keepSession)
                {
                    // Default Sort
                    gvexPersons.SortExpressions = new List<SortExpression>();
                    gvexPersons.SortExpressions.Add(new SortExpression
                    {
                        Column = "FirstName",
                        Direction = "ASC"
                    });
                    gvexPersons.SortExpressions.Add(new SortExpression
                    {
                        Column = "LastName",
                        Direction = "ASC"
                    });
                }

                LoadTableViews();
                gvexPersons_DataBind();
            }
        }

        private List<dynamic> gvexPersons_Source(bool isExport = false)
        {
            // Select columns
            var source = dl.Person.Select(x => new
            {
                x.PersonType,
                x.Title,
                x.NameStyle,
                x.FirstName,
                x.MiddleName,
                x.LastName,
                x.Suffix,
                x.EmailPromotion
            });

            return (!isExport)
                ? gvexPersons.GridViewExDataSource(source)
                : gvexPersons.GridViewExDataSource(source, false, true);
        }

        #region GRIDVIEW DEFAULT EVENTS / FUNCTIONS
        private void gvexPersons_DataBind(bool initPager = false)
        {
            gvexPersons.DataSource = gvexPersons_Source();
            gvexPersons.DataBind();

            if (initPager)
                gvexPersons.InitControls();
        }

        protected void gvexPersons_Sorting(object sender, GridViewSortEventArgs e)
        {
            gvexPersons_DataBind();
        }

        protected void gvexPersons_SortingChanged(object sender, EventArgs e)
        {
            gvexPersons_DataBind();
        }

        protected void gvexPersons_PageChanged(object sender, EventArgs e)
        {
            gvexPersons_DataBind();
        }

        protected void gvexPersons_FilterApplied(object sender, EventArgs e)
        {
            gvexPersons_DataBind(true);
        }

        protected void gvexPersons_FilterDeleted(object sender, EventArgs e)
        {
            gvexPersons_DataBind();
        }

        protected void gvexPersons_ColumnSelectionChanged(object sender, EventArgs e)
        {
            gvexPersons_DataBind();
        }

        protected void gvexPersons_ExcelExport(object sender, EventArgs e)
        {
            GridViewEx.Extensions.ExportExcel(gvexPersons_Source(true),
                (List<ColumnExpression>)Session[gvexPersons.ID + "_Columns"],
                gvexPersons.Title);
        }

        protected void gvexPersons_ViewChanged(object sender, EventArgs e)
        {
            // If ViewExpression is not null means we want to save a new view with following details
            var view = sender as ViewExpression;
            if (view != null)
            {
                var userID = 1; // Set with a UNIQUE user ID on your system

                var jsonObject = new
                {
                    ColumnExpressions = view.ColumnExpressions,
                    FilterExpressions = view.FilterExpressions,
                    SortExpressions = view.SortExpressions,
                    PageSize = view.PageSize
                };

                // Unset other views as default if is selected for this view
                if (view.DefaultView)
                {
                    var views = dl.UserTableViews.Where(x => x.UserID == userID
                        && x.IsDefault);
                    foreach (var tableView in views)
                        tableView.IsDefault = false;
                }

                dl.UserTableViews.InsertOnSubmit(new UserTableViews
                {
                    TableName = gvexPersons.ID,
                    ViewName = view.Name,
                    JSON = JsonConvert.SerializeObject(jsonObject),
                    UserID = userID,
                    IsDefault = view.DefaultView
                });

                dl.SubmitChanges();
                LoadTableViews();
            }

            var viewID = sender as Int32?;
            if (viewID != null)
            {
                var userTableView = dl.UserTableViews.SingleOrDefault(x => x.ID == viewID);
                if (userTableView != null)
                {
                    var json = JsonConvert.DeserializeObject<dynamic>(userTableView.JSON);
                    gvexPersons.SetView(new ViewExpression
                    {
                        ID = userTableView.ID,
                        Name = userTableView.ViewName,
                        ColumnExpressions = json.ColumnExpressions.ToObject<List<ColumnExpression>>(),
                        FilterExpressions = json.FilterExpressions.ToObject<List<FilterExpression>>(),
                        SortExpressions = json.SortExpressions.ToObject<List<SortExpression>>(),
                        PageSize = json.PageSize
                    });
                }
            }

            gvexPersons_DataBind();
        }

        private void LoadTableViews()
        {
            var userID = 1; // Set with a UNIQUE user ID on your system
            if (userID != 0)
            {
                // Get user views for this table
                var userTableViews = dl.UserTableViews.Where(x => x.UserID == userID
                    && x.TableName == gvexPersons.ID);

                // IMPORTANT NOTE: Just needed ID and Name, we'll get the view from DB each time user change view. Don't want to store too much
                //                 data on the session
                gvexPersons.LoadViews(userTableViews.Select(x => new ViewExpression
                {
                    ID = x.ID,
                    Name = x.ViewName
                }).ToList());

                // Set the default view on load if any
                var defaultView = userTableViews.SingleOrDefault(x => x.IsDefault);
                if (defaultView != null)
                {
                    Session[gvexPersons.ID + "_DefaultView"] = true;

                    var json = JsonConvert.DeserializeObject<dynamic>(defaultView.JSON);
                    gvexPersons.SetView(new ViewExpression
                    {
                        ID = defaultView.ID,
                        Name = defaultView.ViewName,
                        ColumnExpressions = json.ColumnExpressions.ToObject<List<ColumnExpression>>(),
                        FilterExpressions = json.FilterExpressions.ToObject<List<FilterExpression>>(),
                        SortExpressions = json.SortExpressions.ToObject<List<SortExpression>>(),
                        PageSize = json.PageSize
                    });
                }
                else
                    Session[gvexPersons.ID + "_DefaultView"] = false;
            }
        }
        #endregion
    }
}