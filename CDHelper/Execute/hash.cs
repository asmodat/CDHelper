using System;
using System.Linq;
using AsmodatStandard.Cryptography;
using AsmodatStandard.Extensions;
using AsmodatStandard.IO;
using AsmodatStandard.Extensions.IO;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace CDHelper
{
    public partial class Program
    {
        private static void executeHash(string[] args)
        {
            var nArgs = CLIHelper.GetNamedArguments(args);

            void Verify(string verify, string hash, string hash_type)
            {
                var verifiers = new List<string>();

                if (verify.ContainsAny(".", "/", "\\", "json") && File.Exists(verify))
                {
                    Console.WriteLine("Determined that verify argument is a file, extracting hash whitelist string array...");
                    var fi = verify.ToFileInfo();
                    var arr = fi.ReadAllText()?.JsonDeserialize<string[]>();
                    Console.WriteLine($"Success, found '{arr?.Length ?? 0}' hashes within '{fi.FullName}' file.");
                    verifiers.AddRange(arr);
                }
                else
                    verifiers.Add(verify);

                if (verifiers.Any(x => x != "*" && !x.IsNullOrEmpty() && hash.HexEquals(x)))
                    Console.WriteLine($"{hash_type} Hash Verification Succeeded");
                else
                {
                    Console.WriteLine($"{hash_type} Hash Verification Failed");

                    var err_message = $"{hash_type} Hash Verification failed, expected one of: '{verifiers?.JsonSerialize()}', but was: '{hash}'.";

                    if (verifiers.Any(x => x == "*"))
                        Console.WriteLine($"WARNING!!! Hash wildcard was present, following error will not be thrown: {err_message}");
                    else
                        throw new Exception(err_message);
                }
            }

            switch (args[1]?.ToLower())
            {
                case "sha256":
                    {
                        var path = (nArgs.FirstOrDefault(x => x.Key.EquailsAny(StringComparison.InvariantCultureIgnoreCase, "p", "path")).Value ?? args[2]);
                        string hash;

                        var excludeRootName = nArgs.Keys.Any(k => k.EquailsAny(StringComparison.InvariantCultureIgnoreCase, "x", "exclude-root-name"));

                        if (Directory.Exists(path))
                        {
                            var di = path.ToDirectoryInfo();
                            var recursive = nArgs.Keys.Any(k => k.EquailsAny(StringComparison.InvariantCultureIgnoreCase, "r", "recursive"));

                            Console.WriteLine($"Directory verificarion started, Path: {di.FullName}, Exclude Root Name: {excludeRootName}, Recursive: {recursive}");
                            hash = HashHelper.SHA256(di, excludeRootName: excludeRootName, recursive: recursive, encoding: Encoding.UTF8).Result.ToHexString();
                        }
                        else if(File.Exists(path))
                        {
                            var fi = path.ToFileInfo();
                            Console.WriteLine($"File verificarion started, Path: {fi.FullName}, Exclude Name: {excludeRootName}");
                            if (excludeRootName)
                                hash = HashHelper.SHA256(fi).ToHexString();
                            else
                                hash = HashHelper.SHA256(fi, Encoding.UTF8).ToHexString();
                        }
                        else
                            throw new Exception($"Can't hash path '{path}' because it does not exist.");

                        var verify = nArgs.FirstOrDefault(kvp => kvp.Key.EquailsAny(StringComparison.InvariantCultureIgnoreCase, "v", "verify") && !kvp.Value.IsNullOrEmpty());

                        if (verify.Value != null)
                            Verify(verify.Value, hash, "SHA256");
                        else
                            Console.WriteLine(hash);
                    }
                     break;
                case "md5":
                    {
                        var path = (nArgs.FirstOrDefault(x => x.Key.EquailsAny(StringComparison.InvariantCultureIgnoreCase, "p", "path")).Value ?? args[2]);
                        string hash;

                        var excludeRootName = nArgs.Keys.Any(k => k.EquailsAny(StringComparison.InvariantCultureIgnoreCase, "x", "exclude-root-name"));

                        if (Directory.Exists(path))
                        {
                            var di = path.ToDirectoryInfo();
                            var recursive = nArgs.Keys.Any(k => k.EquailsAny(StringComparison.InvariantCultureIgnoreCase, "r", "recursive"));

                            Console.WriteLine($"Directory verificarion started, Path: {di.FullName}, Exclude Root Name: {excludeRootName}, Recursive: {recursive}");
                            hash = HashHelper.MD5(di, excludeRootName: excludeRootName, recursive: recursive, encoding: Encoding.UTF8).Result.ToHexString();
                        }
                        else if (File.Exists(path))
                        {
                            var fi = path.ToFileInfo();
                            Console.WriteLine($"File verificarion started, Path: {fi.FullName}, Exclude Name: {excludeRootName}");
                            if (excludeRootName)
                                hash = HashHelper.MD5(fi).ToHexString();
                            else
                                hash = HashHelper.MD5(fi, Encoding.UTF8).ToHexString();
                        }
                        else
                            throw new Exception($"Can't hash path '{path}' because it does not exist.");

                        var verify = nArgs.FirstOrDefault(kvp => kvp.Key.EquailsAny(StringComparison.InvariantCultureIgnoreCase, "v", "verify") && !kvp.Value.IsNullOrEmpty());

                        if (verify.Value != null)
                            Verify(verify.Value, hash, "MD5");
                        else
                            Console.WriteLine(hash);
                    }
                    break;
                case "help":
                case "--help":
                case "-help":
                case "-h":
                case "h":
                    HelpPrinter($"{args[0]}", "Hash Helper",
                    ("SHA256", "Accepts params: [as first param] [p]ath='<dir/file>', [v]erify='0x<hex string>', Accepted Flags: [r]ecursive, e[x]clude-root-name"),
                    ("MD5", "Accepts params: [as first param] [p]ath='<dir/file>', [v]erify='0x<hex string>', Accepted Flags: [r]ecursive, e[x]clude-root-name"));
                    break;
                default:
                    {
                        Console.WriteLine($"Try '{args[0]} help' to find out list of available commands.");
                        throw new Exception($"Unknown ECS command: '{args[0]} {args[1]}'");
                    }
            }
        }
    }
}
