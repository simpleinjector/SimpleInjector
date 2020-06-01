@ECHO OFF

for /f "tokens=2 delims==" %%I in ('wmic os get localdatetime /format:list') do set datetime=%%I

set buildNumber=0 
set copyrightYear=%datetime:~0,4%
set version=%1
set prereleasePostfix=
set releaseNotesUrl=https://github.com/simpleinjector/SimpleInjector/releases/tag/v

IF "%2"=="1" ( 
	set step=%2
) ELSE IF "%2"=="2" (
	set step=%2
) ELSE IF "%2"=="3" (
	set step=%2
) ELSE IF "%2"=="4" (
	set step=%2
) ELSE IF "%2"=="5" (
	set step=%2
) ELSE (
	set step=%3
	set prereleasePostfix=%2
)

set inputOk=true
IF "%version%"=="" ( 
	set inputOk=false
) ELSE IF "%step%"=="" (
	set inputOk=false
)

for /f "tokens=1,2,3 delims=. " %%a in ("%version%") do set version_major=%%a&set version_minor=%%b&set version_patch=%%c

echo step: %step%
echo version: %version% (major: %version_major%, minor: %version_minor%, patch: %version_patch%)
echo prereleasePostfix: %prereleasePostfix%

IF "%inputOk%"=="false" (
	rem I would have loved doing this all in one single step, but for some reason, I can't seem to build using MSBuild
	rem while running from the command line using MSBuild 2017. See https://stackoverflow.com/questions/47002571/.
	rem That's why this build file must be called multiple times.
    echo Please provide both the version number and the number of the of the build step. Starting with 1.
	echo Usage: "%0 [version] {-[prerelease postfix]} [step]
	echo Example1: "%0 5.0.0 -beta1 1
	echo Example2: "%0 5.0.0 1
    goto :EOF
)

set /a nextMajorVersion=%version_major%+1

set version_Core=%version%
set version_DynamicAssemblyCompilation=%version_Core%
set version_Packaging=%version_Core%
set version_Integration_Web=%version_Core%
set version_Integration_WebForms=%version_Core%
set version_Integration_Mvc=%version_Core%
set version_Integration_Wcf=%version_Core%
set version_Integration_WebApi=%version_Core%
set version_Integration_ServiceCollection=%version_Core%
set version_Integration_GenericHost=%version_Core%
set version_Integration_AspNetCore=%version_Core%
set version_Integration_AspNetCore_Mvc_Core=%version_Core%
set version_Integration_AspNetCore_Mvc_ViewFeatures=%version_Core%
set version_Integration_AspNetCore_Mvc=%version_Core%

set referenceLibraryPath=..\..\simpleinjector-website.github.io\ReferenceLibrary
if not exist %referenceLibraryPath% goto :referenceLibraryPath_missing

set vsvars32_bat="%programfiles(x86)%\Microsoft Visual Studio\2019\Community\Common7\Tools\VsMSBuildCmd.bat"
if not exist %vsvars32_bat% goto :vsvars32_bat_missing
@call %vsvars32_bat%

set msbuild="%programfiles(x86)%\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
if not exist %msbuild% goto :msbuild_exe_missing

set attrib=%systemroot%\System32\attrib.exe
set xcopy=%systemroot%\System32\xcopy.exe
if not exist %xcopy% goto :xcopy_missing


set buildToolsPath=BuildTools
set nugetTemplatePath=%buildToolsPath%\NuGet

rem replace.exe can replace one text for another in a given file
set replace=%buildToolsPath%\replace.exe

rem zipreplace.exe does what replace.exe does, but now inside a .zip file.
set zipreplace=%buildToolsPath%\zipreplace.exe

set compress=%systemroot%\System32\CScript.exe %buildToolsPath%\zip.vbs
set configuration=Release
set defineConstantsNet=PUBLISH
set targetPath=bin
set targetPathNet=%targetPath%\NET
set targetPathPcl=%targetPath%\PCL
set targetPathCoreClr=%targetPath%\DOTNET

set named_version=%version%%prereleasePostfix%

set named_version_Core=%version_Core%%prereleasePostfix%
set named_version_DynamicAssemblyCompilation=%version_DynamicAssemblyCompilation%%prereleasePostfix%
set named_version_Integration_ServiceCollection=%version_Integration_ServiceCollection%%prereleasePostfix%
set named_version_Integration_GenericHost=%version_Integration_GenericHost%%prereleasePostfix%
set named_version_Integration_AspNetCore=%version_Integration_AspNetCore%%prereleasePostfix%
set named_version_Integration_AspNetCore_Mvc_Core=%version_Integration_AspNetCore_Mvc_Core%%prereleasePostfix%
set named_version_Integration_AspNetCore_Mvc_ViewFeatures=%version_Integration_AspNetCore_Mvc_ViewFeatures%%prereleasePostfix%
set named_version_Integration_AspNetCore_Mvc=%version_Integration_AspNetCore_Mvc%%prereleasePostfix%
set named_version_Integration_Wcf=%version_Integration_Wcf%%prereleasePostfix%
set named_version_Integration_Web=%version_Integration_Web%%prereleasePostfix%
set named_version_Integration_Mvc=%version_Integration_Mvc%%prereleasePostfix%
set named_version_Integration_WebApi=%version_Integration_WebApi%%prereleasePostfix%
set named_version_Packaging=%version_Packaging%%prereleasePostfix%

