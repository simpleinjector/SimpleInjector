@ECHO OFF
REM This bat file allows publishing packages to Nuget.
REM Usage:
REM publish2nuget.bat [version] [NuGet API key]

IF "%1"=="" (
    ECHO Please supply the version number and API key that should be published to nuget. Example: "%0 1.6.2-beta4 00000000-0000-0000-0000-000000000000"
    GOTO :EOF
)
IF "%2"=="" (
    ECHO Please supply the version number and API key that should be published to nuget. Example: "%0 1.6.2-beta4 00000000-0000-0000-0000-000000000000"
    GOTO :EOF
)

set version=%1
set superSecretApiKey=%2
set packageDirectory=Releases\v%version%
set options=%superSecretApiKey% -Verbosity detailed -source https://www.nuget.org/api/v2/package
set nugetexe=..\..\nuget.exe

IF NOT EXIST %packageDirectory% (
    ECHO The directory "%packageDirectory%" could not be found.
    GOTO :EOF
)

IF NOT EXIST %nugetexe% (
    echo %nugetexe% not found. Please download nuget.exe from https://www.nuget.org/nuget.exe.
    GOTO :EOF
)

%nugetexe% push %packageDirectory%\*.%version%.nupkg %options%


echo Done!