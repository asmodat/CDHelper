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

                        var userWhitelist = nArgs.GetValueOrDefault("users")?.Split(',');
                        var repositoriesWhitelist = nArgs.GetValueOrDefault("repositories")?.Split(',');

                        Console.WriteLine($"Fetching scheduler info.");
                        var workingDirectory = (await GetVariableByKey("working_directory", nArgs: nArgs)).ToDirectoryInfo();
                        var githubSchedule = await GetVariableByKey("github_schedule", nArgs: nArgs);
                        
                        var user = GITWrapper.GitHubHelperEx.GetUserFromUrl(githubSchedule);

                        if (!userWhitelist.IsNullOrEmpty() && !userWhitelist.Any(x => x == user))
                            throw new Exception($"User was specified but, user '{user ?? "undefined"}' was not present among whitelisted users: {userWhitelist.JsonSerialize()}");

                        var accessToken = await GetSecretHexToken("github_token", nArgs);

                        var repo = GITWrapper.GitHubHelperEx.GetRepoFromUrl(githubSchedule);


                        if (!repositoriesWhitelist.IsNullOrEmpty() && !repositoriesWhitelist.Any(x => x == user))
                            throw new Exception($"Repository was specified but, repo '{repo ?? "undefined"}' was not present among whitelisted repositories: {repositoriesWhitelist.JsonSerialize()}");

                        var branch = GITWrapper.GitHubHelperEx.GetBranchFromUrl(githubSchedule);
                        var scheduleLocation = GITWrapper.GitHubHelperEx.GetFileFromUrl(githubSchedule);

                        var git = new GITWrapper.GitHubHelper(new GITWrapper.Models.GitHubRepoConfig()
                        {
                            accessToken = accessToken,
                            user = user,
                            repository = repo,
                            branch = branch
                        });

                        var contentDirectory = PathEx.RuntimeCombine(workingDirectory.FullName, repo).ToDirectoryInfo();
                        var statusDirectory = PathEx.RuntimeCombine(workingDirectory.FullName, "status").ToDirectoryInfo();
                        var logsDirectory = PathEx.RuntimeCombine(workingDirectory.FullName, "logs").ToDirectoryInfo();
                        var scheduleFileInfo = PathEx.RuntimeCombine(contentDirectory.FullName, scheduleLocation).ToFileInfo();

                        contentDirectory.TryDelete(recursive: true, exception: out var contentDirectoryException);
                        Console.WriteLine($"Removing git directory '{contentDirectory.FullName}' {(contentDirectory.Exists ? $"did NOT suceeded, error: {contentDirectoryException.JsonSerializeAsPrettyException()}" : "succeded")}.");

                        statusDirectory.TryCreate();
                        CommandOutput result;

                        var pullCommand = $"git clone https://{accessToken}@github.com/{user}/{repo}.git --branch {branch}";
                        result = CLIHelper.Console(pullCommand, workingDirectory: workingDirectory.FullName);
                        Console.WriteLine(result.JsonSerialize());

                        var gitDirectory = PathEx.RuntimeCombine(contentDirectory.FullName, ".git").ToDirectoryInfo();
                        gitDirectory.TryDelete(recursive: true);
                        Console.WriteLine($"Removing git directory '{gitDirectory.FullName}' {(gitDirectory.Exists ? "did NOT" : "")} succeded.");

                        if (!RuntimeEx.IsWindows())
                        {
                            result = CLIHelper.Console($"chmod 777 -R ./{repo}", workingDirectory: workingDirectory.FullName);
                            Console.WriteLine(result.JsonSerialize());
                        }

                        if (!scheduleFileInfo.Exists)
                        {
                            Console.WriteLine($"FAILURE, schedule file '{scheduleFileInfo.FullName}' does not exist or was not defined.");
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

                        var breakAll = false;
                        async Task TryCatchExecute(ExecutionSchedule s)
                        {
                            var sOld = s.LoadExecutionSchedule(contentDirectory);

                            if (s == null || sOld == null)
                            {
                                Console.WriteLine($"New or old schedule could not be found.");
                                return;
                            }

                            if(!s.IsTriggered(sOld, masterTrigger))
                            {
                                Console.WriteLine($"WARNING, schedule '{s?.id ?? "undefined"}' execution was not triggered.");
                                return;
                            }

                            
                            Console.WriteLine($"Processing executioon schedule '{s.id}', parralelized: {s.parallelizable}, cron: {s.cron ?? "null"}, trigger: {s.trigger}/{sOld.trigger}.");

                            if (s.delay > 0)
                                await Task.Delay(s.delay);

                            if (_debug)
                            {
                                Console.WriteLine($"WARNING! github schedule will be processed in DEBUG mode");
                                await ProcessSchedule(s, sOld, contentDirectory, statusDirectory, logsDirectory, masterTrigger: masterTrigger);
                                return;
                            }

                            try
                            {
                                await ProcessSchedule(s, sOld, contentDirectory, statusDirectory, logsDirectory, masterTrigger: masterTrigger);
                                breakAll = s.breakAllOnFinalize;

                                if (s.sleep > 0)
                                    await Task.Delay(s.sleep);
                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    if (deploymentConfig.throwOnFailure == true)
                                    {
                                        if (deploymentConfig.finalizeOnFailure)
                                        {
                                            deploymentConfig.UpdateDeploymentConfig(statusDirectory);
                                            breakAll = s.breakAllOnFinalize;
                                        }

                                        throw;
                                    }

                                    Console.WriteLine($"FAILED! execution of schedule '{s.id}', parralelized: {s.parallelizable}, error: {ex.JsonSerializeAsPrettyException()}.");
                                }
                                finally
                                {

                                    var logPath = PathEx.Combine(logsDirectory.FullName, $"{s.GetFileSafeId() ?? "tmp.log"}.log").ToFileInfo();
                                    if (logPath.TryCreate())
                                        logPath.AppendAllText(ex.JsonSerializeAsPrettyException());
                                }
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
