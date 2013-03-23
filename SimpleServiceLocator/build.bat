@ECHO OFF

set version=2.2.0
set prereleasePostfix=-beta1
set buildNumber=0


set version_Core=%version%
set version_Packaging=%version_Core%
set version_Extensions=%version_Core%
set version_Integration_Web=%version_Core%
set version_Integration_WebForms=%version_Core%
set version_Integration_Mvc=%version_Core%
set version_Integration_Wcf=%version_Core%
set version_Extensions_LifetimeScoping=%version_Core%



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
set defineConstandsSilverlight=PUBLISH;SILVERLIGHT
set targetPath=bin
set targetPathNet=%targetPath%\NET
set targetPathSilverlight=%targetPath%\Silverlight
set silverlightFrameworkFolder=%PROGRAMFILES(X86)%\Reference Assemblies\Microsoft\Framework\Silverlight\v4.0
set v4targetPlatform="v4,%PROGRAMFILES(X86)%\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0"



set named_version=%version%%prereleasePostfix%

set named_version_Core=%version_Core%%prereleasePostfix%
set named_version_Packaging=%version_Packaging%%prereleasePostfix%
set named_version_Extensions=%version_Extensions%%prereleasePostfix%
set named_version_Integration_Web=%version_Integration_Web%%prereleasePostfix%
set named_version_Integration_WebForms=%version_Integration_WebForms%%prereleasePostfix%
set named_version_Integration_Mvc=%version_Integration_Mvc%%prereleasePostfix%
set named_version_Integration_Wcf=%version_Integration_Wcf%%prereleasePostfix%
set named_version_Extensions_LifetimeScoping=%version_Extensions_LifetimeScoping%%prereleasePostfix%

set numeric_version_Core=%version_Core%.%buildNumber%
set numeric_version_Packaging=%version_Packaging%.%buildNumber%
set numeric_version_Extensions=%version_Extensions%.%buildNumber%
set numeric_version_Integration_Web=%version_Integration_Web%.%buildNumber%
set numeric_version_Integration_WebForms=%version_Integration_WebForms%.%buildNumber%
set numeric_version_Integration_Mvc=%version_Integration_Mvc%.%buildNumber%
set numeric_version_Integration_Wcf=%version_Integration_Wcf%.%buildNumber%
set numeric_version_Extensions_LifetimeScoping=%version_Extensions_LifetimeScoping%.%buildNumber%



mkdir %targetPathNet%

echo BUILD .NET
rmdir %targetPathNet% /s /q
mkdir %targetPathNet%

copy "Shared Assemblies\*.*" %targetPathNet%\*.*


%msbuild% "SimpleInjector.NET\SimpleInjector.NET.csproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstantsNet%"
ren %targetPathNet%\SimpleInjector.dll temp.dll
%ilmerge% %targetPathNet%\temp.dll /ndebug /targetplatform:%v4targetPlatform% /ver:%numeric_version_Core% /out:%targetPathNet%\SimpleInjector.dll /keyfile:SimpleInjector.snk
del %targetPathNet%\temp.dll

%msbuild% "CommonServiceLocator.SimpleInjectorAdapter\CommonServiceLocator.SimpleInjectorAdapter.csproj" /nologo /p:Configuration=Release /p:DefineConstants="%defineConstantsNet%"
ren %targetPathNet%\CommonServiceLocator.SimpleInjectorAdapter.dll temp.dll
%ilmerge% %targetPathNet%\temp.dll /ndebug /targetplatform:%v4targetPlatform% /ver:%numeric_version_Core% /out:%targetPathNet%\CommonServiceLocator.SimpleInjectorAdapter.dll /keyfile:SimpleInjector.snk
del %targetPathNet%\temp.dll

REM %msbuild% "SimpleInjector.Extensions\SimpleInjector.Extensions.csproj" /nologo /p:Configuration=%configuration%
REM ren %targetPathNet%\SimpleInjector.Extensions.dll temp.dll
REM %ilmerge% %targetPathNet%\temp.dll /ndebug /targetplatform:%v4targetPlatform% /ver:%numeric_version_Extensions% /out:%targetPathNet%\SimpleInjector.Extensions.dll /keyfile:SimpleInjector.snk
REM del %targetPathNet%\temp.dll

