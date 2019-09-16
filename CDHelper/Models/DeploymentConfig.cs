using AsmodatStandard.Extensions.IO;
using System.IO;
using AsmodatStandard.Extensions;
using AsmodatStandard.Cryptography;
using System;
using AsmodatStandard.Types;
using AsmodatStandard.Extensions.Collections;

namespace CDHelper.Models
{
    public static class DeploymentConfigEx
    {
        public static bool IsTriggered(this DeploymentConfig schedule, ExecutionSchedule scheduleOld)
        {
            if (schedule?.enable != true)
                return false;

            if (schedule.max > 0 && schedule.max <= scheduleOld.executions)
                return false;

            if (schedule.schedules.IsNullOrEmpty())
                return false;

            if (schedule.cron?.ToCron()?.Compare(DateTime.UtcNow) == 0)
                return true;

            if (schedule.trigger > 0 && scheduleOld.trigger < schedule.trigger)
                return true;

            return false;
        }

        public static DeploymentConfig LoadDeploymentConfig(this DeploymentConfig deployment, DirectoryInfo rootStatus)
        {
            if (deployment.id.IsNullOrEmpty())
                throw new Exception("Deployment ID was not defined, could not load.");

            var fileLocation = PathEx.RuntimeCombine(rootStatus.FullName, $"{deployment.id.SHA256().ToHexString()}.json").ToFileInfo();

            if (!fileLocation.Directory.Exists)
                fileLocation.Directory.Create();

            if (!fileLocation.Exists)
                fileLocation.WriteAllText("{ }");

            return fileLocation.DeserialiseJson<DeploymentConfig>();
        }

        public static void UpdateDeploymentConfig(this DeploymentConfig deployment, DirectoryInfo rootStatus)
        {
            var fileLocation = PathEx.RuntimeCombine(rootStatus.FullName, $"{deployment.id.SHA256().ToHexString()}.json").ToFileInfo();

            if (!fileLocation.Directory.Exists)
                fileLocation.Directory.Create();

            fileLocation.WriteAllText(deployment.JsonSerialize());
        }
    }

    public class DeploymentConfig : ExecutionSchedule
    {
        public ExecutionSchedule[] schedules { get; set; }
    }
}
