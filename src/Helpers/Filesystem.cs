﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;

namespace Zhai.Renamer.Helpers
{
    internal static class Filesystem
    {
        public static bool TestDirectory(string sourcePath, Action<DirectoryInfo> action, out string error)
        {
            error = null;

            try
            {
                action.Invoke(new DirectoryInfo(sourcePath));

                return true;
            }
            catch (DirectoryNotFoundException)
            {
                error = "用户访问的路径无效...";

                return false;
            }
            catch (SecurityException)
            {
                error = $"当前用户对文件夹 {sourcePath} 没有访问权限...";

                return false;
            }
            catch (Exception e)
            {
                error = e.Message;

                return false;
            }
        }

        public static bool TryGetFiles(string sourcePath, bool isRecursive, string[] searchExtensions, out IEnumerable<FileInfo> files, out string error)
        {
            IEnumerable<FileInfo> list = null;

            var isGot = TestDirectory(sourcePath, di =>
            {
                var option = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                list = di.EnumerateFiles("*", option).AsParallel().Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden | FileAttributes.System | FileAttributes.Temporary));

                if (searchExtensions?.Length > 0)
                {
                    list = list.Where(f => !string.IsNullOrWhiteSpace(f.Extension) && searchExtensions.Contains(f.Extension.Substring(1).ToLower()));
                }

            }, out error);

            files = list;

            return isGot;
        }

        public static bool TryGetFiles(string sourcePath, out IEnumerable<FileInfo> files, out string error)
        {
            return TryGetFiles(sourcePath, false, null, out files, out error);
        }

        public static bool TryGetFiles(string sourcePath, bool isRecursive, out IEnumerable<FileInfo> files, out string error)
        {
            return TryGetFiles(sourcePath, isRecursive, null, out files, out error);
        }

        public static bool TryGetFiles(string sourcePath, string[] searchExtensions, out IEnumerable<FileInfo> files, out string error)
        {
            return TryGetFiles(sourcePath, false, searchExtensions, out files, out error);
        }

        public static bool TryGetDirectories(string sourcePath, bool isRecursive, string searchPattern, out IEnumerable<DirectoryInfo> directories, out string error)
        {
            IEnumerable<DirectoryInfo> list = null;

            var isGot = TestDirectory(sourcePath, di =>
            {
                var option = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                searchPattern = String.IsNullOrWhiteSpace(searchPattern) ? "*" : searchPattern;

                list = di.EnumerateDirectories(searchPattern, option).AsParallel().Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden | FileAttributes.System | FileAttributes.Temporary));

            }, out error);

            directories = list;

            return isGot;
        }

        public static bool TryGetDirectories(string sourcePath, out IEnumerable<DirectoryInfo> directories, out string error)
        {
            return TryGetDirectories(sourcePath, false, null, out directories, out error);
        }

        public static bool TryGetDirectories(string sourcePath, bool isRecursive, out IEnumerable<DirectoryInfo> directories, out string error)
        {
            return TryGetDirectories(sourcePath, isRecursive, null, out directories, out error);
        }

        public static bool TryGetDirectories(string sourcePath, string searchPattern, out IEnumerable<DirectoryInfo> directories, out string error)
        {
            return TryGetDirectories(sourcePath, false, searchPattern, out directories, out error);
        }

        public static bool TryCreateDirectory(string sourcePath, string directoryName, out string targetPath, out string error)
        {
            targetPath = Path.Combine(sourcePath, directoryName);

            return TryCreateDirectory(targetPath, out error);
        }

        public static bool TryCreateDirectory(string targetPath, out string error)
        {
            error = null;

            try
            {
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }

                return true;
            }
            catch (Exception e)
            {
                error = e.Message;

                return false;
            }
        }

        public static bool TryDeleteDirectory(string sourcePath, out string error)
        {
            var isDeleted = TestDirectory(sourcePath, directory =>
            {

                try
                {
                    Directory.Delete(sourcePath, true);
                }
                catch (IOException)
                {
                    Directory.Delete(sourcePath, true);
                }
                catch (UnauthorizedAccessException)
                {
                    Directory.Delete(sourcePath, true);
                }


            }, out error);

            return isDeleted;
        }

        public static bool TryCleanDirectory(string sourcePath, out string error)
        {
            var isCleaned = TestDirectory(sourcePath, directory =>
            {
                var list = directory.EnumerateFileSystemInfos();

                foreach (var item in list)
                {
                    if (item is DirectoryInfo dir)
                    {
                        dir.Delete(true);
                    }
                    else
                    {
                        item.Delete();
                    }
                }

            }, out error);

            return isCleaned;
        }

        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            if (source.FullName.ToLower() == target.FullName.ToLower())
            {
                return;
            }

            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it's new directory.
            foreach (FileInfo file in source.GetFiles())
            {
                file.CopyTo(Path.Combine(target.ToString(), file.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        public static void CopyAll(string sourcePath, string targetPath)
        {
            CopyAll(new DirectoryInfo(sourcePath), new DirectoryInfo(targetPath));
        }
    }
}
