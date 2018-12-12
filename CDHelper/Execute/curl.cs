using System;
using System.Collections.Generic;
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
                        var uri = nArgs.FirstOrDefault(x => x.Key == "uri").Value.CoalesceNullOrWhitespace(args[2]);
                        var timeout = nArgs.FirstOrDefault(x => x.Key == "timeout").Value.ToIntOrDefault(15 * 1000);
                        var intensity = nArgs.FirstOrDefault(x => x.Key == "intensity").Value.ToIntOrDefault(1000);
                        var requestTimeout = nArgs.FirstOrDefault(x => x.Key == "request-timeout").Value.ToIntOrDefault(5 * 1000);
                        var showResponse = nArgs.GetValueOrDefault("show-response", "false").ToBoolOrDefault();
                        var sw = Stopwatch.StartNew();
                        var response = CurlHelper.AwaitSuccessCurlGET(
                            uri: uri,
                            timeout: timeout,
                            intensity: intensity,
                            requestTimeout: requestTimeout).Result;
                        
                        Console.WriteLine($"Curl GET commend executed sucessfully, elapsed {sw.ElapsedMilliseconds} [ms]");

                        if (showResponse)
                            Console.WriteLine("Response: " + (response?.JsonSerialize(Newtonsoft.Json.Formatting.Indented) ?? "null"));
                    }
                    ; break;
                case "help":
                case "--help":
                case "-help":
                case "-h":
                case "h":
                    HelpPrinter($"{args[0]}", "Curl Like Web Requests",
                    ("GET", "Accepts params: uri, timeout (optional [ms], 15s default), intensity (default 1000 [ms]), request-timeout (optional [ms], 5s default)"));
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