%msbuild% "SimpleInjector.Packaging\SimpleInjector.Packaging.csproj" /nologo /p:Configuration=%configuration%
ren %targetPathNet%\SimpleInjector.Packaging.dll temp.dll
%ilmerge% %targetPathNet%\temp.dll /ndebug /targetplatform:%v4targetPlatform% /ver:%numeric_version_Packaging% /out:%targetPathNet%\SimpleInjector.Packaging.dll /keyfile:SimpleInjector.snk
del %targetPathNet%\temp.dll

%msbuild% "SimpleInjector.Extensions.LifetimeScoping\SimpleInjector.Extensions.LifetimeScoping.csproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstantsNet%"
ren %targetPathNet%\SimpleInjector.Extensions.LifetimeScoping.dll temp.dll
%ilmerge% %targetPathNet%\temp.dll /ndebug /targetplatform:%v4targetPlatform% /ver:%numeric_version_Extensions_LifetimeScoping% /out:%targetPathNet%\SimpleInjector.Extensions.LifetimeScoping.dll /keyfile:SimpleInjector.snk
del %targetPathNet%\temp.dll

%msbuild% "SimpleInjector.Integration.Web\SimpleInjector.Integration.Web.csproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstantsNet%"
ren %targetPathNet%\SimpleInjector.Integration.Web.dll temp.dll
%ilmerge% %targetPathNet%\temp.dll /ndebug /targetplatform:%v4targetPlatform% /ver:%numeric_version_Integration_Web% /out:%targetPathNet%\SimpleInjector.Integration.Web.dll /keyfile:SimpleInjector.snk
del %targetPathNet%\temp.dll

%msbuild% "SimpleInjector.Integration.Web.WebForms\SimpleInjector.Integration.Web.Forms.csproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstantsNet%"
ren %targetPathNet%\SimpleInjector.Integration.Web.Forms.dll temp.dll
%ilmerge% %targetPathNet%\temp.dll /ndebug /targetplatform:%v4targetPlatform% /ver:%numeric_version_Integration_WebForms% /out:%targetPathNet%\SimpleInjector.Integration.Web.Forms.dll /keyfile:SimpleInjector.snk
del %targetPathNet%\temp.dll

%msbuild% "SimpleInjector.Integration.Web.Mvc\SimpleInjector.Integration.Web.Mvc.csproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstantsNet%"
ren %targetPathNet%\SimpleInjector.Integration.Web.Mvc.dll temp.dll
%ilmerge% %targetPathNet%\temp.dll /ndebug /targetplatform:%v4targetPlatform% /ver:%numeric_version_Integration_Mvc% /out:%targetPathNet%\SimpleInjector.Integration.Web.Mvc.dll /keyfile:SimpleInjector.snk
del %targetPathNet%\temp.dll

%msbuild% "SimpleInjector.Integration.Wcf\SimpleInjector.Integration.Wcf.csproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstantsNet%"
ren %targetPathNet%\SimpleInjector.Integration.Wcf.dll temp.dll
%ilmerge% %targetPathNet%\temp.dll /ndebug /targetplatform:%v4targetPlatform% /ver:%numeric_version_Integration_Wcf% /out:%targetPathNet%\SimpleInjector.Integration.Wcf.dll /keyfile:SimpleInjector.snk
del %targetPathNet%\temp.dll


echo BUILD SILVERLIGHT
rmdir %targetPathSilverlight% /s /q
mkdir %targetPathSilverlight%

copy "Shared Silverlight Assemblies\*.*" %targetPathSilverlight%\*.*

%msbuild32% "SimpleInjector.Silverlight\SimpleInjector.Silverlight.csproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstandsSilverlight%"
ren %targetPathSilverlight%\SimpleInjector.dll temp.dll
%ilmerge% %targetPathSilverlight%\temp.dll /ndebug /targetplatform:v4,"%silverlightFrameworkFolder%" /ver:%numeric_version_Core% /out:%targetPathSilverlight%\SimpleInjector.dll /keyfile:SimpleInjector.snk
del %targetPathSilverlight%\temp.dll