set numeric_version_Core=%version_Core%.%buildNumber%
set numeric_version_DynamicAssemblyCompilation=%version_DynamicAssemblyCompilation%.%buildNumber%
set numeric_version_Integration_ServiceCollection=%version_Integration_ServiceCollection%.%buildNumber%
set numeric_version_Integration_GenericHost=%version_Integration_GenericHost%.%buildNumber%
set numeric_version_Integration_AspNetCore=%version_Integration_AspNetCore%.%buildNumber%
set numeric_version_Integration_Wcf=%version_Integration_Wcf%.%buildNumber%
set numeric_version_Integration_Web=%version_Integration_Web%.%buildNumber%
set numeric_version_Integration_Mvc=%version_Integration_Mvc%.%buildNumber%
set numeric_version_Integration_WebApi=%version_Integration_WebApi%.%buildNumber%
set numeric_version_Packaging=%version_Packaging%.%buildNumber%

if not exist SimpleInjector.snk goto :strong_name_key_missing

echo Initialization complete

IF %step%==1 (
	echo Running step 1: Cleaning solution and setting version numbers
	if exist Releases\v%named_version% goto :release_directory_already_exists

	echo BUILDING
	rmdir %targetPathNet% /s /q
	mkdir %targetPathNet%
	rmdir %targetPathPcl% /s /q
	mkdir %targetPathPcl%
	rmdir %targetPathCoreClr% /s /q
	mkdir %targetPathCoreClr%
	
	echo CLEAN SOLUTION
	%msbuild% /t:Clean /p:Configuration=Release

	echo SET VERSION NUMBERS
	%replace% /line "<VersionPrefix>" "<VersionPrefix>%named_version_Core%</VersionPrefix>" /source:SimpleInjector\SimpleInjector.csproj
	%replace% /line "<VersionPrefix>" "<VersionPrefix>%named_version_DynamicAssemblyCompilation%</VersionPrefix>" /source:SimpleInjector.DynamicAssemblyCompilation\SimpleInjector.DynamicAssemblyCompilation.csproj
	%replace% /line "<VersionPrefix>" "<VersionPrefix>%named_version_Integration_ServiceCollection%</VersionPrefix>" /source:SimpleInjector.Integration.ServiceCollection\SimpleInjector.Integration.ServiceCollection.csproj
	%replace% /line "<VersionPrefix>" "<VersionPrefix>%named_version_Integration_GenericHost%</VersionPrefix>" /source:SimpleInjector.Integration.GenericHost\SimpleInjector.Integration.GenericHost.csproj
	%replace% /line "<VersionPrefix>" "<VersionPrefix>%named_version_Integration_AspNetCore%</VersionPrefix>" /source:SimpleInjector.Integration.AspNetCore\SimpleInjector.Integration.AspNetCore.csproj
	%replace% /line "<VersionPrefix>" "<VersionPrefix>%named_version_Integration_AspNetCore_Mvc_Core%</VersionPrefix>" /source:SimpleInjector.Integration.AspNetCore.Mvc.Core\SimpleInjector.Integration.AspNetCore.Mvc.Core.csproj
	%replace% /line "<VersionPrefix>" "<VersionPrefix>%named_version_Integration_AspNetCore_Mvc_ViewFeatures%</VersionPrefix>" /source:SimpleInjector.Integration.AspNetCore.Mvc.ViewFeatures\SimpleInjector.Integration.AspNetCore.Mvc.ViewFeatures.csproj
	%replace% /line "<VersionPrefix>" "<VersionPrefix>%named_version_Integration_AspNetCore_Mvc%</VersionPrefix>" /source:SimpleInjector.Integration.AspNetCore.Mvc\SimpleInjector.Integration.AspNetCore.Mvc.csproj
	%replace% /line "<VersionPrefix>" "<VersionPrefix>%named_version_Integration_Wcf%</VersionPrefix>" /source:SimpleInjector.Integration.Wcf\SimpleInjector.Integration.Wcf.csproj
	%replace% /line "<VersionPrefix>" "<VersionPrefix>%named_version_Integration_Web%</VersionPrefix>" /source:SimpleInjector.Integration.Web\SimpleInjector.Integration.Web.csproj
	%replace% /line "<VersionPrefix>" "<VersionPrefix>%named_version_Integration_Mvc%</VersionPrefix>" /source:SimpleInjector.Integration.Web.Mvc\SimpleInjector.Integration.Web.Mvc.csproj
	%replace% /line "<VersionPrefix>" "<VersionPrefix>%named_version_Integration_WebApi%</VersionPrefix>" /source:SimpleInjector.Integration.WebApi\SimpleInjector.Integration.WebApi.csproj
	%replace% /line "<VersionPrefix>" "<VersionPrefix>%named_version_Packaging%</VersionPrefix>" /source:SimpleInjector.Packaging\SimpleInjector.Packaging.csproj
	
	echo SET PACKAGE RELEASE NOTES
	
	%replace% /line "<PackageReleaseNotes>" "<PackageReleaseNotes>%releaseNotesUrl%%version_DynamicAssemblyCompilation%</PackageReleaseNotes>" /source:SimpleInjector.DynamicAssemblyCompilation\SimpleInjector.DynamicAssemblyCompilation.csproj
	%replace% /line "<PackageReleaseNotes>" "<PackageReleaseNotes>%releaseNotesUrl%%version_Integration_ServiceCollection%</PackageReleaseNotes>" /source:SimpleInjector.Integration.ServiceCollection\SimpleInjector.Integration.ServiceCollection.csproj
	%replace% /line "<PackageReleaseNotes>" "<PackageReleaseNotes>%releaseNotesUrl%%version_Integration_GenericHost%</PackageReleaseNotes>" /source:SimpleInjector.Integration.GenericHost\SimpleInjector.Integration.GenericHost.csproj
	%replace% /line "<PackageReleaseNotes>" "<PackageReleaseNotes>%releaseNotesUrl%%version_Integration_AspNetCore%</PackageReleaseNotes>" /source:SimpleInjector.Integration.AspNetCore\SimpleInjector.Integration.AspNetCore.csproj
	%replace% /line "<PackageReleaseNotes>" "<PackageReleaseNotes>%releaseNotesUrl%%version_Integration_AspNetCore_Mvc_Core%</PackageReleaseNotes>" /source:SimpleInjector.Integration.AspNetCore.Mvc.Core\SimpleInjector.Integration.AspNetCore.Mvc.Core.csproj
	%replace% /line "<PackageReleaseNotes>" "<PackageReleaseNotes>%releaseNotesUrl%%version_Integration_AspNetCore_Mvc%</PackageReleaseNotes>" /source:SimpleInjector.Integration.AspNetCore.Mvc\SimpleInjector.Integration.AspNetCore.Mvc.csproj

	
	rem echo BUILD SOLUTION

	rem %msbuild% "SimpleInjector\SimpleInjector.csproj" /nologo
	rem %msbuild% "SimpleInjector.Integration.ServiceCollection\SimpleInjector.Integration.ServiceCollection.csproj" /nologo
	rem %msbuild% "SimpleInjector.Integration.AspNetCore\SimpleInjector.Integration.AspNetCore.csproj" /nologo
	rem %msbuild% "SimpleInjector.Integration.AspNetCore.Mvc.Core\SimpleInjector.Integration.AspNetCore.Mvc.Core.csproj" /nologo
	rem %msbuild% "SimpleInjector.Integration.AspNetCore.Mvc\SimpleInjector.Integration.AspNetCore.Mvc.csproj" /nologo
	rem %msbuild% "SimpleInjector.Integration.Wcf\SimpleInjector.Integration.Wcf.csproj" /nologo
	rem %msbuild% "SimpleInjector.Integration.Web\SimpleInjector.Integration.Web.csproj" /nologo
	rem %msbuild% "SimpleInjector.Integration.Web.Mvc\SimpleInjector.Integration.Web.Mvc.csproj" /nologo
	rem %msbuild% "SimpleInjector.Integration.WebApi\SimpleInjector.Integration.WebApi.csproj" /nologo
	rem %msbuild% "SimpleInjector.Packaging\SimpleInjector.Packaging.csproj" /nologo
	
    echo Please compile the solution in RELEASE mode and run step 2
    goto :EOF
)

