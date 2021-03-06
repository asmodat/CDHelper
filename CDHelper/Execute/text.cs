﻿using System;
using AsmodatStandard.Extensions;
using AsmodatStandard.IO;
using AsmodatStandard.Extensions.IO;
using AsmodatStandard.Extensions.Collections;
using System.IO;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CDHelper
{
    public partial class Program
    {
        private static void executeText(string[] args)
        {
            var nArgs = CLIHelper.GetNamedArguments(args);

            switch (args[1]?.ToLower())
            {
                case "vereq": // this function allows to check if software versions are equal
                    {
                        var old = nArgs["old"].RemoveNonDigits(exceptions: new char[] { '.' });
                        var @new = nArgs["new"].RemoveNonDigits(exceptions: new char[] { '.' });

                        int[] Split(string s)
                        {
                            var values = new List<int>();
                            var splits = s.Split('.');
                            bool firstDigitFound = false;
                            foreach (var split in splits)
                            {
                                var i = split.ToIntOrDefault(-1);
                                if (firstDigitFound && i <= 0)
                                    continue;

                                if (!firstDigitFound && i > 0)
                                    firstDigitFound = true;

                                values.Add(i);
                            }
                            return values.ToArray();
                        }

                        var ovar = Split(old);
                        var nvar = Split(@new);

                        WriteLine($"INFO: Comparing 'v{ovar.StringJoin(".")}' and 'v{nvar.StringJoin(".")}' ...");

                        if (ovar.Length <= 0 && ovar.Length <= 0)
                            throw new Exception($"Both '{old}' and '{@new}' are NOT valid versions");

                        for (int i = 0; i < Math.Max(ovar.Length, nvar.Length); i++)
                        {
                            var o = ovar.Length > i ? ovar[i] : 0;
                            var n = nvar.Length > i ? nvar[i] : 0;

                            if (o == n)
                                continue;
       
                            Console.WriteLine(o > n ? "1" : "-1");
                            return;
                        }

                        Console.WriteLine("0");
                    }
                    ; break;
                case "lineswap":
                    {
                        var insert = nArgs["insert"];
                        var appendIfFoundNot = nArgs.GetValueOrDefault("append-if-found-not").ToBoolOrDefault(false);
                        var prependIfFoundNot = nArgs.GetValueOrDefault("prepend-if-found-not").ToBoolOrDefault(false);
                        var dryRun = nArgs.GetValueOrDefault("dry-run").ToBoolOrDefault(false);

                        var prefix = nArgs.GetValueOrDefault("prefix", "");
                        var suffix = nArgs.GetValueOrDefault("suffix", "");
                        var regex = nArgs.GetValueOrDefault("regex", "");
                        var contains = nArgs.GetValueOrDefault("contains", "");

                        // mach only after given regex sequence is found 
                        var after = nArgs.GetValueOrDefault("after-regex", "");
                        // mach only before given regex sequence is found 
                        var before = nArgs.GetValueOrDefault("before-regex", "");

                        var prefixNot = nArgs.GetValueOrDefault("prefix-not", "");
                        var suffixNot = nArgs.GetValueOrDefault("suffix-not", "");
                        var regexNot = nArgs.GetValueOrDefault("regex-not", "");
                        var containsNot = nArgs.GetValueOrDefault("contains-not", "");

                        var andPrefix = nArgs.GetValueOrDefault("and-prefix", "");
                        var andSuffix = nArgs.GetValueOrDefault("and-suffix", "");
                        var andRegex = nArgs.GetValueOrDefault("and-regex", "");
                        var andContains = nArgs.GetValueOrDefault("and-contains", "");

                        var andPrefixNot = nArgs.GetValueOrDefault("and-prefix-not", "");
                        var andSuffixNot = nArgs.GetValueOrDefault("and-suffix-not", "");
                        var andRegexNot = nArgs.GetValueOrDefault("and-regex-not", "");
                        var andContainsNot = nArgs.GetValueOrDefault("and-contains-not", "");

                        var path = nArgs["path"];
                        var files = FileHelper.GetFiles(path,
                            pattern: nArgs.GetValueOrDefault("path-pattern") ?? "*",
                            recursive: nArgs.GetValueOrDefault("path-recursive").ToBoolOrDefault(false),
                            split: ',');

                        var rx = regex.IsNullOrEmpty() ? null : new Regex(regex);
                        var rxNot = regexNot.IsNullOrEmpty() ? null : new Regex(regexNot);
                        var rxAnd = andRegex.IsNullOrEmpty() ? null : new Regex(andRegex);
                        var rxAndNot = andRegexNot.IsNullOrEmpty() ? null : new Regex(andRegexNot);

                        var rxAfter = after.IsNullOrEmpty() ? null : new Regex(after);
                        var rxBefore = before.IsNullOrEmpty() ? null : new Regex(before);

                        if (!after.IsNullOrEmpty())
                            WriteLine($"Matching AFTER regex: '{after}'");
                        if (!before.IsNullOrEmpty())
                            WriteLine($"Matching BEFORE regex: '{before}'");

                        if (!regex.IsNullOrEmpty())
                            WriteLine($"Matching OR regex: '{regex}'");
                        if (!prefix.IsNullOrEmpty())
                            WriteLine($"Matching OR prefix: '{prefix}'");
                        if (!suffix.IsNullOrEmpty())
                            WriteLine($"Matching OR suffix: '{suffix}'");
                        if (!contains.IsNullOrEmpty())
                            WriteLine($"Matching OR contains: '{contains}'");

                        if (!regexNot.IsNullOrEmpty())
                            WriteLine($"Matching OR NOT regex: '{regexNot}'");
                        if (!prefixNot.IsNullOrEmpty())
                            WriteLine($"Matching OR NOT prefix: '{prefixNot}'");
                        if (!suffixNot.IsNullOrEmpty())
                            WriteLine($"Matching OR NOT suffix: '{suffixNot}'");
                        if (!containsNot.IsNullOrEmpty())
                            WriteLine($"Matching OR NOT contains: '{containsNot}'");

                        if (!andPrefix.IsNullOrEmpty())
                            WriteLine($"Matching AND prefix: '{andPrefix}'");
                        if (!andSuffix.IsNullOrEmpty())
                            WriteLine($"Matching AND suffix: '{andSuffix}'");
                        if (!andRegex.IsNullOrEmpty())
                            WriteLine($"Matching AND regex: '{andRegex}'");
                        if (!andContains.IsNullOrEmpty())
                            WriteLine($"Matching AND contains: '{andContains}'");

                        if (!andPrefixNot.IsNullOrEmpty())
                            WriteLine($"Matching AND NOT prefix: '{andPrefixNot}'");
                        if (!andSuffixNot.IsNullOrEmpty())
                            WriteLine($"Matching AND NOT suffix: '{andSuffixNot}'");
                        if (!andRegexNot.IsNullOrEmpty())
                            WriteLine($"Matching AND NOT regex: '{andRegexNot}'");
                        if (!andContainsNot.IsNullOrEmpty())
                            WriteLine($"Matching AND NOT conatins: '{andContainsNot}'");

                        if (appendIfFoundNot)
                            WriteLine($"Appending if NOT matching");
                        if (prependIfFoundNot)
                            WriteLine($"Prepending if NOT matching");

                        WriteLine($"Swapping matched lines with: '{insert}'");

                        int count = 0;
                        files.ParallelForEach(file =>
                        {
                            if (!file.Exists)
                                throw new Exception($"Input file does not exists '{file.FullName}'");

                            WriteLine($"Reading Text File '{file.FullName}' ...");
                            var lines = new List<string>();
                            int cntr = 0;
                            int nr = 0;
                            using (var s = new StreamReader(file.FullName))
                            {
                                string line;
                                bool match;
                                bool afterMatch = false;
                                bool beforeMatch = false;
                                while ((line = s.ReadLine()) != null)
                                {
                                    ++nr;
                                    if (!afterMatch && !after.IsNullOrEmpty())
                                    {
                                        if (rxAfter.Match(line).Success)
                                            afterMatch = true; // after match found

                                        lines.Add(line);
                                        continue;
                                    }

                                    if (!beforeMatch && !before.IsNullOrEmpty() && rxBefore.Match(line).Success)
                                            beforeMatch = true; // before match found

                                    if (beforeMatch)
                                    {   // do not match, nothing left to process as before regex was hit
                                        lines.Add(line);
                                        continue;
                                    }

                                    match = !prefix.IsNullOrEmpty() && line.StartsWith(prefix);
                                    match = match || (!suffix.IsNullOrEmpty() && line.EndsWith(suffix));
                                    match = match || (!regex.IsNullOrEmpty() && rx.Match(line).Success);
                                    match = match || (!contains.IsNullOrEmpty() && line.Contains(contains));

                                    match = match || (!prefixNot.IsNullOrEmpty() && !line.StartsWith(prefixNot));
                                    match = match || (!suffixNot.IsNullOrEmpty() && !line.EndsWith(suffixNot));
                                    match = match || (!regexNot.IsNullOrEmpty() && !rxNot.Match(line).Success);
                                    match = match || (!containsNot.IsNullOrEmpty() && !line.Contains(containsNot));

                                    if (match)
                                    {
                                        match = match && !andPrefix.IsNullOrEmpty() ? line.StartsWith(andPrefix) : match;
                                        match = match && !andSuffix.IsNullOrEmpty() ? line.EndsWith(andSuffix) : match;
                                        match = match && !andRegex.IsNullOrEmpty() ? rxAnd.Match(line).Success : match;
                                        match = match && !andContains.IsNullOrEmpty() ? line.Contains(andContains) : match;

                                        match = match && !andPrefixNot.IsNullOrEmpty() ? !line.StartsWith(andPrefixNot) : match;
                                        match = match && !andSuffixNot.IsNullOrEmpty() ? !line.EndsWith(andSuffixNot) : match;
                                        match = match && !andRegexNot.IsNullOrEmpty() ? !rxAndNot.Match(line).Success : match;
                                        match = match && !andContainsNot.IsNullOrEmpty() ? !line.Contains(andContainsNot) : match;
                                    }

                                    if (match)
                                    {
                                        ++cntr;
                                        WriteLine($"Match {cntr} found for line {nr}: '{line}' in '{file.FullName}'");
                                        lines.Add(insert);
                                    }
                                    else
                                        lines.Add(line);
                                }
                                s.Close();
                            }

                            appendIfFoundNot = cntr <= 0 && appendIfFoundNot;
                            prependIfFoundNot = cntr <= 0 && prependIfFoundNot;

                            if (cntr > 0 || appendIfFoundNot || prependIfFoundNot)
                            {
                                WriteLine($"Writing changes to '{file.FullName}' ...");
                                if (!_dryRun) // used for command testing
                                    using (var s = new StreamWriter(file.FullName, false))
                                    {
                                        if (prependIfFoundNot)
                                            s.WriteLine(insert);
                                        foreach (var line in lines)
                                            s.WriteLine(line);
                                        if (appendIfFoundNot)
                                            s.WriteLine(insert);
                                        s.Close();
                                    }

                                Interlocked.Increment(ref count);
                            }
                        });

                        if (count > 0)
                            WriteLine($"SUCCESS: Replaced or Added text in {count} file/s. ");
                        else
                            WriteLine($"WAFNING: No lines were matched in any of the files specified by path {path}");
                    }
                    ; break;
                case "replace":
                    {
                        var @new = nArgs["new"];
                        var old = nArgs["old"];
                        var files = FileHelper.GetFiles(nArgs["input"],
                            pattern: nArgs.GetValueOrDefault("pattern") ?? "*",
                            recursive: nArgs.GetValueOrDefault("recursive").ToBoolOrDefault(false));

                        int count = 0;
                        files.ParallelForEach(file =>
                        {
                            if (!file.Exists)
                                throw new Exception($"Input file does not exists '{file.FullName}'");

                            WriteLine($"Reading Text File '{file.FullName}' ...");
                            var text = file.ReadAllText();

                            if (text?.Contains(old) == true)
                            {
                                WriteLine($"Replacing text '{old}' with '{@new}' in file '{file.FullName}' ...");
                                text = text.Replace(old, @new);
                                file.WriteAllText(text);
                                Interlocked.Increment(ref count);
                                WriteLine($"Success, Replaced text in file '{file.FullName}' [{file?.Length ?? 0}]");
                            }
                            else
                                WriteLine($"File '{file.FullName}' does NOT contain text '{old}', skipping replacement.");
                        });

                        WriteLine($"SUCCESS, Replaced text in {count} files. ");
                    }
                    ; break;
                case "dos2unix":
                    {
                        var files = FileHelper.GetFiles(nArgs["input"],
                            pattern: nArgs.GetValueOrDefault("pattern") ?? "*",
                            recursive: nArgs.GetValueOrDefault("recursive").ToBoolOrDefault(false));

                        files.ParallelForEach(file =>
                        {
                            WriteLine($"Converting '{file?.FullName}' [{file?.Length ?? 0}] from dos to unix format...");
                            file.ConvertDosToUnix();
                            file.Refresh();
                            WriteLine($"Success, Converted '{file?.FullName}' [{file?.Length ?? 0}]");
                        });

                        WriteLine($"SUCCESS, Converted {files?.Length ?? 0} files to unix format. ");
                    }
                    ; break;
                case "help":
                case "--help":
                case "-help":
                case "-h":
                case "h":
                    HelpPrinter($"{args[0]}", "String Manipulation",
                    ("lineswap", "Accepts: insert, path, (and-)prefix(-not), (and-)suffix(-not), (and-)regex(-not), (after-/before-)regex, (and-)contains(-not), (append-/prepend-)if-found-not (optiona: False)"),
                    ("replace", "Accepts: old, new, input"),
                    ("dos2unix", "Accepts: input"),
                    ("vereq", "Accepts: old, new | Returns: 0 if equal, -1 if old is smaller, 1 if old is greater"));
                    break;
                default:
                    {
                        Console.WriteLine($"Try '{args[0]} help' to find out list of available commands.");
                        throw new Exception($"Unknown String command: '{args[0]} {args[1]}'");
                    }
            }
        }
    }
}
