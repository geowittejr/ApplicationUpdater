using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpdateExecutableWipe
{
    class Program
    {
        static void Main(string[] args)
        {
            var wiper = new ClientWiper(args);
            wiper.Start();
        }
    }
}
