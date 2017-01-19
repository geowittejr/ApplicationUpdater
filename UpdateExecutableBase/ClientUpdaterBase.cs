using System;
using System.Collections.Generic;
using System.IO;
using UpdateExecutableCommon.Args;
using UpdateExecutableCommon.Utilities;
using AppCommon.IO;
using UpdateExecutableCommon.DataModels;
using AppCommon.Args;

namespace UpdateExecutableBase
{
    public abstract class ClientUpdaterBase
    {
        /// <summary>
        /// Directory path of extracted new App zip file
        /// </summary>
        protected string _AppNewUpdateDirectoryPath = string.Empty;

        /// <summary>
        /// The file path of the old zipped up App directory
        /// </summary>
        protected string _AppOldDirectoryZipFilePath = string.Empty;

        /// <summary>
        /// The file path of the new downloaded App zip file
        /// </summary>
        protected string _AppNewUpdateZipFilePath = string.Empty;

        /// <summary>
        /// The file path of the existing App web.config file
        /// </summary>
        protected string _AppProductionWebConfigFilePath = string.Empty;

        /// <summary>
        /// The file path of the old App web.config file
        /// </summary>
        protected string _AppOldWebConfigFilePath = string.Empty;

        /// <summary>
        /// Arguments passed in
        /// </summary>
        protected UpdateExecutableArguments _Arguments = null;

        public ClientUpdaterBase(string[] args)
        {
            //Set unhandled exception handler
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            var argString = args.ToArgumentsString();
            Logger.LogInfo("ClientUpdaterBase.Ctor", "Constructor started. args=" + argString, string.Empty);

            //Parse the arguments into an instance of UpdaterArguments
            var parser = new UpdateExecutableArgumentsParser();
            _Arguments = parser.ParseArguments(args);
            if (_Arguments == null)
                throw new Exception("ArgumentsParser failed to parse UpdaterArguments.");

            _AppNewUpdateDirectoryPath = _Arguments.ClientUpdateDirectoryPath + "\\App-New-" + _Arguments.AppNewVersion;
            _AppOldDirectoryZipFilePath = _Arguments.ClientUpdateDirectoryPath + "\\App-Old-" + _Arguments.AppOldVersion + ".zip";
            _AppNewUpdateZipFilePath = _Arguments.ClientUpdateDirectoryPath + "\\App-New-" + _Arguments.AppNewVersion + ".zip";
            _AppProductionWebConfigFilePath = _Arguments.AppDirectoryPath + "\\Web.config";
            _AppOldWebConfigFilePath = _Arguments.ClientUpdateDirectoryPath + "\\App-Old-Web.config";
        }
        
