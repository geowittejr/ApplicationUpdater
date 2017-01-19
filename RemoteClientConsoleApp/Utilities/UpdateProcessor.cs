using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using UpdateExecutableCommon.DataModels;
using UpdateExecutableCommon.Utilities;
using AppCommon.IO;
using UpdateExecutableCommon.Args;
using AppCommon.Args;

namespace RemoteClientConsoleApp.Utilities
{
    public class UpdateProcessor
    {
        private ClientRepo _ClientRepo = null;
        private UpdateRepository _SourceRepo = null;
        private string _CustomerName = string.Empty;

        public UpdateProcessor(Guid customerGuid, string customerName, string appDirectoryPath, string updateWebApiUrl)
        {
            _ClientRepo = new ClientRepo(customerGuid, customerName, appDirectoryPath);
            _SourceRepo = new UpdateRepository(updateWebApiUrl, customerName);
            _CustomerName = customerName;
        }

        public void Start()
        {           
            Logger.LogInfo("UpdateProcessor.Start()", "Starting update processor.", _CustomerName);

            //Send client info home to check for updates
            ClientInfo clientInfo = _ClientRepo.GetClientInfo();
            UpdateInfo updateInfo = _SourceRepo.GetUpdateInfo(clientInfo);

            //Does this client have any updates?
            if (updateInfo.UpdateIsAvailable)
            {
                //Get the updater exe
                byte[] updaterZip = _SourceRepo.GetUpdaterZipFile(updateInfo.ApplicationUpdaterZipUrl);
                if (updaterZip != null)
                {
                    Logger.LogInfo("UpdateProcessor.Start()", "An update was downloaded. WipeClient=" + (updateInfo.WipeClient ? "true" : "false"), _CustomerName);

                    //Parse the arguments array to an object
                    UpdateExecutableArgumentsParser parser = new UpdateExecutableArgumentsParser();
                    UpdateExecutableArguments arguments = parser.ParseArguments(updateInfo.ApplicationUpdaterArguments);

                    //Create the update or wipe directory and save the path to the arguments object
                    string clientUpdateDirectoryPath = updateInfo.WipeClient ? "Wipes\\" + DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss.ff") : "Updates\\" + DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss.ff");
                    DirectoryInfo clientUpdateDir = Directory.CreateDirectory(clientUpdateDirectoryPath);
                    arguments.ClientUpdateDirectoryPath = clientUpdateDir.FullName;
                    Logger.LogInfo("UpdateProcessor.Start()", "Created the client update directory at \"" + arguments.ClientUpdateDirectoryPath + "\".", _CustomerName);

                    //Kick off the process
                    RunApplicationUpdater(
                        arguments.ClientUpdateDirectoryPath,
                        updaterZip,
                        updateInfo.ApplicationUpdaterZipFilename,
                        updateInfo.ApplicationUpdaterExecutableFilename,
                        arguments.ToArgumentsArray<UpdateExecutableArguments>());
                }
                else
                {
                    Logger.LogError(
                        new Exception("Updater zip file was not downloaded successfully. UpdaterProcessor.Start()."), 
                        "Updater zip file was not downloaded successfully", _CustomerName);
                }                
            }
            else
            {
                Logger.LogInfo("UpdaterProcessor.Start()", "No updates were necessary.", _CustomerName);
            }

            //Keep the console app running for a little while longer
            //so that all logs have time to be written to the database.
            System.Threading.Thread.Sleep(15000);
        }

        private void RunApplicationUpdater(string updateDirectoryPath, byte[] updaterZipFile, string updaterZipFilename, string updaterExeFilename, string[] updaterArguments)
        {
            Logger.LogInfo("UpdateProcessor.RunUpdater()", "Preparing to run updater.", _CustomerName);

            //Save the updater zip      
            string updaterZipFilePath = updateDirectoryPath + "\\" + updaterZipFilename;
            File.WriteAllBytes(updaterZipFilePath, updaterZipFile);
            Logger.LogInfo("UpdateProcessor.RunUpdater()", "Saved updater zip file to \"" + updaterZipFilePath + "\".", _CustomerName);

            //Extract the zip contents
            string updaterExtractedPath = updateDirectoryPath + "\\" + updaterZipFilename.Replace(".zip", string.Empty);
            ZipUtility.ExtractZipFile(updaterZipFilePath, updaterExtractedPath);
            Logger.LogInfo("UpdateProcessor.RunUpdater()", "Extracted updater zip file to \"" + updaterExtractedPath + "\".", _CustomerName);

            //Start the process and pass arguments
            var exeFilePath = updaterExtractedPath + "\\" + updaterExeFilename;
            var startInfo = new ProcessStartInfo()
            {
                FileName = exeFilePath,
                Arguments = updaterArguments.ToArgumentsString()
            };
            Logger.LogInfo("UpdateProcessor.RunUpdater()", "Starting updater process: \"" + exeFilePath + "\".", _CustomerName);
            Process.Start(startInfo);
        }
    }
}
