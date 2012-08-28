@ECHO OFF

set version=1.5.0.12199
set versionCore=1.5.0.12199
set version_Integration_Mvc=1.5.0.12199
set version_Extensions=1.5.0.12238
set version_Extensions_LifetimeScoping=1.5.0.12199

call "%PROGRAMFILES%\Microsoft Visual Studio 10.0\Common7\Tools\vsvars32.bat"


set msbuild=%SYSTEMROOT%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe
set msbuild32=%SYSTEMROOT%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
set buildToolsPath=BuildTools
set nugetTemplatePath=%buildToolsPath%\NuGet
set ilmerge=%buildToolsPath%\ILMerge.exe
set sandcastlegui=%buildToolsPath%\SandcastleGUI.exe
set replace=%buildToolsPath%\replace.exe
set compress=CScript %buildToolsPath%\zip.vbs
set configuration=Release
set defineConstantsNet=PUBLISH
set defineConstandsSilverlight=PUBLISH;SILVERLIGHT
set targetPath=bin
set targetPathNet=%targetPath%\NET
set targetPathSilverlight=%targetPath%\Silverlight\
set silverlightFrameworkFolder=%PROGRAMFILES(X86)%\Reference Assemblies\Microsoft\Framework\Silverlight\v4.0

mkdir %targetPathNet%

echo BUILD .NET
rmdir %targetPathNet% /s /q
mkdir %targetPathNet%

copy "Shared Assemblies\*.*" %targetPathNet%\*.*

%msbuild% "SimpleInjector.NET\SimpleInjector.NET.csproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstantsNet%"
ren %targetPathNet%\SimpleInjector.dll temp.dll
%ilmerge% %targetPathNet%\temp.dll /ndebug /ver:%versionCore% /out:%targetPathNet%\SimpleInjector.dll /keyfile:SimpleInjector.snk
del %targetPathNet%\temp.dll

%msbuild% "CommonServiceLocator.SimpleInjectorAdapter\CommonServiceLocator.SimpleInjectorAdapter.csproj" /nologo /p:Configuration=Release /p:DefineConstants="%defineConstantsNet%"
ren %targetPathNet%\CommonServiceLocator.SimpleInjectorAdapter.dll temp.dll
%ilmerge% %targetPathNet%\temp.dll /ndebug /ver:%versionCore% /out:%targetPathNet%\CommonServiceLocator.SimpleInjectorAdapter.dll /keyfile:SimpleInjector.snk
del %targetPathNet%\temp.dll

%msbuild% "SimpleInjector.Extensions\SimpleInjector.Extensions.csproj" /nologo /p:Configuration=%configuration%
ren %targetPathNet%\SimpleInjector.Extensions.dll temp.dll
%ilmerge% %targetPathNet%\temp.dll /ndebug /ver:%version_Extensions% /out:%targetPathNet%\SimpleInjector.Extensions.dll /keyfile:SimpleInjector.snk
del %targetPathNet%\temp.dll

%msbuild% "SimpleInjector.Packaging\SimpleInjector.Packaging.csproj" /nologo /p:Configuration=%configuration%
ren %targetPathNet%\SimpleInjector.Packaging.dll temp.dll
%ilmerge% %targetPathNet%\temp.dll /ndebug /ver:%version% /out:%targetPathNet%\SimpleInjector.Packaging.dll /keyfile:SimpleInjector.snk
del %targetPathNet%\temp.dll

%msbuild% "SimpleInjector.Extensions.LifetimeScoping\SimpleInjector.Extensions.LifetimeScoping.csproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstantsNet%"
ren %targetPathNet%\SimpleInjector.Extensions.LifetimeScoping.dll temp.dll
%ilmerge% %targetPathNet%\temp.dll /ndebug /targetplatform:v4 /ver:%version_Extensions_LifetimeScoping% /out:%targetPathNet%\SimpleInjector.Extensions.LifetimeScoping.dll /keyfile:SimpleInjector.snk
del %targetPathNet%\temp.dll

