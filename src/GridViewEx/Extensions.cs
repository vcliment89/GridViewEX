using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.UI.WebControls;

namespace GridViewEx
{
    /// <summary>
    /// Class to store the sort expression
    /// </summary>
    [Serializable]
    public class SortExpression
    {
        /// <summary>
        /// Stores the column name (DataField)
        /// </summary>
        public string Column;
        /// <summary>
        /// Stores the column display name
        /// </summary>
        public string DisplayName;
        /// <summary>
        /// Stores the direction of the sorting (ASC or DESC)
        /// </summary>
        public string Direction;
        /// <summary>
        /// Stores the previous direction of the sorting expression (ASC or DESC)
        /// </summary>
        public string PreviousDirection;
    }

    /// <summary>
    /// Class to store the filter expression
    /// </summary>
    [Serializable]
    public class FilterExpression
    {
        /// <summary>
        /// Stores the column name (DataField)
        /// </summary>
        public string Column;
        /// <summary>
        /// Stores the column display name
        /// </summary>
        public string DisplayName;
        /// <summary>
        /// Stores the filter expression full name
        /// </summary>
        public string Expression;
        /// <summary>
        /// Stores the filter expression short name
        /// </summary>
        public string ExpressionShortName;
        /// <summary>
        /// Stores the text of the filter
        /// </summary>
        public string Text;
    }

    /// <summary>
    /// Class to store the column expression
    /// </summary>
    [Serializable]
    public class ColumnExpression
    {
        /// <summary>
        /// Stores the assigned ID
        /// </summary>
        public int ID;
        /// <summary>
        /// Stores the column name (DataField)
        /// </summary>
        public string Column;
        /// <summary>
        /// Stores the column display name
        /// </summary>
        public string DisplayName;
        /// <summary>
        /// Stores if the column is visible or not
        /// </summary>
        public bool Visible;
        /// <summary>
        /// Stores the column type
        /// </summary>
        public string Type;
        /// <summary>
        /// Stores the column order index
        /// </summary>
        public int Index;
        /// <summary>
        /// Stores the column data format
        /// </summary>
        public DataFormatEnum DataFormat;
        /// <summary>
        /// Stores the column data format expression in case DataFormat is set to 'Expression'
        /// </summary>
        public string DataFormatExpression;
    }

    /// <summary>
    /// Class to store the column expression
    /// </summary>
    [Serializable]
    public class ColumnExpressionCookie
    {
        /// <summary>
        /// Stores the assigned ID
        /// </summary>
        public int ID;
        /// <summary>
        /// Stores if the column is visible or not
        /// </summary>
        public int V;
        /// <summary>
        /// Stores the column order index
        /// </summary>
        public int I;
    }

    /// <summary>
    /// Class to store the view expression
    /// </summary>
    [Serializable]
    public class ViewExpression
    {
        /// <summary>
        /// Stores the unique ID of the view
        /// </summary>
        public int ID;
        /// <summary>
        /// Stores the name of the view
        /// </summary>
        public string Name;
        /// <summary>
        /// Stores the sort expression
        /// </summary>
        public List<SortExpression> SortExpressions;
        /// <summary>
        /// Stores the filter expression
        /// </summary>
        public List<FilterExpression> FilterExpressions;
        /// <summary>
        /// Stores the column expression
        /// </summary>
        public List<ColumnExpression> ColumnExpressions;
        /// <summary>
        /// Stores the table page size
        /// </summary>
        public int PageSize;
        /// <summary>
        /// Stores if is the default view
        /// </summary>
        public bool DefaultView;
    }

    /// <summary>
    /// Enumerator with the types of search allowed
    /// </summary>
    public enum SearchTypeEnum
    {
        /// <summary>
        /// Default. No search
        /// </summary>
        None,
        /// <summary>
        /// Text input
        /// </summary>
        TextBox,
        /// <summary>
        /// Dropdown list
        /// </summary>
        DropDownList
    }

