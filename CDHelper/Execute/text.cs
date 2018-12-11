using System;
using AsmodatStandard.Extensions;
using AsmodatStandard.IO;
using AsmodatStandard.Extensions.IO;
using AsmodatStandard.Extensions.Collections;
using System.IO;
using System.Threading;

namespace CDHelper
{
    public partial class Program
    {
        private static void executeText(string[] args)
        {
            var nArgs = CLIHelper.GetNamedArguments(args);

            switch (args[1]?.ToLower())
            {
                case "replace":
                    {
                        var @new = nArgs["new"];
                        var old = nArgs["old"];
                        var files = FileHelper.GetFiles(nArgs["input"],
                            pattern: nArgs.GetValueOrDefault("pattern") ?? "*",
                            recursive: nArgs.GetValueOrDefault("recursive").ToBoolOrDefault(false));


                        int count = 0;
                        files.ParallelForEach(file =>
                        {
                            if (!file.Exists)
                                throw new Exception($"Input file does not exists '{file.FullName}'");

                            Console.WriteLine($"Reading Text File '{file.FullName}' ...");
                            var text = file.ReadAllText();

                            if (text?.Contains(old) == true)
                            {
                                Console.WriteLine($"Replacing text '{old}' with '{@new}' in file '{file.FullName}' ...");
                                text = text.Replace(old, @new);
                                file.WriteAllText(text);
                                Interlocked.Increment(ref count);
                                Console.WriteLine($"Success, Replaced text in file '{file.FullName}' [{file?.Length ?? 0}]");
                            }
                            else
                                Console.WriteLine($"File '{file.FullName}' does NOT contain text '{old}', skipping replacement.");
                        });

                        Console.WriteLine($"SUCCESS, Replaced text in {count} files. ");
                    }
                    ; break;
                case "dos2unix":
                    {
                        var files = FileHelper.GetFiles(nArgs["input"],
                            pattern: nArgs.GetValueOrDefault("pattern") ?? "*",
                            recursive: nArgs.GetValueOrDefault("recursive").ToBoolOrDefault(false));

                        files.ParallelForEach(file =>
                        {
                            Console.WriteLine($"Converting '{file?.FullName}' [{file?.Length ?? 0}] from dos to unix format...");
                            file.ConvertDosToUnix();
                            file.Refresh();
                            Console.WriteLine($"Success, Converted '{file?.FullName}' [{file?.Length ?? 0}]");
                        });

                        Console.WriteLine($"SUCCESS, Converted {files?.Length ?? 0} files to unix format. ");
                    }
                    ; break;
                case "help":
                case "--help":
                case "-help":
                case "-h":
                case "h":
                    HelpPrinter($"{args[0]}", "String Manipulation",
                    ("replace", "Accepts params: old, new"),
                    ("dos2unix", "Accepts params: input"));
                    break;
                default:
                    {
                        Console.WriteLine($"Try '{args[0]} help' to find out list of available commands.");
                        throw new Exception($"Unknown String command: '{args[0]} {args[1]}'");
                    }
            }
        }
    }
}
