@ECHO OFF

set version=4.0.0
set prereleasePostfix=
set buildNumber=0 
set copyrightYear=2017

set version_Core=%version%
set version_Packaging=%version_Core%
set version_Extensions=%version_Core%
set version_Integration_Web=%version_Core%
set version_Integration_WebForms=%version_Core%
set version_Integration_Mvc=%version_Core%
set version_Integration_Wcf=%version_Core%
set version_Integration_WebApi=%version_Core%
set version_Integration_AspNetCore=%version_Core%
set version_Integration_AspNetCore_Mvc_Core=%version_Core%
set version_Integration_AspNetCore_Mvc=%version_Core%

set vsvars32_bat="%programfiles(x86)%\Microsoft Visual Studio 14.0\Common7\Tools\vsvars32.bat"
set msbuild="%programfiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe"

if not exist %vsvars32_bat% goto :vsvars32_bat_missing
if not exist %msbuild% goto :msbuild_exe_missing

call %vsvars32_bat%

set buildToolsPath=BuildTools
set nugetTemplatePath=%buildToolsPath%\NuGet
set ilmerge=%buildToolsPath%\ILMerge.exe
set replace=%buildToolsPath%\replace.exe
set compress=CScript %buildToolsPath%\zip.vbs
set configuration=Release
set defineConstantsNet=PUBLISH
set defineConstantsPcl=PUBLISH;PCL
set targetPath=bin
set targetPathNet=%targetPath%\NET
set targetPathPcl=%targetPath%\PCL
set targetPathCoreClr=%targetPath%\DOTNET

set named_version=%version%%prereleasePostfix%

set named_version_Core=%version_Core%%prereleasePostfix%
set named_version_Integration_AspNetCore=%version_Integration_AspNetCore%%prereleasePostfix%
set named_version_Integration_AspNetCore_Mvc_Core=%version_Integration_AspNetCore_Mvc_Core%%prereleasePostfix%
set named_version_Integration_AspNetCore_Mvc=%version_Integration_AspNetCore_Mvc%%prereleasePostfix%
set named_version_Integration_Wcf=%version_Integration_Wcf%%prereleasePostfix%
set named_version_Integration_Web=%version_Integration_Web%%prereleasePostfix%
set named_version_Integration_Mvc=%version_Integration_Mvc%%prereleasePostfix%
set named_version_Integration_WebApi=%version_Integration_WebApi%%prereleasePostfix%
set named_version_Packaging=%version_Packaging%%prereleasePostfix%

set numeric_version_Core=%version_Core%.%buildNumber%
set numeric_version_Integration_AspNetCore=%version_Integration_AspNetCore%.%buildNumber%
set numeric_version_Integration_AspNetCore_Mvc=%version_Integration_AspNetCore_Mvc%.%buildNumber%
set numeric_version_Integration_Wcf=%version_Integration_Wcf%.%buildNumber%
set numeric_version_Integration_Web=%version_Integration_Web%.%buildNumber%
set numeric_version_Integration_Mvc=%version_Integration_Mvc%.%buildNumber%
set numeric_version_Integration_WebApi=%version_Integration_WebApi%.%buildNumber%
set numeric_version_Packaging=%version_Packaging%.%buildNumber%

if exist Releases\v%named_version% goto :release_directory_already_exists
if not exist SimpleInjector.snk goto :strong_name_key_missing


echo RUNNING TESTS IN PARTIAL TRUST
%msbuild% "SimpleInjector.Tests.Unit\SimpleInjector,Tests.Unit.xproj" /nologo

"SimpleInjector.Tests.Unit\bin\Release\net451\win7-x64\PartialTrustTestRunner.exe" SimpleInjector.Tests.Unit\bin\Release\net451\win7-x64\SimpleInjector.Tests.Unit.dll


echo BUILDING
rmdir %targetPathNet% /s /q
mkdir %targetPathNet%
rmdir %targetPathPcl% /s /q
mkdir %targetPathPcl%
rmdir %targetPathCoreClr% /s /q
mkdir %targetPathCoreClr%

