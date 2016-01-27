using System;
using System.IO;
using System.Linq;

namespace replace
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Replaces text in a specified file.");
                Console.WriteLine("replace /source:test.txt /target:test2.txt foo bar [/line]");
                return;
            }

            try
            {
                var arguments = GetArguments(args);

                var targetLines =
                    from line in File.ReadLines(arguments.Source)
                    select arguments.ReplaceCompleteLine
                        ? line.Contains(arguments.SearchText) ? arguments.ReplaceText : line
                        : line.Replace(arguments.SearchText, arguments.ReplaceText);

                File.WriteAllLines(arguments.Target, targetLines.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().FullName);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                Environment.Exit(1);
            }
        }

        private static Arguments GetArguments(string[] args)
        {
            string source = args.Single(a => a.StartsWith("/source:"));
            string target = args.SingleOrDefault(a => a.StartsWith("/target:")) ?? source;

            return new Arguments
            {
                Source = source.Substring("/source:".Length),
                Target = target.Substring("/target:".Length),
                SearchText = args.First(a => !a.StartsWith("/")),
                ReplaceText = args.Last(a => !a.StartsWith("/")),
                ReplaceCompleteLine = args.Any(a => a == "/line"),
            };
        }

        class Arguments
        {
            public string Source { get; set; }
            public string Target { get; set; }
            public string SearchText { get; set; }
            public string ReplaceText { get; set; }
            public bool ReplaceCompleteLine { get; set; }
        }
    }
}
