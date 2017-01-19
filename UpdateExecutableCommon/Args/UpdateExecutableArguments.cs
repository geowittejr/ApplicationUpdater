using System;
using AppCommon.Args;
using UpdateExecutableCommon.Args;

namespace UpdateExecutableCommon.Args
{
    public class UpdateExecutableArguments
    {
        //NOTE: If you add any properties to this class you also need to update the mapping on the 
        //UpdaterArgumentsParser class.

        public UpdateExecutableArguments()
        {
        }

        public string ClientUpdateDirectoryPath { get; set; }
        public string CustomerGuid { get; set; }
        public string CustomerName { get; set; }
        public string UpdaterWebApiBaseUrl { get; set; }
        
        //App arguments
        public string AppDirectoryPath { get; set; }
        public string AppOldVersion { get; set; }
        public string AppNewVersion { get; set; }
        public string AppZipFileUrl { get; set; }
        [DoNotExportAsArgument]
        public bool UpdateApp 
        {
            get
            {
                return !string.IsNullOrEmpty(AppZipFileUrl);
            }
        }
    }
}
