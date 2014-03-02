@ECHO OFF

set version=2.5.0
set prereleasePostfix=
set buildNumber=0


set version_Core=%version%
set version_Packaging=%version_Core%
set version_Extensions=%version_Core%
set version_Integration_Web=%version_Core%
set version_Integration_WebForms=%version_Core%
set version_Integration_Mvc=%version_Core%
set version_Integration_Wcf=%version_Core%
set version_Integration_WebApi=%version_Core%
set version_Extensions_LifetimeScoping=%version_Core%
set version_Extensions_ExecutionContextScoping=%version_Core%


call "%PROGRAMFILES%\Microsoft Visual Studio 10.0\Common7\Tools\vsvars32.bat"

set msbuild=%SYSTEMROOT%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe
set msbuild32=%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
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
set targetPathSilverlight=%targetPath%\Silverlight
set silverlightFrameworkFolder=%PROGRAMFILES(X86)%\Reference Assemblies\Microsoft\Framework\Silverlight\v4.0
set v4targetPlatform="v4,%PROGRAMFILES(X86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0"
set v45targetPlatform="v4,%PROGRAMFILES(X86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5"
set net40ClientProfile=TargetFrameworkVersion=v4.0;TargetFrameworkProfile=Client;DefineConstants="%defineConstantsNet%";Configuration=%configuration%
set net40FullProfile=TargetFrameworkVersion=v4.0;TargetFrameworkProfile=;DefineConstants="%defineConstantsNet%";Configuration=%configuration%
set net45Profile=TargetFrameworkVersion=v4.5;TargetFrameworkProfile=;DefineConstants="NET45;%defineConstantsNet%";Configuration=%configuration%

set named_version=%version%%prereleasePostfix%

set named_version_Core=%version_Core%%prereleasePostfix%
set named_version_Packaging=%version_Packaging%%prereleasePostfix%
set named_version_Extensions=%version_Extensions%%prereleasePostfix%
set named_version_Integration_Web=%version_Integration_Web%%prereleasePostfix%
set named_version_Integration_WebForms=%version_Integration_WebForms%%prereleasePostfix%
set named_version_Integration_Mvc=%version_Integration_Mvc%%prereleasePostfix%
set named_version_Integration_Wcf=%version_Integration_Wcf%%prereleasePostfix%
set named_version_Integration_WebApi=%version_Integration_WebApi%%prereleasePostfix%
set named_version_Extensions_LifetimeScoping=%version_Extensions_LifetimeScoping%%prereleasePostfix%
set named_version_Extensions_ExecutionContextScoping=%version_Extensions_ExecutionContextScoping%%prereleasePostfix%

set numeric_version_Core=%version_Core%.%buildNumber%
set numeric_version_Packaging=%version_Packaging%.%buildNumber%
set numeric_version_Extensions=%version_Extensions%.%buildNumber%
set numeric_version_Integration_Web=%version_Integration_Web%.%buildNumber%
set numeric_version_Integration_WebForms=%version_Integration_WebForms%.%buildNumber%
set numeric_version_Integration_Mvc=%version_Integration_Mvc%.%buildNumber%
set numeric_version_Integration_Wcf=%version_Integration_Wcf%.%buildNumber%
set numeric_version_Integration_WebApi=%version_Integration_WebApi%.%buildNumber%
set numeric_version_Extensions_LifetimeScoping=%version_Extensions_LifetimeScoping%.%buildNumber%
set numeric_version_Extensions_ExecutionContextScoping=%version_Extensions_ExecutionContextScoping%.%buildNumber%


if not exist SimpleInjector.snk goto :strong_name_key_missing

echo BUILDING
rmdir %targetPathNet% /s /q
mkdir %targetPathNet%
rmdir %targetPathPcl% /s /q
mkdir %targetPathPcl%

copy "Shared Assemblies\*.*" %targetPathPcl%\*.*

%msbuild% "SimpleInjector.NET\SimpleInjector.NET.csproj" /nologo /p:%net40ClientProfile% /p:VersionNumber=%numeric_version_Core%
ren %targetPathNet%\SimpleInjector.dll SimpleInjector_40.dll
ren %targetPathNet%\SimpleInjector.xml SimpleInjector_40.xml

