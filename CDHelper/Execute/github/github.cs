using System;
using AsmodatStandard.IO;
using AsmodatStandard.Extensions.Threading;
using AsmodatStandard.Extensions.Collections;
using AsmodatBitbucket;
using System.Diagnostics;
using AsmodatStandard.Extensions;
using CDHelper.Models;
using System.Threading;

namespace CDHelper
{
    public partial class Program
    {
        private static void executeGithub(string[] args)
        {
            var nArgs = CLIHelper.GetNamedArguments(args);

            switch (args[1]?.ToLower())
            {
                case "on-change-process":
                    {
                        GithubOnChangeProcess(
                            branch: nArgs["branch"],
                            user: nArgs["user"],
                            repository: nArgs["repository"],
                            file: nArgs["file"],
                            arguments: nArgs["arguments"],
                            wait: nArgs["wait"].ToInt32(),
                            intensity: nArgs.GetValueOrDefault("intensity").ToIntOrDefault(60000),
                            userAgent: nArgs.GetValueOrDefault("user-agent", "Asmodat Deployment Toolkit"),
                            accessToken: nArgs.GetValueOrDefault("access_token"));
                    }
                    ;break;
                case "help":
                case "--help":
                case "-help":
                case "-h":
                case "h":
                    HelpPrinter($"{args[0]}", "Github API Control",
                    ("on-change-process", "Accepts params: branch, user, repository, file, arguments, wait, intensity"));
                    break;
                default:
                    {
                        Console.WriteLine($"Try '{args[0]} help' to find out list of available commands.");
                        throw new Exception($"Unknown AES command: '{args[0]} {args[1]}'");
                    }
            }
        }

        private static void Proc_Exited(object sender, EventArgs e)
        {
            Console.WriteLine("Process Finisehed Running...");
        }

        private static void Proc_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data.IsNullOrEmpty())
                return;

            Console.WriteLine(e.Data);
        }
    }
}