set version_search_line=""version"": ""4.0.0""
set version_replace_line=""version"": ""%named_version_Core%""

echo SET VERSION NUMBERS
%replace% /source:SimpleInjector\project.json "%version_search_line%" """version"": ""%named_version_Core%""" 
%replace% /source:SimpleInjector.Integration.AspNetCore\project.json "%version_search_line%" """version"": ""%named_version_Integration_AspNetCore%"""
%replace% /source:SimpleInjector.Integration.AspNetCore.Mvc.Core\project.json "%version_search_line%" """version"": ""%named_version_Integration_AspNetCore_Mvc_Core%"""
%replace% /source:SimpleInjector.Integration.AspNetCore.Mvc\project.json "%version_search_line%" """version"": ""%named_version_Integration_AspNetCore_Mvc%"""
%replace% /source:SimpleInjector.Integration.Wcf\project.json "%version_search_line%" """version"": ""%named_version_Integration_Wcf%"""
%replace% /source:SimpleInjector.Integration.Web\project.json "%version_search_line%" """version"": ""%named_version_Integration_Web%"""
%replace% /source:SimpleInjector.Integration.Web.Mvc\project.json "%version_search_line%" """version"": ""%named_version_Integration_Mvc%"""
%replace% /source:SimpleInjector.Integration.WebApi\project.json "%version_search_line%" """version"": ""%named_version_Integration_WebApi%"""
%replace% /source:SimpleInjector.Packaging\project.json "%version_search_line%" """version"": ""%named_version_Packaging%"""

echo BUILD PROJECTS
%msbuild% "SimpleInjector\SimpleInjector.xproj" /nologo
%msbuild% "SimpleInjector.Integration.AspNetCore\SimpleInjector.Integration.AspNetCore.xproj" /nologo
%msbuild% "SimpleInjector.Integration.AspNetCore.Mvc.Core\SimpleInjector.Integration.AspNetCore.Mvc.Core.xproj" /nologo
%msbuild% "SimpleInjector.Integration.AspNetCore.Mvc\SimpleInjector.Integration.AspNetCore.Mvc.xproj" /nologo
%msbuild% "SimpleInjector.Integration.Wcf\SimpleInjector.Integration.Wcf.xproj" /nologo
%msbuild% "SimpleInjector.Integration.Web\SimpleInjector.Integration.Web.xproj" /nologo
%msbuild% "SimpleInjector.Integration.Web.Mvc\SimpleInjector.Integration.Web.Mvc.xproj" /nologo
%msbuild% "SimpleInjector.Integration.WebApi\SimpleInjector.Integration.WebApi.xproj" /nologo
%msbuild% "SimpleInjector.Packaging\SimpleInjector.Packaging.xproj" /nologo

echo RESTORE VERSION NUMBERS
%replace% /source:SimpleInjector\project.json """version"": ""%named_version_Core%""" "%version_search_line%" 
%replace% /source:SimpleInjector.Integration.AspNetCore\project.json """version"": ""%named_version_Integration_AspNetCore%""" "%version_search_line%" 
%replace% /source:SimpleInjector.Integration.AspNetCore.Mvc.Core\project.json """version"": ""%named_version_Integration_AspNetCore_Mvc_Core%""" "%version_search_line%" 
%replace% /source:SimpleInjector.Integration.AspNetCore.Mvc\project.json """version"": ""%named_version_Integration_AspNetCore_Mvc%""" "%version_search_line%" 
%replace% /source:SimpleInjector.Integration.Wcf\project.json """version"": ""%named_version_Integration_Wcf%""" "%version_search_line%" 
%replace% /source:SimpleInjector.Integration.Web\project.json """version"": ""%named_version_Integration_Web%""" "%version_search_line%" 
%replace% /source:SimpleInjector.Integration.Web.Mvc\project.json """version"": ""%named_version_Integration_Mvc%""" "%version_search_line%" 
%replace% /source:SimpleInjector.Integration.WebApi\project.json """version"": ""%named_version_Integration_WebApi%""" "%version_search_line%" 
%replace% /source:SimpleInjector.Packaging\project.json """version"": ""%named_version_Packaging%""" "%version_search_line%" 