    /// <summary>
    /// Enumerator with the data formats allowed
    /// </summary>
    public enum DataFormatEnum
    {
        /// <summary>
        /// Default. Plain text
        /// </summary>
        Text,
        /// <summary>
        /// Numeric (2.00)
        /// </summary>
        Number,
        /// <summary>
        /// Percentage number (3.25%)
        /// </summary>
        Percentage,
        /// <summary>
        /// Currency number ($ 3.25)
        /// </summary>
        Currency,
        /// <summary>
        /// Date number (29/08/2012)
        /// </summary>
        Date,
        /// <summary>
        /// Short date number (29/08)
        /// </summary>
        ShortDate,
        /// <summary>
        /// Hour number (3H)
        /// </summary>
        Hour,
        /// <summary>
        /// Custom expression. Need to fill also DataFormatExpression
        /// </summary>
        Expression
    }

    /// <summary>
    /// Enumerator with the checkboxes disabled modes
    /// </summary>
    public enum CheckboxDisabledModesEnum
    {
        /// <summary>
        /// Default. Any disabled
        /// </summary>
        None,
        /// <summary>
        /// All textboxes disabled
        /// </summary>
        All,
        /// <summary>
        /// Only checkboxes with false value
        /// </summary>
        Unchecked,
        /// <summary>
        /// Only checkboxes with true value
        /// </summary>
        Checked,
        /// <summary>
        /// Only checkboxes with NULL value
        /// </summary>
        Indeterminate,
        /// <summary>
        /// Only checkboxes with true or NULL value
        /// </summary>
        CheckedOrIndeterminate
    }

    /// <summary>
    /// Extension methods used by GridViewEx
    /// </summary>
    /// <remarks>
    /// [{"Author": "Vicent Climent";
    /// "Created Date": "08/03/2013"}]
    /// </remarks>
    public static class Extensions
    {
        /// <summary>
        /// Create the multi-sorting query to the <paramref name="query"/> based on the <paramref name="sortExpressions"/>
        /// </summary>
        /// <param name="query">Query where to apply the sortings</param>
        /// <param name="sortExpressions">Sort expressions</param>
        public static IQueryable<T> Order<T>(this IQueryable<T> query, List<SortExpression> sortExpressions)
        {
            if (sortExpressions != null)
            {
                // x =>
                var xParameter = Expression.Parameter(query.GetType(), "x");

                // x => x.DataParameter
                var dataParameter = Expression.Parameter(query.AsQueryable().GetType().GetGenericArguments()[0], "x");

                var count = 0;
                foreach (var sortExpression in sortExpressions)
                {
                    var sort = Expression.Lambda(Expression.Property(dataParameter, sortExpression.Column), dataParameter);
                    var orderByCall = (count > 0)
                        ? Expression.Call(typeof(Queryable), (sortExpression.Direction == "ASC") ? "ThenBy" : "ThenByDescending", sort.Type.GetGenericArguments(), xParameter, sort)
                        : Expression.Call(typeof(Queryable), (sortExpression.Direction == "ASC") ? "OrderBy" : "OrderByDescending", sort.Type.GetGenericArguments(), xParameter, sort);

                    LambdaExpression lambda = Expression.Lambda(orderByCall, xParameter);
                    query = (IQueryable<T>)lambda.Compile().DynamicInvoke(query);
                    count++;
                }
            }

            return query;
        }

