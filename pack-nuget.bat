if not exist "bin" mkdir "bin"
if not exist "bin\content" mkdir "bin\content"
if not exist "bin\lib" mkdir "bin\lib"
if not exist "bin\lib\net40" mkdir "bin\lib\net40"
if not exist "bin\tools" mkdir "bin\tools"
if not exist "bin\tools\gettext-0.14.4" mkdir "bin\tools\gettext-0.14.4"

copy LICENSE.md bin
copy README.md bin

copy "tools\gettext-0.14.4\*.*" "bin\tools\gettext-0.14.4"
copy "tools\init.ps1" "bin\tools"

copy "src\i18n\bin\Release\i18n.dll" "bin\lib\net40"
copy "src\i18n\bin\Release\i18n.xml" "bin\lib\net40"
copy "src\i18n.Domain\bin\Release\i18n.Domain.dll" "bin\lib\net40"
copy "src\i18n.PostBuild\bin\Release\i18n.PostBuild.exe" "bin\lib\net40"
copy "src\i18n.POTGenerator\bin\Release\i18n.POTGenerator.vsix" "bin\tools"

".nuget\NuGet.exe" pack i18n.nuspec -BasePath bin
