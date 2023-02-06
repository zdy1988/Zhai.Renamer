using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Zhai.Renamer.Helpers;

namespace Zhai.Renamer.Models
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
}
