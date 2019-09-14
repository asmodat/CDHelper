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

namespace CDHelper
{
    public partial class Program
    {
        private static async Task<string> GetVariableByKey(string key, Dictionary<string, string> nArgs = null, bool throwIfNotFound = true)
        {
            var result = Environment.GetEnvironmentVariable(key);
            if (!result.IsNullOrEmpty())
                return result;

            if (nArgs != null)
                result = nArgs.GetValueOrDefault(key, @default: null);

            if (!result.IsNullOrEmpty())
                return result;

            var tags = await EC2HelperEx.TryGetEnvironmentTags(timeoutSeconds: 60, throwIfNotFound: throwIfNotFound);

            if (tags.IsNullOrEmpty())
            {
                var msg = $"Instance Tag's were not found.";
                if (throwIfNotFound)
                    throw new Exception(msg);

                Console.WriteLine(msg);
                return null;
            }

            result = tags.GetValueOrDefault(key: key, @default: null);

            if (throwIfNotFound && result.IsNullOrEmpty())
                throw new Exception($"Key '{key ?? "undefined"}' was not found among following tags: {tags?.JsonSerialize() ?? "undefined"}.");

            return result;
        }
    }
}
