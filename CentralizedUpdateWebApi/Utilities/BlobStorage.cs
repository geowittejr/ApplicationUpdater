using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;

namespace CentralizedUpdateWebApi.Utilities
{
    //This class downloads App's DLL version, and then the entire zipp'd up app from azure blob storage
    public class BlobStorage
    {
        private CloudBlobContainer _BlobContainer = null;
        private CloudBlobClient _BlobClient = null;

        public BlobStorage()
        {
            //Create a storage account
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Properties.Settings.Default.AppAzureBlobConnectionString);
            _BlobClient = storageAccount.CreateCloudBlobClient();
            _BlobContainer = _BlobClient.GetContainerReference(Properties.Settings.Default.AppAzureContainerName);
        }

        /// <summary>
        /// Get a binary resource by key
        /// </summary>
        /// <param name="key">Key that identifies the resource</param>
        /// <returns></returns>
        public byte[] GetResourceByKey(string key)
        {
            return GetBlobByteArray(key);
        }

        /// <summary>
        /// Get version info for a stored dll by key
        /// </summary>
        /// <param name="key">Key that identifies the stored dll</param>
        /// <returns></returns>
        public FileVersionInfo GetVersionInfoByKey(string key)
        {
            return GetBlobFileVersionInfo(key);
        }

        private FileVersionInfo GetBlobFileVersionInfo(string blobName)
        {            
            byte[] blobFile = GetBlobByteArray(blobName);
            string path = Path.GetTempPath() + "\\" + blobName + "_" + DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss.ff");
            File.WriteAllBytes(path, blobFile);
            return FileVersionInfo.GetVersionInfo(path);
        }

        private byte[] GetBlobByteArray(string blobName)
        {
            //GFW 
            //Ended up changing this method to keep the bytes in memory because when we were saving it to temp path on disk
            //I had trouble getting the downloaded zip file to save without corrupting it.
            CloudBlockBlob blockBlob = _BlobContainer.GetBlockBlobReference(blobName);

            byte[] blobFile = null;
            using(var stream = new MemoryStream())
            {
                blockBlob.DownloadToStream(stream);
                blobFile = stream.ToArray();
            }

            return blobFile;
        }
    }
}