REM %msbuild32% "SimpleInjector.Extensions.Silverlight\SimpleInjector.Extensions.Silverlight.csproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstandsSilverlight%"
REM ren %targetPathSilverlight%\SimpleInjector.Extensions.dll temp.dll
REM %ilmerge% %targetPathSilverlight%\temp.dll /ndebug /targetplatform:v4,"%silverlightFrameworkFolder%" /ver:%numeric_version_Extensions% /out:%targetPathSilverlight%\SimpleInjector.Extensions.dll /keyfile:SimpleInjector.snk
REM del %targetPathSilverlight%\temp.dll

%msbuild32% "CommonServiceLocator.SimpleInjectorAdapter.Silverlight\CommonServiceLocator.SimpleInjectorAdapter.Silverlight.csproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstandsSilverlight%"
ren %targetPathSilverlight%\CommonServiceLocator.SimpleInjectorAdapter.dll temp.dll
%ilmerge% %targetPathSilverlight%\temp.dll /ndebug /targetplatform:v4,"%silverlightFrameworkFolder%" /ver:%numeric_version_Core% /out:%targetPathSilverlight%\CommonServiceLocator.SimpleInjectorAdapter.dll /keyfile:SimpleInjector.snk
del %targetPathSilverlight%\temp.dll


echo BUILD DOCUMENTATION

%msbuild% "SimpleInjector.Documentation\SimpleInjector.Documentation.shfbproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstantsNet%"


mkdir Releases\v%named_version%
mkdir Releases\v%named_version%\.NET
mkdir Releases\v%named_version%\.NET\Documentation
mkdir Releases\v%named_version%\Silverlight
mkdir Releases\v%named_version%\Silverlight\Documentation

copy Help\SimpleInjector.chm Releases\v%named_version%\SimpleInjector.chm
copy Help\SimpleInjector.chm Releases\v%named_version%\.NET\Documentation\SimpleInjector.chm
copy Help\SimpleInjector.chm Releases\v%named_version%\Silverlight\Documentation\SimpleInjector.chm

copy bin\NET\SimpleInjector.dll Releases\v%named_version%\.NET\SimpleInjector.dll
copy bin\NET\SimpleInjector.xml Releases\v%named_version%\.NET\SimpleInjector.xml
copy bin\Silverlight\SimpleInjector.dll Releases\v%named_version%\Silverlight\SimpleInjector.dll
copy bin\Silverlight\SimpleInjector.xml Releases\v%named_version%\Silverlight\SimpleInjector.xml

echo %named_version% >> Releases\v%named_version%\version.txt 

rmdir Releases\temp /s /q


echo CODEPLEX DOWNLOAD .NET

mkdir Releases\temp
copy licence.txt Releases\temp\licence.txt
mkdir Releases\temp\Documentation
copy Help\SimpleInjector.chm Releases\temp\Documentation\SimpleInjector.chm


mkdir Releases\temp\NET40
copy bin\NET\SimpleInjector.dll Releases\temp\NET40\SimpleInjector.dll
copy bin\NET\SimpleInjector.xml Releases\temp\NET40\SimpleInjector.xml

mkdir Releases\temp\NET40\CommonServiceLocator
copy bin\NET\CommonServiceLocator.SimpleInjectorAdapter.dll Releases\temp\NET40\CommonServiceLocator\CommonServiceLocator.SimpleInjectorAdapter.dll
copy bin\NET\CommonServiceLocator.SimpleInjectorAdapter.xml Releases\temp\NET40\CommonServiceLocator\CommonServiceLocator.SimpleInjectorAdapter.xml
copy bin\NET\Microsoft.Practices.ServiceLocation.dll Releases\temp\NET40\CommonServiceLocator\Microsoft.Practices.ServiceLocation.dll
copy bin\NET\Microsoft.Practices.ServiceLocation.xml Releases\temp\NET40\CommonServiceLocator\Microsoft.Practices.ServiceLocation.xml

