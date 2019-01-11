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
        private static void executeCURL(string[] args)
        {
            var nArgs = CLIHelper.GetNamedArguments(args);

            switch (args[1]?.ToLower())
            {
                case "get":
                    {
                        var uri = nArgs.FirstOrDefault(x => x.Key == "uri").Value.CoalesceNullOrWhitespace(args[2]);
                        var intensity = nArgs.FirstOrDefault(x => x.Key == "intensity").Value.ToIntOrDefault(1000);
                        var requestTimeout = nArgs.FirstOrDefault(x => x.Key == "request-timeout").Value.ToIntOrDefault(5 * 1000);

                        var timeout = Math.Max(
                            requestTimeout,
                            nArgs.FirstOrDefault(x => x.Key == "timeout").Value.ToIntOrDefault(15 * 1000));

                        var showResponse = nArgs.GetValueOrDefault("show-response", "false").ToBoolOrDefault();
                        var sw = Stopwatch.StartNew();

                        Console.WriteLine($"Executing Curl GET command... timeout will occur if execution takes longer then {timeout} [ms]");

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
                case "get-file":
                    {
                        var uri = nArgs.FirstOrDefault(x => x.Key == "uri").Value.CoalesceNullOrWhitespace(args[2]);
                        var intensity = nArgs.FirstOrDefault(x => x.Key == "intensity").Value.ToIntOrDefault(1000);
                        var requestTimeout = nArgs.FirstOrDefault(x => x.Key == "request-timeout").Value.ToIntOrDefault(5 * 1000);
                        var output = nArgs["output"].ToFileInfo();
                        var @override = nArgs["override"].ToBool();
                        var basicAuth = nArgs.FirstOrDefault(x => x.Key == "basic-auth").Value;

                        if (!output.Directory.Exists)
                            throw new Exception($"Output directory was NOT found: {output.Directory}");

                        if (!@override && output.Exists)
                        {
                            Console.WriteLine($"File will not be downloaded because it already exists at: {output.FullName}");
                            return;
                        }

                        var timeout = Math.Max(
                            requestTimeout,
                            nArgs.FirstOrDefault(x => x.Key == "timeout").Value.ToIntOrDefault(15 * 1000));

                        var sw = Stopwatch.StartNew();

                        BasicAuthSecret basicAuthSecret;
                        if (!basicAuth.IsNullOrEmpty() && File.Exists(basicAuth) && basicAuth.ToFileInfo().Extension == ".json")
                        {
                            Console.WriteLine($"Basic Auth file was found at: {basicAuth}");
                            var secret = FileHelper.ReadAllAsString(basicAuth.ToFileInfo().FullName);
                            basicAuthSecret = secret.JsonDeserialize<BasicAuthSecret>();

                            if (basicAuthSecret.login == null || basicAuthSecret.password == null)
                                throw new Exception("Login or password was not defined within BasicAuthSecret json config file.");
                        }
                        else
                            throw new NotSupportedException("Basic Auth Config file was not found, and no other authorization methods are supported.");
                        
                        Console.WriteLine($"Executing Curl GET command... timeout will occur if execution takes longer then {timeout} [ms]");

                       
                        var response = CurlHelper.AwaitSuccessCurlGET(
                            uri: uri,
                            timeout: timeout,
                            intensity: intensity,
                            requestTimeout: requestTimeout,
                            headers: new (string, string)[] {
                                ("Authorization", $"Bearer {($"{basicAuthSecret.login}:{basicAuthSecret.password}").Base64Encode()}")
                            }).Result;

                        var fileContent = response.Content.ReadAsByteArrayAsync().Result;

                        Console.WriteLine($"Curl GET commend executed sucessfully, elapsed {sw.ElapsedMilliseconds} [ms], read: {fileContent.Length} bytes, writing result to: '{output.FullName}'.");

                        if (!output.Exists)
                        {
                            var stream = output.Create();
                            stream.Write(fileContent, 0, fileContent.Length);
                            stream.Close();
                        } else
                            output.WriteAllBytes(fileContent);

                        Console.WriteLine($"Success, fileContent was saved.");
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
