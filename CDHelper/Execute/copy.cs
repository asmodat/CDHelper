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

                        Console.WriteLine($"Local Copy Execution according to '{config.FullName}'.");

                        var configs = config.DeserialiseJson<CopyConfig[]>();

                        foreach (var cfg in configs)
                        {
                            string src, dst;

                            if (!RuntimeEx.IsWindows())
                            {
                                Console.WriteLine($"Detected OS is NOT Windows, paths containing '\\' and ':' will be replaced with '/'");
                                src = cfg.Source.ToLinuxPath();
                                dst = cfg.Destination.ToLinuxPath();
                            }
                            else
                            {
                                src = cfg.Source;
                                dst = cfg.Destination;
                            }

                            if (src.IsFile())
                            {
                                Console.WriteLine($"Detected, that Source is a FILE");

                                var srcInfo = src.ToFileInfo();
                                var dstInfo = dst.ToFileInfo();

                                if (!srcInfo.Exists)
                                    throw new Exception($"Source file: '{srcInfo.FullName}' does not exit.");

                                if (!dstInfo.Directory.Exists)
                                {
                                    Console.WriteLine($"Destination directory '{dstInfo.Directory.FullName}' does not exist, creating...");
                                    dstInfo.Directory.Create();
                                }

                                Console.WriteLine($"Copying Files '{srcInfo.FullName}' => '{dstInfo.FullName}' (Override: {cfg.Override}).");
                                srcInfo.Copy(dstInfo, cfg.Override);
                            }
                            else if (src.IsDirectory())
                            {
                                Console.WriteLine($"Detected, that Source is a DIRECTORY");

                                var srcInfo = src.ToDirectoryInfo();
                                var dstInfo = dst.ToDirectoryInfo();

                                if (!srcInfo.Exists)
                                    throw new Exception($"Source directory: '{srcInfo.FullName}' does not exit.");

                                if (!dstInfo.Exists)
                                {
                                    Console.WriteLine($"Destination directory '{dstInfo.FullName}' does not exist, creating...");
                                    dstInfo.Create();
                                }

                                if (!dst.IsDirectory())
                                    throw new Exception("If source is a directory then destination also must be a directory.");

                                Console.WriteLine($"Recursive Copy: '{cfg.Recursive}'");
                                foreach (string newPath in Directory.GetFiles(srcInfo.FullName, "*.*", cfg.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
                                {
                                    Console.WriteLine($"Copying Files '{srcInfo.FullName}' => '{dstInfo.FullName}' (Override: {cfg.Override}).");
                                    File.Copy(newPath, newPath.Replace(srcInfo.FullName, dstInfo.FullName), cfg.Override);
                                }
                            }
                            else
                                throw new Exception($"Source '{src}' is neither a file or a directory.");
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
