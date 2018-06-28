using System;
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
                    CurlHelper.AwaitSuccessCurlGET(
                        uri: nArgs.FirstOrDefault(x => x.Key == "uri").Value.CoalesceNullOrWhitespace(args[2]),
                        timeout: nArgs.FirstOrDefault(x => x.Key == "timeout").Value.ToIntOrDefault(0),
                        intensity: nArgs.FirstOrDefault(x => x.Key == "intensity").Value.ToIntOrDefault(1000)).Wait();
                    ; break;
                case "help":
                    HelpPrinter($"{args[0]}", "Curl Like Web Requests",
                    ("GET", "Accepts params: uri, timeout (optional [ms]), intensity (default 1000 [ms])"));
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
