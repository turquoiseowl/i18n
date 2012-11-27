if not exist "bin" mkdir "bin"
if not exist "bin\content" mkdir "bin\content"
if not exist "bin\tools" mkdir "bin\tools"
if not exist "bin\tools\gettext-0.14.4" mkdir "bin\tools\gettext-0.14.4"
copy LICENSE.md bin
copy README.md bin
copy "tools\gettext-0.14.4\*.*" "bin\tools\gettext-0.14.4"
".nuget\NuGet.exe" pack i18n.nuspec -BasePath bin