        /// <summary>
        /// Used to fill the filter dropdown list from the <paramref name="query"/>
        /// </summary>
        /// <param name="query">Query where to apply the sortings</param>
        /// <param name="dataField">Column than we want the data</param>
        /// <param name="dataFormat">Type of data</param>
        /// <param name="dataFormatExpression">If we use a custom expression, here is where we pass it</param>
        internal static List<ListItem> GetDropDownDataSource<T>(this IQueryable<T> query, string dataField, DataFormatEnum dataFormat, string dataFormatExpression)
        {
            var ddlSource = new List<ListItem>();

            // x =>
            var xParameter = Expression.Parameter(typeof(T), "x");

            // x.Property
            var propery = typeof(T).GetProperty(dataField);

            // x => x.Property
            var columnLambda = Expression.Lambda(Expression.Property(xParameter, propery), xParameter);

            // query.Select(x => x.Property)
            var selectCall = Expression.Call(typeof(Queryable),
                "Select",
                new Type[] { query.ElementType, columnLambda.Body.Type },
                query.Expression,
                columnLambda);

            // query.Select(x => x.Property).Distinct()
            var distinctCall = Expression.Call(typeof(Queryable),
                "Distinct",
                new Type[] { propery.PropertyType },
                selectCall);

            // x => x
            var sortParam = Expression.Parameter(propery.PropertyType, "x");
            var columnResultLambda = Expression.Lambda(sortParam, sortParam);

            // query.Select(x => x.Property).Distinct().OrderBy(x => x)
            var ordercall = Expression.Call(typeof(Queryable),
                "OrderBy",
                new Type[] { propery.PropertyType, columnResultLambda.Body.Type },
                distinctCall,
                columnResultLambda);

            // Apply the expression call to the query
            var result = query.Provider.CreateQuery(ordercall);

            // Fill the dropdown list
            ddlSource.Add(new ListItem(""));
            foreach (var item in result)
                if (item != null)
                {
                    string text = String.Empty;
                    switch (dataFormat)
                    {
                        case DataFormatEnum.Percentage:
                            Decimal pValue;
                            if (Decimal.TryParse(item.ToString(), out pValue))
                                text = pValue % 1 == 0
                                    ? String.Format(CultureInfo.InvariantCulture, "{0:0%}", pValue)
                                    : String.Format(CultureInfo.InvariantCulture, "{0:0.00%}", pValue);
                            else
                                text = item.ToString();
                            break;
                        case DataFormatEnum.Currency:
                            Decimal cValue;
                            if (Decimal.TryParse(item.ToString(), out cValue))
                                text = cValue % 1 == 0
                                    ? String.Format(new CultureInfo("en-US"), "{0:C0}", cValue)
                                    : String.Format(new CultureInfo("en-US"), "{0:C}", cValue);
                            else
                                text = item.ToString();
                            break;
                        case DataFormatEnum.Date:
                            DateTime dValue;
                            if (DateTime.TryParse(item.ToString(), out dValue))
                                text = dValue.ToShortDateString();
                            else
                                text = item.ToString();
                            break;
                        case DataFormatEnum.ShortDate:
                            DateTime sdValue;
                            if (DateTime.TryParse(item.ToString(), out sdValue))
                                text = String.Format("{0:MM/dd}", sdValue);
                            else
                                text = item.ToString();
                            break;
                        case DataFormatEnum.Hour:
                            Decimal hValue;
                            if (Decimal.TryParse(item.ToString(), out hValue))
                                text = hValue % 1 == 0
                                    ? String.Format(CultureInfo.InvariantCulture, "{0:0 H}", hValue)
                                    : String.Format(CultureInfo.InvariantCulture, "{0:0.00 H}", hValue);
                            else
                                text = item.ToString();
                            break;
                        case DataFormatEnum.Expression:
                            text = (!String.IsNullOrWhiteSpace(dataFormatExpression))
                                ? String.Format(dataFormatExpression, item.ToString())
                                : item.ToString();
                            break;
                        default:
                            text = item.ToString();
                            break;
                    }

                    ddlSource.Add(new ListItem(text, item.ToString()));
                }

            return ddlSource;
        }

