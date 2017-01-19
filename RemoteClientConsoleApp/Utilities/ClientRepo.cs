using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using UpdateExecutableCommon.DataModels;
using AppCommon.Environment;
using UpdateExecutableCommon.Utilities;

namespace RemoteClientConsoleApp.Utilities
{
    public class ClientRepo
    {
        private Guid _CustomerGuid;
        private string _CustomerName = string.Empty;
        private string _AppDirectoryPath = string.Empty;

        public ClientRepo(Guid customerGuid, string customerName, string appDirectoryPath)
        {
            _CustomerGuid = customerGuid;
            _CustomerName = customerName;
            _AppDirectoryPath = appDirectoryPath;
        }

        public ClientInfo GetClientInfo()
        {
            return new ClientInfo()
            {
                CustomerGuid = _CustomerGuid,
                AppVersion = GetAppVersion(),
                AppDirectoryPath = _AppDirectoryPath,
                ServerOS = ServerSpecs.GetServerOS(),
                ServerMemory = ServerSpecs.GetServerMemory(),
                DatabaseVersion = GetDatabaseVersion()          
            };
        }

        /// <summary>
        /// Get the current version of SQL Server used by the client
        /// </summary>
        /// <returns></returns>
        private string GetDatabaseVersion()
        {
            //For now, we will get the database connection string from the App application web.config.
            //Maybe in the future this will change.
            string webConfigPath = _AppDirectoryPath + "\\web.config";
            string dbConnectionString = ConfigUtility.GetConnectionStringFromWebConfig(webConfigPath, "AppConnectionString");

            string version = ServerSpecs.GetSqlServerDatabaseVersion(dbConnectionString);
            
            Logger.LogInfo("ClientRepo.GetDatabaseVersion()", "SQL Server version: " + version, _CustomerName);

            return version;
        }

        /// <summary>
        /// Get the current version of App
        /// </summary>
        /// <returns></returns>
        private string GetAppVersion()
        {
            string version = "0.0.0.0";
            var appDllPath = _AppDirectoryPath + "\\bin\\App.dll";

            //If file doesn't exist this would be a new install, or the directory is corrupt, make the version 0 so we can update/install/or fix
            if (File.Exists(appDllPath))
            {
                var info = FileVersionInfo.GetVersionInfo(appDllPath);
                version = info != null ? info.FileVersion : "0.0.0.0";
            }
            Logger.LogInfo("ClientRepo.GetAppVersion()", "Client App version: " + version + ".", _CustomerName);

            return version;         
        }
    }
}
