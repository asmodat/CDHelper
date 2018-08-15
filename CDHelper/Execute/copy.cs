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
        private static void executeCopy(string[] args)
        {
            var nArgs = CLIHelper.GetNamedArguments(args);

            switch (args[1]?.ToLower())
            {
                case "local":
                    {
                        var config = nArgs["config"].ToFileInfo();
                        var @override = nArgs["override"].ToBoolOrDefault(false);
                        var configs = config.DeserialiseJson<CopyConfig[]>();

                        foreach (var cfg in configs)
                        {
                            var src = cfg.Source.ToFileInfo();
                            var dst = cfg.Destination.ToFileInfo();

                            if (!src.Exists)
                                throw new Exception($"Source file: '{src.FullName}' does not exit.");

                            if (!dst.Directory.Exists)
                            {
                                Console.WriteLine($"Destination directory '{dst.Directory.FullName}' does not exist, creating...");
                                dst.Directory.Create();
                            }

                            Console.WriteLine($"Copying Files '{src.FullName}' => '{dst.FullName}' (Override: {cfg.Override}).");
                            src.Copy(dst, cfg.Override);
                        }
                    }
                    ; break;
                case "help":
                case "--help":
                case "-help":
                case "-h":
                case "h":
                    HelpPrinter($"{args[0]}", "Copy",
                    ("local", "Accepts params: config, override"));
                    break;
                default:
                    {
                        Console.WriteLine($"Try '{args[0]} help' to find out list of available commands.");
                        throw new Exception($"Unknown Copy command: '{args[0]} {args[1]}'");
                    }
            }
        }
    }
}
