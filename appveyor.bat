REM nuget restore

ren fake.snk SimpleInjector.snk

msbuild "SimpleInjector.NET\SimpleInjector.NET.csproj" /nologo
msbuild "SimpleInjector.Packaging\SimpleInjector.Packaging.csproj" /nologo
msbuild "SimpleInjector.Extensions.LifetimeScoping\SimpleInjector.Extensions.LifetimeScoping.csproj" /nologo
msbuild "SimpleInjector.Integration.Web\SimpleInjector.Integration.Web.csproj" /nologo
rem msbuild "SimpleInjector.Integration.Web.Mvc\SimpleInjector.Integration.Web.Mvc.csproj" /nologo
msbuild "SimpleInjector.Integration.Wcf\SimpleInjector.Integration.Wcf.csproj" /nologo

msbuild "SimpleInjector.NET.Tests.Unit\SimpleInjector.NET.Tests.Unit.csproj" /nologo
msbuild "SimpleInjector.Extensions.LifetimeScoping.Tests.Unit\SimpleInjector.Extensions.LifetimeScoping.Tests.Unit.csproj" /nologo
msbuild "SimpleInjector.Integration.Web.Tests.Unit\SimpleInjector.Integration.Web.Tests.Unit.csproj" /nologo

ren SimpleInjector.snk fake.snk
