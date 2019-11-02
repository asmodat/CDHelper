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
        private static void executeCLI(string[] args)
        {
            var nArgs = CLIHelper.GetNamedArguments(args);

            switch (args[1]?.ToLower())
            {
                case "command":
                    {
                        var file = nArgs["file"];
                        var argsuments = nArgs["args"];
                        var directory = nArgs["dir"];
                        var timeout = nArgs.GetValueOrDefault("timeout").ToIntOrDefault(0);
                        var output = CLIHelper.Command(
                            fileName: file, 
                            args: argsuments, 
                            workingDirectory: directory,
                            timeout: timeout);

                        Console.WriteLine(output.JsonSerialize(Newtonsoft.Json.Formatting.Indented));

                        Console.WriteLine($"Success, command was executed.");
                    }
                    ; break;
                case "help":
                case "--help":
                case "-help":
                case "-h":
                case "h":
                    HelpPrinter($"{args[0]}", "Execute command",
                    ("command", "Accepts params: file, args, dir, timeout)")
                    );
                    break;
                default:
                    {
                        Console.WriteLine($"Try '{args[0]} help' to find out list of available commands.");
                        throw new Exception($"Unknown CLI command: '{args[0]} {args[1]}'");
                    }
            }
        }
    }
}
