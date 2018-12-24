namespace CDHelper
{
    public class DockergenConfig
    {
        public long version = 1;

        public int port;

        /// <summary>
        /// project relative path in the repository
        /// </summary>
        public string relativePath;

        /// <summary>
        /// Working Directory, where content of the project will be copied - a random GUID if not specified
        /// </summary>
        public string workingDirectory;

        /// <summary>
        /// base dockerfile image
        /// </summary>
        public string baseImage;

        public string buildpackId;

        public string customBuildCommand;
        public string customStartCommand;

        /// <summary>
        /// environment variables
        /// </summary>
        public string[] env;

        /// <summary>
        /// forces config file to be generated even if coresponding docker image configuraion files already exist
        /// </summary>
        public bool force;
    }
}
