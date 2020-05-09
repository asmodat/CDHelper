using System;
using System.Diagnostics;
using System.Threading;
using AsmodatStandard.Extensions;
using AsmodatStandard.Extensions.Collections;
using AsmodatStandard.IO;
using AsmodatStandard.Types;
using System.Threading.Tasks;

namespace CDHelper
{
    public partial class Program
    {
        public static string _version = "0.6.2";
        public static bool _debug = false;
        public static bool _silent = false;
        public static string[] _hide_input_values = new string[0];

        private static string ModerateString(string s, string[] hide = null)
        {
            var hideStrings = hide.Merge(_hide_input_values);
            if (!hideStrings.IsNullOrEmpty())
                foreach (var v in hideStrings)
                    if (!v.IsNullOrEmpty())
                        s = s.Replace(v, "*".Repeat(v.Length));

            return s;
        }

        public static void WriteLine(string s)
        {
            if (!_silent)
                Console.WriteLine(ModerateString(s));
        }

        static async Task Main(string[] args)
        {
            var nArgs = CLIHelper.GetNamedArguments(args);
            _silent = nArgs.GetValueOrDefault("silent").ToBoolOrDefault(false);

            WriteLine($"[{TickTime.Now.ToLongDateTimeString()}] *** Started CDHelper v{_version} by Asmodat ***");

            if (args.Length < 1)
            {
                WriteLine("Try 'help' to find out list of available commands.");
                throw new Exception("At least 1 argument must be specified.");
            }

            _hide_input_values = nArgs.GetValueOrDefault("hide-input-values", "[ ]").JsonDeserialize<string[]>();

            if (args.Length > 1 && !nArgs.GetValueOrDefault("hide-input").ToBoolOrDefault(false))
            {
                var nArgsString = nArgs.JsonSerialize(Newtonsoft.Json.Formatting.Indented);
                WriteLine($"Executing command: '{args[0]} {args[1]}' Named Arguments: \n{nArgsString}\n");
            }

            string executionMode;
            if (nArgs.ContainsKey("execution-mode") &&
                !(executionMode = nArgs["execution-mode"]).IsNullOrEmpty())
            {
                if (executionMode == "debug")
                {
                    _debug = true;
                    await Execute(args);
                }
                else if (executionMode == "silent-errors")
                {
                    try
                    {
                        await Execute(args);
                    }
                    catch (Exception ex)
                    {
                        WriteLine($"[{TickTime.Now.ToLongDateTimeString()}] Failure, Error Message: {ex.JsonSerializeAsPrettyException()}");
                    }
                }
                else if (executionMode == "retry")
                {
                    int counter = 0;
                    var sw = Stopwatch.StartNew();
                    int times = (nArgs.ContainsKey("retry-times") ? nArgs["retry-times"] : "1").ToIntOrDefault(1);
                    int delay = (nArgs.ContainsKey("retry-delay") ? nArgs["retry-delay"] : "1000").ToIntOrDefault(1);
                    bool throws = (nArgs.ContainsKey("retry-throws") ? nArgs["retry-throws"] : "true").ToBoolOrDefault(true);
                    int timeout = (nArgs.ContainsKey("retry-timeout") ? nArgs["retry-timeout"] : $"{60 * 3600}").ToIntOrDefault(60 * 3600);

                    WriteLine($"Execution with retry: Max: {times}, Delay: {delay} [ms], Throws: {(throws ? "Yes" : "No")}, Timeout: {timeout} [s]");

                    do
                    {
                        WriteLine($"Execution trial: {counter}/{(times + 1)}, Elapsed/Timeout: {sw.ElapsedMilliseconds / 1000}/{timeout} [s]");

                        try
                        {
                            await Execute(args);
                            return;
                        }
                        catch (Exception ex)
                        {
                            WriteLine($"[{TickTime.Now.ToLongDateTimeString()}] Failure, Error Message: {ex.JsonSerializeAsPrettyException()}");

                            if ((sw.ElapsedMilliseconds / 1000) >= timeout || (throws && counter == times))
                                throw;

                            WriteLine($"Execution retry delay: {delay} [ms]");
                            Thread.Sleep(delay);
                        }
                    }
                    while (++counter <= times);

                    return;
                }
                else
                    throw new Exception($"[{TickTime.Now.ToLongDateTimeString()}] Unknown execution-mode: '{executionMode}', try: 'debug' or 'silent-errors'.");
            }
            else
            {
                try
                {
                    await Execute(args);
                }
                catch(Exception ex)
                {
                    WriteLine($"[{TickTime.Now.ToLongDateTimeString()}] Failure, Error Message: {ex.JsonSerializeAsPrettyException()}");
                    throw new Exception($"CDHelper v{_version} failed during execution of {args[0]} command.");
                }
            }

            WriteLine($"[{TickTime.Now.ToLongDateTimeString()}] Success");
        }

