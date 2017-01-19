using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpdateExecutableCommon.DataModels
{
    public class UpdateStartInfo
    {
        public UpdateStartInfo() { }

        public string CustomerGuid { get; set; }
        public bool IsWipeOperation { get; set; }
        public DateTime StartTime { get; set; }
    }
}
