using System;
using AsmodatStandard.Extensions;
using AsmodatStandard.IO;
using AsmodatStandard.Cryptography;
using AsmodatStandard.Extensions.IO;
using AsmodatStandard.Extensions.Threading;
using AsmodatStandard.Extensions.Collections;
using System.IO;

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
                        var input = nArgs["input"].ToFileInfo();

                        if (!input.Exists)
                            throw new Exception($"Input file does not exists '{input.FullName}'");

                        Console.WriteLine($"Reading '{input.FullName}' ...");
                        var text = input.ReadAllText();

                        var @new = nArgs["new"];
                        var old = nArgs["old"];

                        Console.WriteLine($"Replacing '{old}' with '{@new}' ...");
                        text = text.Replace(old, @new);

                        Console.WriteLine($"Saving '{input.FullName}' ...");
                        input.WriteAllText(text);
                        Console.WriteLine("SUCCESS");
                    }
                    ; break;
                case "dos2unix":
                    {
                        var input = nArgs["input"].ToFileInfo();

                        if (!input.Exists)
                            throw new Exception($"Input file does not exists '{input.FullName}'");

                        Console.WriteLine($"Converting '{input?.FullName}' [{input?.Length ?? 0}] from dos to unix format...");
                        input.ConvertDosToUnix();

                        input.Refresh();
                        Console.WriteLine($"SUCCESS [{input?.Length ?? 0}]");
                    }
                    ; break;
                case "help":
                case "--help":
                case "-help":
                case "-h":
                case "h":
                    HelpPrinter($"{args[0]}", "String",
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