%msbuild% "SimpleInjector.Integration.Web\SimpleInjector.Integration.Web.csproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstantsNet%"
ren %targetPathNet%\SimpleInjector.Integration.Web.dll temp.dll
%ilmerge% %targetPathNet%\temp.dll /ndebug /targetplatform:v4 /ver:%version% /out:%targetPathNet%\SimpleInjector.Integration.Web.dll /keyfile:SimpleInjector.snk
del %targetPathNet%\temp.dll

%msbuild% "SimpleInjector.Integration.Web.WebForms\SimpleInjector.Integration.Web.WebForms.csproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstantsNet%"
ren %targetPathNet%\SimpleInjector.Integration.Web.WebForms.dll temp.dll
%ilmerge% %targetPathNet%\temp.dll /ndebug /ver:%version% /out:%targetPathNet%\SimpleInjector.Integration.Web.WebForms.dll /keyfile:SimpleInjector.snk
del %targetPathNet%\temp.dll

%msbuild% "SimpleInjector.Integration.Web.Mvc\SimpleInjector.Integration.Web.Mvc.csproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstantsNet%"
ren %targetPathNet%\SimpleInjector.Integration.Web.Mvc.dll temp.dll
%ilmerge% %targetPathNet%\temp.dll /ndebug /targetplatform:v4 /ver:%version_Integration_Mvc% /out:%targetPathNet%\SimpleInjector.Integration.Web.Mvc.dll /keyfile:SimpleInjector.snk
del %targetPathNet%\temp.dll


echo BUILD SILVERLIGHT
rmdir %targetPathSilverlight% /s /q
mkdir %targetPathSilverlight%

copy "Shared Silverlight Assemblies\*.*" %targetPathSilverlight%\*.*

%msbuild32% "SimpleInjector.Silverlight\SimpleInjector.Silverlight.csproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstandsSilverlight%"
ren %targetPathSilverlight%\SimpleInjector.dll temp.dll
%ilmerge% %targetPathSilverlight%\temp.dll /ndebug /targetplatform:v4,"%silverlightFrameworkFolder%" /ver:%versionCore% /out:%targetPathSilverlight%\SimpleInjector.dll /keyfile:SimpleInjector.snk
del %targetPathSilverlight%\temp.dll

%msbuild32% "SimpleInjector.Extensions.Silverlight\SimpleInjector.Extensions.Silverlight.csproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstandsSilverlight%"
ren %targetPathSilverlight%\SimpleInjector.Extensions.dll temp.dll
%ilmerge% %targetPathSilverlight%\temp.dll /ndebug /targetplatform:v4,"%silverlightFrameworkFolder%" /ver:%version_Extensions% /out:%targetPathSilverlight%\SimpleInjector.Extensions.dll /keyfile:SimpleInjector.snk
del %targetPathSilverlight%\temp.dll

%msbuild32% "CommonServiceLocator.SimpleInjectorAdapter.Silverlight\CommonServiceLocator.SimpleInjectorAdapter.Silverlight.csproj" /nologo /p:Configuration=%configuration% /p:DefineConstants="%defineConstandsSilverlight%"
ren %targetPathSilverlight%\CommonServiceLocator.SimpleInjectorAdapter.Silverlight.dll temp.dll
%ilmerge% %targetPathSilverlight%\temp.dll /ndebug /targetplatform:v4,"%silverlightFrameworkFolder%" /ver:%versionCore% /out:%targetPathSilverlight%\CommonServiceLocator.SimpleInjectorAdapter.dll /keyfile:SimpleInjector.snk
del %targetPathSilverlight%\temp.dll


echo BUILD DOCUMENTATION

mkdir Help
mkdir Help.Asm

copy bin\NET\SimpleInjector.dll Help.Asm\SimpleInjector.dll
copy bin\NET\SimpleInjector.xml Help.Asm\SimpleInjector.xml
copy bin\NET\SimpleInjector.Extensions.dll Help.Asm\SimpleInjector.Extensions.dll
copy bin\NET\SimpleInjector.Extensions.xml Help.Asm\SimpleInjector.Extensions.xml


