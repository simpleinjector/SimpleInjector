mkdir coverage

cd SimpleInjector.Core.Tests.Unit

..\packages\OpenCover.4.6.519\tools\OpenCover.Console.exe -mergeoutput -register:user -excludebyattribute:*.ExcludeFromCodeCoverage*^ "-target:C:\Program Files\dotnet\dotnet.exe" -targetargs:test -filter:"+[*]SimpleInjector.* -[*.Tests.*]* -[*.CodeSamples*]*" -output:..\coverage\coverage.xml -oldStyle

..\packages\ReportGenerator.2.5.3\tools\ReportGenerator.exe "-reports:..\coverage\coverage.xml" "-targetdir:..\coverage\report" "-filters:-*.Tests*;" "-historydir:coverage\history"

..\coverage\report\index.htm