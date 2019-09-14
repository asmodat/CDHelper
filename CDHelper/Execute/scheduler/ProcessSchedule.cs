using System;
using AsmodatStandard.Extensions;
using AsmodatStandard.IO;
using AsmodatStandard.Extensions.IO;
using AsmodatStandard.Extensions.Collections;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using AWSWrapper.EC2;
using System.Threading.Tasks;
using AsmodatStandard.Networking;
using AWSWrapper.SM;
using CDHelper.Models;
using AsmodatStandard.Cryptography;
using AsmodatStandard.Types;
using AsmodatStandard.Threading;
using AsmodatStandard.Extensions.Threading;
using System.Linq;

namespace CDHelper
{
    public partial class Program
    {
        private static async Task ProcessSchedule(ExecutionSchedule schedule, DirectoryInfo rootSource, DirectoryInfo rootStatus, bool masterTrigger)
        {
            var scheduleOld = schedule.LoadExecutionSchedule(rootStatus);
            var failed = false;

            if (schedule.IsTriggered(scheduleOld, masterTrigger))
            {
                try
                {
                    foreach (var cmd in schedule.commands)
                    {
                        CommandOutput result;

                        if (schedule.timeout > 0)
                            result = await TaskEx.ToTask(CLIHelper.Console, cmd, rootSource.FullName, schedule.timeout);
                        else
                            result = await TaskEx.ToTask(CLIHelper.Console, cmd, rootSource.FullName, 0);

                        if (!result.Error.IsNullOrEmpty() && schedule.throwOnFailure)
                        {
                            failed = true;
                            throw new Exception($"Failed, schedule '{schedule.id}' command: '{cmd}', error: {result.Error}");
                        }

                        Console.WriteLine(result.JsonSerialize());
                    }
                }
                finally
                {
                    if (!failed || schedule.finalizeOnFailure)
                    {
                        schedule.executions = scheduleOld.executions + 1;
                        schedule.UpdateExecutionSchedule(rootStatus);
                    }
                }
            }
            else
                Console.WriteLine($"WARNING, schedule '{schedule?.id ?? "undefined"}' execution was not triggered.");

        }
    }
}