%sandcastlegui% /document %buildToolsPath%\SimpleInjector.SandcastleGUI
ren Help\Documentation.chm SimpleInjector.chm
ren Help\Presentation.css presentation.xxx
ren Help\presentation.xxx presentation.css
rmdir Help.Asm /q /s


mkdir Releases\v%version%
mkdir Releases\v%version%\.NET
mkdir Releases\v%version%\.NET\Documentation
mkdir Releases\v%version%\Silverlight
mkdir Releases\v%version%\Silverlight\Documentation

copy Help\SimpleInjector.chm Releases\v%version%\SimpleInjector.chm
copy Help\SimpleInjector.chm Releases\v%version%\.NET\Documentation\SimpleInjector.chm
copy Help\SimpleInjector.chm Releases\v%version%\Silverlight\Documentation\SimpleInjector.chm

copy bin\NET\SimpleInjector.dll Releases\v%version%\.NET\SimpleInjector.dll
copy bin\NET\SimpleInjector.xml Releases\v%version%\.NET\SimpleInjector.xml
copy bin\Silverlight\SimpleInjector.dll Releases\v%version%\Silverlight\SimpleInjector.dll
copy bin\Silverlight\SimpleInjector.xml Releases\v%version%\Silverlight\SimpleInjector.xml

echo %version% >> Releases\v%version%\version.txt 

rmdir Releases\temp /s /q


echo CODEPLEX DOWNLOAD .NET

mkdir Releases\temp
copy licence.txt Releases\temp\licence.txt
mkdir Releases\temp\Documentation
copy Help\SimpleInjector.chm Releases\temp\Documentation\SimpleInjector.chm
mkdir Releases\temp\NET35
copy bin\NET\SimpleInjector.dll Releases\temp\NET35\SimpleInjector.dll
copy bin\NET\SimpleInjector.xml Releases\temp\NET35\SimpleInjector.xml
mkdir Releases\temp\NET35\CommonServiceLocator
copy bin\NET\CommonServiceLocator.SimpleInjectorAdapter.dll Releases\temp\NET35\CommonServiceLocator\CommonServiceLocator.SimpleInjectorAdapter.dll
copy bin\NET\CommonServiceLocator.SimpleInjectorAdapter.xml Releases\temp\NET35\CommonServiceLocator\CommonServiceLocator.SimpleInjectorAdapter.xml
copy bin\NET\Microsoft.Practices.ServiceLocation.dll Releases\temp\NET35\CommonServiceLocator\Microsoft.Practices.ServiceLocation.dll
copy bin\NET\Microsoft.Practices.ServiceLocation.xml Releases\temp\NET35\CommonServiceLocator\Microsoft.Practices.ServiceLocation.xml
mkdir Releases\temp\NET35\Extensions
copy bin\NET\SimpleInjector.Extensions.dll Releases\temp\NET35\Extensions\SimpleInjector.Extensions.dll
copy bin\NET\SimpleInjector.Extensions.xml Releases\temp\NET35\Extensions\SimpleInjector.Extensions.xml
copy bin\NET\SimpleInjector.Packaging.dll Releases\temp\NET35\Extensions\SimpleInjector.Packaging.dll
copy bin\NET\SimpleInjector.Packaging.xml Releases\temp\NET35\Extensions\SimpleInjector.Packaging.xml
mkdir Releases\temp\NET40
copy bin\NET\SimpleInjector.dll Releases\temp\NET40\SimpleInjector.dll
copy bin\NET\SimpleInjector.xml Releases\temp\NET40\SimpleInjector.xml
mkdir Releases\temp\NET40\CommonServiceLocator
copy bin\NET\CommonServiceLocator.SimpleInjectorAdapter.dll Releases\temp\NET40\CommonServiceLocator\CommonServiceLocator.SimpleInjectorAdapter.dll
copy bin\NET\CommonServiceLocator.SimpleInjectorAdapter.xml Releases\temp\NET40\CommonServiceLocator\CommonServiceLocator.SimpleInjectorAdapter.xml
copy bin\NET\Microsoft.Practices.ServiceLocation.dll Releases\temp\NET40\CommonServiceLocator\Microsoft.Practices.ServiceLocation.dll
copy bin\NET\Microsoft.Practices.ServiceLocation.xml Releases\temp\NET40\CommonServiceLocator\Microsoft.Practices.ServiceLocation.xml
mkdir Releases\temp\NET40\Extensions
copy bin\NET\SimpleInjector.Extensions.dll Releases\temp\NET40\Extensions\SimpleInjector.Extensions.dll
copy bin\NET\SimpleInjector.Extensions.xml Releases\temp\NET40\Extensions\SimpleInjector.Extensions.xml
copy bin\NET\SimpleInjector.Packaging.dll Releases\temp\NET40\Extensions\SimpleInjector.Packaging.dll
copy bin\NET\SimpleInjector.Packaging.xml Releases\temp\NET40\Extensions\SimpleInjector.Packaging.xml
copy bin\NET\SimpleInjector.Extensions.LifetimeScoping.dll Releases\temp\NET40\Extensions\SimpleInjector.Extensions.LifetimeScoping.dll
copy bin\NET\SimpleInjector.Extensions.LifetimeScoping.xml Releases\temp\NET40\Extensions\SimpleInjector.Extensions.LifetimeScoping.xml
mkdir Releases\temp\NET40\Integration
copy bin\NET\SimpleInjector.Integration.Web.dll Releases\temp\NET40\Integration\SimpleInjector.Integration.Web.dll
copy bin\NET\SimpleInjector.Integration.Web.xml Releases\temp\NET40\Integration\SimpleInjector.Integration.Web.xml
copy bin\NET\SimpleInjector.Integration.Web.Mvc.dll Releases\temp\NET40\Integration\SimpleInjector.Integration.Web.Mvc.dll
copy bin\NET\SimpleInjector.Integration.Web.Mvc.xml Releases\temp\NET40\Integration\SimpleInjector.Integration.Web.Mvc.xml
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%version%\SimpleInjector Runtime Library v%version%.zip"

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
mkdir Releases\temp\Extensions
copy bin\Silverlight\SimpleInjector.Extensions.dll Releases\temp\Extensions\SimpleInjector.Extensions.dll
copy bin\Silverlight\SimpleInjector.Extensions.xml Releases\temp\Extensions\SimpleInjector.Extensions.xml
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%version%\SimpleInjector Silverlight Runtime Library v%version%.zip"
rmdir Releases\temp /s /q


