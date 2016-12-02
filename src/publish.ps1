# publish.ps1
#
# i18n Release Preparation Script
#
# Helper script for updating the assembly version numbers according to a specified parameter
# passed to the script.
#
# We follow the NuGet versioning guidelines: https://docs.nuget.org/ndocs/create-packages/prerelease-packages
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


function update_version_string_in_code($path)
{
    Get-ChildItem $path -Recurse | ForEach-Object -Process {
        (Get-Content $_) -replace '"(\d+)(\.\d+)(\.\d+)(?:(?:\.\d+)?|(\-\w+)?)?"', ('"' + $ver + $pre + '"') | Set-Content $_
    }
}

update_version_string_in_code "..\AssemblyInfo.cs"
update_version_string_in_code "..\*.nuspec"

