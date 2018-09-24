﻿using System;
using System.Diagnostics;
using System.Linq;
using AsmodatStandard.Extensions;
using AsmodatStandard.IO;

namespace CDHelper
{
    public partial class Program
    {
        private static void executeCURL(string[] args)
        {
            var nArgs = CLIHelper.GetNamedArguments(args);

            switch (args[1]?.ToLower())
            {
                case "get":
                    {
                        var sw = Stopwatch.StartNew();
                        CurlHelper.AwaitSuccessCurlGET(
                            uri: nArgs.FirstOrDefault(x => x.Key == "uri").Value.CoalesceNullOrWhitespace(args[2]),
                            timeout: nArgs.FirstOrDefault(x => x.Key == "timeout").Value.ToIntOrDefault(0),
                            intensity: nArgs.FirstOrDefault(x => x.Key == "intensity").Value.ToIntOrDefault(1000),
                            requestTimeout: nArgs.FirstOrDefault(x => x.Key == "request-timeout").Value.ToIntOrDefault(6 * 1000)).Wait();
                        Console.WriteLine($"Curl GET commend executed sucessfully, elapsed {sw.ElapsedMilliseconds} [ms]");
                    }
                    ; break;
                case "help":
                case "--help":
                case "-help":
                case "-h":
                case "h":
                    HelpPrinter($"{args[0]}", "Curl Like Web Requests",
                    ("GET", "Accepts params: uri, timeout (optional [ms]), intensity (default 1000 [ms]), request-timeout (optional [ms], 5 min default)"));
                    break;
                default:
                    {
                        Console.WriteLine($"Try '{args[0]} help' to find out list of available commands.");
                        throw new Exception($"Unknown ECS command: '{args[0]} {args[1]}'");
                    }
            }
        }
    }
}
