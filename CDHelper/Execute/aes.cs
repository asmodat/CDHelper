﻿using System;
using AsmodatStandard.Extensions;
using AsmodatStandard.IO;
using AsmodatStandard.Cryptography;
using AsmodatStandard.Extensions.IO;
using AsmodatStandard.Extensions.Threading;
using AsmodatStandard.Extensions.Collections;
using System.IO;

namespace CDHelper
{
    public partial class Program
    {
        private static void executeAES(string[] args)
        {
            var nArgs = CLIHelper.GetNamedArguments(args);

            switch (args[1]?.ToLower())
            {
                case "create-key":
                    {
                        var path = nArgs["path"].ToFileInfo();

                        if (path.Exists)
                            throw new Exception($"Can't create key file: '{path.FullName}' because it already exists.");

                        path.WriteAllText(AES.CreateAesSecret().JsonSerialize());
                    }
                    ;break;
                case "encrypt":
                    {
                        var key = FileHelper.DeserialiseJson<AesSecret>(nArgs["key"]);
                        var config = nArgs["config"].ToFileInfo();
                        var configs = config.DeserialiseJson<SecretConfig[]>();

                        foreach (var cfg in configs)
                        {
                            var src = cfg.Source.ToFileInfo();
                            var dst = cfg.Destination.ToFileInfo();

                            if (File.Exists(dst.FullName))
                                dst.Delete();

                            if (!dst.Directory.Exists)
                            {
                                Console.WriteLine($"Creating directory '{dst.Directory.FullName}' for secret located at '{src.FullName}'.");
                                dst.Directory.Create();
                            }

                            Console.WriteLine($"Encrypting '{src.FullName}' => '{dst.FullName}'.");
                            src.EncryptAsync(key, dst).Await();
                        }
                    }
                    ; break;
                case "decrypt":
                    {
                        var key = FileHelper.DeserialiseJson<AesSecret>(nArgs["key"]);
                        var dir = nArgs["dir"].ToDirectoryInfo();

                        if (!dir.Exists)
                            throw new Exception($"Could not find directory '{dir.Extension}'.");

                        var files = dir.GetFiles("*.aes", SearchOption.AllDirectories);

                        if (files.IsNullOrEmpty())
                            Console.WriteLine($"No '.aes' files were found in directory: '{dir.FullName}'");

                        foreach (var f in files)
                        {
                            var newFile = Path.Combine(f.Directory.FullName, f.NameWithoutExtension()).ToFileInfo();
                            if (newFile.Exists)
                            {
                                Console.WriteLine($"Decrypted file '{newFile}' was already present, removing.");
                                newFile.Delete();
                            }

                            Console.WriteLine($"Decrypting '{f.FullName}' => '{newFile.FullName}'");
                            f.DecryptAsync(key, newFile).Await();
                        }
                    }
                    ; break;
                case "help":
                case "--help":
                case "-help":
                case "-h":
                case "h":
                    HelpPrinter($"{args[0]}", "Curl Like Web Requests",
                    ("create-key", "Accepts params: path"),
                    ("encrypt", "Accepts params: key, config"),
                    ("decrypt", "Accepts params: key, dir"));
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