        /// <summary>
        /// Create the multi-filter query to the <paramref name="query"/> based on the <paramref name="filterExpressions"/>
        /// </summary>
        /// <param name="query">Query where to apply the sortings</param>
        /// <param name="filterExpressions">Filter expressions</param>
        public static IQueryable<T> Filter<T>(this IQueryable<T> query, List<FilterExpression> filterExpressions)
        {
            // Check if have any filter to apply
            if (filterExpressions != null)
            {
                // x =>
                var xParameter = Expression.Parameter(typeof(T), "x");

                foreach (var filterExpression in filterExpressions)
                {
                    // Get the properity type of the column (ie. string, double...)
                    var typeOfPropery = typeof(T).GetProperty(filterExpression.Column).PropertyType;

                    // Left of expression is our property
                    var left = Expression.Property(xParameter, typeof(T).GetProperty(filterExpression.Column));

                    // Convert the filter text to the same type as the column type
                    var o = TypeDescriptor.GetConverter(typeOfPropery).ConvertFrom(filterExpression.Text);

                    // This is going to be our expression to apply on the where
                    Expression expr;

                    // Check if it's a valid operator (IE. for numbers, strings are handled different)
                    ExpressionType expressionType;
                    if (ExpressionType.TryParse(filterExpression.Expression, out expressionType))
                    {
                        // Right side is our constant
                        var right = Expression.Constant(o, typeOfPropery);

                        // Apply .ToLower() if string, so search is no case sensitive
                        var toLowerLeft = typeOfPropery.GetMethod("ToLower", new Type[] { });
                        var toLowerRight = right.Type.GetMethod("ToLower", new Type[] { });

                        // Create the Filter expression
                        expr = (toLowerLeft != null && toLowerRight != null)
                            ? Expression.MakeBinary(expressionType, Expression.Call(left, toLowerLeft), Expression.Call(right, toLowerRight))
                            : Expression.MakeBinary(expressionType, left, right);
                    }
                    else
                    {
                        // This expressions are not native and are applied using a Not
                        if (filterExpression.Expression != "NotContains"
                            && filterExpression.Expression != "NotStartsWith"
                            && filterExpression.Expression != "NotEndsWith")
                        {
                            var method = typeOfPropery.GetMethod(filterExpression.Expression, new[] { typeOfPropery });
                            var tParam = method.GetParameters()[0].ParameterType;

                            // Right side is our constant
                            var right = Expression.Constant(Convert.ChangeType(o, tParam));

                            // Apply .ToLower() if string, so search is no case sensitive
                            var toLowerLeft = typeOfPropery.GetMethod("ToLower", new Type[] { });
                            var toLowerRight = right.Type.GetMethod("ToLower", new Type[] { });

                            // Create the Filter expression
                            expr = (toLowerLeft != null && toLowerRight != null)
                                ? Expression.Call(Expression.Call(left, toLowerLeft), method, Expression.Call(right, toLowerRight))
                                : Expression.Call(left, method, right);
                        }
                        else
                        {
                            var method = typeOfPropery.GetMethod(filterExpression.Expression.Replace("Not", ""), new[] { typeOfPropery });
                            var tParam = method.GetParameters()[0].ParameterType;

                            // Right side is our constant
                            var right = Expression.Constant(Convert.ChangeType(o, tParam));

                            // Apply .ToLower() if string, so search is no case sensitive
                            var toLowerLeft = typeOfPropery.GetMethod("ToLower", new Type[] { });
                            var toLowerRight = right.Type.GetMethod("ToLower", new Type[] { });

                            // Create the Filter expression
                            expr = (toLowerLeft != null && toLowerRight != null)
                                ? Expression.Call(Expression.Call(left, toLowerLeft), method, Expression.Call(right, toLowerRight))
                                : Expression.Call(left, method, right);

                            // Apply the Not to the expression
                            expr = Expression.Not(expr);
                        }
                    }

                    // Call the where methodd with the expression we created
                    var where = Expression.Call(typeof(Queryable),
                        "Where",
                        new Type[] { query.ElementType },
                        query.Expression,
                        Expression.Lambda<Func<T, bool>>(expr, new ParameterExpression[] { xParameter }));

                    // Apply the where to the query
                    query = query.Provider.CreateQuery<T>(where);
                }
            }

            return query;
        }

        /// <summary>
        /// Get the sort direction as a short string
        /// </summary>
        /// <param name="sortDirection">Query where to apply the sortings</param>
        internal static string ToSQLString(this SortDirection sortDirection)
        {
            return (sortDirection == SortDirection.Descending)
                ? "DESC"
                : "ASC";
        }

        /// <summary>
        /// Create the custom pager list
        /// </summary>
        /// <param name="totalRecordCount">Number of records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        internal static List<ListItem> FillPager(int totalRecordCount, int pageIndex, int pageSize)
        {
            var pages = new List<ListItem>();

            var pageCount = (int)Math.Ceiling(((decimal)totalRecordCount / (decimal)pageSize));
            if (pageCount > 1)
            {
                var numberPages = new List<ListItem>();
                ListItem previousDots = null;
                int numberElements = 5;
                int aftli = 0;

                for (int i = 1; i <= pageCount; i++)
                {
                    // Save last one only
                    if (i < (pageIndex + 1) - numberElements)
                        previousDots = new ListItem("...", i.ToString(), i != (pageIndex + 1));
                    else if ((i >= (pageIndex + 1) - numberElements) && (i <= (pageIndex + 1) + numberElements))
                        numberPages.Add(new ListItem(i.ToString(), i.ToString(), i != (pageIndex + 1)));
                    // Save first one
                    else if (i > (pageIndex + 1) + numberElements && aftli == 0)
                    {
                        numberPages.Add(new ListItem("...", i.ToString(), i != (pageIndex + 1)));
                        aftli++;
                    }
                }

                pages.Add(new ListItem("FIRST", "1", pageIndex + 1 > 1));

                // Add previous dots if necessary
                if (previousDots != null)
                    pages.Add(previousDots);

                pages.AddRange(numberPages);
                pages.Add(new ListItem("LAST", pageCount.ToString(), pageIndex + 1 < pageCount));
            }

            return pages;
        }