%msbuild% "SimpleInjector.NET\SimpleInjector.NET.csproj" /nologo /p:%net45Profile% /p:VersionNumber=%numeric_version_Core%

%msbuild% "SimpleInjector.Packaging\SimpleInjector.Packaging.csproj" /nologo /p:%net40ClientProfile% /p:VersionNumber=%numeric_version_Packaging%
%msbuild% "SimpleInjector.Extensions.LifetimeScoping\SimpleInjector.Extensions.LifetimeScoping.csproj" /nologo /p:%net40ClientProfile% /p:VersionNumber=%numeric_version_Extensions_LifetimeScoping%
%msbuild% "SimpleInjector.Extensions.ExecutionContextScoping\SimpleInjector.Extensions.ExecutionContextScoping.csproj" /nologo /p:%net45Profile% /p:VersionNumber=%numeric_version_Extensions_ExecutionContextScoping%
%msbuild% "SimpleInjector.Integration.Web\SimpleInjector.Integration.Web.csproj" /nologo /p:%net40FullProfile% /p:VersionNumber=%numeric_version_Integration_Web%
%msbuild% "SimpleInjector.Integration.Web.Mvc\SimpleInjector.Integration.Web.Mvc.csproj" /nologo /p:%net40FullProfile% /p:VersionNumber=%numeric_version_Integration_Mvc%
%msbuild% "SimpleInjector.Integration.Wcf\SimpleInjector.Integration.Wcf.csproj" /nologo /p:%net40FullProfile% /p:VersionNumber=%numeric_version_Integration_Wcf%
%msbuild% "SimpleInjector.Integration.WebApi\SimpleInjector.Integration.WebApi.csproj" /nologo /p:%net45Profile% /p:VersionNumber=%numeric_version_Integration_WebApi%

%msbuild% "SimpleInjector.PCL\SimpleInjector.PCL.csproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstantsPcl%" /p:VersionNumber=%numeric_version_Core%
%msbuild% "SimpleInjector.Diagnostics\SimpleInjector.Diagnostics.csproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstantsPcl%" /p:VersionNumber=%numeric_version_Core%
%msbuild% "CommonServiceLocator.SimpleInjectorAdapter\CommonServiceLocator.SimpleInjectorAdapter.csproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstantsPcl%" /p:VersionNumber=%numeric_version_Core%

REM Build a .NET version of the Diagnostics. This is needed for the Documentation project.
%msbuild% "SimpleInjector.Diagnostics\SimpleInjector.Diagnostics.Net.csproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstantsNet%" /p:VersionNumber=%numeric_version_Core%

echo BUILD DOCUMENTATION

%msbuild% "SimpleInjector.Documentation\SimpleInjector.Documentation.shfbproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstantsNet%"


mkdir Releases\v%named_version%
mkdir Releases\v%named_version%\.NET
mkdir Releases\v%named_version%\.NET\Documentation

copy Help\SimpleInjector.chm Releases\v%named_version%\SimpleInjector.chm
copy Help\SimpleInjector.chm Releases\v%named_version%\.NET\Documentation\SimpleInjector.chm
copy Help\SimpleInjector.chm Releases\v%named_version%\Portable\Documentation\SimpleInjector.chm

copy bin\NET\SimpleInjector.dll Releases\v%named_version%\.NET\SimpleInjector.dll
copy bin\NET\SimpleInjector.xml Releases\v%named_version%\.NET\SimpleInjector.xml
copy bin\PCL\SimpleInjector.Diagnostics.dll Releases\v%named_version%\.NET\SimpleInjector.Diagnostics.dll
copy bin\PCL\SimpleInjector.Diagnostics.xml Releases\v%named_version%\.NET\SimpleInjector.Diagnostics.xml

echo %named_version% >> Releases\v%named_version%\version.txt 

rmdir Releases\temp /s /q


echo CREATING CODEPLEX DOWNLOAD

mkdir Releases\temp
copy licence.txt Releases\temp\licence.txt
mkdir Releases\temp\Documentation
copy Help\SimpleInjector.chm Releases\temp\Documentation\SimpleInjector.chm


