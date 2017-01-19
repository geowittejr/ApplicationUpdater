using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpdateExecutableCommon.DataModels
{
    public class UpdateResults
    {
        public UpdateResults()
        {
            this.IsWipeOperation = false;
            this.RootUpdatesDirectoryWipeWasSuccessful = false;
            this.AppUpdateWasSuccessful = false;
        }

        public string CustomerGuid { get; set; }
        public DateTime UpdateFinishDate { get; set; }
        public bool IsWipeOperation { get; set; }
        public bool RootUpdatesDirectoryWipeWasSuccessful { get; set; }
        public bool AppUpdateWasSuccessful { get; set; }
        public string AppVersion { get; set; }
    }
}
