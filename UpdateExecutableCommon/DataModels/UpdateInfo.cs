

namespace UpdateExecutableCommon.DataModels
{
    public class UpdateInfo
    {
        public UpdateInfo()
        {
            this.UpdateIsAvailable = false;
            this.WipeClient = false;
            this.ApplicationUpdaterZipUrl = string.Empty;
            this.ApplicationUpdaterArguments = new string[] { };
        }

        /// <summary>
        /// Denotes whether or not an update is available for the client
        /// </summary>
        public bool UpdateIsAvailable { get; set; }
        /// <summary>
        /// Denotes whether or not the update is a wipe
        /// </summary>
        public bool WipeClient { get; set; }
        /// <summary>
        /// Url of the updater zip file
        /// </summary>
        public string ApplicationUpdaterZipUrl { get; set; }
        /// <summary>
        /// The name of the updater zip file
        /// </summary>
        public string ApplicationUpdaterZipFilename { get; set; }
        /// <summary>
        /// The name of the updater executable file
        /// </summary>
        public string ApplicationUpdaterExecutableFilename { get; set; }
        /// <summary>
        /// Array of arguments to pass to the updater exe
        /// </summary>
        public string[] ApplicationUpdaterArguments { get; set; }
    }
}
