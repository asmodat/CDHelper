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
                        var branch = nArgs["branch"];
                        var user = nArgs["user"];
                        var repository = nArgs["repository"];
                        var file = nArgs["file"];
                        var arguments = nArgs["arguments"];
                        var wait = nArgs["wait"].ToInt32();
                        var intensity = nArgs["intensity"].ToInt32();
                        var userAgent = nArgs.GetValueOrDefault("user-agent", "Asmodat Deployment Toolkit");

                        var request = $"https://api.github.com/repos/{user}/{repository}/commits/{branch}";

                        var initialCommit = HttpHelper.GET<GithubReposCommits>(
                                    requestUri: request,
                                    ensureStatusCode: System.Net.HttpStatusCode.OK,
                                    defaultHeaders: new (string, string)[] {
                                        ("User-Agent", userAgent)
                                    }).Result;

                        if ((initialCommit?.sha).IsNullOrEmpty())
                            throw new Exception($"No Commits were Found, Request: {request}");

                        var sw = Stopwatch.StartNew();
                        while (true)
                        {
                            var newCommit = HttpHelper.GET<GithubReposCommits>(
                                    requestUri: request,
                                    ensureStatusCode: System.Net.HttpStatusCode.OK,
                                    defaultHeaders: new (string, string)[] {
                                        ("User-Agent", userAgent)
                                    }).Result;

                            if (!(newCommit?.sha).IsNullOrEmpty() && newCommit.sha != initialCommit.sha)
                            {
                                Console.WriteLine($"New Commit: {newCommit?.sha ?? "undefined"}");
                                break;
                            }

                            Console.WriteLine($"Last Commit: {newCommit?.sha ?? "undefined"}, Elapsed: {sw.ElapsedMilliseconds/1000}s");
                            Thread.Sleep(intensity);
                        }

                        using (var proc = new Process())
                        {
                            proc.StartInfo.FileName = file;
                            proc.StartInfo.Arguments = arguments;
                            proc.StartInfo.CreateNoWindow = true;
                            proc.StartInfo.UseShellExecute = false;
                            proc.StartInfo.RedirectStandardOutput = true;
                            proc.StartInfo.RedirectStandardError = true;

                            proc.OutputDataReceived += Proc_DataReceived;
                            proc.ErrorDataReceived += Proc_DataReceived;
                            proc.Exited += Proc_Exited;

                            proc.Start();
                            proc.BeginOutputReadLine();
                            proc.BeginErrorReadLine();

                            proc.WaitForExit(milliseconds: wait);
                        }

                        Console.WriteLine($"SUCCESS");
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
