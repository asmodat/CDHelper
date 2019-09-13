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
        private static async Task<string> GetSecretHexToken(string key, Dictionary<string, string> nArgs = null)
        {
            var hexToken = await GetVariableByKey(key, nArgs: nArgs);

            if (hexToken.IsHex())
                return hexToken;

            Console.WriteLine($"Fetching hex token from secrets manager.");
            var sm = new SMHelper();
            var result = (await sm.GetSecret(hexToken)).JsonDeserialize<AmazonSecretsToken>()?.token;

            if (!result.IsHex())
                throw new Exception("Secret hex token is invalid or coudn't be found, expected hex value.");

            return result;
        }
    }
}