echo NUGET PACKAGES .NET

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy  bin\NET\SimpleInjector.dll Releases\temp\lib\net35\SimpleInjector.dll
copy  bin\NET\SimpleInjector.xml Releases\temp\lib\net35\SimpleInjector.xml
%replace% /source:Releases\temp\SimpleInjector.nuspec {version} %versionCore%
%replace% /source:Releases\temp\package\services\metadata\core-properties\c8082e2254fe4defafc3b452026f048d.psmdcp {version} %versionCore%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%version%\.NET\SimpleInjector.%versionCore%.zip"
ren "%CD%\Releases\v%version%\.NET\SimpleInjector.%versionCore%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\CommonServiceLocator.SimpleInjectorAdapter Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy  bin\NET\CommonServiceLocator.SimpleInjectorAdapter.dll Releases\temp\lib\net35\CommonServiceLocator.SimpleInjectorAdapter.dll
copy  bin\NET\CommonServiceLocator.SimpleInjectorAdapter.xml Releases\temp\lib\net35\CommonServiceLocator.SimpleInjectorAdapter.xml
%replace% /source:Releases\temp\CommonServiceLocator.SimpleInjectorAdapter.nuspec {version} %versionCore%
%replace% /source:Releases\temp\package\services\metadata\core-properties\1fea7be7f6324eb68593116ecd0864e4.psmdcp {version} %versionCore%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%version%\.NET\CommonServiceLocator.SimpleInjectorAdapter.%versionCore%.zip"
ren "%CD%\Releases\v%version%\.NET\CommonServiceLocator.SimpleInjectorAdapter.%versionCore%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Extensions Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy  bin\NET\SimpleInjector.Extensions.dll Releases\temp\lib\net35\SimpleInjector.Extensions.dll
copy  bin\NET\SimpleInjector.Extensions.xml Releases\temp\lib\net35\SimpleInjector.Extensions.xml
%replace% /source:Releases\temp\SimpleInjector.Extensions.nuspec {version} %version_Extensions%
%replace% /source:Releases\temp\SimpleInjector.Extensions.nuspec {versionCore} %versionCore%
%replace% /source:Releases\temp\package\services\metadata\core-properties\3b15d35fbc3a4556960337dcd95cf0f4.psmdcp {version} %version_Extensions%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%version%\.NET\SimpleInjector.Extensions.%version_Extensions%.zip"
ren "%CD%\Releases\v%version%\.NET\SimpleInjector.Extensions.%version_Extensions%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Packaging Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy  bin\NET\SimpleInjector.Packaging.dll Releases\temp\lib\net35\SimpleInjector.Packaging.dll
copy  bin\NET\SimpleInjector.Packaging.xml Releases\temp\lib\net35\SimpleInjector.Packaging.xml
%replace% /source:Releases\temp\SimpleInjector.Packaging.nuspec {version} %version%
%replace% /source:Releases\temp\SimpleInjector.Packaging.nuspec {versionCore} %versionCore%
%replace% /source:Releases\temp\package\services\metadata\core-properties\4d447eef3ba54c2da48c4d25f475fcbe.psmdcp {version} %version%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%version%\.NET\SimpleInjector.Packaging.%version%.zip"
ren "%CD%\Releases\v%version%\.NET\SimpleInjector.Packaging.%version%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Extensions.LifetimeScoping Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy  bin\NET\SimpleInjector.Extensions.LifetimeScoping.dll Releases\temp\lib\net40\SimpleInjector.Extensions.LifetimeScoping.dll
copy  bin\NET\SimpleInjector.Extensions.LifetimeScoping.xml Releases\temp\lib\net40\SimpleInjector.Extensions.LifetimeScoping.xml
%replace% /source:Releases\temp\SimpleInjector.Extensions.LifetimeScoping.nuspec {version} %version_Extensions_LifetimeScoping%
%replace% /source:Releases\temp\SimpleInjector.Extensions.LifetimeScoping.nuspec {versionCore} %versionCore%
%replace% /source:Releases\temp\package\services\metadata\core-properties\3c829585afae419fa2b861a3b473739c.psmdcp {version} %version_Extensions_LifetimeScoping%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%version%\.NET\SimpleInjector.Extensions.LifetimeScoping.%version_Extensions_LifetimeScoping%.zip"
ren "%CD%\Releases\v%version%\.NET\SimpleInjector.Extensions.LifetimeScoping.%version_Extensions_LifetimeScoping%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.Integration.Web Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy  bin\NET\SimpleInjector.Integration.Web.dll Releases\temp\lib\net40\SimpleInjector.Integration.Web.dll
copy  bin\NET\SimpleInjector.Integration.Web.xml Releases\temp\lib\net40\SimpleInjector.Integration.Web.xml
%replace% /source:Releases\temp\SimpleInjector.Integration.Web.nuspec {version} %version%
%replace% /source:Releases\temp\SimpleInjector.Integration.Web.nuspec {versionCore} %versionCore%
%replace% /source:Releases\temp\package\services\metadata\core-properties\fb4dd696b20548afa09bcbbf3ea6c7d0.psmdcp {version} %version%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%version%\.NET\SimpleInjector.Integration.Web.%version%.zip"
ren "%CD%\Releases\v%version%\.NET\SimpleInjector.Integration.Web.%version%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\.NET\SimpleInjector.MVC3 Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy  bin\NET\SimpleInjector.Integration.Web.Mvc.dll Releases\temp\lib\net40\SimpleInjector.Integration.Web.Mvc.dll
copy  bin\NET\SimpleInjector.Integration.Web.Mvc.xml Releases\temp\lib\net40\SimpleInjector.Integration.Web.Mvc.xml
%replace% /source:Releases\temp\SimpleInjector.MVC3.nuspec {version} %version%
%replace% /source:Releases\temp\SimpleInjector.MVC3.nuspec {version_Integration_Mvc} %version_Integration_Mvc%
%replace% /source:Releases\temp\SimpleInjector.MVC3.nuspec {versionCore} %versionCore%
%replace% /source:Releases\temp\package\services\metadata\core-properties\7594fa13b1164869a9b2b67b8b5ad9a3.psmdcp {version} %version_Integration_Mvc%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%version%\.NET\SimpleInjector.MVC3.%version_Integration_Mvc%.zip"
ren "%CD%\Releases\v%version%\.NET\SimpleInjector.MVC3.%version_Integration_Mvc%.zip" "*.nupkg"
rmdir Releases\temp /s /q



