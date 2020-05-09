using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AsmodatStandard.Extensions;
using AsmodatStandard.Extensions.IO;
using AsmodatStandard.IO;

namespace CDHelper
{
    public partial class Program
    {
        private static void executeConsole(string[] args)
        {
            var nArgs = CLIHelper.GetNamedArguments(args);

            switch (args[1]?.ToLower())
            {
                case "prompt-read-line":
                    {
                        var prompts = nArgs.GetValueOrDefault("prompts","").Split(",");
                        var output = new List<string>();
                        foreach(var prompt in prompts)
                        {
                            Console.Write(prompt);
                            output.Add(Console.ReadLine());
                        }
                        Console.WriteLine(output.ToArray().JsonSerialize());
                    }
                    ; break;
                case "help":
                case "--help":
                case "-help":
                case "-h":
                case "h":
                    HelpPrinter($"{args[0]}", "Execute command",
                    ("prompt-read-line", "Accepts params: input)")
                    );
                    break;
                default:
                    {
                        Console.WriteLine($"Try '{args[0]} help' to find out list of available commands.");
                        throw new Exception($"Unknown console command: '{args[0]} {args[1]}'");
                    }
            }
        }
    }
}