echo BUILD DOCUMENTATION

%msbuild% "SimpleInjector.Documentation\SimpleInjector.Documentation.shfbproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstantsNet%"

mkdir Releases\v%named_version%
copy Help\SimpleInjector.chm Releases\v%named_version%\SimpleInjector.chm


echo CREATING ONLINE DOCUMENTATION

del Help\SimpleInjector.chm
del Help\*.aspx
del Help\*.php
copy Help\Index.html Help\index.tmp
del Help\Index.html
copy Help\index.tmp Help\index.htm
del Help\index.tmp
REM For some strange reason, the following call does not compress the complete help directory, while calling it manually does work
REM %compress% "%CD%\Help" "%CD%\Releases\v%named_version%\SimpleInjector Online Documentation v%named_version%.zip"


echo CREATING NUGET PACKAGES

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
del Releases\temp\.gitignore /s /q
copy SimpleInjector\bin\Release\net40\SimpleInjector.dll Releases\temp\lib\net40\SimpleInjector.dll
copy SimpleInjector\bin\Release\net40\SimpleInjector.xml Releases\temp\lib\net40\SimpleInjector.xml
copy SimpleInjector\bin\Release\net45\SimpleInjector.dll Releases\temp\lib\net45\SimpleInjector.dll
copy SimpleInjector\bin\Release\net45\SimpleInjector.xml Releases\temp\lib\net45\SimpleInjector.xml
copy SimpleInjector\bin\Release\netstandard1.0\SimpleInjector.dll "Releases\temp\lib\netstandard1.0\SimpleInjector.dll"
copy SimpleInjector\bin\Release\netstandard1.0\SimpleInjector.xml "Releases\temp\lib\netstandard1.0\SimpleInjector.xml"
copy SimpleInjector\bin\Release\netstandard1.3\SimpleInjector.dll "Releases\temp\lib\netstandard1.3\SimpleInjector.dll"
copy SimpleInjector\bin\Release\netstandard1.3\SimpleInjector.xml "Releases\temp\lib\netstandard1.3\SimpleInjector.xml"
%replace% /source:Releases\temp\SimpleInjector.nuspec {version} %named_version_Core%
%replace% /source:Releases\temp\SimpleInjector.nuspec {year} %copyrightYear%
%replace% /source:Releases\temp\package\services\metadata\core-properties\c8082e2254fe4defafc3b452026f048d.psmdcp {version} %named_version_Core%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\SimpleInjector.%named_version_Core%.zip"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Packaging Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
del Releases\temp\.gitignore /s /q
copy SimpleInjector.Packaging\bin\Release\net40\SimpleInjector.Packaging.dll "Releases\temp\lib\net40\SimpleInjector.Packaging.dll"
copy SimpleInjector.Packaging\bin\Release\net40\SimpleInjector.Packaging.xml "Releases\temp\lib\net40\SimpleInjector.Packaging.xml"
copy SimpleInjector.Packaging\bin\Release\netstandard1.0\SimpleInjector.Packaging.dll "Releases\temp\lib\netstandard1.0\SimpleInjector.Packaging.dll"
copy SimpleInjector.Packaging\bin\Release\netstandard1.0\SimpleInjector.Packaging.xml "Releases\temp\lib\netstandard1.0\SimpleInjector.Packaging.xml"
%replace% /source:Releases\temp\SimpleInjector.Packaging.nuspec {version} %named_version_Packaging%
%replace% /source:Releases\temp\SimpleInjector.Packaging.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\SimpleInjector.Packaging.nuspec {year} %copyrightYear%
%replace% /source:Releases\temp\package\services\metadata\core-properties\4d447eef3ba54c2da48c4d25f475fcbe.psmdcp {version} %named_version_Packaging%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\SimpleInjector.Packaging.%named_version_Packaging%.zip"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Integration.Web Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
del Releases\temp\.gitignore /s /q
copy SimpleInjector.Integration.Web\bin\Release\net40\SimpleInjector.Integration.Web.dll Releases\temp\lib\net40\SimpleInjector.Integration.Web.dll
copy SimpleInjector.Integration.Web\bin\Release\net40\SimpleInjector.Integration.Web.xml Releases\temp\lib\net40\SimpleInjector.Integration.Web.xml
%replace% /source:Releases\temp\SimpleInjector.Integration.Web.nuspec {version} %named_version_Integration_Web%
%replace% /source:Releases\temp\SimpleInjector.Integration.Web.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\SimpleInjector.Integration.Web.nuspec {year} %copyrightYear%
%replace% /source:Releases\temp\package\services\metadata\core-properties\fb4dd696b20548afa09bcbbf3ea6c7d0.psmdcp {version} %named_version_Integration_Web%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\SimpleInjector.Integration.Web.%named_version_Integration_Web%.zip"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Integration.Web.Mvc Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
del Releases\temp\.gitignore /s /q
copy SimpleInjector.Integration.Web.Mvc\bin\Release\net40\SimpleInjector.Integration.Web.Mvc.dll Releases\temp\lib\net40\SimpleInjector.Integration.Web.Mvc.dll
copy SimpleInjector.Integration.Web.Mvc\bin\Release\net40\SimpleInjector.Integration.Web.Mvc.xml Releases\temp\lib\net40\SimpleInjector.Integration.Web.Mvc.xml
%replace% /source:Releases\temp\SimpleInjector.Integration.Web.Mvc.nuspec {version} %named_version_Integration_Mvc%
%replace% /source:Releases\temp\SimpleInjector.Integration.Web.Mvc.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\SimpleInjector.Integration.Web.Mvc.nuspec {version_Integration_Web} %named_version_Integration_Web%
%replace% /source:Releases\temp\SimpleInjector.Integration.Web.Mvc.nuspec {year} %copyrightYear%
%replace% /source:Releases\temp\package\services\metadata\core-properties\916f6977dc7a462395f59f05d2cc6a76.psmdcp {version} %named_version_Integration_Mvc%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\SimpleInjector.Integration.Web.Mvc.%named_version_Integration_Mvc%.zip"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Integration.Web.Mvc.QuickStart Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
del Releases\temp\.gitignore /s /q
%replace% /source:Releases\temp\SimpleInjector.MVC3.nuspec {version} %named_version_Integration_Mvc%
%replace% /source:Releases\temp\SimpleInjector.MVC3.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\SimpleInjector.MVC3.nuspec {version_Integration_Mvc} %named_version_Integration_Mvc%
%replace% /source:Releases\temp\SimpleInjector.MVC3.nuspec {year} %copyrightYear%
%replace% /source:Releases\temp\package\services\metadata\core-properties\7594fa13b1164869a9b2b67b8b5ad9a3.psmdcp {version} %named_version_Integration_Mvc%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\SimpleInjector.MVC3.%named_version_Integration_Mvc%.zip"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Integration.Wcf Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
del Releases\temp\.gitignore /s /q
copy SimpleInjector.Integration.Wcf\bin\Release\net40\SimpleInjector.Integration.Wcf.dll Releases\temp\lib\net40\SimpleInjector.Integration.Wcf.dll
copy SimpleInjector.Integration.Wcf\bin\Release\net40\SimpleInjector.Integration.Wcf.xml Releases\temp\lib\net40\SimpleInjector.Integration.Wcf.xml
%replace% /source:Releases\temp\SimpleInjector.Integration.Wcf.nuspec {version} %named_version_Integration_Wcf%
%replace% /source:Releases\temp\SimpleInjector.Integration.Wcf.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\SimpleInjector.Integration.Wcf.nuspec {year} %copyrightYear%
%replace% /source:Releases\temp\package\services\metadata\core-properties\13850374b87d467da12f21ca32dac632.psmdcp {version} %named_version_Integration_Wcf%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\SimpleInjector.Integration.Wcf.%named_version_Integration_Wcf%.zip"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Integration.Wcf.QuickStart Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
del Releases\temp\.gitignore /s /q
%replace% /source:Releases\temp\SimpleInjector.Integration.Wcf.QuickStart.nuspec {version} %named_version_Integration_Wcf%
%replace% /source:Releases\temp\SimpleInjector.Integration.Wcf.QuickStart.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\SimpleInjector.Integration.Wcf.QuickStart.nuspec {version_Integration_Wcf} %named_version_Integration_Wcf%
%replace% /source:Releases\temp\SimpleInjector.Integration.Wcf.QuickStart.nuspec {year} %copyrightYear%
%replace% /source:Releases\temp\package\services\metadata\core-properties\a47c8b1328f541fc8a5cd4c0446d5fe1.psmdcp {version} %named_version_Integration_Wcf%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\SimpleInjector.Integration.Wcf.QuickStart.%named_version_Integration_Wcf%.zip"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Integration.WebApi Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
del Releases\temp\.gitignore /s /q
copy SimpleInjector.Integration.WebApi\bin\Release\net45\SimpleInjector.Integration.WebApi.dll Releases\temp\lib\net45\SimpleInjector.Integration.WebApi.dll
copy SimpleInjector.Integration.WebApi\bin\Release\net45\SimpleInjector.Integration.WebApi.xml Releases\temp\lib\net45\SimpleInjector.Integration.WebApi.xml
%replace% /source:Releases\temp\SimpleInjector.Integration.WebApi.nuspec {version} %named_version_Integration_WebApi%
%replace% /source:Releases\temp\SimpleInjector.Integration.WebApi.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\SimpleInjector.Integration.WebApi.nuspec {year} %copyrightYear%
%replace% /source:Releases\temp\package\services\metadata\core-properties\a3b1f940a1584e868b3266ff38ba4e08.psmdcp {version} %named_version_Integration_WebApi%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\SimpleInjector.Integration.WebApi.%named_version_Integration_WebApi%.zip"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Integration.WebApi.WebHost.QuickStart Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
del Releases\temp\.gitignore /s /q
%replace% /source:Releases\temp\SimpleInjector.Integration.WebApi.WebHost.QuickStart.nuspec {version} %named_version_Integration_WebApi%
%replace% /source:Releases\temp\SimpleInjector.Integration.WebApi.WebHost.QuickStart.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\SimpleInjector.Integration.WebApi.WebHost.QuickStart.nuspec {version_Integration_WebApi} %named_version_Integration_WebApi%
%replace% /source:Releases\temp\SimpleInjector.Integration.WebApi.WebHost.QuickStart.nuspec {year} %copyrightYear%
%replace% /source:Releases\temp\package\services\metadata\core-properties\28cf0010982e4a44bd982823a7b4b6be.psmdcp {version} %named_version_Integration_WebApi%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\SimpleInjector.Integration.WebApi.WebHost.QuickStart.%named_version_Integration_WebApi%.zip"
rmdir Releases\temp /s /q

