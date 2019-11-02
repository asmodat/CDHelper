using System;
using AsmodatStandard.Extensions;
using AsmodatStandard.IO;
using System.Collections.Generic;

namespace CDHelper
{
    public partial class Program
    {
        private static void executeTime(string[] args)
        {
            var nArgs = CLIHelper.GetNamedArguments(args);
            switch (args[1]?.ToLower())
            {
                case "now":
                    {
                        Console.WriteLine(DateTimeEx.UnixTimestampNow());
                    }
                    ; break;
                case "add":
                    {
                        var time = nArgs.GetValueOrDefault("unix").ToLongOrDefault(0);
                        var unit = nArgs.GetValueOrDefault("unit", "s").Trim();
                        var value = nArgs.GetValueOrDefault("value").ToDoubleOrDefault(0);

                        if (time <= 0)
                            time = DateTimeEx.UnixTimestampNow();

                        var dt = time.ToDateTimeFromUnixTimestamp();

                        var unitLow = unit.ToLower();
                        if (unitLow.EquailsAny<string>("f", "ms", "milisecond", "milisec", "milisecs", "miliseconds"))
                            dt = dt.AddMilliseconds(value);
                        else if (unitLow.EquailsAny<string>("s", "second", "sec", "secs", "seconds"))
                            dt = dt.AddSeconds(value);
                        else if (unit == "m" || unitLow.EquailsAny<string>("min", "minut", "minute", "minutes"))
                            dt = dt.AddMinutes(value);
                        else if (unitLow.EquailsAny<string>("h", "hour", "hours"))
                            dt = dt.AddHours(value);
                        else if (unitLow.EquailsAny<string>("d", "day", "days"))
                            dt = dt.AddDays(value);
                        else if (unit == "M" || unitLow.EquailsAny<string>("month", "months"))
                            dt = dt.AddMonths((int)value);
                        else if (unitLow.EquailsAny<string>("y", "year", "years", "annum", "annums"))
                            dt = dt.AddYears((int)value);
                        else if (unitLow.EquailsAny<string>("tick", "ticks"))
                            dt = dt.AddTicks((long)value);
                        else
                            throw new Exception($"Unknown unit '{unit ?? "undefined"}'");

                        Console.WriteLine(dt.ToUnixTimestamp());
                    }
                    ; break;
                case "unix2timestamp":
                    {
                        var time = nArgs.GetValueOrDefault("unix").ToLongOrDefault(0);
                        Console.WriteLine(time.ToDateTimeFromUnixTimestamp().ToTimestamp());
                    }
                    ; break;
                case "help":
                case "--help":
                case "-help":
                case "-h":
                case "h":
                    HelpPrinter($"{args[0]}", "Date Time Manipulation",
                    ("now", "No Params, returns unix UTC timestamp"),
                    ("add", "Accepts params: unix, value, unit (f,s,m,h,d,w,M,y)"),
                    ("unix2timestamp", "Accepts params: unix"));
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