mkdir Releases\temp\NET40\Extensions
REM copy bin\NET\SimpleInjector.Extensions.dll Releases\temp\NET40\Extensions\SimpleInjector.Extensions.dll
REM copy bin\NET\SimpleInjector.Extensions.xml Releases\temp\NET40\Extensions\SimpleInjector.Extensions.xml
copy bin\NET\SimpleInjector.Packaging.dll Releases\temp\NET40\Extensions\SimpleInjector.Packaging.dll
copy bin\NET\SimpleInjector.Packaging.xml Releases\temp\NET40\Extensions\SimpleInjector.Packaging.xml
copy bin\NET\SimpleInjector.Extensions.LifetimeScoping.dll Releases\temp\NET40\Extensions\SimpleInjector.Extensions.LifetimeScoping.dll
copy bin\NET\SimpleInjector.Extensions.LifetimeScoping.xml Releases\temp\NET40\Extensions\SimpleInjector.Extensions.LifetimeScoping.xml

mkdir Releases\temp\NET40\Integration
copy bin\NET\SimpleInjector.Integration.Web.dll Releases\temp\NET40\Integration\SimpleInjector.Integration.Web.dll
copy bin\NET\SimpleInjector.Integration.Web.xml Releases\temp\NET40\Integration\SimpleInjector.Integration.Web.xml
copy bin\NET\SimpleInjector.Integration.Web.Forms.dll Releases\temp\NET40\Integration\SimpleInjector.Integration.Web.Forms.dll
copy bin\NET\SimpleInjector.Integration.Web.Forms.xml Releases\temp\NET40\Integration\SimpleInjector.Integration.Web.Forms.xml
copy bin\NET\SimpleInjector.Integration.Web.Mvc.dll Releases\temp\NET40\Integration\SimpleInjector.Integration.Web.Mvc.dll
copy bin\NET\SimpleInjector.Integration.Web.Mvc.xml Releases\temp\NET40\Integration\SimpleInjector.Integration.Web.Mvc.xml
copy bin\NET\SimpleInjector.Integration.Wcf.dll Releases\temp\NET40\Integration\SimpleInjector.Integration.Wcf.dll
copy bin\NET\SimpleInjector.Integration.Wcf.xml Releases\temp\NET40\Integration\SimpleInjector.Integration.Wcf.xml
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\SimpleInjector Runtime Library v%named_version%.zip"

rmdir Releases\temp /s /q


echo CODEPLEX DOWNLOAD SILVERLIGHT

mkdir Releases\temp
copy licence.txt Releases\temp\licence.txt
mkdir Releases\temp\Documentation
copy Help\SimpleInjector.chm Releases\temp\Documentation\SimpleInjector.chm
copy bin\Silverlight\SimpleInjector.dll Releases\temp\SimpleInjector.dll
copy bin\Silverlight\SimpleInjector.xml Releases\temp\SimpleInjector.xml
mkdir Releases\temp\CommonServiceLocator
copy bin\Silverlight\CommonServiceLocator.SimpleInjectorAdapter.dll Releases\temp\CommonServiceLocator\CommonServiceLocator.SimpleInjectorAdapter.dll
copy bin\Silverlight\CommonServiceLocator.SimpleInjectorAdapter.xml Releases\temp\CommonServiceLocator\CommonServiceLocator.SimpleInjectorAdapter.xml
copy bin\Silverlight\Microsoft.Practices.ServiceLocation.dll Releases\temp\CommonServiceLocator\Microsoft.Practices.ServiceLocation.dll
copy bin\Silverlight\Microsoft.Practices.ServiceLocation.xml Releases\temp\CommonServiceLocator\Microsoft.Practices.ServiceLocation.xml
REM mkdir Releases\temp\Extensions
REM copy bin\Silverlight\SimpleInjector.Extensions.dll Releases\temp\Extensions\SimpleInjector.Extensions.dll
REM copy bin\Silverlight\SimpleInjector.Extensions.xml Releases\temp\Extensions\SimpleInjector.Extensions.xml
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\SimpleInjector Silverlight Runtime Library v%named_version%.zip"

