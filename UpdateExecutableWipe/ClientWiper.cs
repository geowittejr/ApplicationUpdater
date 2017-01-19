using System;
using System.IO;
using UpdateExecutableCommon.Utilities;
using UpdateExecutableCommon.Args;
using UpdateExecutableCommon.DataModels;
using AppCommon.Args;
using AppCommon.IO;

namespace UpdateExecutableWipe
{
    public class ClientWiper
    {
        /// <summary>
        /// Arguments passed in
        /// </summary>
        protected UpdateExecutableArguments _Arguments = null;

        public ClientWiper(string[] args)
        {
            //Set unhandled exception handler
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            var argString = args.ToArgumentsString();
            Logger.LogInfo("ClientWiper.Ctor", "Constructor started. args=" + argString, string.Empty);

            //Parse the arguments into an instance of UpdaterArguments
            var parser = new UpdateExecutableArgumentsParser();
            _Arguments = parser.ParseArguments(args);
            if (_Arguments == null)
                throw new Exception("ArgumentsParser failed to parse UpdaterArguments.");
        }
        
        /// <summary>
        /// Start the client update process
        /// </summary>
        public void Start()
        {
            Logger.LogInfo("ClientWiper.Start()", "Starting wipe process.", _Arguments.CustomerName);

            //TODO: Track success of steps and call back to web api to report failures
            bool reportStart = ReportUpdateStart();
            bool appSuccess = WipeAppApplication();
            bool wipeRootUpdateDirSuccess = WipeRootUpdatesDirectory();
            bool reportResults = ReportUpdateResults(wipeRootUpdateDirSuccess, appSuccess);

            Logger.LogInfo("ClientWiper.Start()", "Finished wipe process", _Arguments.CustomerName);

            //Keep the console app running for a little while longer
            //so that all logs have time to be written to the database.
            System.Threading.Thread.Sleep(15000);
        }

        /// <summary>
        /// Report the start of the update process.
        /// </summary>
        /// <returns>Bool value denoting success or failure of the reporting process</returns>
        public virtual bool ReportUpdateStart()
        {
            bool success = true;
            try
            {
                var url = HttpHelper.GetUrl(_Arguments.UpdaterWebApiBaseUrl, "api/update/postupdatestartinfo", string.Empty);
                UpdateStartInfo info = new UpdateStartInfo
                {
                    CustomerGuid = _Arguments.CustomerGuid,
                    IsWipeOperation = true,
                    StartTime = DateTime.Now
                };
                success = HttpHelper.SendPostRequest<UpdateStartInfo>(url, info);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ClientWiper.ReportUpdateStart()", _Arguments.CustomerName);
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Report results of the update operation.
        /// </summary>
        /// <returns>Bool value denoting success or failure of the reporting process</returns>
        public virtual bool ReportUpdateResults(bool wipeRootUpdateDirSuccess, bool appUpdateWasSuccessful)
        {
            bool success = true;
            try
            {
                var url = HttpHelper.GetUrl(_Arguments.UpdaterWebApiBaseUrl, "api/update/postupdateresults", string.Empty);
                UpdateResults results = new UpdateResults
                {
                    CustomerGuid = _Arguments.CustomerGuid,
                    UpdateFinishDate = DateTime.Now,
                    IsWipeOperation = true,
                    RootUpdatesDirectoryWipeWasSuccessful = wipeRootUpdateDirSuccess,
                    AppUpdateWasSuccessful = appUpdateWasSuccessful,
                    AppVersion = appUpdateWasSuccessful ? "0.0.0.0" : _Arguments.AppOldVersion
                };
                success = HttpHelper.SendPostRequest<UpdateResults>(url, results);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ClientWiper.ReportUpdateResults()", _Arguments.CustomerName);
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Wipe the client App application from the server
        /// </summary>
        protected bool WipeAppApplication()
        {
            bool success = true;
            var productionWebConfigPath = _Arguments.AppDirectoryPath + "\\Web.config";
            var archivedWebConfigPath = _Arguments.ClientUpdateDirectoryPath + "\\App-Old-Web.config";

            Logger.LogInfo("ClientWiper.WipeAppApplication()", "Preparing to wipe the App application.", _Arguments.CustomerName);

            //Save temp copy of production App web.config
            try
            {
                if (File.Exists(productionWebConfigPath))
                {
                    File.Copy(productionWebConfigPath, archivedWebConfigPath, true);
                    Logger.LogInfo("ClientWiper.WipeAppApplication()", "Archived production App web.config from \"" + productionWebConfigPath + "\".", _Arguments.CustomerName);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "UpdaterWipe.WipeAppApplication() - save temp web.config", _Arguments.CustomerName);
                success = false;
            }
            
            //Delete existing App directory contents
            try
            {
                DirectoryInfo currentAppDir = new DirectoryInfo(_Arguments.AppDirectoryPath);
                currentAppDir.DeleteChildFilesAndFolders();
                Logger.LogInfo("ClientWiper.WipeAppApplication()", "Deleted contents of production App directory at \"" + _Arguments.AppDirectoryPath + "\".", _Arguments.CustomerName);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "UpdaterWipe.WipeAppApplication() - Delete existing contents", _Arguments.CustomerName);
                success = false;
            }

            //Copy the old web.config file back
            try
            {
                if (File.Exists(archivedWebConfigPath))
                {
                    File.Copy(archivedWebConfigPath, productionWebConfigPath, true);
                    Logger.LogInfo("ClientWiper.WipeAppApplication()", "Copied archived App web.config back to the production directory.", _Arguments.CustomerName);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "UpdaterWipe.WipeAppApplication() - Copy archived web.config back.", _Arguments.CustomerName);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Delete the root updates directory
        /// </summary>
        protected bool WipeRootUpdatesDirectory()
        {
            bool success = true;

            DirectoryInfo clientUpdateDir = new DirectoryInfo(_Arguments.ClientUpdateDirectoryPath);
            if (clientUpdateDir.Exists)
            {
                DirectoryInfo rootUpdateDir = clientUpdateDir.Parent;
                if (rootUpdateDir.Exists)
                {
                    try
                    {
                        rootUpdateDir.DeleteChildFilesAndFoldersAndCombineExceptions();
                        Logger.LogInfo("ClientWiper.WipeRootUpdatesDirectory()", "Deleted contents of the root updates directory at \"" + rootUpdateDir.FullName + "\".", _Arguments.CustomerName);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "ClientWiper.WipeRootUpdatesDirectory() error", _Arguments.CustomerName);
                        success = false;
                    }                    
                }
            }

            return success;
        }

        /// <summary>
        /// Unhandled exception handler for the updater
        /// </summary>
        protected void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            var customer = _Arguments != null ? _Arguments.CustomerName : string.Empty;
            Logger.LogError(e.ExceptionObject as Exception, "UpdaterWipe exception", customer);

            //Keep the console app running for a little while longer
            //so that all logs have time to be written to the database.
            System.Threading.Thread.Sleep(15000);
        }
    }
}
