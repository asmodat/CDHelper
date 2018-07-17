using System;
using System.Linq;
using AsmodatStandard.Cryptography;
using AsmodatStandard.Extensions;
using AsmodatStandard.IO;
using AsmodatStandard.Extensions.IO;
using System.Text;
using System.IO;

namespace CDHelper
{
    public partial class Program
    {
        private static void executeHash(string[] args)
        {
            var nArgs = CLIHelper.GetNamedArguments(args);

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
                        {
                            if (hash.HexEquals(verify.Value))
                                Console.WriteLine("SHA256 Hash Verification Succeeded");
                            else
                            {
                                Console.WriteLine("SHA256 Hash Verification Failed");
                                throw new Exception($"SHA256 Hash Verification failed, expected: {verify.Value}, but was: {hash}.");
                            }
                        }
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
                        {
                            if (hash.HexEquals(verify.Value))
                                Console.WriteLine("MD5 Hash Verification Succeeded");
                            else
                            {
                                Console.WriteLine("MD5 Hash Verification Failed");
                                throw new Exception($"MD5 Hash Verification failed, expected: {verify.Value}, but was: {hash}.");
                            }
                        }
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