        /// <summary>
        /// Fill the custom pager size dropdownlist
        /// </summary>
        /// <param name="ddl">Dropdown list to fill</param>
        /// <param name="pagerSelectorOptions">PagerSelectorOptions from the table definition</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="totalRecords">Total records on the table</param>
        internal static void FillPageRecordsSelector(this DropDownList ddl, string[] pagerSelectorOptions, int pageSize, int totalRecords)
        {
            ddl.Items.Clear();
            var items = pagerSelectorOptions.ToList();

            if (!items.Contains(pageSize.ToString())
                && pageSize != totalRecords)
                items.Add(pageSize.ToString());

            // Sort the list of strings
            items.Sort((x, y) =>
            {
                int ix, iy;
                return int.TryParse(x, out ix) && int.TryParse(y, out iy)
                      ? ix.CompareTo(iy)
                      : string.Compare(x, y);
            });

            foreach (var item in items)
                ddl.Items.Add(new ListItem
                {
                    Text = item,
                    Value = item == "All"
                        ? totalRecords.ToString()
                        : item,
                    Selected = item == "All"
                        ? pageSize == totalRecords
                        : pageSize == totalRecords
                            ? false
                            : item == pageSize.ToString()
                });
        }

        /// <summary>
        /// Get the LINQ expression as string from the short filter expression
        /// </summary>
        /// <param name="filterExpression">Short filter expression</param>
        internal static string GetExpressionType(string filterExpression)
        {
            switch (filterExpression)
            {
                case "=":
                    return "Equal";
                case "!=":
                    return "NotEqual";
                case ">":
                    return "GreaterThan";
                case ">=":
                    return "GreaterThanOrEqual";
                case "<":
                    return "LessThan";
                case "<=":
                    return "LessThanOrEqual";
                case "*":
                    return "Contains";
                case "!*":
                    return "NotContains";
                case "˄":
                    return "StartsWith";
                case "!˄":
                    return "NotStartsWith";
                case "˅":
                    return "EndsWith";
                case "!˅":
                    return "NotEndsWith";
                default:
                    return "Equal";
            }
        }

        /// <summary>
        /// Check if the short filter expression is valid
        /// </summary>
        /// <param name="filterExpression">Short filter expression</param>
        internal static bool IsValidExpressionType(string filterExpression)
        {
            string[] arr = new string[] { "=", "!=", ">", ">=", "<", "<=", "*", "!*", "˄", "!˄", "˅", "!˅" };
            return arr.Contains(filterExpression);
        }

        /// <summary>
        /// Export to Excel the <paramref name="source"/> data with the selected <paramref name="columns"/>
        /// </summary>
        /// <param name="source">List with the data</param>
        /// <param name="columns">Selected columns to include on the file</param>
        /// <param name="title">Optional title of the file. If no title, it use 'Export' instead</param>
        public static byte[] ExportExcel(List<dynamic> source, List<ColumnExpression> columns, string title)
        {
            var excel = GetExcel(source, columns, title);

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Buffer = true;
            HttpContext.Current.Response.Charset = "";
            HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            HttpContext.Current.Response.AddHeader("content-disposition", "attachment; filename=" + HttpUtility.UrlEncode((String.IsNullOrWhiteSpace(title) ? "Export" : title) + ".xlsx"));
            HttpContext.Current.Response.BinaryWrite(excel);
            HttpContext.Current.Response.Flush();

            return excel;
        }

