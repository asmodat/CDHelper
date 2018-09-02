using System;
using System.Collections.Generic;

namespace CDHelper
{
    public class ParametersConfig
    {
        public string Id;
        public ConfigParameter[] Parameters;
        public ConfigParameter[] EnvironmentVariables;

        public void Process()
        {
            throw new NotImplementedException();
        }
    }

    public class ConfigParameter
    {
        public string Name;
        public string Value;
        public string DefaultValue;
    }
}
