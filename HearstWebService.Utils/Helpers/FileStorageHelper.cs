using System;
using System.IO;
using System.Linq;

namespace HearstWebService.Common.Helpers
{
    public class FileStorageHelper
    {
        private const string FileIdDivider = "_";

        public static Guid CopyFileToStorage(string filePath, string newFileName = null)
        {
            var storagePath = ConfigHelper.Instance.FileStoragePath;

            if (!string.IsNullOrEmpty(storagePath) && !Directory.Exists(storagePath))
            {
                try
                {
                    Directory.CreateDirectory(storagePath);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to create directory: {storagePath}", ex);
                }
            }

            var guid = Guid.NewGuid();
            var inStorageFilePath = "";
            try
            {
                inStorageFilePath = CreateFileStoragePath(storagePath, filePath, guid, newFileName);
                File.Copy(filePath, inStorageFilePath);
                return guid;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to copy \"{filePath}\" to \"{inStorageFilePath}\"", ex);
            }
        }

        public static string FindFile(Guid fileId)
        {
            var storagePath = ConfigHelper.Instance.FileStoragePath;

            if (!string.IsNullOrEmpty(storagePath) && !Directory.Exists(storagePath))
            {
                try
                {
                    Directory.CreateDirectory(storagePath);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to create directory: {storagePath}", ex);
                }
            }

            try
            {
                var di = new DirectoryInfo(storagePath);
                var file = di.GetFiles($"*{fileId.ToString()}*").FirstOrDefault();
                return file == null ? null : file.FullName;
            }
            catch (Exception ex)
            {
                throw new Exception($"File with id=\"{fileId.ToString()}\" not found in \"{storagePath}\"", ex);
            }
        }

        public static string GetClearFileName(string fileNameWithId, Guid fileId)
        {
            return fileNameWithId.Replace(FileIdDivider + fileId.ToString(), "");
        }

        private static string CreateFileStoragePath(string storagePath, string originalFilePath, Guid fileGuid, string newFileName) {
            return Path.Combine(storagePath, 
                Path.GetFileNameWithoutExtension(string.IsNullOrEmpty(newFileName) ? originalFilePath : newFileName) + 
                FileIdDivider + 
                fileGuid.ToString() + 
                Path.GetExtension(originalFilePath));
        }
    }
}