IF %step%==2 (
	echo Running step 2: RUNNING TESTS IN PARTIAL TRUST

	set testDll=SimpleInjector.Tests.Unit\bin\Release\net451\SimpleInjector.Tests.Unit.dll
	set testRunner=SimpleInjector.Tests.Unit\bin\Release\net451\PartialTrustTestRunner.exe
	
	echo %testRunner% %testDll%
	%testRunner% %testDll%
	
	set testDll2=SimpleInjector.Conventions.Tests\bin\Release\net472\SimpleInjector.Conventions.Tests.dll

	echo %testRunner% %testDll2%
	%testRunner% %testDll2%

    echo If tests were green, please run step 3
    goto :EOF	
)

IF %step%==3 (
	echo Running step 3: BUILDING DOCUMENTATION

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

	IF "%prereleasePostfix%"=="" (
		echo COPYING ONLINE DOCUMENTATION TO WEBSITE REPOSITORY

		%xcopy% Help %referenceLibraryPath% /E /H /Y /I
		%xcopy% Help %referenceLibraryPath%\%version_major%.%version_minor% /E /H /Y /I
	)

    echo Please run step 4
    goto :EOF	
)

IF %step%==4 (
	echo Running step 4: CREATING NUGET PACKAGES
	
    rmdir Releases\temp /s /q
	
	mkdir Releases\temp
	%xcopy% %nugetTemplatePath%\.NET\SimpleInjector Releases\temp /E /H
	%attrib% -r "%CD%\Releases\temp\*.*" /s /d
	del Releases\temp\.gitignore /s /q
	copy SimpleInjector\bin\Release\net45\SimpleInjector.dll Releases\temp\lib\net45\SimpleInjector.dll
	copy SimpleInjector\bin\Release\net45\SimpleInjector.xml Releases\temp\lib\net45\SimpleInjector.xml
	copy SimpleInjector\bin\Release\net461\SimpleInjector.dll Releases\temp\lib\net461\SimpleInjector.dll
	copy SimpleInjector\bin\Release\net461\SimpleInjector.xml Releases\temp\lib\net461\SimpleInjector.xml
	copy SimpleInjector\bin\Release\netstandard1.0\SimpleInjector.dll "Releases\temp\lib\netstandard1.0\SimpleInjector.dll"
	copy SimpleInjector\bin\Release\netstandard1.0\SimpleInjector.xml "Releases\temp\lib\netstandard1.0\SimpleInjector.xml"
	copy SimpleInjector\bin\Release\netstandard1.3\SimpleInjector.dll "Releases\temp\lib\netstandard1.3\SimpleInjector.dll"
	copy SimpleInjector\bin\Release\netstandard1.3\SimpleInjector.xml "Releases\temp\lib\netstandard1.3\SimpleInjector.xml"
	copy SimpleInjector\bin\Release\netstandard2.0\SimpleInjector.dll "Releases\temp\lib\netstandard2.0\SimpleInjector.dll"
	copy SimpleInjector\bin\Release\netstandard2.0\SimpleInjector.xml "Releases\temp\lib\netstandard2.0\SimpleInjector.xml"
	copy SimpleInjector\bin\Release\netstandard2.1\SimpleInjector.dll "Releases\temp\lib\netstandard2.1\SimpleInjector.dll"
	copy SimpleInjector\bin\Release\netstandard2.1\SimpleInjector.xml "Releases\temp\lib\netstandard2.1\SimpleInjector.xml"
	%replace% /source:Releases\temp\SimpleInjector.nuspec {version} %named_version_Core%
	%replace% /source:Releases\temp\SimpleInjector.nuspec {year} %copyrightYear%
	%replace% /source:Releases\temp\package\services\metadata\core-properties\c8082e2254fe4defafc3b452026f048d.psmdcp {version} %named_version_Core%
	%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\SimpleInjector.%named_version_Core%.zip"
	rmdir Releases\temp /s /q

	mkdir Releases\temp
	%xcopy% %nugetTemplatePath%\.NET\SimpleInjector.Packaging Releases\temp /E /H
	%attrib% -r "%CD%\Releases\temp\*.*" /s /d
	del Releases\temp\.gitignore /s /q
	copy SimpleInjector.Packaging\bin\Release\net45\SimpleInjector.Packaging.dll "Releases\temp\lib\net45\SimpleInjector.Packaging.dll"
	copy SimpleInjector.Packaging\bin\Release\net45\SimpleInjector.Packaging.xml "Releases\temp\lib\net45\SimpleInjector.Packaging.xml"
	copy SimpleInjector.Packaging\bin\Release\netstandard1.0\SimpleInjector.Packaging.dll "Releases\temp\lib\netstandard1.0\SimpleInjector.Packaging.dll"
	copy SimpleInjector.Packaging\bin\Release\netstandard1.0\SimpleInjector.Packaging.xml "Releases\temp\lib\netstandard1.0\SimpleInjector.Packaging.xml"
	%replace% /source:Releases\temp\SimpleInjector.Packaging.nuspec {version} %named_version_Packaging%
	%replace% /source:Releases\temp\SimpleInjector.Packaging.nuspec {versionCore} %named_version_Core%
	%replace% /source:Releases\temp\SimpleInjector.Packaging.nuspec {year} %copyrightYear%
	%replace% /source:Releases\temp\package\services\metadata\core-properties\4d447eef3ba54c2da48c4d25f475fcbe.psmdcp {version} %named_version_Packaging%
	%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\SimpleInjector.Packaging.%named_version_Packaging%.zip"
	rmdir Releases\temp /s /q

	mkdir Releases\temp
	%xcopy% %nugetTemplatePath%\.NET\SimpleInjector.Integration.Web Releases\temp /E /H
	%attrib% -r "%CD%\Releases\temp\*.*" /s /d
	del Releases\temp\.gitignore /s /q
	copy SimpleInjector.Integration.Web\bin\Release\net45\SimpleInjector.Integration.Web.dll Releases\temp\lib\net45\SimpleInjector.Integration.Web.dll
	copy SimpleInjector.Integration.Web\bin\Release\net45\SimpleInjector.Integration.Web.xml Releases\temp\lib\net45\SimpleInjector.Integration.Web.xml
	%replace% /source:Releases\temp\SimpleInjector.Integration.Web.nuspec {version} %named_version_Integration_Web%
	%replace% /source:Releases\temp\SimpleInjector.Integration.Web.nuspec {versionCore} %named_version_Core%
	%replace% /source:Releases\temp\SimpleInjector.Integration.Web.nuspec {year} %copyrightYear%
	%replace% /source:Releases\temp\package\services\metadata\core-properties\fb4dd696b20548afa09bcbbf3ea6c7d0.psmdcp {version} %named_version_Integration_Web%
	%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\SimpleInjector.Integration.Web.%named_version_Integration_Web%.zip"
	rmdir Releases\temp /s /q

	mkdir Releases\temp
	%xcopy% %nugetTemplatePath%\.NET\SimpleInjector.Integration.Web.Mvc Releases\temp /E /H
	%attrib% -r "%CD%\Releases\temp\*.*" /s /d
	del Releases\temp\.gitignore /s /q
	copy SimpleInjector.Integration.Web.Mvc\bin\Release\net45\SimpleInjector.Integration.Web.Mvc.dll Releases\temp\lib\net45\SimpleInjector.Integration.Web.Mvc.dll
	copy SimpleInjector.Integration.Web.Mvc\bin\Release\net45\SimpleInjector.Integration.Web.Mvc.xml Releases\temp\lib\net45\SimpleInjector.Integration.Web.Mvc.xml
	%replace% /source:Releases\temp\SimpleInjector.Integration.Web.Mvc.nuspec {version} %named_version_Integration_Mvc%
	%replace% /source:Releases\temp\SimpleInjector.Integration.Web.Mvc.nuspec {versionCore} %named_version_Core%
	%replace% /source:Releases\temp\SimpleInjector.Integration.Web.Mvc.nuspec {version_Integration_Web} %named_version_Integration_Web%
	%replace% /source:Releases\temp\SimpleInjector.Integration.Web.Mvc.nuspec {year} %copyrightYear%
	%replace% /source:Releases\temp\package\services\metadata\core-properties\916f6977dc7a462395f59f05d2cc6a76.psmdcp {version} %named_version_Integration_Mvc%
	%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\SimpleInjector.Integration.Web.Mvc.%named_version_Integration_Mvc%.zip"
	rmdir Releases\temp /s /q

	mkdir Releases\temp
	%xcopy% %nugetTemplatePath%\.NET\SimpleInjector.Integration.Web.Mvc.QuickStart Releases\temp /E /H
	%attrib% -r "%CD%\Releases\temp\*.*" /s /d
	del Releases\temp\.gitignore /s /q
	%replace% /source:Releases\temp\SimpleInjector.MVC3.nuspec {version} %named_version_Integration_Mvc%
	%replace% /source:Releases\temp\SimpleInjector.MVC3.nuspec {versionCore} %named_version_Core%
	%replace% /source:Releases\temp\SimpleInjector.MVC3.nuspec {version_Integration_Mvc} %named_version_Integration_Mvc%
	%replace% /source:Releases\temp\SimpleInjector.MVC3.nuspec {year} %copyrightYear%
	%replace% /source:Releases\temp\package\services\metadata\core-properties\7594fa13b1164869a9b2b67b8b5ad9a3.psmdcp {version} %named_version_Integration_Mvc%
	%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\SimpleInjector.MVC3.%named_version_Integration_Mvc%.zip"
	rmdir Releases\temp /s /q

	mkdir Releases\temp
	%xcopy% %nugetTemplatePath%\.NET\SimpleInjector.Integration.Wcf Releases\temp /E /H
	%attrib% -r "%CD%\Releases\temp\*.*" /s /d
	del Releases\temp\.gitignore /s /q
	copy SimpleInjector.Integration.Wcf\bin\Release\net45\SimpleInjector.Integration.Wcf.dll Releases\temp\lib\net45\SimpleInjector.Integration.Wcf.dll
	copy SimpleInjector.Integration.Wcf\bin\Release\net45\SimpleInjector.Integration.Wcf.xml Releases\temp\lib\net45\SimpleInjector.Integration.Wcf.xml
	%replace% /source:Releases\temp\SimpleInjector.Integration.Wcf.nuspec {version} %named_version_Integration_Wcf%
	%replace% /source:Releases\temp\SimpleInjector.Integration.Wcf.nuspec {versionCore} %named_version_Core%
	%replace% /source:Releases\temp\SimpleInjector.Integration.Wcf.nuspec {year} %copyrightYear%
	%replace% /source:Releases\temp\package\services\metadata\core-properties\13850374b87d467da12f21ca32dac632.psmdcp {version} %named_version_Integration_Wcf%
	%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\SimpleInjector.Integration.Wcf.%named_version_Integration_Wcf%.zip"
	rmdir Releases\temp /s /q

	mkdir Releases\temp
	%xcopy% %nugetTemplatePath%\.NET\SimpleInjector.Integration.Wcf.QuickStart Releases\temp /E /H
	%attrib% -r "%CD%\Releases\temp\*.*" /s /d
	del Releases\temp\.gitignore /s /q
	%replace% /source:Releases\temp\SimpleInjector.Integration.Wcf.QuickStart.nuspec {version} %named_version_Integration_Wcf%
	%replace% /source:Releases\temp\SimpleInjector.Integration.Wcf.QuickStart.nuspec {versionCore} %named_version_Core%
	%replace% /source:Releases\temp\SimpleInjector.Integration.Wcf.QuickStart.nuspec {version_Integration_Wcf} %named_version_Integration_Wcf%
	%replace% /source:Releases\temp\SimpleInjector.Integration.Wcf.QuickStart.nuspec {year} %copyrightYear%
	%replace% /source:Releases\temp\package\services\metadata\core-properties\a47c8b1328f541fc8a5cd4c0446d5fe1.psmdcp {version} %named_version_Integration_Wcf%
	%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\SimpleInjector.Integration.Wcf.QuickStart.%named_version_Integration_Wcf%.zip"
	rmdir Releases\temp /s /q

	mkdir Releases\temp
	%xcopy% %nugetTemplatePath%\.NET\SimpleInjector.Integration.WebApi Releases\temp /E /H
	%attrib% -r "%CD%\Releases\temp\*.*" /s /d
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
	%xcopy% %nugetTemplatePath%\.NET\SimpleInjector.Integration.WebApi.WebHost.QuickStart Releases\temp /E /H
	%attrib% -r "%CD%\Releases\temp\*.*" /s /d
	del Releases\temp\.gitignore /s /q
	%replace% /source:Releases\temp\SimpleInjector.Integration.WebApi.WebHost.QuickStart.nuspec {version} %named_version_Integration_WebApi%
	%replace% /source:Releases\temp\SimpleInjector.Integration.WebApi.WebHost.QuickStart.nuspec {versionCore} %named_version_Core%
	%replace% /source:Releases\temp\SimpleInjector.Integration.WebApi.WebHost.QuickStart.nuspec {version_Integration_WebApi} %named_version_Integration_WebApi%
	%replace% /source:Releases\temp\SimpleInjector.Integration.WebApi.WebHost.QuickStart.nuspec {year} %copyrightYear%
	%replace% /source:Releases\temp\package\services\metadata\core-properties\28cf0010982e4a44bd982823a7b4b6be.psmdcp {version} %named_version_Integration_WebApi%
	%compress% "%CD%\Releases\temp" "%CD%\Releases\v%named_version%\SimpleInjector.Integration.WebApi.WebHost.QuickStart.%named_version_Integration_WebApi%.zip"
	rmdir Releases\temp /s /q

	ren "%CD%\Releases\v%named_version%\*.zip" "*.nupkg"

	rem We need to reaplce the version number of the core library from 'named_version_Core >= version' to 'named_version_Core >= version < 5'
	rem from the following .nupkg files, because ReferenceGenerator does not allow setting a version range of a dependency.
	set coreLibraryNupkgDependencySearch="<dependency id=""SimpleInjector"" version=""%named_version_Core%"""
	set coreLibraryNupkgDependencyReplace="<dependency id=""SimpleInjector"" version=""[%named_version_Core%,%nextMajorVersion%)"""

	copy "SimpleInjector.DynamicAssemblyCompilation\bin\Release\SimpleInjector.DynamicAssemblyCompilation.%named_version_DynamicAssemblyCompilation%.nupkg" Releases\v%named_version%\
	%zipreplace% /zipSource:Releases\v%named_version%\SimpleInjector.DynamicAssemblyCompilation.%named_version_DynamicAssemblyCompilation%.nupkg /sourceFile:SimpleInjector.DynamicAssemblyCompilation.nuspec /search:%coreLibraryNupkgDependencySearch% /replace:%coreLibraryNupkgDependencyReplace% /force

	copy "SimpleInjector.Integration.ServiceCollection\bin\Release\SimpleInjector.Integration.ServiceCollection.%named_version_Integration_ServiceCollection%.nupkg" Releases\v%named_version%\
	%zipreplace% /zipSource:Releases\v%named_version%\SimpleInjector.Integration.ServiceCollection.%named_version_Integration_ServiceCollection%.nupkg /sourceFile:SimpleInjector.Integration.ServiceCollection.nuspec /search:%coreLibraryNupkgDependencySearch% /replace:%coreLibraryNupkgDependencyReplace% /force
	
	copy "SimpleInjector.Integration.GenericHost\bin\Release\SimpleInjector.Integration.GenericHost.%named_version_Integration_GenericHost%.nupkg" Releases\v%named_version%\
	%zipreplace% /zipSource:Releases\v%named_version%\SimpleInjector.Integration.GenericHost.%named_version_Integration_GenericHost%.nupkg /sourceFile:SimpleInjector.Integration.GenericHost.nuspec /search:%coreLibraryNupkgDependencySearch% /replace:%coreLibraryNupkgDependencyReplace% /force
	
	copy "SimpleInjector.Integration.AspNetCore\bin\Release\SimpleInjector.Integration.AspNetCore.%named_version_Integration_AspNetCore%.nupkg" Releases\v%named_version%\
	%zipreplace% /zipSource:Releases\v%named_version%\SimpleInjector.Integration.AspNetCore.%named_version_Integration_AspNetCore%.nupkg /sourceFile:SimpleInjector.Integration.AspNetCore.nuspec /search:%coreLibraryNupkgDependencySearch% /replace:%coreLibraryNupkgDependencyReplace% /force
	
	copy "SimpleInjector.Integration.AspNetCore.Mvc.Core\bin\Release\SimpleInjector.Integration.AspNetCore.Mvc.Core.%named_version_Integration_AspNetCore_Mvc_Core%.nupkg" Releases\v%named_version%\
	%zipreplace% /zipSource:Releases\v%named_version%\SimpleInjector.Integration.AspNetCore.Mvc.Core.%named_version_Integration_AspNetCore_Mvc_Core%.nupkg /sourceFile:SimpleInjector.Integration.AspNetCore.Mvc.Core.nuspec /search:%coreLibraryNupkgDependencySearch% /replace:%coreLibraryNupkgDependencyReplace%
	
	copy "SimpleInjector.Integration.AspNetCore.Mvc.ViewFeatures\bin\Release\SimpleInjector.Integration.AspNetCore.Mvc.ViewFeatures.%named_version_Integration_AspNetCore_Mvc_ViewFeatures%.nupkg" Releases\v%named_version%\
	%zipreplace% /zipSource:Releases\v%named_version%\SimpleInjector.Integration.AspNetCore.Mvc.ViewFeatures.%named_version_Integration_AspNetCore_Mvc_ViewFeatures%.nupkg /sourceFile:SimpleInjector.Integration.AspNetCore.Mvc.ViewFeatures.nuspec /search:%coreLibraryNupkgDependencySearch% /replace:%coreLibraryNupkgDependencyReplace%
	
	copy "SimpleInjector.Integration.AspNetCore.Mvc\bin\Release\SimpleInjector.Integration.AspNetCore.Mvc.%named_version_Integration_AspNetCore_Mvc%.nupkg" Releases\v%named_version%\
	%zipreplace% /zipSource:Releases\v%named_version%\SimpleInjector.Integration.AspNetCore.Mvc.%named_version_Integration_AspNetCore_Mvc%.nupkg /sourceFile:SimpleInjector.Integration.AspNetCore.Mvc.nuspec /search:%coreLibraryNupkgDependencySearch% /replace:%coreLibraryNupkgDependencyReplace%

    echo Please run step 5
    goto :EOF	
)

