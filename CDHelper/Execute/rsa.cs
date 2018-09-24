using System;
using AsmodatStandard.Extensions;
using AsmodatStandard.IO;
using AsmodatStandard.Cryptography;
using AsmodatStandard.Extensions.IO;
using AsmodatStandard.Extensions.Threading;
using AsmodatStandard.Extensions.Collections;
using System.IO;
using System.Collections.Generic;

namespace CDHelper
{
    public partial class Program
    {
        private static void executeRSA(string[] args)
        {
            var nArgs = CLIHelper.GetNamedArguments(args);

            switch (args[1]?.ToLower())
            {
                case "create-pem-key":
                    {
                        var dir = nArgs["output"].ToDirectoryInfo();
                        var size = nArgs["size"].ToInt32();

                        if (!dir.Exists)
                            dir.Create();

                        var keys = RSA.GeneratePemKeyPair(size);

                        Console.WriteLine($"Public Key:\n{keys.Public}");

                        var pubFile = Path.Combine(dir.FullName, $"{dir.Name}.pub.pem").ToFileInfo();
                        var prvFile = Path.Combine(dir.FullName, $"{dir.Name}.prv.pem").ToFileInfo();

                        Console.WriteLine($"Saving RSA Public Key to: '{pubFile.FullName}'.");
                        pubFile.WriteAllText(keys.Public);
                        Console.WriteLine($"Saving RSA private Key to: '{prvFile.FullName}'.");
                        prvFile.WriteAllText(keys.Private);
                    }
                    ;break;
                case "sign":
                    {
                        var key = nArgs["key"].ToFileInfo();
                        var input = nArgs["input"].ToFileInfo();
                        var output = nArgs["output"].ToFileInfo();
                        var algo = EnumEx.ToEnum<RSA.Algorithm>(nArgs["algorithm"]);

                        Console.WriteLine($"Signing: '{input.FullName}' with Algorithm: '{algo.ToString()}'.");
                        var sig = RSA.SignWithPemKey(input: input, pem: key, algorithm: algo);

                        Console.WriteLine($"Signature:\n{sig.ToHexString()}");
                        Console.WriteLine($"Saving Signature to: '{output.FullName}'.");
                        output.WriteAllBytes(sig);
                    }
                    ; break;
                case "verify":
                    {
                        var key = nArgs["key"].ToFileInfo();
                        var input = nArgs["input"].ToFileInfo();
                        var signature = nArgs["signature"].ToFileInfo();
                        var algo = EnumEx.ToEnum<RSA.Algorithm>(nArgs["algorithm"]);
                        var @throw = nArgs.GetValueOrDefault("throw").ToBoolOrDefault(true);

                        Console.WriteLine($"Veryfying: '{input.FullName}' with Algorithm: '{algo.ToString()}' and Public Key: {key.FullName}.");
                        var success = RSA.VerifyWithPemKey(input: input, signature: signature, pem: key, algorithm: algo);

                        if (success)
                            Console.WriteLine("Veryfication Suceeded.");
                        else
                        {
                            Console.WriteLine("Veryfication Failed.");

                            if (@throw)
                                throw new Exception($"Signature verification failed, input: '{input?.FullName}'");
                        }
                    }
                    ; break;
                case "help":
                case "--help":
                case "-help":
                case "-h":
                case "h":
                    HelpPrinter($"{args[0]}", $"Rivest–Shamir–Adleman Algoritm, with following allowed algorithms: '{EnumEx.ToStringArray<RSA.Algorithm>().JsonSerialize()}'.",
                    ("create-key", "Accepts params: output, size"),
                    ("sign", "Accepts params: key, input, output, algorithm"),
                    ("verify", "Accepts params: key, input, signature, algorithm, throw (optional, default: true)"));
                    break;
                default:
                    {
                        Console.WriteLine($"Try '{args[0]} help' to find out list of available commands.");
                        throw new Exception($"Unknown RSA command: '{args[0]} {args[1]}'");
                    }
            }
        }
    }
}
