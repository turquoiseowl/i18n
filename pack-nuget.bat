@echo off
setlocal

set config=Release
set targetfv=v4.0
set warninglevel=0
set gettextversion=gettext-0.18.1.1

:housekeeping
".nuget\NuGet.exe" update -Self
if exist "bin" rmdir "bin" /s /q
mkdir bin
mkdir bin\tools\gettext
mkdir bin\build

goto msbuild

:msbuild
@rem VC11
if not defined VS110COMNTOOLS goto vc-set-2010
if not exist "%VS110COMNTOOLS%\..\..\vc\vcvarsall.bat" goto vc-set-2010
call "%VS110COMNTOOLS%\..\..\vc\vcvarsall.bat"
if not defined VCINSTALLDIR goto msbuild-not-found
goto msbuild-found

:vc-set-2010
@rem VC10
if not defined VS100COMNTOOLS goto msbuild-not-found
if not exist "%VS100COMNTOOLS%\..\..\vc\vcvarsall.bat" goto msbuild-not-found
call "%VS100COMNTOOLS%\..\..\vc\vcvarsall.bat"
if not defined VCINSTALLDIR goto msbuild-not-found
goto msbuild-found

:msbuild-found
@rem Build NET40/NET35
echo Building i18n-net40
msbuild src\i18n.sln /p:TargetFrameworkVersion=%targetfv%;Configuration=%config%;WarningLevel=%warninglevel% /m /t:Rebuild /clp:NoSummary;NoItemAndPropertyList;Verbosity=minimal /nologo
if errorlevel 1 goto exit

:copy-dependencies
@rem Copy all dependencies.

copy LICENSE.md bin
copy README.md bin
copy i18n.targets bin\build
copy "tools\%gettextversion%\*.*" "bin\tools\gettext"

:nuget-pack
".nuget\NuGet.exe" pack i18n.nuspec -BasePath bin

:exit
@rem Done.