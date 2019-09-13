using System;
using AsmodatStandard.Extensions;
using AsmodatStandard.IO;
using AsmodatStandard.Extensions.IO;
using AsmodatStandard.Extensions.Collections;
using AsmodatStandard.Extensions.Threading;
using AsmodatStandard.Threading;
using AsmodatStandard.Types;
using AsmodatStandard.Extensions.Types;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using AWSWrapper.EC2;
using System.Threading.Tasks;
using AsmodatStandard.Networking;
using AWSWrapper.SM;
using CDHelper.Models;
using System.Linq;

namespace CDHelper
{
    public partial class Program
    {
        private static async Task<bool> executeScheduler(string[] args)
        {
            var nArgs = CLIHelper.GetNamedArguments(args);

            switch (args[1]?.ToLower())
            {
                case "github":
                    {
                        if (Environment.GetEnvironmentVariable("test_connection").ToBoolOrDefault(false))
                            Console.WriteLine($"Your Internet Connection is {(SilyWebClientEx.CheckInternetAccess(timeout: 5000) ? "" : "NOT")} available.");

                        Console.WriteLine($"Fetching scheduler info.");
                        var workingDirectory = await GetVariableByKey("working_directory", nArgs: nArgs);
                        var githubSchedule = await GetVariableByKey("github_schedule", nArgs: nArgs);
                        var accessToken = await GetSecretHexToken("github_token", nArgs);
                        var user = GITWrapper.GitHubHelperEx.GetUserFromUrl(githubSchedule);
                        var repo = GITWrapper.GitHubHelperEx.GetRepoFromUrl(githubSchedule);
                        var branch = GITWrapper.GitHubHelperEx.GetBranchFromUrl(githubSchedule);
                        var scheduleLocation = GITWrapper.GitHubHelperEx.GetFileFromUrl(githubSchedule);

                        var git = new GITWrapper.GitHubHelper(new GITWrapper.Models.GitHubRepoConfig()
                        {
                            accessToken = accessToken,
                            user = user,
                            repository = repo,
                            branch = branch
                        });

                        var contentDirectory = PathEx.RuntimeCombine(workingDirectory, repo).ToDirectoryInfo();
                        var statusDirectory = PathEx.RuntimeCombine(workingDirectory, "status").ToDirectoryInfo();
                        var scheduleFileInfo = PathEx.RuntimeCombine(contentDirectory.FullName, scheduleLocation).ToFileInfo();

                        contentDirectory.TryDelete(recursive: true);
                        contentDirectory.TryCreate();
                        statusDirectory.TryCreate();

                        var result = CLIHelper.Console("git init", workingDirectory: contentDirectory.FullName, redirectError: true);
                        Console.WriteLine(result.JsonSerialize());

                        var pullCommand = $"git pull https://{accessToken}@github.com/{user}/{repo}.git";
                        result = CLIHelper.Console(pullCommand, workingDirectory: contentDirectory.FullName, redirectError: true);
                        Console.WriteLine(result.JsonSerialize());

                        if (!RuntimeEx.IsWindows())
                        {
                            result = CLIHelper.Console("chmod 555 -R ./", workingDirectory: contentDirectory.FullName, redirectError: true);
                            Console.WriteLine(result.JsonSerialize());
                        }

                        if (!scheduleFileInfo.Exists)
                        {
                            Console.WriteLine($"FAILURE, schedule file '{scheduleFileInfo.FullName}' doe not exist or was not defined.");
                            return false;
                        }

                        var deploymentConfig = scheduleFileInfo.DeserialiseJson<DeploymentConfig>();
                        var deploymentConfigOld = deploymentConfig.LoadDeploymentConfig(statusDirectory);

                        if (deploymentConfig?.enable != true || deploymentConfig.schedules.IsNullOrEmpty())
                        {
                            Console.WriteLine($"Deployment config '{scheduleFileInfo.FullName}' was not enabled or schedules were not defined.");
                            return false;
                        }

                        //Defines if schedule executuions should be triggered
                        var masterTrigger = deploymentConfig.IsTriggered(deploymentConfigOld);

                        var serialSchedules = deploymentConfig.schedules
                            .Where(x =>!(x?.id).IsNullOrEmpty() && x.parallelizable == false)
                            ?.OrderBy(x => x.priority)?.DistinctBy(x => x.id)?.ToArray();

                        var parallelSchedules = deploymentConfig.schedules
                            .Where(x => !(x?.id).IsNullOrEmpty() && x.parallelizable == true)
                            ?.OrderBy(x => x.priority)?.DistinctBy(x => x.id)?.ToArray();

                        async Task TryCatchExecute(ExecutionSchedule s) {
                            if (s == null)
                                return;

                            Console.WriteLine($"Processing executioon schedule '{s.id}', parralelized: {s.parallelizable}.");

                            if(s.delay > 0)
                                await Task.Delay(s.delay);

                            if (_debug)
                            {
                                await ProcessSchedule(s, contentDirectory, statusDirectory, masterTrigger: masterTrigger);
                                return;
                            }

                            try
                            {
                                await ProcessSchedule(s, contentDirectory, statusDirectory, masterTrigger: masterTrigger);

                                if(s.sleep > 0)
                                    await Task.Delay(s.sleep);
                            }
                            catch (Exception ex)
                            {
                                if (deploymentConfig.throwOnFailure == true)
                                {
                                    if(deploymentConfig.finalizeOnFailure)
                                        deploymentConfig.UpdateDeploymentConfig(statusDirectory);

                                    throw;
                                }

                                Console.WriteLine($"FAILED! execution of schedule '{s.id}', parralelized: {s.parallelizable}, error: {ex.JsonSerializeAsPrettyException()}.");
                            }
                        }

                        if (deploymentConfig.delay > 0)
                            await Task.Delay(deploymentConfig.delay);

                        var sum = 0;
                        if (!serialSchedules.IsNullOrEmpty())
                        {
                            sum += serialSchedules.Length;
                            foreach (var s in serialSchedules)
                                await TryCatchExecute(s);
                        }

                        if (!parallelSchedules.IsNullOrEmpty())
                        {
                            sum += serialSchedules.Length;
                            await ParallelEx.ForEachAsync(parallelSchedules,
                                s => TryCatchExecute(s), maxDegreeOfParallelism: parallelSchedules.Count());
                        }

                        deploymentConfig.UpdateDeploymentConfig(statusDirectory); 

                        Console.WriteLine($"SUCCESS, {sum} github schedule/s was/were executed out of {deploymentConfig.schedules.Length}.");

                        if (deploymentConfig.sleep > 0)
                            await Task.Delay(deploymentConfig.sleep);


                        return true;
                    }
                case "help":
                case "--help":
                case "-help":
                case "-h":
                case "h":
                    {
                        HelpPrinter($"{args[0]}", "Command Deployment",
                        ("github", "Accepts params: working_directory, github_schedule, github_token"));
                        return true;
                    }
                default:
                    {
                        Console.WriteLine($"Try '{args[0]} help' to find out list of available commands.");
                        throw new Exception($"Unknown String command: '{args[0]} {args[1]}'");
                    }
            }
        }
    }
}