echo NUGET PACKAGES SILVERLIGHT

mkdir Releases\temp
xcopy %nugetTemplatePath%\Silverlight\SimpleInjector.Silverlight Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy  bin\Silverlight\SimpleInjector.dll Releases\temp\lib\sl30\SimpleInjector.dll
copy  bin\Silverlight\SimpleInjector.xml Releases\temp\lib\sl30\SimpleInjector.xml
%replace% /source:Releases\temp\SimpleInjector.Silverlight.nuspec {version} %versionCore%
%replace% /source:Releases\temp\package\services\metadata\core-properties\bc50420a966a46388a7509b095da88af.psmdcp {version} %versionCore%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%version%\Silverlight\SimpleInjector.Silverlight.%versionCore%.zip"
ren "%CD%\Releases\v%version%\Silverlight\SimpleInjector.Silverlight.%versionCore%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\Silverlight\CommonServiceLocator.SimpleInjectorAdapter.Silverlight Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy  bin\Silverlight\CommonServiceLocator.SimpleInjectorAdapter.Silverlight.dll Releases\temp\lib\sl30\CommonServiceLocator.SimpleInjectorAdapter.Silverlight.dll
copy  bin\Silverlight\CommonServiceLocator.SimpleInjectorAdapter.Silverlight.xml Releases\temp\lib\sl30\CommonServiceLocator.SimpleInjectorAdapter.Silverlight.xml
%replace% /source:Releases\temp\CommonServiceLocator.SimpleInjectorAdapter.Silverlight.nuspec {versionCore} %versionCore%
%replace% /source:Releases\temp\CommonServiceLocator.SimpleInjectorAdapter.Silverlight.nuspec {version} %version%
%replace% /source:Releases\temp\package\services\metadata\core-properties\b0dd4e78d398462ead742df1961bccc2.psmdcp {version} %version%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%version%\Silverlight\CommonServiceLocator.SimpleInjectorAdapter.Silverlight.%versionCore%.zip"
ren "%CD%\Releases\v%version%\Silverlight\CommonServiceLocator.SimpleInjectorAdapter.Silverlight.%versionCore%.zip" "*.nupkg"
rmdir Releases\temp /s /q

