using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Zhai.Renamer.Model
{
    public class PathNode
    {
        #region Constructors

        public PathNode(FileSystemInfo info)
        {
            if (info is FileInfo file)
            {
                ConstructFileNode(file);
            }
            else if (info is DirectoryInfo directory)
            {
                ConstructDirectoryNode(directory);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public PathNode(string localPath)
        {
            // 检查本地路径中是否有盘符，如果没有将获取正确路径
            //if (!Regex.IsMatch(localPath.Substring(0, 2), "[A-Za-z]:"))
            //{
            //    localPath = PathManager.GetMediaLocalPath(localPath);
            //}

            //url "^[a-zA-z]+://(\w+(-\w+)*)(\.(\w+(-\w+)*))*(\?\S*)?$"　　

            // 检查本地路径合法性
            try
            {
                if (File.Exists(localPath))
                {
                    ConstructFileNode(new FileInfo(localPath));
                }
                else if (Directory.Exists(localPath))
                {
                    ConstructDirectoryNode(new DirectoryInfo(localPath));
                }
                else
                {
                    ConstructLostNode(localPath);
                }
            }
            catch (Exception)
            {
                throw new FileNotFoundException(localPath);
            }
        }

        protected void ConstructFileNode(FileInfo file)
        {
            this.Name = System.IO.Path.GetFileNameWithoutExtension(file.Name);
            this.Extension = file.Extension.ToLowerInvariant();
            this.Path = file.FullName;
            this.ParentPath = file.DirectoryName;
            this.Size = file.Length;
            this.CreationTime = file.CreationTime;
            this.ModifiedTime = file.LastWriteTime;
            this.NativeName = file.Name;
        }

        protected void ConstructDirectoryNode(DirectoryInfo directory)
        {
            this.Name = directory.Name;
            this.Extension = "文件夹";
            this.Path = directory.FullName;
            this.ParentPath = directory.Parent == null ? string.Empty : directory.Parent.FullName;
            this.Size = null;
            this.CreationTime = directory.CreationTime;
            this.ModifiedTime = directory.LastWriteTime;
            this.NativeName = directory.Name;

            this.IsDirectory = true;
        }

        protected void ConstructLostNode(string localPath)
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(localPath);
            string extension = System.IO.Path.GetExtension(localPath);

            this.Name = System.IO.Path.GetFileNameWithoutExtension(localPath);
            this.Extension = String.IsNullOrWhiteSpace(extension) ? "丢失文件夹" : $"丢失文件({extension.ToLower()})";
            this.Path = localPath;
            this.ParentPath = System.IO.Path.GetDirectoryName(localPath);
            this.NativeName = String.IsNullOrWhiteSpace(extension) ? name : $"{name}{extension}";

            this.IsLost = true;
        }

        #endregion

        #region  Properties

        public string Name { get; set; }

        public string Extension { get; set; }

        public string Path { get; set; }

        public string ParentPath { get; set; }

        public long? Size { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime ModifiedTime { get; set; }

        /// <summary>
        /// 文件源名称（用于统一提供文件或文件夹的名称）
        /// </summary>
        public string NativeName { get; set; }

        public bool IsLost { get; private set; }

        public bool IsDirectory { get; private set; }

        public bool IsFile
        {
            get
            {
                return !this.IsDirectory;
            }
        }

        #endregion

        #region Methods

        public IEnumerable<PathNode> GetFiles(string[] searchExtensions, bool isRecursive = false)
        {
            if (Directory.Exists(this.Path))
            {
                if (Filesystem.TryGetFiles(this.Path, isRecursive, searchExtensions, out IEnumerable<FileInfo> files, out _))
                {
                    return files.Select(t => new PathNode(t));
                }
            }

            return default;
        }

        public IEnumerable<PathNode> GetFiles(bool isRecursive = false)
        {
            return GetFiles(null, isRecursive);
        }

        public IEnumerable<PathNode> GetDirectories(string searchPattern, bool isRecursive = false)
        {
            if (Directory.Exists(this.Path))
            {
                if (Filesystem.TryGetDirectories(this.Path, isRecursive, searchPattern, out IEnumerable<DirectoryInfo> directories, out _))
                {
                    return directories.Select(t => new PathNode(t));
                }
            }

            return default;
        }

        public IEnumerable<PathNode> GetDirectories(bool isRecursive = false)
        {
            return GetDirectories(null, isRecursive);
        }

        #endregion
    }

    public static partial class Filesystem
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
                error = $"当前用户对目录 {sourcePath} 没有访问权限...";

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
