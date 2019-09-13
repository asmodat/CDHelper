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
        private static async Task<string> GetVariableByKey(string key, Dictionary<string, string> nArgs = null)
        {
            var result = Environment.GetEnvironmentVariable(key);
            if (!result.IsNullOrEmpty())
                return result;

            if (nArgs != null)
                result = nArgs.GetValueOrDefault(key, @default: null);

            if (!result.IsNullOrEmpty())
                return result;

            var tags = await EC2HelperEx.TryGetEnvironmentTags(timeoutSeconds: 5);

            if (tags.IsNullOrEmpty())
                return null;

            result = tags.GetValueOrDefault(key, @default: null);

            if (result == null)
                throw new Exception($"Key '{key}' was not defined.");

            return result;
        }
    }
}
