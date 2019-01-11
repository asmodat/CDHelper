namespace CDHelper
{
    public class DockergenConfig
    {
        public long version = 1;

        /// <summary>
        /// port where application starts after the start command is run
        /// </summary>
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

        /// <summary>
        /// heroku buildpack used to build the source project
        /// </summary>
        public string buildpackId;

        /// <summary>
        /// command used to replace default heroku build command 
        /// </summary>
        public string customBuildCommand;

        /// <summary>
        /// command used to start the application (heroku local is default)
        /// </summary>
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
