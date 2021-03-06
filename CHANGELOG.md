## v2.0.0 (April 16, 2014)
- **Moved JS & CSS to external files**. You'll need to include them on your website.
- **Handle different cultures**. If not defined, it takes the default for the thread.
- **Allow different currency symbols**. Add it to the Source and call it on 'DataFieldCurrencySymbol'.
- **Support Bootstrap V3**. Add 'BootstrapVersion=3' on the GridViewEX table definition.
- The column manager handle now column type 'TemplateField'.
- Popover for coulumn, filter & sorting management now works on click.
- Links with href=# now are buttons.
- 'ItemStyle-CssClass' on checkbox now apply for the filter too.
- Change string concat on JS to StringBuilder.
- Bug fixes.

## v1.3.1 (October 16, 2013)
- Column management cookie size reduced by 13% more.
- Improved speed by not creating the controls if the column is not visible.
- Delete the cookie after use it.
- Bug fixes.

## v1.3.0 (September 18, 2013)
- **Added 3rd level checked on the checkbox column type** (Uses bool? instead of bool).
- **Page size selector 'All'**. Show all records.
- **Ability to disable functionalities**. Like Compact Table, or the views.
- Added new parameter for checkbox column allowing to add a tooltip.
- Fixed "The method 'Skip' is only supported for sorted input in LINQ to Entities. The method 'OrderBy' must be called before the method 'Skip'." when using Entity Framework.
- Column management cookie size reduced by 60%.
- Updated demo project.
- Bug fixes.

## v1.2.1 (April 23, 2013)
- **Added new type of column** to extend the checkbox allowing filters on it.
- **Added demo project**. Shows how to implement a basic example of the GridViewEx.
- Modified Readme.md to show how to use the demo.
- Deleted repeated files added by accident.
- Bug fixed deleting cookie on IE7.

## v1.1.2 (April 17, 2013)
- Disabled export icon when no records to export.
- Disabled filter icon when no records and no filters applied.
- Improved views management adding some checkings.
- Added all basic code documentation.
- Added ability to show an alert message when an action happens, like when a view is saved.
- Tested and working on Windows Azure. Just need to make sure your choose a good session configuration on your Web.Config as described on [https://www.simple-talk.com/cloud/platform-as-a-service/managing-session-state-in-windows-azure-what-are-the-options/](https://www.simple-talk.com/cloud/platform-as-a-service/managing-session-state-in-windows-azure-what-are-the-options/).
- Bug fixes.

## v1.1.1 (April 1, 2013)
- Added basic code documentation.
- Removed massive use of 'this' keyword.
- Project now generate xml with documentation.
- Moved inline JS from search icon to a separate function.
- Created general function for popover of sorting, filters and columns reducing output HTML.
- Sorting management now show the `DisplayName` instead of the `DataField` name of the column.
- Added new parameter called `IsFilterShown` which decide the default behaviour of the inline filters. Also now the show/hide filters maintain the state on AJAX calls.
- Bug fixes.

## v1.1.0 (March 25, 2013)
- **Added view management**. Allow the final user to save views of the current filters, sortings... applied on the table.
- Removed [JSON.NET v4.5.11](http://json.codeplex.com/) dependency (Use now standard built in .NET JavaScriptSerializer which is slower in performance but as JSON is not used a lot the final user won't notice the difference on the speed).
- Clear session data inside the dll.
- Bug fixes.

## v1.0.0 (March 8, 2013)
- **Initial release**