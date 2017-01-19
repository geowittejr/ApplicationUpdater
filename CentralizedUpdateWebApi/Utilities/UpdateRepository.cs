using System;
using System.Collections.Generic;
using System.Linq;
using UpdateExecutableCommon.Args;
using UpdateExecutableCommon.DataModels;
using UpdateExecutableCommon.Utilities;
using AppCommon.Args;

namespace CentralizedUpdateWebApi.Utilities
{
    public class UpdateRepository
    {
        private BlobStorage _BlobStore = null;
        private string _EmptyArgumentValue = " "; //one space. This is in lieu of empty string, which our arguments parser doesn't like.

        public UpdateRepository()
        {
            _BlobStore = new BlobStorage();
        }

        /// <summary>
        /// Get the update info that will be used to let our console application know how to proceed with updating the client
        /// </summary>
        public UpdateInfo GetUpdateInfo(ClientInfo clientInfo, string baseUrl)
        {
            //Set defaults
            Customer customer = null;
            bool updateIsAvailable = false;
            bool updateApp = false;
            bool wipeClient = false;
            string updaterKey = string.Empty;
            Version appProductionVersion = null;
            Version appClientVersion = null;

            //Get customer data and update their info
            using (var context = new UpdaterEntities())
            {
                customer = (from c in context.Customers
                    where c.CustomerGuid == clientInfo.CustomerGuid
                    select c).FirstOrDefault<Customer>();

                if (customer != null)
                {
                    customer.LastUpdateCheck = DateTime.Now;
                    customer.ServerOS = clientInfo.ServerOS;
                    customer.ServerMemory = clientInfo.ServerMemory;
                    customer.DatabaseVersion = clientInfo.DatabaseVersion;
                    customer.AppVersion = clientInfo.AppVersion;
                    context.SaveChanges();
                }
            }            

            if (customer == null)
                return new UpdateInfo();       

            //Populate client versions
            appClientVersion = new Version(string.IsNullOrEmpty(clientInfo.AppVersion) ? "0.0.0.0" : clientInfo.AppVersion);

            //Check if this is a wipe update operation or not
            if (!customer.IsActive)
            {
                wipeClient = true;
                updateIsAvailable = true;

                //Get the updater key specified for customer wipes
                updaterKey = Properties.Settings.Default.ApplicationUpdaterWipeZipFilename;
            }
            else
            {
                //Get the updater key set for this customer
                updaterKey = customer.UpdaterKey;

                //Get production version info
                appProductionVersion = new Version(_BlobStore.GetVersionInfoByKey(Properties.Settings.Default.AppDllFileName).FileVersion);

                //If the versions are different, updates are true
                if (!string.IsNullOrEmpty(customer.UpdaterKey) &&
                    (appClientVersion < appProductionVersion || Properties.Settings.Default.AlwaysUpdateApp))
                {
                    updateApp = true;
                    updateIsAvailable = true;
                }
            }

            //Build the arguments that our console application will eventually pass on to the downloaded updater executable
            //NOTE: Don't pass back empty strings for argument values. Replace empty string values with this._EmptyArgumentValue. 
            //It plays nice with our client side args parser.
            UpdateExecutableArguments updaterArgs = new UpdateExecutableArguments
            {
                CustomerGuid = customer.CustomerGuid.ToString(),
                CustomerName = !string.IsNullOrEmpty(customer.CustomerName) ? customer.CustomerName : this._EmptyArgumentValue,
                ClientUpdateDirectoryPath = this._EmptyArgumentValue,
                UpdaterWebApiBaseUrl = HttpHelper.GetUrl(baseUrl, string.Empty, string.Empty),
                AppDirectoryPath = !string.IsNullOrEmpty(clientInfo.AppDirectoryPath) ? clientInfo.AppDirectoryPath : this._EmptyArgumentValue,
                AppOldVersion = appClientVersion.ToString(),
                AppNewVersion = updateApp ? appProductionVersion.ToString() : this._EmptyArgumentValue,
                AppZipFileUrl = updateApp ? HttpHelper.GetUrl(baseUrl, "api/update/getresource", "key=" + Properties.Settings.Default.AppZipFileName) : this._EmptyArgumentValue
            };

            //Pass back update info to let our console application know if there is an update to process.
            //Also provide the arguments array to pass to the updater executable that the console app will download and run.
            return new UpdateInfo
            {
                UpdateIsAvailable = updateIsAvailable,
                WipeClient = wipeClient,
                ApplicationUpdaterArguments = updateIsAvailable ? updaterArgs.ToArgumentsArray<UpdateExecutableArguments>() : default(string[]),
                ApplicationUpdaterZipUrl = updateIsAvailable ? HttpHelper.GetUrl(baseUrl, "api/update/getresource", "key=" + updaterKey) : string.Empty,
                ApplicationUpdaterZipFilename = updateIsAvailable ? updaterKey : string.Empty,
                ApplicationUpdaterExecutableFilename = updateIsAvailable ? updaterKey.Replace(".zip", ".exe") : string.Empty
            };
        }

        /// <summary>
        /// Save the update results to the database.
        /// </summary>
        public void SaveUpdateStartInfo(UpdateStartInfo info)
        {
            var customerGuid = new Guid(info.CustomerGuid);

            using (var context = new UpdaterEntities())
            {
                var customer = (from c in context.Customers
                                where c.CustomerGuid == customerGuid
                                select c).FirstOrDefault<Customer>();

                if (customer != null)
                {
                    customer.LastUpdateStart = info.StartTime;
                    customer.LastUpdateFinish = null;
                    customer.LastUpdateFinishedWithErrors = null;
                    context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Save the update results to the database.
        /// </summary>
        public void SaveUpdateResults(UpdateResults results)
        {
            var customerGuid = new Guid(results.CustomerGuid);

            //Check for errors
            var updateFinishedWithErrors =
                !results.AppUpdateWasSuccessful;
                //|| results.IsWipeOperation ? !results.RootUpdatesDirectoryWipeWasSuccessful : false;
            
            //Right now, the root update directory wipe usually doesn't report success.
            //This is because the delete of all folders fails because some folders are not empty.
            //The files usually all get deleted, so that is the most important thing for this step.
            //So for now, don't count this as an actual error situation.
            if(results.IsWipeOperation && !results.RootUpdatesDirectoryWipeWasSuccessful)
            {
                //updateFinishedWithErrors = false;
            }                    

            using (var context = new UpdaterEntities())
            {
                var customer = (from c in context.Customers
                            where c.CustomerGuid == customerGuid
                            select c).FirstOrDefault<Customer>();

                if (customer != null)
                {
                    customer.LastUpdateFinish = results.UpdateFinishDate;
                    customer.LastUpdateFinishedWithErrors = updateFinishedWithErrors;
                    customer.AppVersion = results.AppVersion;
                    context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Get a binary resource by key
        /// </summary>
        public byte[] GetUpdateResource(string key)
        {
            byte[] resource = _BlobStore.GetResourceByKey(key);
            return resource;
        }
    }
}