IF %step%==5 (
	echo Running step 5: RESTORING VERSION NUMBERS
	%replace% /line "<VersionPrefix>" "    <VersionPrefix>5.0.0</VersionPrefix>" /source:SimpleInjector\SimpleInjector.csproj
	%replace% /line "<VersionPrefix>" "    <VersionPrefix>5.0.0</VersionPrefix>" /source:SimpleInjector.DynamicAssemblyCompilation\SimpleInjector.DynamicAssemblyCompilation.csproj
	%replace% /line "<VersionPrefix>" "    <VersionPrefix>5.0.0</VersionPrefix>" /source:SimpleInjector.Integration.ServiceCollection\SimpleInjector.Integration.ServiceCollection.csproj
	%replace% /line "<VersionPrefix>" "    <VersionPrefix>5.0.0</VersionPrefix>" /source:SimpleInjector.Integration.GenericHost\SimpleInjector.Integration.GenericHost.csproj
	%replace% /line "<VersionPrefix>" "    <VersionPrefix>5.0.0</VersionPrefix>" /source:SimpleInjector.Integration.AspNetCore\SimpleInjector.Integration.AspNetCore.csproj
	%replace% /line "<VersionPrefix>" "    <VersionPrefix>5.0.0</VersionPrefix>" /source:SimpleInjector.Integration.AspNetCore.Mvc.Core\SimpleInjector.Integration.AspNetCore.Mvc.Core.csproj
	%replace% /line "<VersionPrefix>" "    <VersionPrefix>5.0.0</VersionPrefix>" /source:SimpleInjector.Integration.AspNetCore.Mvc.ViewFeatures\SimpleInjector.Integration.AspNetCore.Mvc.ViewFeatures.csproj
	%replace% /line "<VersionPrefix>" "    <VersionPrefix>5.0.0</VersionPrefix>" /source:SimpleInjector.Integration.AspNetCore.Mvc\SimpleInjector.Integration.AspNetCore.Mvc.csproj
	%replace% /line "<VersionPrefix>" "    <VersionPrefix>5.0.0</VersionPrefix>" /source:SimpleInjector.Integration.Wcf\SimpleInjector.Integration.Wcf.csproj
	%replace% /line "<VersionPrefix>" "    <VersionPrefix>5.0.0</VersionPrefix>" /source:SimpleInjector.Integration.Web\SimpleInjector.Integration.Web.csproj
	%replace% /line "<VersionPrefix>" "    <VersionPrefix>5.0.0</VersionPrefix>" /source:SimpleInjector.Integration.Web.Mvc\SimpleInjector.Integration.Web.Mvc.csproj
	%replace% /line "<VersionPrefix>" "    <VersionPrefix>5.0.0</VersionPrefix>" /source:SimpleInjector.Integration.WebApi\SimpleInjector.Integration.WebApi.csproj
	%replace% /line "<VersionPrefix>" "    <VersionPrefix>5.0.0</VersionPrefix>" /source:SimpleInjector.Packaging\SimpleInjector.Packaging.csproj
	
	echo RESTORING PACKAGE RELEASE NOTES
	
	%replace% /line "<PackageReleaseNotes>" "    <PackageReleaseNotes></PackageReleaseNotes>" /source:SimpleInjector.Integration.ServiceCollection\SimpleInjector.Integration.ServiceCollection.csproj
	%replace% /line "<PackageReleaseNotes>" "    <PackageReleaseNotes></PackageReleaseNotes>" /source:SimpleInjector.Integration.GenericHost\SimpleInjector.Integration.GenericHost.csproj
	%replace% /line "<PackageReleaseNotes>" "    <PackageReleaseNotes></PackageReleaseNotes>" /source:SimpleInjector.Integration.AspNetCore\SimpleInjector.Integration.AspNetCore.csproj
	%replace% /line "<PackageReleaseNotes>" "    <PackageReleaseNotes></PackageReleaseNotes>" /source:SimpleInjector.Integration.AspNetCore.Mvc.Core\SimpleInjector.Integration.AspNetCore.Mvc.Core.csproj
	%replace% /line "<PackageReleaseNotes>" "    <PackageReleaseNotes></PackageReleaseNotes>" /source:SimpleInjector.Integration.AspNetCore.Mvc.ViewFeatures\SimpleInjector.Integration.AspNetCore.Mvc.ViewFeatures.csproj
	%replace% /line "<PackageReleaseNotes>" "    <PackageReleaseNotes></PackageReleaseNotes>" /source:SimpleInjector.Integration.AspNetCore.Mvc\SimpleInjector.Integration.AspNetCore.Mvc.csproj
	
	echo Done!
	GOTO :EOF	
)

echo Unknown step number %step%.
GOTO :EOF

:release_directory_already_exists
echo The release directory v%named_version% already exists.
GOTO :EOF

:strong_name_key_missing
echo The strong name key SimpleInjector.snk does not exist. You should generate (a fake) one for this build script to work or copy fake.snk to SimpleInjector.snk to get things running.
GOTO :EOF

:msbuild_exe_missing
echo Couldn't locate MSBuild.exe. Expected  it  to be here: %msbuild%
GOTO :EOF

:vsvars32_bat_missing
echo Couldn't locate vsvars32.bat. Expected it to be here: %vsvars32_bat%
GOTO :EOF

:xcopy_missing
echo Couldn't locate xcopy. Expected it to be here: %xcopy%
GOTO :EOF

:referenceLibraryPath_missing
echo The directory %cd%\%referenceLibraryPath% does not exist.
GOTO :EOF