        private static async Task Execute(string[] args)
        {
            switch (args[0]?.ToLower().TrimStart("-"))
            {
                case "ssh":
                    executeSSH(args);
                    break;
                case "curl":
                    executeCURL(args);
                    break;
                case "hash":
                    executeHash(args);
                    break;
                case "aes":
                    executeAES(args);
                    break;
                case "rsa":
                    executeRSA(args);
                    break;
                case "bitbucket":
                    executeBitbucket(args);
                    break;
                case "github":
                    executeGithub(args);
                    break;
                case "email":
                    await executeEmail(args);
                    break;
                case "scheduler":
                    await executeScheduler(args);
                    break;
                case "console":
                    executeConsole(args);
                    break;
                case "copy":
                    executeCopy(args);
                    break;
                case "cli":
                    executeCLI(args);
                    break;
                case "text":
                    executeText(args);
                    break;
                case "time":
                    executeTime(args);
                    break;
                case "docker":
                    executeDocker(args);
                    break;
                case "version":
                case "ver":
                case "v":
                    Console.WriteLine($"v{_version}");
                    break;
                case "help":
                case "h":
                    HelpPrinter($"{args[0]}", "CDHelper List of available commands",
                    ("ssh", "Accepts params: command"),
                    ("cli", "Accepts params: command"),
                    ("console", "Accepts params: prompt-read-line"),
                    ("copy", "Accepts params: local"),
                    ("curl", "Accepts params: GET, GET-FILE"),
                    ("docker", "Accepts params: gen"),
                    ("github", "Accepts params: on-change-process"),
                    ("email", "Accepts params: send"),
                    ("scheduler", "Accepts params: github"),
                    ("hash", "Accepts params: SHA256"),
                    ("text", "Accepts params: replace, dos2unix"),
                    ("time", "Accepts params: add, unix2timestamp"),
                    ("AES", "Accepts params: create-key, encrypt, decrypt"),
                    ("RSA", "Accepts params: create-key, sign, verify"),
                    ("bitbucket", "Accepts params: pull-approve, pull-unapprove, pull-comment"),
                    ("[flags]", "Allowed Syntax: key=value, --key=value, -key='v1 v2 v3', -k, --key"),
                    ("--execution-mode=silent-errors", "[All commands] Don't throw errors, only displays exception message."),
                    ("--execution-mode=debug", "[All commands] Throw instantly without reporting a failure."));
                    break;
                default:
                    {
                        Console.WriteLine("Try 'help' to find out list of available commands.");
                        throw new Exception($"Unknown command: '{args[0]}'.");
                    }
            }
        }

        private static void HelpPrinter(string cmd, string description, params (string param, string descritpion)[] args)
        {
            Console.WriteLine($"### HELP: {cmd}");
            Console.WriteLine($"### DESCRIPTION: ");
            Console.WriteLine($"{description}");

            if (!args.IsNullOrEmpty())
            {
                Console.WriteLine($"### OPTIONS");
                for (int i = 0; i < args.Length; i++)
                {
                    var arg = args[i];
                    Console.WriteLine($"### Option-{(i + 1)}: {arg.param}");

                    if (!arg.descritpion.IsNullOrEmpty())
                        Console.WriteLine($"{arg.descritpion}");
                }
            }

            Console.WriteLine($"### HELP: {cmd}");
        }
    }
}