mkdir Releases\temp
xcopy %nugetTemplatePath%\Silverlight\SimpleInjector.Extensions.Silverlight Releases\temp /E /H
attrib -r "%CD%\Releases\temp\*.*" /s /d
copy  bin\Silverlight\SimpleInjector.Extensions.dll Releases\temp\lib\sl30\SimpleInjector.Extensions.dll
copy  bin\Silverlight\SimpleInjector.Extensions.xml Releases\temp\lib\sl30\SimpleInjector.Extensions.xml
%replace% /source:Releases\temp\SimpleInjector.Extensions.Silverlight.nuspec {versionCore} %versionCore%
%replace% /source:Releases\temp\SimpleInjector.Extensions.Silverlight.nuspec {version} %version_Extensions%
%replace% /source:Releases\temp\package\services\metadata\core-properties\7ed90488e5714295854ab251e2959afe.psmdcp {version} %version_Extensions%
%compress% "%CD%\Releases\temp" "%CD%\Releases\v%version%\Silverlight\SimpleInjector.Extensions.Silverlight.%version_Extensions%.zip"
ren "%CD%\Releases\v%version%\Silverlight\SimpleInjector.Extensions.Silverlight.%version_Extensions%.zip" "*.nupkg"
rmdir Releases\temp /s /q
