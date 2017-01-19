using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpdateExecutable1
{
    class Program
    {
        static void Main(string[] args)
        {
            ClientUpdater updater = new ClientUpdater(args);
            updater.Start();
        }
    }
}
