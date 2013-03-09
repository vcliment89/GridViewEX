using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.UI.WebControls;

namespace GridViewEx
{
    [Serializable]
    public class SortExpression
    {
        public string Column;
        public string Direction;
        public string PreviousDirection;
    }

    [Serializable]
    public class FilterExpression
    {
        public string Column;
        public string DisplayName;
        public string Expression;
        public string ExpressionShortName;
        public string Text;
    }

    [Serializable]
    public class ColumnExpression
    {
        public string ColumnName;
        public string DisplayName;
        public bool Visible;
        public string Type;
        public int Index;
        public DataFormatEnum DataFormat;
        public string DataFormatExpression;
    }

    public enum SearchTypeEnum
    {
        None,
        TextBox,
        DropDownList
    }

    public enum DataFormatEnum
    {
        Text,
        Number,
        Percentage,
        Currency,
        Date,
        ShortDate,
        Hour,
        Expression
    }

    public static class Extensions
    {
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
                                    ? String.Format("{0:0%}", pValue)
                                    : String.Format("{0:0.00%}", pValue);
                            else
                                text = item.ToString();
                            break;
                        case DataFormatEnum.Currency:
                            Decimal cValue;
                            if (Decimal.TryParse(item.ToString(), out cValue))
                                text = cValue % 1 == 0
                                    ? String.Format("{0:C0}", cValue)
                                    : String.Format("{0:C}", cValue);
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
                                    ? String.Format("{0:0 H}", hValue)
                                    : String.Format("{0:0.00 H}", hValue);
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

        internal static string ToSQLString(this SortDirection sortDirection)
        {
            return (sortDirection == SortDirection.Descending)
                ? "DESC"
                : "ASC";
        }

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

        internal static void FillPageRecordsSelector(this DropDownList ddl, string[] pagerSelectorOptions, int pageSize)
        {
            ddl.Items.Clear();
            var items = pagerSelectorOptions.ToList();
            if (!items.Contains(pageSize.ToString()))
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
                    Value = item,
                    Selected = item == pageSize.ToString()
                });
        }

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

        internal static bool IsValidExpressionType(string filterExpression)
        {
            string[] arr = new string[] { "=", "!=", ">", ">=", "<", "<=", "*", "!*", "˄", "!˄", "˅", "!˅" };
            return arr.Contains(filterExpression);
        }

        public static void GetExcel(List<dynamic> source, List<ColumnExpression> columns, string title)
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
                    var property = properties.SingleOrDefault(x => x.Name == column.ColumnName);
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
                        if (columns.Select(x => new { ColumnName = (string)x.ColumnName, Visible = (bool)x.Visible }).Any(x => x.ColumnName == dc.ToString() && x.Visible))
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

                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.Buffer = true;
                HttpContext.Current.Response.Charset = "";
                HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
                HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                HttpContext.Current.Response.AddHeader("content-disposition", "attachment;  filename=" + title + ".xlsx");
                HttpContext.Current.Response.BinaryWrite(excelFile.GetAsByteArray());
                HttpContext.Current.Response.Flush();
                HttpContext.Current.Response.End();
            }
        }
    }
}
