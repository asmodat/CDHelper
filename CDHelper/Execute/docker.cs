using System;
using AsmodatStandard.Extensions;
using AsmodatStandard.IO;
using AsmodatStandard.Cryptography;
using AsmodatStandard.Extensions.IO;
using AsmodatStandard.Extensions.Threading;
using AsmodatStandard.Extensions.Collections;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using CDHelper.Processing;

namespace CDHelper
{
    public partial class Program
    {
        private static void executeDocker(string[] args)
        {
            var nArgs = CLIHelper.GetNamedArguments(args);

            switch (args[1]?.ToLower())
            {
                case "gen":
                    {
                        var tmpOutp = nArgs.GetValueOrDefault("output");
                        if (tmpOutp.IsNullOrWhitespace() || tmpOutp == "./" || tmpOutp == "\\" || tmpOutp == "/")
                        {
                            tmpOutp = Environment.CurrentDirectory;
                            Console.WriteLine($"Output is set to be the current console directory: {tmpOutp}");
                        }

                        var output = tmpOutp.ToDirectoryInfo();
                        Console.WriteLine($"Directory output is set to: {output.FullName}");

                        var configInfo = nArgs["config"].ToFileInfo();

                        if (!configInfo.Exists)
                            throw new Exception($"Can't find config file: '{configInfo.FullName}'.");
                        else
                            Console.WriteLine($"Config file set to: {configInfo.FullName}");

                        var config = configInfo.ReadAllText().JsonDeserialize<DockergenConfig>();
                        config.workingDirectory = config.workingDirectory.CoalesceNullOrWhitespace($"/{Guid.NewGuid()}");
                        config.relativePath = config.relativePath.CoalesceNullOrWhitespace("/");

                        if (config.version != 1)
                            throw new Exception($"Incompatible config version, was: {config.version}, but expected 1");

                        var imageName = nArgs["image-name"];
                        var force = nArgs.GetValueOrDefault("force", "false").ToBoolOrDefault() || config.force;
                        var exposedPort = nArgs.GetValueOrDefault("exposed-port", "80").ToIntOrDefault(80);

                        if (force)
                            output.Create();

                        if (!output.Exists)
                            throw new Exception($"Directory not found: '{output.FullName}'.");
                        else
                            Console.WriteLine($"Output directory: {output.FullName}");

                        var nginxConfigInfo = PathEx.Combine(
                            output.FullName,
                            config.relativePath,
                            "nginx.conf").ToFileInfo();

                        if (force)
                            nginxConfigInfo.TryDelete();
                        else
                            Console.WriteLine($"Nginx config directory: {nginxConfigInfo.FullName}");

                        var useEnginx = config.port != exposedPort && !nginxConfigInfo.Exists;
                        if (useEnginx)
                        {
                            var nginxConfig = NginxConfigGenerator.GetSinglePortProxy(
                                sourcePort: exposedPort, destinationPort: config.port);

                            nginxConfigInfo.WriteAllText(nginxConfig);
                            Console.WriteLine($"Saved nginx configuration in: '{nginxConfigInfo.FullName}'");
                            Console.WriteLine($"Nginx Config: \n'{nginxConfig}'");
                        }
                        else
                            Console.WriteLine($"Nginx configuration will not be set because defined application port ({config.port}) is the same as exposed port ({exposedPort}) and request proxy is not needed or nginx config is already defined ({nginxConfigInfo.Exists}).");

                        var dockerComposeInfo = PathEx.Combine(
                            output.FullName,
                            config.relativePath,
                            "docker-compose.app.yml").ToFileInfo();

                        if (force)
                            dockerComposeInfo.TryDelete();

                        if (!dockerComposeInfo.Exists)
                        {
                            var dockerCompose = DockerfileConfigGenerator.GetDockerCompose(
                                imageName: imageName, 
                                port: exposedPort,
                                exposedPorts: config.exposedPorts,
                                portsMap: config.portsMap);

                            dockerComposeInfo.WriteAllText(dockerCompose);
                            Console.WriteLine($"Saved docker compose file in: '{dockerComposeInfo.FullName}'");
                            Console.WriteLine($"Docker Compose: \n'{dockerCompose}'");
                        }
                        else
                            Console.WriteLine($"Docker Compose file was not creaed because it already existed.");


                        var envFileInfo = PathEx.Combine(
                            output.FullName,
                            config.relativePath,
                            ".env").ToFileInfo();

                        if (force)
                            envFileInfo.TryDelete();

                        if (!envFileInfo.Exists)
                        {
                            if (!envFileInfo.Exists)
                                envFileInfo.Create().Close();

                            if (!config.env.IsNullOrEmpty())
                            {
                                var text = envFileInfo.ReadAllText() + "\r\n";

                                foreach (var line in config.env)
                                    text += line.Trim() + "\r\n";

                                text = text.Trim("\r\n");

                                envFileInfo.WriteAllText(text, System.IO.Compression.CompressionLevel.NoCompression, mode: FileMode.Create);
                            }

                            Console.WriteLine($"Env file location: '{envFileInfo.FullName}'");
                            Console.WriteLine($"Env file: \n'{envFileInfo.ReadAllText()}'");
                        }
                        else
                            Console.WriteLine($"Env file was not creaed because it already existed.");


                        var dockerfileInfo = PathEx.Combine(
                            output.FullName,
                            config.relativePath,
                            "Dockerfile").ToFileInfo();

                        if (force)
                            dockerfileInfo.TryDelete();

                        if (!dockerfileInfo.Exists)
                        {
                            var dockerfile = DockerfileConfigGenerator.GetDockerfile(
                                baseImage: config.baseImage,
                                port: config.port,
                                exposedPorts: config.exposedPorts,
                                workingDirectory: config.workingDirectory,
                                buildpackId: config.buildpackId,
                                customBuildCommand: config.customBuildCommand,
                                customStartCommand: config.customStartCommand,
                                enginx: useEnginx,
                                env: config.env);

                            dockerfileInfo.WriteAllText(dockerfile);
                            Console.WriteLine($"Saved dockerfile in: '{dockerfileInfo.FullName}'");
                            Console.WriteLine($"Dockerfile: \n'{dockerfile}'");
                        }
                        else
                            Console.WriteLine($"Docker Compose file was not creaed because it already existed.");

                        Console.WriteLine("SUCCESS, dockergen finished running.");
                    }
                    ; break;
                case "help":
                case "--help":
                case "-help":
                case "-h":
                case "h":
                    HelpPrinter($"{args[0]}", "Docker Helper",
                    ("gen", "Accepts params: config, output, image-name, force (optional/false), exposed-port (optional/80)"));
                    break;
                default:
                    {
                        Console.WriteLine($"Try '{args[0]} help' to find out list of available commands.");
                        throw new Exception($"Unknown docker command: '{args[0]} {args[1]}'");
                    }
            }
        }
    }
}
