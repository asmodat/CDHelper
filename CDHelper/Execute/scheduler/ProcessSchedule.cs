using System;
using AsmodatStandard.Extensions;
using AsmodatStandard.IO;
using AsmodatStandard.Extensions.IO;
using AsmodatStandard.Extensions.Collections;
using System.IO;
using System.Threading.Tasks;
using CDHelper.Models;
using AsmodatStandard.Extensions.Threading;

namespace CDHelper
{
    public partial class Program
    {
        private static async Task ProcessSchedule(
            ExecutionSchedule schedule,
            ExecutionSchedule scheduleOld,
            DirectoryInfo rootSource,
            DirectoryInfo rootStatus,
            DirectoryInfo rootLoggs,
            bool masterTrigger)
        {
            var failed = false;
            var logPath = schedule.GetLogFileInfo(rootLoggs);
            logPath.TryClear();

            try
            {
                foreach (var cmd in schedule.commands)
                {
                    var command = cmd?.Trim();

                    if (command.IsNullOrEmpty())
                        continue;

                    CommandOutput result;
                    string file;
                    string args;

                    if (command.Contains(" "))
                    {
                        var parts = command.SplitByFirst(' ');
                        file = parts[0];
                        args = parts[1];
                    }
                    else
                    {
                        file = command;
                        args = "";
                    }

                    if (schedule.timeout > 0)
                        result = await TaskEx.ToTask(CLIHelper.Command, file, args, rootSource.FullName, schedule.timeout);
                    else
                        result = await TaskEx.ToTask(CLIHelper.Command, file, args, rootSource.FullName, 0);


                    if (!result.Error.IsNullOrEmpty() && schedule.throwOnFailure)
                    {
                        failed = true;
                        throw new Exception($"Failed, schedule '{schedule.id}' command: '{cmd}', error: {result.Error}");
                    }

                    var output = result.JsonSerialize(Newtonsoft.Json.Formatting.Indented);

                    if (logPath.TryCreate())
                        logPath.AppendAllText(output);

                    Console.WriteLine(output);
                }
            }
            finally
            {
                if (!failed || schedule.finalizeOnFailure)
                {
                    Console.WriteLine($"Updating schedule '{schedule.id}' status file...");
                    schedule.executions = scheduleOld.executions + 1;
                    schedule.UpdateExecutionSchedule(rootStatus);
                }
                else
                    Console.WriteLine($"Schedule '{schedule.id}' status file was not updated.");
            }
        }
    }
}
