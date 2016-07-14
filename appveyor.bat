REM nuget restore

if not exist SimpleInjector.snk copy fake.snk SimpleInjector.snk

msbuild "SimpleInjector.NET\SimpleInjector.NET.csproj" /nologo /p:Configuration=Debug
msbuild "SimpleInjector.Packaging\SimpleInjector.Packaging.csproj" /nologo /p:Configuration=Debug
msbuild "SimpleInjector.Extensions.LifetimeScoping\SimpleInjector.Extensions.LifetimeScoping.csproj" /nologo /p:Configuration=Debug
msbuild "SimpleInjector.Extensions.ExecutionContextScoping\SimpleInjector.Extensions.ExecutionContextScoping.csproj" /nologo /p:Configuration=Debug
msbuild "SimpleInjector.Integration.Web\SimpleInjector.Integration.Web.csproj" /nologo /p:Configuration=Debug
msbuild "SimpleInjector.Integration.Web.Mvc\SimpleInjector.Integration.Web.Mvc.csproj" /nologo /p:Configuration=Debug
msbuild "SimpleInjector.Integration.Wcf\SimpleInjector.Integration.Wcf.csproj" /nologo /p:Configuration=Debug

msbuild "SimpleInjector.NET.Tests.Unit\SimpleInjector.NET.Tests.Unit.csproj" /nologo /p:Configuration=Debug
msbuild "SimpleInjector.Extensions.LifetimeScoping.Tests.Unit\SimpleInjector.Extensions.LifetimeScoping.Tests.Unit.csproj" /nologo /p:Configuration=Debug
msbuild "SimpleInjector.Integration.Web.Tests.Unit\SimpleInjector.Integration.Web.Tests.Unit.csproj" /nologo /p:Configuration=Debug
msbuild "SimpleInjector.Extensions.ExecutionContextScoping.Tests.Unit\SimpleInjector.Extensions.ExecutionContextScoping.Tests.Unit.csproj" /nologo /p:Configuration=Debug
