using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace UpdateExecutableCommon.Utilities
{
    public class ZipUtility
    {
        /// <summary>
        /// Create a zip file from the specified directory path and return it as a byte array
        /// </summary>
        /// <param name="directoryPathToZip">Directory to zip up</param>
        public static byte[] CreateZipFileFromDirectory(string directoryPathToZip)
        {
            if (!Directory.Exists(directoryPathToZip))
                throw new Exception("Directory path to zip up was not found.");

            var zipDirectory = new ZipFile();
            zipDirectory.AddDirectory(directoryPathToZip);

            byte[] zip = null;

            using (var stream = new MemoryStream())
            {
                zipDirectory.Save(stream);
                zip = stream.ToArray();
            }
            
            return zip;
        }

        /// <summary>
        /// Create a zip file from the specified directory path and save it
        /// </summary>
        /// <param name="directoryPathToZip">Directory to zip up</param>
        /// <param name="destinationFilePath">File path to save the resulting zip file</param>
        public static void CreateZipFileFromDirectory(string directoryPathToZip, string destinationFilePath)
        {
            if (!Directory.Exists(directoryPathToZip))
                throw new Exception("Directory path to zip (\"" + directoryPathToZip + "\") was not found.");

            var zipDirectory = new ZipFile();
            zipDirectory.AddDirectory(directoryPathToZip);

            zipDirectory.Save(destinationFilePath);
        }

        /// <summary>
        /// Extract the specified zip file to the destination directory
        /// </summary>
        /// <param name="zipFilePath">Zip file path</param>
        /// <param name="destinationDirectory">Destination directory</param>
        public static void ExtractZipFile(string zipFilePath, string destinationDirectory)
        {
            using (ZipFile zipFile = ZipFile.Read(zipFilePath))
            { 
                foreach (ZipEntry entry in zipFile)
                {
                    entry.Extract(destinationDirectory, ExtractExistingFileAction.OverwriteSilently);
                }
            }
        }
    }
}