mkdir Releases\temp\NET45
copy bin\NET\SimpleInjector.dll Releases\temp\NET45\SimpleInjector.dll
copy bin\NET\SimpleInjector.xml Releases\temp\NET45\SimpleInjector.xml
copy bin\PCL\SimpleInjector.Diagnostics.dll Releases\temp\NET45\SimpleInjector.Diagnostics.dll
copy bin\PCL\SimpleInjector.Diagnostics.xml Releases\temp\NET45\SimpleInjector.Diagnostics.xml

mkdir Releases\temp\NET40
copy bin\NET\SimpleInjector_40.dll Releases\temp\NET40\SimpleInjector.dll
copy bin\NET\SimpleInjector_40.xml Releases\temp\NET40\SimpleInjector.xml
copy bin\PCL\SimpleInjector.Diagnostics.dll Releases\temp\NET40\SimpleInjector.Diagnostics.dll
copy bin\PCL\SimpleInjector.Diagnostics.xml Releases\temp\NET40\SimpleInjector.Diagnostics.xml

mkdir Releases\temp\NET45\CommonServiceLocator
copy bin\PCL\CommonServiceLocator.SimpleInjectorAdapter.dll Releases\temp\NET45\CommonServiceLocator\CommonServiceLocator.SimpleInjectorAdapter.dll
copy bin\PCL\CommonServiceLocator.SimpleInjectorAdapter.xml Releases\temp\NET45\CommonServiceLocator\CommonServiceLocator.SimpleInjectorAdapter.xml
copy bin\PCL\Microsoft.Practices.ServiceLocation.dll Releases\temp\NET45\CommonServiceLocator\Microsoft.Practices.ServiceLocation.dll
copy bin\PCL\Microsoft.Practices.ServiceLocation.xml Releases\temp\NET45\CommonServiceLocator\Microsoft.Practices.ServiceLocation.xml

mkdir Releases\temp\NET40\CommonServiceLocator
copy bin\PCL\CommonServiceLocator.SimpleInjectorAdapter.dll Releases\temp\NET40\CommonServiceLocator\CommonServiceLocator.SimpleInjectorAdapter.dll
copy bin\PCL\CommonServiceLocator.SimpleInjectorAdapter.xml Releases\temp\NET40\CommonServiceLocator\CommonServiceLocator.SimpleInjectorAdapter.xml
copy bin\PCL\Microsoft.Practices.ServiceLocation.dll Releases\temp\NET40\CommonServiceLocator\Microsoft.Practices.ServiceLocation.dll
copy bin\PCL\Microsoft.Practices.ServiceLocation.xml Releases\temp\NET40\CommonServiceLocator\Microsoft.Practices.ServiceLocation.xml

mkdir Releases\temp\NET45\Extensions
copy bin\NET\SimpleInjector.Packaging.dll Releases\temp\NET45\Extensions\SimpleInjector.Packaging.dll
copy bin\NET\SimpleInjector.Packaging.xml Releases\temp\NET45\Extensions\SimpleInjector.Packaging.xml
copy bin\NET\SimpleInjector.Extensions.LifetimeScoping.dll Releases\temp\NET45\Extensions\SimpleInjector.Extensions.LifetimeScoping.dll
copy bin\NET\SimpleInjector.Extensions.LifetimeScoping.xml Releases\temp\NET45\Extensions\SimpleInjector.Extensions.LifetimeScoping.xml
copy bin\NET\SimpleInjector.Extensions.ExecutionContextScoping.dll Releases\temp\NET45\Extensions\SimpleInjector.Extensions.ExecutionContextScoping.dll
copy bin\NET\SimpleInjector.Extensions.ExecutionContextScoping.xml Releases\temp\NET45\Extensions\SimpleInjector.Extensions.ExecutionContextScoping.xml

