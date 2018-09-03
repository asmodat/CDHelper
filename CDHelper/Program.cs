using System;
using AsmodatStandard.Extensions;
using AsmodatStandard.Extensions.Collections;
using AsmodatStandard.IO;
using AsmodatStandard.Types;

namespace CDHelper
{
    public partial class Program
    {
        public static string _version = "0.1.6";

        static void Main(string[] args)
        {
            Console.WriteLine($"[{TickTime.Now.ToLongDateTimeString()}] *** Started CDHelper v{_version} by Asmodat ***");

            if (args.Length < 1)
            {
                Console.WriteLine("Try 'help' to find out list of available commands.");
                throw new Exception("At least 1 argument must be specified.");
            }

            var nArgs = CLIHelper.GetNamedArguments(args);

            if (args.Length > 1 && !nArgs.GetValueOrDefault("hide-input").ToBoolOrDefault(false))
            {
                var nArgsString = nArgs.JsonSerialize(Newtonsoft.Json.Formatting.Indented);
                var hide_input_values = nArgs.GetValueOrDefault("hide-input-values", "[ ]").JsonDeserialize<string[]>();

                foreach (var v in hide_input_values)
                    if(!v.IsNullOrEmpty())
                        nArgsString = nArgsString.Replace(v, "*".Repeat(v.Length));

                Console.WriteLine($"Executing command: '{args[0]} {args[1]}' Named Arguments: \n{nArgsString}\n");
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
                        Console.WriteLine($"[{TickTime.Now.ToLongDateTimeString()}] Failure, Error Message: {ex.JsonSerializeAsPrettyException()}");
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
                catch
                {
                    Console.WriteLine($"[{TickTime.Now.ToLongDateTimeString()}] Failure");
                    throw;
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
                case "bitbucket":
                    executeBitbucket(args);
                    break;
                case "copy":
                    executeCopy(args);
                    break;
                case "text":
                    executeText(args);
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
                    ("curl", "Accepts params: GET"),
                    ("hash", "Accepts params: SHA256"),
                    ("text", "Accepts params: replace, dos2unix"),
                    ("AES", "Accepts params: create-key, encrypt, decrypt"),
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