        /// <summary>
        /// Start the client update process
        /// </summary>
        public virtual void Start()
        {
            Logger.LogInfo("ClientUpdaterBase.Start()", "Starting the updater process.", _Arguments.CustomerName);

            //Report that we are starting
            bool reportStart = ReportUpdateStart();

            //Run update steps
            bool updateSucceeded = RunAppUpdateStep();

            //Report the update results
            bool reportResults = ReportUpdateResults(updateSucceeded);

            Logger.LogInfo("ClientUpdaterBase.Start()", "Finished the updater process.", _Arguments.CustomerName);

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
                    IsWipeOperation = false,
                    StartTime = DateTime.Now
                };
                success = HttpHelper.SendPostRequest<UpdateStartInfo>(url, info);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ClientUpdaterBase.ReportUpdateStart()", _Arguments.CustomerName);
                success = false;
            }
            return success;
        }

        /// <summary>
        /// Report results of the update operation.
        /// </summary>
        /// <returns>Bool value denoting success or failure of the reporting process</returns>
        public virtual bool ReportUpdateResults(bool appUpdateWasSuccessful)
        {
            bool success = true;
            try
            {
                var url = HttpHelper.GetUrl(_Arguments.UpdaterWebApiBaseUrl, "api/update/postupdateresults", string.Empty);
                UpdateResults results = new UpdateResults
                {
                    CustomerGuid = _Arguments.CustomerGuid,
                    UpdateFinishDate = DateTime.Now,
                    IsWipeOperation = false,
                    RootUpdatesDirectoryWipeWasSuccessful = false,
                    AppUpdateWasSuccessful = appUpdateWasSuccessful,
                    AppVersion = appUpdateWasSuccessful ? _Arguments.AppNewVersion : _Arguments.AppOldVersion
                };
                success = HttpHelper.SendPostRequest<UpdateResults>(url, results);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ClientUpdaterBase.ReportUpdateResults()", _Arguments.CustomerName);
                success = false;
            }
            return success;
        }

        public virtual bool RunAppUpdateStep()
        {
            bool resourcesPrepared = false;
            bool updateSucceeded = false;

            //Update App
            if (_Arguments.UpdateApp)
            {
                try
                {
                    //Get all external resources and save them so we can work with them.
                    //This includes downloadable resources, as well as zipped up client directories.
                    PrepareAppExternalResources();
                    resourcesPrepared = true;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "ClientUpdaterBase.RunAppUpdateStep() - Prepare App external resources", _Arguments.CustomerName);
                }

                //Process the App updates if preparation was successful
                if (resourcesPrepared)
                {
                    try
                    {
                        updateSucceeded = ProcessAppUpdate();                        
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "ClientUpdaterBase.RunAppUpdateStep() - Process App Update", _Arguments.CustomerName);
                    }

                    if (updateSucceeded)
                    {
                        Logger.LogInfo("ClientUpdaterBase.RunAppUpdateStep()", "App update succeeded.", _Arguments.CustomerName);
                    }
                    else
                    {
                        Logger.LogInfo("ClientUpdaterBase.RunAppUpdateStep()", "App did not complete without errors.", _Arguments.CustomerName);
                    }
                }
                else
                {
                    Logger.LogInfo("ClientUpdaterBase.RunAppUpdateStep()", "Skipped App update due to preparation errors.", _Arguments.CustomerName);
                }                
            }
            else
            {
                Logger.LogInfo("ClientUpdaterBase.RunAppUpdateStep()", "No App update is available.", _Arguments.CustomerName);
                updateSucceeded = true;
            }

            return updateSucceeded;
        }
        
        /// <summary>
        /// Process the App updates
        /// </summary>
        public virtual bool ProcessAppUpdate()
        {
            bool continueProcessing = true;

            Logger.LogInfo("ClientUpdaterBase.ProcessAppUpdate()", "Preparing to process the App update.", _Arguments.CustomerName);

            //Extract App zip
            DirectoryInfo appDir = new DirectoryInfo(_AppNewUpdateDirectoryPath);
            try
            {                
                ZipUtility.ExtractZipFile(_AppNewUpdateZipFilePath, _AppNewUpdateDirectoryPath);
                Logger.LogInfo("ClientUpdaterBase.ProcessAppUpdate()", "Extracted App zip file to \"" + _AppNewUpdateZipFilePath + "\".", _Arguments.CustomerName);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ClientUpdaterBase.ProcessAppUpdate() - Extract App update zip file.", _Arguments.CustomerName);
                continueProcessing = false;
            }

            //If extracting the update failed, don't continue
            if (!continueProcessing)
            {
                return false;
            }

            //Delete existing App directory contents
            DirectoryInfo currentAppDir = new DirectoryInfo(_Arguments.AppDirectoryPath);
            try
            {
                currentAppDir.DeleteChildFilesAndFoldersAndCombineExceptions();
                Logger.LogInfo("ClientUpdaterBase.ProcessAppUpdate()", "Deleted contents of production App directory at \"" + currentAppDir.FullName + "\".", _Arguments.CustomerName);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ClientUpdaterBase.ProcessAppUpdate() - Delete existing App directory contents.", _Arguments.CustomerName);
            }

            //Copy update contents into production directory.
            //Then copy archived web.config back.
            try
            {                
                appDir.CopyFilesAndFoldersToDirectory(currentAppDir);
                Logger.LogInfo("ClientUpdaterBase.ProcessAppUpdate()", "Copied new App files to production App directory at \"" + currentAppDir.FullName + "\".", _Arguments.CustomerName);

                if (File.Exists(_AppOldWebConfigFilePath))
                {
                    File.Copy(_AppOldWebConfigFilePath, _AppProductionWebConfigFilePath, true);
                    Logger.LogInfo("ClientUpdaterBase.ProcessAppUpdate()", "Copied archived App web.config to production directory.", _Arguments.CustomerName);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "ClientUpdaterBase.ProcessAppUpdate() - Copy update contents to App directory.", _Arguments.CustomerName);
                continueProcessing = false;
            }
            
            //Copying update files into App directory failed.
            //We need to rollback
            if (!continueProcessing)
            {
                bool result = RollbackWebDirectoryUpdate(_AppOldDirectoryZipFilePath, _Arguments.AppDirectoryPath);
                return false;
            }

            //Delete the extracted App directory
            try
            {
                appDir.DeleteChildFilesAndFoldersAndCombineExceptions();
                appDir.Delete(true);
                Logger.LogInfo("ClientUpdaterBase.ProcessAppUpdate()", "Deleted extracted App directory to clean up after ourselves.", _Arguments.CustomerName);
            }
            catch(Exception ex)
            {
                Logger.LogError(ex, "ClientUpdaterBase.ProcessAppUpdate() - Delete extracted App directory", _Arguments.CustomerName);
            }

            return true;            
        }

        /// <summary>
        /// Get all external App resources including downloadable resources and the client's current directories zipped.
        /// </summary>
        public virtual void PrepareAppExternalResources()
        {
            Logger.LogInfo("ClientUpdaterBase.PrepareAppExternalResources()", "Preparing to get App external resources.", _Arguments.CustomerName);
            try
            {
                HttpHelper.DownloadBinaryFile(_Arguments.AppZipFileUrl, _AppNewUpdateZipFilePath);
                Logger.LogInfo("ClientUpdaterBase.PrepareAppExternalResources()", "Downloaded App update zip file to \"" + _AppNewUpdateZipFilePath + "\".", _Arguments.CustomerName);

                //Save temp copy of production App web.config
                if (File.Exists(_AppProductionWebConfigFilePath))
                {
                    File.Copy(_AppProductionWebConfigFilePath, _AppOldWebConfigFilePath, true);
                    Logger.LogInfo("ClientUpdaterBase.PrepareAppExternalResources()", "Archived production App web.config from \"" + _AppOldWebConfigFilePath + "\".", _Arguments.CustomerName);
                }

                //Zip up old App directory and save it
                ZipUtility.CreateZipFileFromDirectory(_Arguments.AppDirectoryPath, _AppOldDirectoryZipFilePath);
                Logger.LogInfo("ClientUpdaterBase.PrepareAppExternalResources()", "Zipped up production App directory at \"" + _Arguments.AppDirectoryPath + "\".", _Arguments.CustomerName);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Rollback a web directory update with the contents of the specified zip archive file.
        /// </summary>
        /// <param name="zipArchivePath">The zip archive file to extract into the destination directory</param>
        /// <param name="destinationPath">The directory of the web application</param>
        /// <returns></returns>
        public virtual bool RollbackWebDirectoryUpdate(string zipArchivePath, string destinationPath)
        {
            bool success = false;

            //If the archived directory exists, proceed
            if (File.Exists(zipArchivePath))
            {
                try
                {
                    //Delete all contents of the destination directory
                    DirectoryInfo destinationDir = new DirectoryInfo(destinationPath);
                    if (destinationDir.Exists)
                    {
                        try
                        {
                            destinationDir.DeleteChildFilesAndFoldersAndCombineExceptions();
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "ClientUpdaterBase.RollbackWebDirectoryUpdate() - Delete destination directory contents.", _Arguments.CustomerName);
                        }                        
                    }

                    //Extract the archive file contents into destination
                    ZipUtility.ExtractZipFile(zipArchivePath, destinationDir.FullName);

                    success = true;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "ClientUpdaterBase.RollbackWebDirectoryUpdate()", _Arguments.CustomerName);
                    success = false;
                }              
            }

            return success;
        }

        /// <summary>
        /// Unhandled exception handler for the updater
        /// </summary>
        public virtual void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            var customer = _Arguments != null ? _Arguments.CustomerName : string.Empty;
            Logger.LogError(e.ExceptionObject as Exception, "Updater exception", customer);

            //Keep the console app running for a little while longer
            //so that all logs have time to be written to the database.
            System.Threading.Thread.Sleep(15000);
        }
    }
}
