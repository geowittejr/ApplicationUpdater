using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AppCommon.IO
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Delete all files and folders in this directory.
        /// </summary>
        /// <param name="directory"></param>
        public static void DeleteChildFilesAndFolders(this DirectoryInfo directory)
        {
            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                dir.DeleteChildFilesAndFolders();
                dir.Delete();
            }
        }

        /// <summary>
        /// Delete all files and folders in this directory. If multiple exceptions occur, continue processing and throw only one exception when done.
        /// </summary>
        /// <param name="directory"></param>
        public static void DeleteChildFilesAndFoldersAndCombineExceptions(this DirectoryInfo directory)
        {
            List<Exception> errors = new List<Exception>();
            directory.DeleteChildFilesAndFoldersAndTrackErrors(ref errors);
            if(errors.Count > 0)
            {
                var ex = new Exception("Exception(s) occurred during the delete process. Error count: " + errors.Count.ToString() + ". First exception: " + errors.First().Message);
                throw ex;
            }
        }

        /// <summary>
        /// Delete all files and folders in this directory.
        /// This private implementation is used by the public method.
        /// </summary>
        private static void DeleteChildFilesAndFoldersAndTrackErrors(this DirectoryInfo directory, ref List<Exception> errors)
        {
            if (errors == null)
                errors = new List<Exception>();

            foreach (FileInfo file in directory.GetFiles())
            {
                try
                {
                    file.Delete();
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }

            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                try
                {
                    dir.DeleteChildFilesAndFoldersAndTrackErrors(ref errors);
                    dir.Delete();
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }
        }

        /// <summary>
        /// Copy all files and folders to the destination directory
        /// </summary>
        public static void CopyFilesAndFoldersToDirectory(this DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory)
        {
            sourceDirectory.CopyFilesAndFolders(destinationDirectory);
        }

        /// <summary>
        /// Copy all files and folders from the source directory to the destination directory.
        /// This is the private recursive method called by the public method.
        /// </summary>
        /// <param name="sourceDirectory"></param>
        /// <param name="destinationDirectory"></param>
        private static void CopyFilesAndFolders(this DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory)
        {
            if (!sourceDirectory.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirectory.FullName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!destinationDirectory.Exists)
            {
                destinationDirectory = Directory.CreateDirectory(destinationDirectory.FullName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = sourceDirectory.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destinationDirectory.FullName, file.Name);
                file.CopyTo(temppath, true);
            }

            foreach (DirectoryInfo subDirectory in sourceDirectory.GetDirectories())
            {
                DirectoryInfo sub = new DirectoryInfo(Path.Combine(destinationDirectory.FullName, subDirectory.Name));
                subDirectory.CopyFilesAndFolders(sub);
            }
        }
    }
}
