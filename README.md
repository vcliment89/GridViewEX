[![githalytics.com alpha](https://cruel-carlota.pagodabox.com/ae50e99ffb68962588c0be3b387931dc "githalytics.com")](http://githalytics.com/vcliment89/GridViewEX)
<a href="https://github.com/vcliment89/GridViewEX/">
  <img src="http://cdn1.iconfinder.com/data/icons/cc_mono_icon_set/blacks/48x48/2x2_grid.png">
</a>

GridViewEX
==========

GridViewEX is a .NET extended Gridview lot more functional and powerful than the original, created and maintained by [Vicent Climent](http://www.linkedin.com/in/vcliment89/en).

## Main features
 * Ability to run multiple sorting on the same table each one with different sort order. No restriction on the limit, but of course the order of the sorting matter.
 * Built in inline filters and filter expressions with ability to find text or select from a dropdown list very easy to implement all options.
 * Improved default .NET Gridview pager with one more functional allowing first and last page selection. Also added option to allow user to change the page size on live time.
 * Excel export of all results with filters, orders and column selection applied by the user.
 * Management of the above functions (Columns, filters & sorts).

## Dependencies
 * [jQuery v1.8.3](https://github.com/jquery/jquery)
 * [jQuery UI v1.9.2](https://github.com/jquery/jquery)
 * [Bootstrap v2.2.1](https://github.com/twitter/bootstrap)
 * [EPPlus v3.1.3.3](http://epplus.codeplex.com/)

## How to run the demo
 1. Download the [AdventureWorks Sample DB](http://msftdbprodsamples.codeplex.com/releases/view/93587)
 2. Copy the `*.mdf` and `*.ldf` files to `/demo/Demo/App_Data/`
 3. Modify and execute the script on `/demo/Demo/App_Data/InstallDB.sql`
 4. Check the ConnectionString and run the solution on your browser

## Bug tracker

Have a bug or a feature request? [Please open a new issue](https://github.com/vcliment89/GridViewEX/issues). Before opening any issue, please search for existing issues and read the [Issue Guidelines](https://github.com/necolas/issue-guidelines), written by [Nicolas Gallagher](https://github.com/necolas/).

## Versioning

For transparency and insight into the release cycle, and for striving to maintain backward compatibility, GridViewEX will be maintained under the Semantic Versioning guidelines.

Releases will be numbered with the following format:

`<major>.<minor>.<patch>`

And constructed with the following guidelines:

* Breaking backward compatibility bumps the major (and resets the minor and patch)
* New additions without breaking backward compatibility bumps the minor (and resets the patch)
* Bug fixes and misc changes bumps the patch

For more information on SemVer, please visit [http://semver.org/](http://semver.org/).

## Copyright and license

Copyright (c) 2013 Vicent Climent.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this work except in compliance with the License.
You may obtain a copy of the License in the LICENSE file, or at:

  [http://www.apache.org/licenses/LICENSE-2.0](http://www.apache.org/licenses/LICENSE-2.0)

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
