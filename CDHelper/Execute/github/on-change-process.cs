using System;
using AsmodatStandard.IO;
using AsmodatStandard.Extensions.Threading;
using AsmodatStandard.Extensions.Collections;
using AsmodatBitbucket;
using System.Diagnostics;
using AsmodatStandard.Extensions;
using CDHelper.Models;
using System.Threading;
using GITWrapper.Models;

namespace CDHelper
{
    public partial class Program
    {
        private static void GithubOnChangeProcess(
            string branch, 
            string user, 
            string repository, 
            string file,
            string arguments,
            int wait,
            int intensity, 
            string userAgent,
            string accessToken)
        {
            if (accessToken.IsNullOrWhitespace() && intensity < 60000)
                Console.WriteLine("WARNING! access_token was not defined and rewuest intensity is below 1 request per minute!");

            accessToken = accessToken.IsNullOrWhitespace() ? "" : $"?access_token={accessToken}";
            var request = $"https://api.github.com/repos/{user}/{repository}/commits/{branch}{accessToken}";

            var initialCommit = HttpHelper.GET<GitHubRepoCommits>(
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
                var newCommit = HttpHelper.GET<GitHubRepoCommits>(
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

                Console.WriteLine($"Last Commit: {newCommit?.sha ?? "undefined"}, Elapsed: {sw.ElapsedMilliseconds / 1000}s");
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
    }
}
