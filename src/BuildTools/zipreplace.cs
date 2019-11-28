using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace zipreplace
{
    internal static class Program
    {
        // Example: /zipSource:test/Test.zip /zipTarget:test/Test2.zip /sourceFile:foobar.text foo bar
        private static void Main(string[] args)
        {
            if (args == null || args.Length == 0 ||
                (new [] { "?", "-h", "-help", "help" }).Contains(args[0]))
            {
                Console.WriteLine("Replaces text in a specified file within a .zip file.");
                Console.WriteLine(
                    "zipreplace.exe " +
                        "/zipSource:source.zip " +
                        "[/zipTarget:target.zip] " +
                        "/source:source.txt " +
                        "[/target:target.txt] " +
                        "\"search text\" " +
                        "\"replacement text\" " +
                        "[/force] " +
                        "[/line]");
                return;
            }

            string decompressPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            Arguments arguments = GetArguments(args);

            if (arguments.InvalidArguments.Any())
                ExitWithMessage("Invalid arguments found.", arguments);

            if (string.IsNullOrWhiteSpace(arguments.ZipSource))
                ExitWithMessage("/zipSource not supplied.", arguments);

            if (!File.Exists(arguments.ZipSource))
                ExitWithMessage(arguments.ZipSource + " not found.", arguments);

            if (string.IsNullOrWhiteSpace(arguments.SourceFile))
                ExitWithMessage("/sourceFile not supplied.", arguments);

            if (Path.IsPathRooted(arguments.SourceFile))
                ExitWithMessage(arguments.SourceFile + " should be a relative path.", arguments);

            try
            {
                Directory.CreateDirectory(decompressPath);

                ZipFile.ExtractToDirectory(
                    sourceArchiveFileName: arguments.ZipSource,
                    destinationDirectoryName: decompressPath);

                string sourceFile = Path.Combine(decompressPath, arguments.SourceFile);
                string targetFile = Path.Combine(decompressPath, arguments.TargetFile);

                if (!File.Exists(sourceFile))
                    throw new FileNotFoundException(
                        arguments.SourceFile + " not found in " + arguments.ZipSource);

                if (arguments.ForceReplace)
                {
                    CheckInForceMode(arguments, sourceFile);
                }

                var targetLines =
                    from line in File.ReadLines(sourceFile)
                    select arguments.ReplaceCompleteLine
                        ? (line.Contains(arguments.SearchText) ? arguments.ReplaceText : line)
                        : line.Replace(arguments.SearchText, arguments.ReplaceText);

                File.WriteAllLines(targetFile, targetLines.ToArray());

                string tmpZip = arguments.ZipTarget + ".zipreplace_tmp";

                ZipFile.CreateFromDirectory(
                    sourceDirectoryName: decompressPath,
                    destinationArchiveFileName: tmpZip,
                    compressionLevel: CompressionLevel.Optimal,
                    includeBaseDirectory: false);

                File.Delete(arguments.ZipTarget);
                File.Move(tmpZip, arguments.ZipTarget);

                Directory.Delete(decompressPath, recursive: true);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);

                if (arguments.IncludeStackTrace)
                {
                    Console.WriteLine(ex.GetType().FullName);
                    Console.WriteLine(ex.StackTrace);
                }

                Console.ForegroundColor = ConsoleColor.White;

                Directory.Delete(decompressPath, recursive: true);

                Environment.Exit(1);
            }
        }

        private static void CheckInForceMode(Arguments arguments, string sourceFile)
        {
            var matches =
                from line in File.ReadLines(sourceFile)
                where line.Contains(arguments.SearchText)
                select line;

            if (!matches.Any())
            {
                throw new FileNotFoundException(
                    $"Search text '{arguments.SearchText}' was not found in {arguments.SourceFile}.");
            }
        }

        private static void ExitWithMessage(string message, Arguments arguments)
        {
            Console.WriteLine("Error: " + message);

            Console.WriteLine("Supplied values:");
            Console.WriteLine($"  * zip source:            {arguments.ZipSource}");
            Console.WriteLine($"  * zip target:            {arguments.ZipTarget}");
            Console.WriteLine($"  * source file:           {arguments.SourceFile}");
            Console.WriteLine($"  * target file:           {arguments.TargetFile}");
            Console.WriteLine($"  * search text:           {arguments.SearchText}");
            Console.WriteLine($"  * replace text:          {arguments.ReplaceText}");
            Console.WriteLine($"  * replace complete line: {arguments.ReplaceCompleteLine}");
            Console.WriteLine($"  * force replace:         {arguments.ForceReplace}");

            if (arguments.InvalidArguments.Any())
            {
                Console.WriteLine();
                Console.WriteLine("Invalid arguments:");
                foreach (var arg in arguments.InvalidArguments)
                {
                    Console.WriteLine($"  * {arg}");
                }
            }

            Environment.Exit(1);
        }

        private static Arguments GetArguments(string[] args)
        {
            string zipSource = args.SingleOrDefault(a => a.StartsWith("/zipSource:"))
                ?.Substring("/zipSource:".Length);

            string zipTarget = args.SingleOrDefault(a => a.StartsWith("/zipTarget:"))
                ?.Substring("/zipTarget:".Length) ?? zipSource;

            string sourceFile = args.SingleOrDefault(a => a.StartsWith("/sourceFile:"))
                ?.Substring("/sourceFile:".Length);

            string targetFile = args.SingleOrDefault(a => a.StartsWith("/targetFile:"))
                ?.Substring("/targetFile:".Length) ?? sourceFile;

            return new Arguments
            {
                ZipSource = zipSource,
                ZipTarget = zipTarget,
                SourceFile = sourceFile,
                TargetFile = targetFile,
                SearchText = args.First(a => !a.StartsWith("/")),
                ReplaceText = args.Last(a => !a.StartsWith("/")),
                ReplaceCompleteLine = args.Any(a => a == "/line"),
                ForceReplace = args.Any(a => a == "/force"),
                IncludeStackTrace = args.Any(a => a == "/stackTrace"),
                InvalidArguments = (
                    from arg in args
                    where arg.StartsWith("/")
                    where !arg.StartsWith("/zipSource:")
                    where !arg.StartsWith("/zipTarget:")
                    where !arg.StartsWith("/sourceFile:")
                    where !arg.StartsWith("/targetFile:")
                    where !arg.StartsWith("/force")
                    select arg)
                    .ToArray()
            };
        }

        class Arguments
        {
            public string ZipSource { get; set; }
            public string ZipTarget { get; set; }
            public string SourceFile { get; set; }
            public string TargetFile { get; set; }
            public string SearchText { get; set; }
            public string ReplaceText { get; set; }
            public bool ReplaceCompleteLine { get; set; }
            public bool ForceReplace { get; set; }
            public bool IncludeStackTrace { get; set; }
            public string[] InvalidArguments { get; set; }
        }
    }
}