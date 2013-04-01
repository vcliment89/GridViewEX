## v1.1.1 (April 1, 2013)
- Added basic code documentation.
- Removed massive use of 'this' keyword.
- Project now generate xml with documentation.
- Moved inline JS from search icon to a separate function.
- Created general function for popover of sortng, filters and columns reducing output HTML.
- Sorting management now show the `DisplayName` instead of the `DataField` name of the column.
- Added new parameter called `IsFilterShown` which decide the default behaviour of the inline filters. Also now the show/hide filters mantain the state on AJAX calls.
- Bug fixes.

## v1.1.0 (March 25, 2013)
- **Added view managemet**. Allow the final user to save views of the current filters, sortings... applied on the table.
- Removed [JSON.NET v4.5.11](http://json.codeplex.com/) dependency (Use now standard built in .NET JavaScriptSerializer which is slower in performance but as JSON is not used a lot the final user won't notice the difference on the speed).
- Clear session data inside the dll.
- Bug fixes.

## v1.0.0 (March 8, 2013)
- **Initial release**