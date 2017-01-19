using System;


namespace UpdateExecutableCommon.DataModels
{
    public class ClientInfo
    {
        public ClientInfo()
        {
            this.AppVersion = "0.0.0.0";
        }

        /// <summary>
        /// Unique customer identifier
        /// </summary>
        public Guid CustomerGuid { get; set; }
        /// <summary>
        /// The current version of App installed on the client server
        /// </summary>
        public string AppVersion { get; set; }
        /// <summary>
        /// The App application directory root
        /// </summary>
        public string AppDirectoryPath { get; set; }
        /// <summary>
        /// Operating system on the client's server
        /// </summary>
        public string ServerOS { get; set; }
        /// <summary>
        /// Amount of memory on the client's server
        /// </summary>
        public string ServerMemory { get; set; }
        /// <summary>
        /// Database version the client is using
        /// </summary>
        public string DatabaseVersion { get; set; }
    }
}
