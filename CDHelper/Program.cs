using System;
using AsmodatStandard.Extensions;
using AsmodatStandard.Extensions.Collections;
using AsmodatStandard.IO;
using AsmodatStandard.Types;

namespace CDHelper
{
    public partial class Program
    {
        public static string _version = "0.3.0";

        private static string ModerateString(string s, string[] mod_array)
        {
            foreach (var v in mod_array)
                if (!v.IsNullOrEmpty())
                    s = s.Replace(v, "*".Repeat(v.Length));

            return s;
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"[{TickTime.Now.ToLongDateTimeString()}] *** Started CDHelper v{_version} by Asmodat ***");

            if (args.Length < 1)
            {
                Console.WriteLine("Try 'help' to find out list of available commands.");
                throw new Exception("At least 1 argument must be specified.");
            }

            var nArgs = CLIHelper.GetNamedArguments(args);
            var hide_input_values = nArgs.GetValueOrDefault("hide-input-values", "[ ]").JsonDeserialize<string[]>();

            if (args.Length > 1 && !nArgs.GetValueOrDefault("hide-input").ToBoolOrDefault(false))
            {
                var nArgsString = nArgs.JsonSerialize(Newtonsoft.Json.Formatting.Indented);
                Console.WriteLine($"Executing command: '{args[0]} {args[1]}' Named Arguments: \n{ModerateString(nArgsString, hide_input_values)}\n");
            }

            string executionMode;
            if (nArgs.ContainsKey("execution-mode") &&
                !(executionMode = nArgs["execution-mode"]).IsNullOrEmpty())
            {
                if (executionMode == "debug")
                {
                    Execute(args);
                }
                else if (executionMode == "silent-errors")
                {
                    try
                    {
                        Execute(args);
                    }
                    catch (Exception ex)
                    {
                        var errMessage = ModerateString(ex.JsonSerializeAsPrettyException(), hide_input_values);
                        Console.WriteLine($"[{TickTime.Now.ToLongDateTimeString()}] Failure, Error Message: {errMessage}");
                    }
                }
                else
                    throw new Exception($"[{TickTime.Now.ToLongDateTimeString()}] Unknown execution-mode: '{executionMode}', try: 'debug' or 'silent-errors'.");
            }
            else
            {
                try
                {
                    Execute(args);
                }
                catch(Exception ex)
                {
                    var errMessage = ModerateString(ex.JsonSerializeAsPrettyException(), hide_input_values);
                    Console.WriteLine($"[{TickTime.Now.ToLongDateTimeString()}] Failure, Error Message: {errMessage}");
                    throw new Exception($"CDHelper v{_version} failed during execution of {args[0]} command.");
                }
            }

            Console.WriteLine($"[{TickTime.Now.ToLongDateTimeString()}] Success");
        }

        private static void Execute(string[] args)
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
                case "copy":
                    executeCopy(args);
                    break;
                case "text":
                    executeText(args);
                    break;
                case "docker":
                    executeDocker(args);
                    break;
                case "version":
                case "ver":
                case "v":
                    Console.WriteLine($"Version: v{_version}");
                    break;
                case "help":
                case "h":
                    HelpPrinter($"{args[0]}", "CDHelper List of available commands",
                    ("ssh", "Accepts params: command"),
                    ("copy", "Accepts params: local"),
                    ("curl", "Accepts params: GET, GET-FILE"),
                    ("docker", "Accepts params: gen"),
                    ("hash", "Accepts params: SHA256"),
                    ("text", "Accepts params: replace, dos2unix"),
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