rmdir Releases\temp /s /q


echo ONLINE DOCUMENTATION

del Help\SimpleInjector.chm
del Help\*.aspx
del Help\*.php
copy Help\Index.html Help\index.tmp
del Help\Index.html
copy Help\index.tmp Help\index.htm
del Help\index.tmp
%compress% "%CD%\Help" "%CD%\Releases\v%named_version%\SimpleInjector Online Documentation v%named_version%.zip"


echo NUGET PACKAGES .NET

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy bin\NET\SimpleInjector.dll Releases\temp\lib\net40-client\SimpleInjector.dll
copy bin\NET\SimpleInjector.xml Releases\temp\lib\net40-client\SimpleInjector.xml
%replace% /source:Releases\temp\SimpleInjector.nuspec {version} %named_version_Core%
%replace% /source:Releases\temp\package\services\metadata\core-properties\c8082e2254fe4defafc3b452026f048d.psmdcp {version} %named_version_Core%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\.NET\SimpleInjector.%named_version_Core%.zip"
ren "%CD%\Releases\v%named_version%\.NET\SimpleInjector.%named_version_Core%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\CommonServiceLocator.SimpleInjectorAdapter Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy bin\NET\CommonServiceLocator.SimpleInjectorAdapter.dll Releases\temp\lib\net40-client\CommonServiceLocator.SimpleInjectorAdapter.dll
copy bin\NET\CommonServiceLocator.SimpleInjectorAdapter.xml Releases\temp\lib\net40-client\CommonServiceLocator.SimpleInjectorAdapter.xml
%replace% /source:Releases\temp\CommonServiceLocator.SimpleInjectorAdapter.nuspec {version} %named_version_Core%
%replace% /source:Releases\temp\package\services\metadata\core-properties\1fea7be7f6324eb68593116ecd0864e4.psmdcp {version} %named_version_Core%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\.NET\CommonServiceLocator.SimpleInjectorAdapter.%named_version_Core%.zip"
ren "%CD%\Releases\v%named_version%\.NET\CommonServiceLocator.SimpleInjectorAdapter.%named_version_Core%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Extensions Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
REM copy bin\NET\SimpleInjector.Extensions.dll Releases\temp\lib\net40-client\SimpleInjector.Extensions.dll
REM copy bin\NET\SimpleInjector.Extensions.xml Releases\temp\lib\net40-client\SimpleInjector.Extensions.xml
%replace% /source:Releases\temp\SimpleInjector.Extensions.nuspec {version} %named_version_Extensions%
%replace% /source:Releases\temp\SimpleInjector.Extensions.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\package\services\metadata\core-properties\3b15d35fbc3a4556960337dcd95cf0f4.psmdcp {version} %named_version_Extensions%
REM %compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Extensions.%named_version_Extensions%.zip"
REM ren "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Extensions.%named_version_Extensions%.zip" "*.nupkg"
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
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Integration.Web.Forms Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy bin\NET\SimpleInjector.Integration.Web.Forms.dll Releases\temp\lib\net40\SimpleInjector.Integration.Web.Forms.dll
copy bin\NET\SimpleInjector.Integration.Web.Forms.xml Releases\temp\lib\net40\SimpleInjector.Integration.Web.Forms.xml
%replace% /source:Releases\temp\SimpleInjector.Integration.Web.Forms.nuspec {version} %named_version_Integration_WebForms%
%replace% /source:Releases\temp\SimpleInjector.Integration.Web.Forms.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\SimpleInjector.Integration.Web.Forms.nuspec {version_Integration_Web} %named_version_Integration_Web%
%replace% /source:Releases\temp\package\services\metadata\core-properties\f5118ffcdd6c4fc48e26b35d803ac086.psmdcp {version} %named_version_Integration_WebForms%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Integration.Web.Forms.%named_version_Integration_WebForms%.zip"
ren "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Integration.Web.Forms.%named_version_Integration_WebForms%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Integration.Web.Forms.QuickStart Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
%replace% /source:Releases\temp\SimpleInjector.Integration.Web.Forms.QuickStart.nuspec {version} %named_version_Integration_WebForms%
%replace% /source:Releases\temp\SimpleInjector.Integration.Web.Forms.QuickStart.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\SimpleInjector.Integration.Web.Forms.QuickStart.nuspec {version_Integration_WebForms} %named_version_Integration_WebForms%
%replace% /source:Releases\temp\package\services\metadata\core-properties\bb004cba2f014ed0b6ded4de9f7d3f1b.psmdcp {version} %named_version_Integration_WebForms%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Integration.Web.Forms.QuickStart.%named_version_Integration_WebForms%.zip"
ren "%CD%\Releases\v%named_version%\.NET\SimpleInjector.Integration.Web.Forms.QuickStart.%named_version_Integration_WebForms%.zip" "*.nupkg"
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


