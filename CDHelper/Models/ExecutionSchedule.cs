using AsmodatStandard.Extensions.IO;
using System.IO;
using AsmodatStandard.Extensions;
using AsmodatStandard.Cryptography;
using System;
using AsmodatStandard.Types;
using AsmodatStandard.Extensions.Collections;

namespace CDHelper.Models
{
    public static class ExecutionScheduleEx
    {
        public static FileInfo GetLogFileInfo(this ExecutionSchedule s, DirectoryInfo logDirectory)
            => PathEx.RuntimeCombine(logDirectory.FullName, $"{(s.GetFileSafeId() ?? "tmp")}.log").ToFileInfo();

        public static FileInfo GetStatusFileInfo(this ExecutionSchedule s, DirectoryInfo rootDirectory)
            => PathEx.RuntimeCombine(rootDirectory.FullName, $"{(s.GetFileSafeId() ?? "tmp")}.json").ToFileInfo();

        public static string GetFileSafeId(this ExecutionSchedule s)
            => s?.id?.ToAlphaNumeric(whitelist: new char[] { '-', '_'});

        public static bool IsTriggered(this ExecutionSchedule schedule, ExecutionSchedule scheduleOld, bool masterTrigger)
        {
            if (schedule?.enable != true)
            {
                Console.WriteLine($"Schedule '{schedule.id}' was not enabled.");
                return false;
            }

            if (schedule.max > 0 && schedule.max <= scheduleOld.executions)
            {
                Console.WriteLine($"Schedule '{schedule.id}' was executed maximum number of {schedule.max} times.");
                return false;
            }

            if (schedule.commands.IsNullOrEmpty())
            {
                Console.WriteLine($"Schedule '{schedule.id}' commands were not defined.");
                return false;
            }

            if (schedule.enableMasterTrigger && masterTrigger)
                return true;

            if (schedule.cron?.ToCron()?.Compare(DateTime.UtcNow) == 0)
                return true;

            if (schedule.trigger > 0 && scheduleOld.trigger < schedule.trigger)
                return true;

            //Console.WriteLine($"Schedule '{schedule.id}' trigger: '{schedule.trigger}/{scheduleOld.trigger}', cron: '{schedule.cron ?? "undefined"}' was NOT actived.");
            return false;
        }

        public static ExecutionSchedule LoadExecutionSchedule(this ExecutionSchedule schedule, DirectoryInfo rootStatus)
        {
            if (schedule.id.IsNullOrEmpty())
                throw new Exception("Schedule ID was not defined, could not load.");

            var statusFile = schedule.GetStatusFileInfo(rootStatus);

            if (!statusFile.Directory.Exists)
                statusFile.Directory.Create();

            if (!statusFile.Exists)
                statusFile.WriteAllText("{ }");

            return statusFile.DeserialiseJson<ExecutionSchedule>();
        }

        public static void UpdateExecutionSchedule(this ExecutionSchedule schedule, DirectoryInfo rootStatus)
        {
            var statusFile = schedule.GetStatusFileInfo(rootStatus);

            if (!statusFile.Directory.Exists)
                statusFile.Directory.Create();

            statusFile.WriteAllText(schedule.JsonSerialize());
        }
    }

    public class ExecutionSchedule
    {
        /// <summary>
        /// Defines wheather or not this schedule is active, default false
        /// </summary>
        public bool enable { get; set; } = false;

        /// <summary>
        /// Index defining execution order, the lower the higest priority the schedule will have
        /// </summary>
        public long priority { get; set; } = 0;

        /// <summary>
        /// delay time before command is executed
        /// </summary>
        public int delay { get; set; } = 0;

        /// <summary>
        /// delay time after command is executed
        /// </summary>
        public int sleep { get; set; } = 0;

        /// <summary>
        /// Schedule name
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// Defines if schedule can be parallelized
        /// </summary>
        public bool parallelizable { get; set; } = false;

        /// <summary>
        /// Run on CRON
        /// </summary>
        public string cron { get; set; }

        /// <summary>
        /// Iterative trigger
        /// </summary>
        public long trigger { get; set; }

        /// <summary>
        /// command to be executed
        /// </summary>
        public string[] commands { get; set; }

        /// <summary>
        /// Defines that if the schedule fails entire deployment job should be stopped
        /// </summary>
        public bool throwOnFailure { get; set; } = false;

        /// <summary>
        /// Defines if schould be considered executed in case of failure
        /// </summary>
        public bool finalizeOnFailure { get; set; } = false;

        public bool enableMasterTrigger { get; set; } = false;

        /// <summary>
        /// Stop execution of all schedules after successfull execution
        /// </summary>
        public bool breakAllOnFinalize { get; set; } = false;

        /// <summary>
        /// Max number of the successfull schedule executions
        /// </summary>
        public long max { get; set; } = 0;

        /// <summary>
        /// Current number of successfull executions
        /// </summary>
        public long executions { get; set; } = 0;

        /// <summary>
        /// max time for command execution
        /// </summary>
        public int timeout { get; set; } = 0;
    }
}
