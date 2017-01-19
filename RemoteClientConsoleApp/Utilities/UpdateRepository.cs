using UpdateExecutableCommon.DataModels;
using UpdateExecutableCommon.Utilities;

namespace RemoteClientConsoleApp.Utilities
{
    public class UpdateRepository
    {
        private string _UpdateWebApiUrl = string.Empty;
        private string _CustomerName = string.Empty;

        public UpdateRepository(string updateWebApiUrl, string customerName)
        {
            _UpdateWebApiUrl = updateWebApiUrl;
            _CustomerName = customerName;
        }
        
        public UpdateInfo GetUpdateInfo(ClientInfo clientInfo)
        {            
            UpdateInfo updateInfo = HttpHelper.SendPostRequest<UpdateInfo, ClientInfo>(HttpHelper.GetUrl(_UpdateWebApiUrl, "api/update/postclientinfo", ""), clientInfo);

            Logger.LogInfo("UpdateRepository.GetUpdateInfo()", "Update info for \"" + _CustomerName + "\". UpdateIsAvailable = " + updateInfo.UpdateIsAvailable.ToString() + ".", _CustomerName);

            return updateInfo;
        }

        public byte[] GetUpdaterZipFile(string updaterZipFileUrl)
        {
            byte[] zipFile = HttpHelper.DownloadBinaryFile(updaterZipFileUrl);

            string zipSize = zipFile != null ? zipFile.Length.ToString() + " bytes" : "0 bytes";
            Logger.LogInfo("UpdateRepository.GetUpdaterZipFile()", "Downloaded zip file (" + zipSize + ") from \"" + updaterZipFileUrl + "\"", _CustomerName);

            return zipFile;
        }
    }
}
