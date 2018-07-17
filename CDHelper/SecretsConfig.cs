using Newtonsoft.Json;
using System.IO;
using System;
using System.Linq;
using AsmodatStandard.Extensions;
using AsmodatStandard.Extensions.Collections;

namespace CDHelper
{
    public class SecretConfig
    {
        public string Source; //local source of the secret
        public string Destination; //local destination where encrypted secret must be set
    }
}
