if not exist lib                 mkdir lib
if not exist tools               mkdir tools
if not exist content             mkdir content
if not exist content\controllers mkdir content\controllers

:
: i18n.PostBuild
:
:xcopy ..\..\bin\tools\i18n.PostBuild\net40\Release tools\i18n.PostBuild /s/e/i/y

copy ..\..\bin\LICENSE.md content\i18n.LICENSE.md
copy ..\..\bin\README.md  content\i18n.README.md

nuget pack i18n.csproj -IncludeReferencedProjects -Prop Configuration=Release -Symbols -Verbosity Detailed
