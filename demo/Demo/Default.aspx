<%@ Page Language="C#" CodeBehind="Default.aspx.cs" Inherits="Demo.Default" %>

<!DOCTYPE html>
<html xml:lang="en">
<head id="Head1" runat="server">
    <title></title>
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta http-equiv="X-UA-Compatible" content="IE=edge">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <link rel="stylesheet" href="~/Content/bootstrap.css" type="text/css" />
    <link rel="stylesheet" href="~/Content/bootstrap-responsive.css" type="text/css" />
    <link rel="stylesheet" href="~/Content/datepicker.css" type="text/css" />
    <link rel="stylesheet" href="~/Content/themes/base/minified/jquery-ui.min.css" type="text/css" />
    <link rel="stylesheet" href="~/Content/style.css" type="text/css" />
    <script src='<%= Page.ResolveClientUrl("~/Scripts/jquery-1.8.3.js")%>' type="text/javascript"></script>
    <script src='<%= Page.ResolveClientUrl("~/Scripts/jquery-ui-1.9.2.js")%>' type="text/javascript"></script>
    <script src='<%= Page.ResolveClientUrl("~/Scripts/bootstrap.js")%>' type="text/javascript"></script>
    <script src='<%= Page.ResolveClientUrl("~/Scripts/bootstrap-datepicker.js")%>' type="text/javascript"></script>
    <script src='<%= Page.ResolveClientUrl("~/Scripts/common.js")%>' type="text/javascript"></script>
</head>
<body>
    <form id="form" runat="server">
        <asp:ScriptManager ID="ScriptManager" runat="server" />
        <div id="wrap">
            <div class="container">
                <%--MENU--%>
                <div class="navbar">
                    <div class="navbar-inner">
                        <div class="container">
                            <a class="btn btn-navbar" data-toggle="collapse" data-target=".nav-collapse"><span
                                class="icon-bar"></span><span class="icon-bar"></span><span class="icon-bar"></span>
                            </a><a class="brand" href="https://github.com/vcliment89/GridViewEX" runat="server">GridViewEX</a>
                        </div>
                        <!-- .container -->
                    </div>
                    <!-- .navbar-inner -->
                </div>
                <!-- .navbar -->
                <div id="content">
                    <div class="row">
                        <div class="span12">
                            <asp:UpdatePanel UpdateMode="Conditional" runat="server">
                                <ContentTemplate>
                                    <uc:GridViewEx ID="gvexPersons" LoadingImageUrl="~/Content/images/ajax-loader.gif"
                                        Title="Persons Report" TableHover="true" TableStriped="true" OnSorting="gvexPersons_Sorting"
                                        OnSortingChanged="gvexPersons_SortingChanged" OnFilterDeleted="gvexPersons_FilterDeleted"
                                        OnPageChanged="gvexPersons_PageChanged" OnExcelExport="gvexPersons_ExcelExport"
                                        OnViewChanged="gvexPersons_ViewChanged" OnColumnSelectionChanged="gvexPersons_ColumnSelectionChanged" runat="server">
                                        <Columns>
                                            <%--Person Type--%>
                                            <uc:ColumnEx HeaderText="Person Type" DataField="PersonType" SearchType="DropDownList"
                                                OnFilterApplied="gvexPersons_FilterApplied" />
                                            <%--Name Style--%>
                                            <uc:CheckBoxEx HeaderText="Name Style" DataField="NameStyle" OnFilterApplied="gvexPersons_FilterApplied" />
                                            <%--Title--%>
                                            <uc:ColumnEx HeaderText="Title" HeaderToolTip="Title of the person" DataField="Title"
                                                SearchType="TextBox" OnFilterApplied="gvexPersons_FilterApplied" />
                                            <%--First Name--%>
                                            <uc:ColumnEx HeaderText="First Name" DataField="FirstName"
                                                SearchType="TextBox" OnFilterApplied="gvexPersons_FilterApplied" />
                                            <%--Middle Name--%>
                                            <uc:ColumnEx HeaderText="Middle Name" HeaderToolTip="Middle Name if any" DataField="MiddleName"
                                                SearchType="TextBox" OnFilterApplied="gvexPersons_FilterApplied" />
                                            <%--Last Name--%>
                                            <uc:ColumnEx HeaderText="Last Name" DataField="LastName"
                                                SearchType="TextBox" OnFilterApplied="gvexPersons_FilterApplied" />
                                            <%--Suffix--%>
                                            <uc:ColumnEx HeaderText="Suffix" DataField="Suffix" NullDisplayText="N/A" NullDisplayColor="Red" NullDisplayBold="true" SearchType="TextBox"
                                                OnFilterApplied="gvexPersons_FilterApplied" />
                                            <%--Email Promotion--%>
                                            <uc:ColumnEx HeaderText="Email Promotion" DataField="EmailPromotion" SearchType="TextBox" DataFormat="Number"
                                                OnFilterApplied="gvexPersons_FilterApplied" />
                                        </Columns>
                                    </uc:GridViewEx>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                            <div class="btn-holder">
                                <a href="#" onclick="checkAll(this, '<%= gvexPersons.ClientID + "NameStyle" %>', true);" class="btn btn-primary pull-left">Select All (Name Style)</a>
                                <a href='javascript: history.go(-1);' class="btn btn-link pull-right">Back</a>
                                <div class="clearfix">
                                </div>
                            </div>
                        </div>
                    </div>
                    <!-- .row -->
                </div>
                <!-- #content -->
            </div>
            <!-- .container -->
            <div id="push">
            </div>
        </div>
        <!-- #wrap -->
        <div id="footer">
            <div class="container">
                <div class="content">
                    <div class="row">
                        <div class="span12">
                            <div class="pull-right">
                                Have a bug or a feature request?
                            <asp:HyperLink NavigateUrl="https://github.com/vcliment89/GridViewEX/issues" Text="Please open a new issue"
                                runat="server" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <script type="text/javascript">
            /**
            * Check all the visible checkboxes from the selected column
            * @param link                Hyperlink object who call the function
            * @param cbClass {String}    CSS Class name
            * @param checked {bool}      True if check False if not
            */
            function checkAll(link, cbClass, checked) {
                $('.' + cbClass).prop('checked', checked);

                if (checked)
                    $(link).text('Unselect All (Name Style)').attr('onclick', 'checkAll(this, \'' + cbClass + '\', false);');
                else
                    $(link).text('Select All (Name Style)').attr('onclick', 'checkAll(this, \'' + cbClass + '\', true);');
            }
        </script>
    </form>
</body>
</html>
