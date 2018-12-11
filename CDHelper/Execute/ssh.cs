using System;
using AsmodatStandard.Extensions;
using AsmodatStandard.IO;
using AsmodatStandard.Extensions.IO;
using AsmodatStandard.Extensions.Collections;
using System.IO;
using AsmodatStandard.Networking.SSH;
using System.Collections.Generic;
using System.Linq;
using AWSWrapper.EC2;

namespace CDHelper
{
    public partial class Program
    {
        private static void executeSSH(string[] args)
        {
            var nArgs = CLIHelper.GetNamedArguments(args);

            switch (args[1]?.ToLower())
            {
                case "execute-shell":
                    {
                        string host;
                        var ec2InstanceName = nArgs.GetValueOrDefault("ec2-instance-name");
                        if(!ec2InstanceName.IsNullOrEmpty())
                        {
                            var ec2Helper = new EC2Helper();
                            var instance = ec2Helper.ListInstancesByName(name: ec2InstanceName).Result
                                .SingleOrDefault(x => (x.State.Name != Amazon.EC2.InstanceStateName.Terminated) && (x.State.Name != Amazon.EC2.InstanceStateName.ShuttingDown));

                            host = instance.PublicIpAddress;
                        }
                        else
                            host = nArgs["host"];

                        var user = nArgs["user"];
                        var key = nArgs["key"].ToFileInfo();
                        

                        var commands = nArgs.ContainsKey("cmd[]") ?
                            nArgs["cmd[]"].JsonDeserialize<string[]>() :
                            new string[] { nArgs["cmd"] };

                        var ssh = new SSHManaged(host, user, key);

                        if (commands.IsNullOrEmpty())
                            throw new Exception("Command/s parameter/s 'cmd'/'cmd-<nr.>' was/were not specified, coudn't execute.");

                        var maxConnectionRetry = nArgs.GetValueOrDefault("max-reconnections").ToIntOrDefault(24);
                        var maxConnectionRetryDelay = nArgs.GetValueOrDefault("max-reconnection-delay").ToIntOrDefault(2500);
                        Console.WriteLine($"Connecting to '{host}'... (Max Retries: {maxConnectionRetry}, Max Retry Delay: {maxConnectionRetryDelay})");
                        ssh.Connect(maxConnectionRetry: maxConnectionRetry,
                            maxConnectionRetryDelay: maxConnectionRetryDelay);

                        var checkDelay = nArgs.GetValueOrDefault("check-delay").ToIntOrDefault(500);
                        Console.WriteLine($"Sucessfully connected, executing {commands.Length} command/s...");
                        var results = ssh.ExecuteShellCommands(commands,
                                commandTimeout_ms: nArgs.GetValueOrDefault("command-timeout").ToIntOrDefault(60 * 1000),
                                outputStream: Console.OpenStandardOutput(),
                                checkDelay: checkDelay);

                        Console.WriteLine($"SUCCESS");
                    }
                    ; break;
                case "help":
                case "--help":
                case "-help":
                case "-h":
                case "h":
                    HelpPrinter($"{args[0]}", "Secure Shell",
                    ("execute-shell", "Accepts params: host, user, key, cmd"));
                    break;
                default:
                    {
                        Console.WriteLine($"Try '{args[0]} help' to find out list of available commands.");
                        throw new Exception($"Unknown AES command: '{args[0]} {args[1]}'");
                    }
            }
        }
    }
}