mkdir Releases\temp\NET40\Extensions
copy bin\NET\SimpleInjector.Packaging.dll Releases\temp\NET40\Extensions\SimpleInjector.Packaging.dll
copy bin\NET\SimpleInjector.Packaging.xml Releases\temp\NET40\Extensions\SimpleInjector.Packaging.xml
copy bin\NET\SimpleInjector.Extensions.LifetimeScoping.dll Releases\temp\NET40\Extensions\SimpleInjector.Extensions.LifetimeScoping.dll
copy bin\NET\SimpleInjector.Extensions.LifetimeScoping.xml Releases\temp\NET40\Extensions\SimpleInjector.Extensions.LifetimeScoping.xml

mkdir Releases\temp\NET45\Integration
copy bin\NET\SimpleInjector.Integration.Web.dll Releases\temp\NET45\Integration\SimpleInjector.Integration.Web.dll
copy bin\NET\SimpleInjector.Integration.Web.xml Releases\temp\NET45\Integration\SimpleInjector.Integration.Web.xml
copy bin\NET\SimpleInjector.Integration.Web.Mvc.dll Releases\temp\NET45\Integration\SimpleInjector.Integration.Web.Mvc.dll
copy bin\NET\SimpleInjector.Integration.Web.Mvc.xml Releases\temp\NET45\Integration\SimpleInjector.Integration.Web.Mvc.xml
copy bin\NET\SimpleInjector.Integration.Wcf.dll Releases\temp\NET45\Integration\SimpleInjector.Integration.Wcf.dll
copy bin\NET\SimpleInjector.Integration.Wcf.xml Releases\temp\NET45\Integration\SimpleInjector.Integration.Wcf.xml
copy bin\NET\SimpleInjector.Integration.WebApi.dll Releases\temp\NET45\Integration\SimpleInjector.Integration.WebApi.dll
copy bin\NET\SimpleInjector.Integration.WebApi.xml Releases\temp\NET45\Integration\SimpleInjector.Integration.WebApi.xml

mkdir Releases\temp\NET40\Integration
copy bin\NET\SimpleInjector.Integration.Web.dll Releases\temp\NET40\Integration\SimpleInjector.Integration.Web.dll
copy bin\NET\SimpleInjector.Integration.Web.xml Releases\temp\NET40\Integration\SimpleInjector.Integration.Web.xml
copy bin\NET\SimpleInjector.Integration.Web.Mvc.dll Releases\temp\NET40\Integration\SimpleInjector.Integration.Web.Mvc.dll
copy bin\NET\SimpleInjector.Integration.Web.Mvc.xml Releases\temp\NET40\Integration\SimpleInjector.Integration.Web.Mvc.xml
copy bin\NET\SimpleInjector.Integration.Wcf.dll Releases\temp\NET40\Integration\SimpleInjector.Integration.Wcf.dll
copy bin\NET\SimpleInjector.Integration.Wcf.xml Releases\temp\NET40\Integration\SimpleInjector.Integration.Wcf.xml

mkdir Releases\temp\Portable
copy bin\PCL\SimpleInjector.dll Releases\temp\Portable\SimpleInjector.dll
copy bin\PCL\SimpleInjector.xml Releases\temp\Portable\SimpleInjector.xml
copy bin\PCL\SimpleInjector.Diagnostics.dll Releases\temp\Portable\SimpleInjector.Diagnostics.dll
copy bin\PCL\SimpleInjector.Diagnostics.xml Releases\temp\Portable\SimpleInjector.Diagnostics.xml

mkdir Releases\temp\Portable\CommonServiceLocator
copy bin\PCL\CommonServiceLocator.SimpleInjectorAdapter.dll Releases\temp\Portable\CommonServiceLocator\CommonServiceLocator.SimpleInjectorAdapter.dll
copy bin\PCL\CommonServiceLocator.SimpleInjectorAdapter.xml Releases\temp\Portable\CommonServiceLocator\CommonServiceLocator.SimpleInjectorAdapter.xml
copy bin\PCL\Microsoft.Practices.ServiceLocation.dll Releases\temp\Portable\CommonServiceLocator\Microsoft.Practices.ServiceLocation.dll
copy bin\PCL\Microsoft.Practices.ServiceLocation.xml Releases\temp\Portable\CommonServiceLocator\Microsoft.Practices.ServiceLocation.xml
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\SimpleInjector Runtime Library v%named_version%.zip"

