param($installPath, $toolsPath, $package, $project)

if (-not $toolsPath) { throw "toolsPath parameter wasn't specified" }

$vsixFileName = "i18n.POTGenerator.vsix"

$vsxInstaller = [System.IO.Path]::Combine($toolsPath, $vsixFileName)
Start-Process -FilePath $vsxInstaller 
