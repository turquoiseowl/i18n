# publish.ps1
#
# i18n Release Preparation Script
#
# Helper script for updating the assembly version numbers according to a specified parameter
# passed to the script.
#
# We follow the NuGet versioning guidelines: https://docs.nuget.org/ndocs/create-packages/prerelease-packages
#
# Run this script from the "i18n\.nuget" directory.
#
# Syntax:
#
#    powershell -file publish.ps1 -ver <ver> [-pre <pre>]
#
# where:
#
#   <ver> specifies a version string e.g. "2.1.9"
#   <pre> specifies a pre-release suffix string (according to NuGet semantics) e.g. "pre001"
#
# Example:
#
#    powershell -file publish.ps1 -ver 2.1.9 -pre pre001
#
#       Above results in a (pre-release) version "2.1.9-pre001"
#
#    powershell -file publish.ps1 -ver 2.1.9
#
#       Above results in a (release) version "2.1.9"
#
Param($ver, $pre)

# Validate params.
if (!$ver) {
    throw "Error: -ver param MUST be specified e.g. -ver 2.1.9" }

# Prefix $pre with hyphen.
if ($pre) {
    $pre = '-' + $pre; }

$verPre = $ver + $pre
    # e.g. "2.1.9" or "2.1.9-pre002"

#
# Helper functions
#

function writemsg($msg)
{
    $msg1 = [System.String]::Format("{0,-200}", $msg) # pad string
    Write-Host -BackgroundColor Green "$(Get-Date –f "yyyy-MM-dd HH:mm:ss") - $msg1"
}
function writeinfo($msg)
{
    $msg1 = [System.String]::Format("{0,-200}", $msg) # pad string
    Write-Host -BackgroundColor Cyan "$(Get-Date –f "yyyy-MM-dd HH:mm:ss") - $msg1"
}
function update_version_string_in_code($path)
    # $path {String}
    #    Path to the directory from which to scan for files to update, relative to the
    #    directory from which the script has been run. E.g. "..\
{
    Get-ChildItem $path -Recurse | ForEach-Object -Process {
        (Get-Content $_) `
            -replace 'version="(\d+)(\.\d+)(\.\d+)(?:(?:\.\d+)?|(\-\w+)?)?"', ('version="' + $ver + '"')`
            -replace 'AssemblyVersion."(\d+)(\.\d+)(\.\d+)(?:(?:\.\d+)?|(\-\w+)?)?"', ('AssemblyVersion("' + $ver + '"')`
            -replace 'AssemblyFileVersion."(\d+)(\.\d+)(\.\d+)(?:(?:\.\d+)?|(\-\w+)?)?"', ('AssemblyFileVersion("' + $ver + '"')`
            -replace 'AssemblyInformationalVersion."(\d+)(\.\d+)(\.\d+)(?:(?:\.\d+)?|(\-\w+)?)?"', ('AssemblyInformationalVersion("' + $verPre + '"')`
        | Out-File $_ -Encoding utf8
    }
}

writeinfo "1. Update vesion number in code files..."
update_version_string_in_code "..\AssemblyInfo.cs"
update_version_string_in_code "..\*.nuspec"

#exit 0

writeinfo "2. Commit changes to current branch..."
$commitMsg = '"Version number bumped to ' + $verPre +'"'
git commit -a -m $commitMsg

writeinfo "3. Rebuild All in Release mode..."
&"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\devenv.com" ..\src\i18n.sln /rebuild `"Release`|Any CPU`"

writeinfo "4. Create nuget packages..."
&nuget pack C:\DevRoot\DevGit\i18n\src\i18n\i18n.csproj                                             -BasePath C:\DevRoot\DevGit\i18n\src\i18n                       -IncludeReferencedProjects -Prop Configuration=Release -Symbols -Verbosity Detailed -Properties OutDir=C:\DevRoot\DevGit\i18n\src\i18n\bin\Release
&nuget pack C:\DevRoot\DevGit\i18n\src\i18n.Adapter.OwinSystemWeb\i18n.Adapter.OwinSystemWeb.csproj -BasePath C:\DevRoot\DevGit\i18n\src\i18n.Adapter.OwinSystemWeb -IncludeReferencedProjects -Prop Configuration=Release -Symbols -Verbosity Detailed -Properties OutDir=C:\DevRoot\DevGit\i18n\src\i18n.Adapter.OwinSystemWeb\bin\Release

writeinfo "5. Publish nuget packages..."
$package = 'i18n.' + $verPre +'.nupkg'
&nuget push $package
$package = 'i18n.Adapter.OwinSystemWeb.' + $verPre +'.nupkg'
&nuget push $package