rmdir Releases\temp /s /q


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
copy bin\NET\SimpleInjector.dll Releases\temp\lib\net45\SimpleInjector.dll
copy bin\NET\SimpleInjector.xml Releases\temp\lib\net45\SimpleInjector.xml
copy bin\PCL\SimpleInjector.Diagnostics.dll Releases\temp\lib\net45\SimpleInjector.Diagnostics.dll
copy bin\PCL\SimpleInjector.Diagnostics.xml Releases\temp\lib\net45\SimpleInjector.Diagnostics.xml
copy bin\NET\SimpleInjector_40.dll Releases\temp\lib\net40-client\SimpleInjector.dll
copy bin\NET\SimpleInjector_40.xml Releases\temp\lib\net40-client\SimpleInjector.xml
copy bin\PCL\SimpleInjector.Diagnostics.dll Releases\temp\lib\net40-client\SimpleInjector.Diagnostics.dll
copy bin\PCL\SimpleInjector.Diagnostics.xml Releases\temp\lib\net40-client\SimpleInjector.Diagnostics.xml
copy bin\PCL\SimpleInjector.dll "Releases\temp\lib\portable-net4+sl4+wp8+win8\SimpleInjector.dll"
copy bin\PCL\SimpleInjector.xml "Releases\temp\lib\portable-net4+sl4+wp8+win8\SimpleInjector.xml"
copy bin\PCL\SimpleInjector.Diagnostics.dll "Releases\temp\lib\portable-net4+sl4+wp8+win8\SimpleInjector.Diagnostics.dll"
copy bin\PCL\SimpleInjector.Diagnostics.xml "Releases\temp\lib\portable-net4+sl4+wp8+win8\SimpleInjector.Diagnostics.xml"
%replace% /source:Releases\temp\SimpleInjector.nuspec {version} %named_version_Core%
%replace% /source:Releases\temp\package\services\metadata\core-properties\c8082e2254fe4defafc3b452026f048d.psmdcp {version} %named_version_Core%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\.NET\SimpleInjector.%named_version_Core%.zip"
ren "%CD%\Releases\v%named_version%\.NET\SimpleInjector.%named_version_Core%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\CommonServiceLocator.SimpleInjectorAdapter Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy bin\PCL\CommonServiceLocator.SimpleInjectorAdapter.dll "Releases\temp\lib\portable-net4+sl4+wp8+win8\CommonServiceLocator.SimpleInjectorAdapter.dll"
copy bin\PCL\CommonServiceLocator.SimpleInjectorAdapter.xml "Releases\temp\lib\portable-net4+sl4+wp8+win8\CommonServiceLocator.SimpleInjectorAdapter.xml"
%replace% /source:Releases\temp\CommonServiceLocator.SimpleInjectorAdapter.nuspec {version} %named_version_Core%
%replace% /source:Releases\temp\package\services\metadata\core-properties\1fea7be7f6324eb68593116ecd0864e4.psmdcp {version} %named_version_Core%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\.NET\CommonServiceLocator.SimpleInjectorAdapter.%named_version_Core%.zip"
ren "%CD%\Releases\v%named_version%\.NET\CommonServiceLocator.SimpleInjectorAdapter.%named_version_Core%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Packaging Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy bin\NET\SimpleInjector.Packaging.dll Releases\temp\lib\net40-client\SimpleInjector.Packaging.dll
copy bin\NET\SimpleInjector.Packaging.xml Releases\temp\lib\net40-client\SimpleInjector.Packaging.xml
%replace% /source:Releases\temp\SimpleInjector.Packaging.nuspec {version} %named_version_Packaging%
%replace% /source:Releases\temp\SimpleInjector.Packaging.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\package\services\metadata\core-properties\4d447eef3ba54c2da48c4d25f475fcbe.psmdcp {version} %named_version_Packaging%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Packaging.%named_version_Packaging%.zip"
ren "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Packaging.%named_version_Packaging%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Extensions.LifetimeScoping Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy bin\NET\SimpleInjector.Extensions.LifetimeScoping.dll Releases\temp\lib\net40-client\SimpleInjector.Extensions.LifetimeScoping.dll
copy bin\NET\SimpleInjector.Extensions.LifetimeScoping.xml Releases\temp\lib\net40-client\SimpleInjector.Extensions.LifetimeScoping.xml
%replace% /source:Releases\temp\SimpleInjector.Extensions.LifetimeScoping.nuspec {version} %named_version_Extensions_LifetimeScoping%
%replace% /source:Releases\temp\SimpleInjector.Extensions.LifetimeScoping.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\package\services\metadata\core-properties\3c829585afae419fa2b861a3b473739c.psmdcp {version} %named_version_Extensions_LifetimeScoping%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Extensions.LifetimeScoping.%named_version_Extensions_LifetimeScoping%.zip"
ren "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Extensions.LifetimeScoping.%named_version_Extensions_LifetimeScoping%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Extensions.ExecutionContextScoping Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy bin\NET\SimpleInjector.Extensions.ExecutionContextScoping.dll Releases\temp\lib\net45\SimpleInjector.Extensions.ExecutionContextScoping.dll
copy bin\NET\SimpleInjector.Extensions.ExecutionContextScoping.xml Releases\temp\lib\net45\SimpleInjector.Extensions.ExecutionContextScoping.xml
%replace% /source:Releases\temp\SimpleInjector.Extensions.ExecutionContextScoping.nuspec {version} %named_version_Extensions_ExecutionContextScoping%
%replace% /source:Releases\temp\SimpleInjector.Extensions.ExecutionContextScoping.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\package\services\metadata\core-properties\418513f6bda44f0aaa7ad35e612de928.psmdcp {version} %named_version_Extensions_ExecutionContextScoping%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Extensions.ExecutionContextScoping.%named_version_Extensions_ExecutionContextScoping%.zip"
ren "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Extensions.ExecutionContextScoping.%named_version_Extensions_ExecutionContextScoping%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Integration.Web Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy bin\NET\SimpleInjector.Integration.Web.dll Releases\temp\lib\net40\SimpleInjector.Integration.Web.dll
copy bin\NET\SimpleInjector.Integration.Web.xml Releases\temp\lib\net40\SimpleInjector.Integration.Web.xml
%replace% /source:Releases\temp\SimpleInjector.Integration.Web.nuspec {version} %named_version_Integration_Web%
%replace% /source:Releases\temp\SimpleInjector.Integration.Web.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\package\services\metadata\core-properties\fb4dd696b20548afa09bcbbf3ea6c7d0.psmdcp {version} %named_version_Integration_Web%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Integration.Web.%named_version_Integration_Web%.zip"
ren "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Integration.Web.%named_version_Integration_Web%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Integration.Web.Mvc Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy bin\NET\SimpleInjector.Integration.Web.Mvc.dll Releases\temp\lib\net40\SimpleInjector.Integration.Web.Mvc.dll
copy bin\NET\SimpleInjector.Integration.Web.Mvc.xml Releases\temp\lib\net40\SimpleInjector.Integration.Web.Mvc.xml
%replace% /source:Releases\temp\SimpleInjector.Integration.Web.Mvc.nuspec {version} %named_version_Integration_Mvc%
%replace% /source:Releases\temp\SimpleInjector.Integration.Web.Mvc.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\SimpleInjector.Integration.Web.Mvc.nuspec {version_Integration_Web} %named_version_Integration_Web%
%replace% /source:Releases\temp\package\services\metadata\core-properties\916f6977dc7a462395f59f05d2cc6a76.psmdcp {version} %named_version_Integration_Mvc%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Integration.Web.Mvc.%named_version_Integration_Mvc%.zip"
ren "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Integration.Web.Mvc.%named_version_Integration_Mvc%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Integration.Web.Mvc.QuickStart Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
%replace% /source:Releases\temp\SimpleInjector.MVC3.nuspec {version} %named_version_Integration_Mvc%
%replace% /source:Releases\temp\SimpleInjector.MVC3.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\SimpleInjector.MVC3.nuspec {version_Integration_Mvc} %named_version_Integration_Mvc%
%replace% /source:Releases\temp\package\services\metadata\core-properties\7594fa13b1164869a9b2b67b8b5ad9a3.psmdcp {version} %named_version_Integration_Mvc%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\.NET\SimpleInjector.MVC3.%named_version_Integration_Mvc%.zip"
ren "%CD%\Releases\v%named_version%\.NET\SimpleInjector.MVC3.%named_version_Integration_Mvc%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Integration.Wcf Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy bin\NET\SimpleInjector.Integration.Wcf.dll Releases\temp\lib\net40\SimpleInjector.Integration.Wcf.dll
copy bin\NET\SimpleInjector.Integration.Wcf.xml Releases\temp\lib\net40\SimpleInjector.Integration.Wcf.xml
%replace% /source:Releases\temp\SimpleInjector.Integration.Wcf.nuspec {version} %named_version_Integration_Wcf%
%replace% /source:Releases\temp\SimpleInjector.Integration.Wcf.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\package\services\metadata\core-properties\13850374b87d467da12f21ca32dac632.psmdcp {version} %named_version_Integration_Wcf%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Integration.Wcf.%named_version_Integration_Wcf%.zip"
ren "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Integration.Wcf.%named_version_Integration_Wcf%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Integration.Wcf.QuickStart Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
%replace% /source:Releases\temp\SimpleInjector.Integration.Wcf.QuickStart.nuspec {version} %named_version_Integration_Wcf%
%replace% /source:Releases\temp\SimpleInjector.Integration.Wcf.QuickStart.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\SimpleInjector.Integration.Wcf.QuickStart.nuspec {version_Integration_Wcf} %named_version_Integration_Wcf%
%replace% /source:Releases\temp\package\services\metadata\core-properties\a47c8b1328f541fc8a5cd4c0446d5fe1.psmdcp {version} %named_version_Integration_Wcf%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Integration.Wcf.QuickStart.%named_version_Integration_Wcf%.zip"
ren "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Integration.Wcf.QuickStart.%named_version_Integration_Wcf%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Integration.WebApi Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy bin\NET\SimpleInjector.Integration.WebApi.dll Releases\temp\lib\net45\SimpleInjector.Integration.WebApi.dll
copy bin\NET\SimpleInjector.Integration.WebApi.xml Releases\temp\lib\net45\SimpleInjector.Integration.WebApi.xml
%replace% /source:Releases\temp\SimpleInjector.Integration.WebApi.nuspec {version} %named_version_Integration_WebApi%
%replace% /source:Releases\temp\SimpleInjector.Integration.WebApi.nuspec {version_Extensions_ExecutionContextScoping} %named_version_Extensions_ExecutionContextScoping%
%replace% /source:Releases\temp\SimpleInjector.Integration.WebApi.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\package\services\metadata\core-properties\a3b1f940a1584e868b3266ff38ba4e08.psmdcp {version} %named_version_Integration_WebApi%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Integration.WebApi.%named_version_Integration_WebApi%.zip"
ren "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Integration.WebApi.%named_version_Integration_WebApi%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Integration.WebApi.WebHost.QuickStart Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
%replace% /source:Releases\temp\SimpleInjector.Integration.WebApi.WebHost.QuickStart.nuspec {version} %named_version_Integration_WebApi%
%replace% /source:Releases\temp\SimpleInjector.Integration.WebApi.WebHost.QuickStart.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\SimpleInjector.Integration.WebApi.WebHost.QuickStart.nuspec {version_Integration_WebApi} %named_version_Integration_WebApi%
%replace% /source:Releases\temp\package\services\metadata\core-properties\28cf0010982e4a44bd982823a7b4b6be.psmdcp {version} %named_version_Integration_WebApi%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Integration.WebApi.WebHost.QuickStart.%named_version_Integration_WebApi%.zip"
ren "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Integration.WebApi.WebHost.QuickStart.%named_version_Integration_WebApi%.zip" "*.nupkg"
rmdir Releases\temp /s /q


echo Done!

GOTO :EOF


:strong_name_key_missing
echo The strong name key SimpleInjector.snk does not exist. You should generate (a fake) one for this build script to work.
GOTO :EOF

