using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using RemoteClientConsoleApp.Utilities;
using AppCommon.Crypto;
using UpdateExecutableCommon.Utilities;

namespace RemoteClientConsoleApp
{
    public class Program
    {

        static void Main(string[] args)
        {         
            //Set unhandled exception handler
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
            
            //Start process
            UpdateProcessor updater = new UpdateProcessor(
                Properties.Settings.Default.CustomerGuid,
                Properties.Settings.Default.CustomerName,
                Properties.Settings.Default.AppDirectoryPath,
                Properties.Settings.Default.CentralizedUpdateWebApiBaseUrl);

            updater.Start();
        }

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.LogError(e.ExceptionObject as Exception, "RemoteClientUpdateConsole error", Properties.Settings.Default.CustomerName);

            //Keep the console app running for a little while longer
            //so that all logs have time to be written to the database.
            System.Threading.Thread.Sleep(15000);

            Environment.Exit(1);
        }
    }
}
