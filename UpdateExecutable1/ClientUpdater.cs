using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UpdateExecutableBase;

namespace UpdateExecutable1
{
    public class ClientUpdater : ClientUpdaterBase
    {
        public ClientUpdater(string[] args) : base(args)
        {
        }

        public override void Start()
        {
            base.Start();                       
        }

        public override void PrepareAppExternalResources()
        {
            base.PrepareAppExternalResources();
        }

        public override bool ProcessAppUpdate()
        {
            return base.ProcessAppUpdate();
        }
    }
}
