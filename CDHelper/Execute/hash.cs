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
                            var recursive = nArgs.Keys.Any(k => k.EquailsAny(StringComparison.InvariantCultureIgnoreCase, "r", "recursive"));
                            hash = HashHelper.SHA256(path.ToDirectoryInfo(), excludeRootName: excludeRootName, recursive: recursive, encoding: Encoding.UTF8).Result.ToHexString();
                        }
                        else if(File.Exists(path))
                        {
                            if(excludeRootName)
                                hash = HashHelper.SHA256(path.ToFileInfo()).ToHexString();
                            else
                                hash = HashHelper.SHA256(path.ToFileInfo(), Encoding.UTF8).ToHexString();
                        }
                        else
                            throw new Exception($"Can't hash path '{path}' because it does not exist.");

                        var verify = nArgs.FirstOrDefault(kvp => kvp.Key.EquailsAny(StringComparison.InvariantCultureIgnoreCase, "v", "verify") && !kvp.Value.IsNullOrEmpty());

                        if (verify.Value != null)
                        {
                            if (hash.HexEquals(verify.Value))
                                Console.WriteLine("Hash Verification Succeeded");
                            else
                            {
                                Console.WriteLine("Hash Verification Failed");
                                throw new Exception($"Hash Verification failed, expected: {verify.Value}, but was: {hash}.");
                            }
                        }
                        else
                            Console.WriteLine(hash);
                    }
                     break;
                case "help":
                    HelpPrinter($"{args[0]}", "Hash Helper",
                    ("SHA256", "Accepts params: [as first param] [p]ath='<dir/file>', [v]erify='0x<hex string>', Accepted Flags: [r]ecursive, e[x]clude-root-name"));
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