echo NUGET PACKAGES SILVERLIGHT

mkdir Releases\temp
xcopy %nugetTemplatePath%\Silverlight\SimpleInjector.Silverlight Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy bin\Silverlight\SimpleInjector.dll Releases\temp\lib\sl30\SimpleInjector.dll
copy bin\Silverlight\SimpleInjector.xml Releases\temp\lib\sl30\SimpleInjector.xml
%replace% /source:Releases\temp\SimpleInjector.Silverlight.nuspec {version} %named_version_Core%
%replace% /source:Releases\temp\package\services\metadata\core-properties\bc50420a966a46388a7509b095da88af.psmdcp {version} %named_version_Core%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\Silverlight\SimpleInjector.Silverlight.%named_version_Core%.zip"
ren "%CD%\Releases\v%named_version%\Silverlight\SimpleInjector.Silverlight.%named_version_Core%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\Silverlight\CommonServiceLocator.SimpleInjectorAdapter.Silverlight Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy bin\Silverlight\CommonServiceLocator.SimpleInjectorAdapter.dll Releases\temp\lib\sl30\CommonServiceLocator.SimpleInjectorAdapter.dll
copy bin\Silverlight\CommonServiceLocator.SimpleInjectorAdapter.xml Releases\temp\lib\sl30\CommonServiceLocator.SimpleInjectorAdapter.xml
%replace% /source:Releases\temp\CommonServiceLocator.SimpleInjectorAdapter.Silverlight.nuspec {version} %named_version_Core%
%replace% /source:Releases\temp\CommonServiceLocator.SimpleInjectorAdapter.Silverlight.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\package\services\metadata\core-properties\b0dd4e78d398462ead742df1961bccc2.psmdcp {version} %named_version_Core%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\Silverlight\CommonServiceLocator.SimpleInjectorAdapter.Silverlight.%named_version_Core%.zip"
ren "%CD%\Releases\v%named_version%\Silverlight\CommonServiceLocator.SimpleInjectorAdapter.Silverlight.%named_version_Core%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\Silverlight\SimpleInjector.Extensions.Silverlight Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
REM copy bin\Silverlight\SimpleInjector.Extensions.dll Releases\temp\lib\sl30\SimpleInjector.Extensions.dll
REM copy bin\Silverlight\SimpleInjector.Extensions.xml Releases\temp\lib\sl30\SimpleInjector.Extensions.xml
%replace% /source:Releases\temp\SimpleInjector.Extensions.Silverlight.nuspec {version} %named_version_Extensions%
%replace% /source:Releases\temp\SimpleInjector.Extensions.Silverlight.nuspec {versionCore} %named_version_Core%
%replace% /source:Releases\temp\package\services\metadata\core-properties\7ed90488e5714295854ab251e2959afe.psmdcp {version} %named_version_Extensions%
REM %compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\Silverlight\SimpleInjector.Extensions.Silverlight.%named_version_Extensions%.zip"
REM ren "%CD%\Releases\v%named_version%\Silverlight\SimpleInjector.Extensions.Silverlight.%named_version_Extensions%.zip" "*.nupkg"
rmdir Releases\temp /s /q