        /// <summary>
        /// Get an excel file from the <paramref name="source"/> data with the selected <paramref name="columns"/>
        /// </summary>
        /// <param name="source">List with the data</param>
        /// <param name="columns">Selected columns to include on the file</param>
        /// <param name="title">Optional title of the file. If no title, it use 'Export' instead</param>
        public static byte[] GetExcel(List<dynamic> source, List<ColumnExpression> columns, string title)
        {
            if (String.IsNullOrWhiteSpace(title))
                title = "Export";

            using (var excelFile = new OfficeOpenXml.ExcelPackage())
            {
                //excelFile.Workbook.Properties.Author = "Supplier Invoice Management";
                excelFile.Workbook.Properties.Title = title;
                excelFile.Workbook.Worksheets.Add(title);

                var ws = excelFile.Workbook.Worksheets[1];
                ws.Cells.Style.Font.Size = 11; // Default font size for whole sheet
                ws.Cells.Style.Font.Name = "Calibri"; // Default Font name for whole sheet

                // Create new DataTable.
                var dt = new DataTable();

                // Create new column and add to DataTable.
                PropertyInfo[] properties = source[0].GetType().GetProperties();
                foreach (var column in columns.Where(x => x.Visible))
                {
                    var property = properties.SingleOrDefault(x => x.Name == column.Column);
                    if (property != null)
                        dt.Columns.Add(new DataColumn
                        {
                            ColumnName = property.Name,
                            Caption = column.DisplayName,
                            DataType = Nullable.GetUnderlyingType(
                                property.PropertyType) ?? property.PropertyType
                        });
                }

                // Add the data into the DataTable
                foreach (var item in source)
                {
                    var dr = dt.NewRow();
                    foreach (var dc in dt.Columns)
                    {
                        // Check if the column is visible to incle it or not
                        if (columns.Select(x => new { ColumnName = (string)x.Column, Visible = (bool)x.Visible }).Any(x => x.ColumnName == dc.ToString() && x.Visible))
                            dr[dc.ToString()] = item.GetType().GetProperty(dc.ToString()).GetValue(item, null) ?? DBNull.Value;
                    }

                    dt.Rows.Add(dr);
                }

                // Fill the Excel cells with headers
                ws.Cells.LoadFromDataTable(dt, true);

                // Loop all rows
                for (int i = 1; i <= dt.Rows.Count + 1; i++)
                {
                    // Loop all columns
                    for (int j = 1; j <= dt.Columns.Count; j++)
                    {
                        var cell = ws.Cells[i, j];

                        // Format heading
                        if (i == 1)
                        {
                            cell.Style.Font.Bold = true; // Font Bold
                            cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                            // Setting the background color
                            var fill = cell.Style.Fill;
                            fill.PatternType = ExcelFillStyle.Solid;
                            fill.BackgroundColor.SetColor(Color.SlateGray);

                            // Set border
                            var border = cell.Style.Border;
                            border.Bottom.Style = ExcelBorderStyle.Medium;
                            if (j < dt.Columns.Count)
                                border.Right.Style = ExcelBorderStyle.Thin;

                            // Adding a comment with full name of the column
                            // TODO: Add comment with full column name
                            //cell.AddComment(cell.Value.ToString(), excelFile.Workbook.Properties.Author);
                        }
                        // Format data
                        else
                        {
                            // Setting the background color
                            if (i % 2 == 0)
                            {
                                var fill = cell.Style.Fill;
                                fill.PatternType = ExcelFillStyle.Solid;
                                fill.BackgroundColor.SetColor(Color.WhiteSmoke);
                            }

                            // Setting borders of cell
                            var border = cell.Style.Border;
                            if (j < dt.Columns.Count)
                                border.Right.Style = ExcelBorderStyle.Thin;

                            if (cell.Value is Double)
                            {
                                cell.Style.Numberformat.Format = @"#,##0.00";
                            }
                            else if (cell.Value is DateTime)
                            {
                                cell.Style.Numberformat.Format = @"yyyy-mm-dd";
                            }
                            else if (cell.Value is String)
                            {
                            }
                        }
                    }
                }

                // Create an autofilter for the range
                ws.Cells[1, 1, dt.Rows.Count, dt.Columns.Count].AutoFilter = true;
                // Autofit width
                ws.Cells.AutoFitColumns();

                return excelFile.GetAsByteArray();
            }
        }
    }
}