ren "%CD%\Releases\v%named_version%\*.zip" "*.nupkg"

copy "SimpleInjector.Integration.AspNetCore\bin\Release\SimpleInjector.Integration.AspNetCore.%named_version_Integration_AspNetCore%.nupkg" Releases\v%named_version%\
copy "SimpleInjector.Integration.AspNetCore.Mvc.Core\bin\Release\SimpleInjector.Integration.AspNetCore.Mvc.Core.%named_version_Integration_AspNetCore_Mvc_Core%.nupkg" Releases\v%named_version%\
copy "SimpleInjector.Integration.AspNetCore.Mvc\bin\Release\SimpleInjector.Integration.AspNetCore.Mvc.%named_version_Integration_AspNetCore_Mvc%.nupkg" Releases\v%named_version%\

echo Done!

GOTO :EOF

:release_directory_already_exists
echo The release directory already exists.
GOTO :EOF

:strong_name_key_missing
echo The strong name key SimpleInjector.snk does not exist. You should generate (a fake) one for this build script to work.
GOTO :EOF

:msbuild_exe_missing
echo Couldn't locate MSBuild.exe. Expected  it  to be here: %msbuild%
GOTO :EOF

:vsvars32_bat_missing
echo Couldn't locate vsvars32.bat. Expected it to be here: %vsvars32_bat%
GOTO :EOF