using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Zhai.Famil.Common.Mvvm;

namespace Zhai.Renamer.Model
{
    public class RenameNode : ViewModelBase
    {
        public RenameNode(string directory, string original, string modified, bool isFile, string extension, long size, DateTime creationTime, DateTime modifiedTime)
        {
            UniqueId = Guid.NewGuid();

            Directory = directory.TrimEnd('\\');

            OriginalName = original;
            ModifiedName = modified;

            IsFile = isFile;
            Extension = extension;
            Size = size;
            CreationTime = creationTime;
            ModifiedTime = modifiedTime;
        }

        public RenameNode(string directory, string original, string modified, bool isFile, PathNode node)
            : this(directory, original, modified, isFile, node.Extension, node.Size ?? 0, node.CreationTime, node.ModifiedTime)
        {

        }

        public RenameNode(RenameNode file, bool swap = false)
        {
            UniqueId = file.UniqueId;

            Directory = file.Directory.TrimEnd('\\');

            OriginalName = file.OriginalName;
            ModifiedName = file.ModifiedName;

            IsFile = file.IsFile;
            Extension = file.Extension;
            Size = file.Size;
            CreationTime = file.CreationTime;
            ModifiedTime = file.ModifiedTime;

            if (swap) Swap();
        }

        #region Properties

        public Guid UniqueId { get; }

        public string Directory { get; set; }

        private string originalName;
        public string OriginalName
        {
            get { return originalName; }
            set { Set(() => OriginalName, ref originalName, value); }
        }

        private string modifiedName;
        public string ModifiedName
        {
            get { return modifiedName; }
            set { Set(() => ModifiedName, ref modifiedName, value); }
        }

        public string Extension { get; }

        public long Size { get; }

        public DateTime CreationTime { get; }

        public DateTime ModifiedTime { get; }

        public bool IsFile { get; }


        private bool isError;
        public bool IsError
        {
            get { return isError; }
            set { Set(() => IsError, ref isError, value); }
        }

        private string errorMessage;
        public string ErrorMessage
        {
            get { return errorMessage; }
            set { Set(() => ErrorMessage, ref errorMessage, value); }
        }


        // 递归加载时，赋值此属性
        public RenameNode ParentNode { get; set; }

        #endregion

        #region Methods

        public bool TryRename(string output = "")
        {
            IsError = false;
            ErrorMessage = string.Empty;

            try
            {
                if (File.Exists(FullPath()))
                {
                    if (String.IsNullOrWhiteSpace(output))
                    {
                        File.Move(FullPath(), FullPathModified());
                    }
                    else
                    {
                        File.Copy(FullPath(), FullPathModified(output));
                    }
                }
                else if (System.IO.Directory.Exists(FullPath()))
                {
                    if (String.IsNullOrWhiteSpace(output))
                    {
                        System.IO.Directory.Move(FullPath(), FullPathModified());
                    }
                }
                else
                {
                    IsError = true;
                    ErrorMessage = $"未知或者无效路径：{FullPath()}";

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                IsError = true;
                ErrorMessage = ex.Message;

                return false;
            }
        }

        public void Reset()
        {
            ModifiedName = OriginalName;
        }

        public bool IsValidName()
        {
            if (ModifiedName == "")
                return false;

            return ModifiedName.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
        }

        public void Swap()
        {
            var temp = OriginalName;
            OriginalName = ModifiedName;
            ModifiedName = temp;
        }

        public string GetExtension()
        {
            return Path.GetExtension(this.OriginalName);
        }

        public string GetModifiedNameWithoutExtension()
        {
            return Path.GetFileNameWithoutExtension(ModifiedName);
        }

        public string ParentDirectory()
        {
            var parts = Directory.Split('\\');

            return parts[^1];
        }

        public string FullPath()
        {
            return $"{Directory}\\{OriginalName}";
        }

        public string FullPathModified(string newDirectory = null)
        {
            if (!string.IsNullOrWhiteSpace(newDirectory))
                return $"{newDirectory}\\{ModifiedName}";

            return $"{Directory}\\{ModifiedName}";
        }

        #endregion

        #region Data Counted Properties

        private List<string> allCreationTime = new List<string>();
        public List<string> AllCreationTime
        {
            get { return allCreationTime; }
            set { Set(() => AllCreationTime, ref allCreationTime, value); }
        }

        private string selectedCreationTime;
        public string SelectedCreationTime
        {
            get { return selectedCreationTime; }
            set { Set(() => SelectedCreationTime, ref selectedCreationTime, value); }
        }

        private List<string> allModifiedTime = new List<string>();
        public List<string> AllModifiedTime
        {
            get { return allModifiedTime; }
            set { Set(() => AllModifiedTime, ref allModifiedTime, value); }
        }

        private string selectedModifiedTime;
        public string SelectedModifiedTime
        {
            get { return selectedModifiedTime; }
            set { Set(() => SelectedModifiedTime, ref selectedModifiedTime, value); }
        }

        private string fileQuantity; //套 图 视频 大小
        public string FileQuantity
        {
            get { return fileQuantity; }
            set { Set(() => FileQuantity, ref fileQuantity, value); }
        }

        #endregion

        #region Data Counted Methods

        string dateFormat = "yyyy.MM.dd";

        public void CountedAllModifiedTime()
        {
            PathNode node = new PathNode(FullPath());

            var times = new List<DateTime> { this.ModifiedTime };

            if (!node.IsFile && !node.IsLost)
            {
                times.AddRange(node.GetFiles().Select(t => t.ModifiedTime));

                AllModifiedTime = times.OrderBy(t => t).Select(t => t.ToString(dateFormat)).Distinct().ToList();
            }
            else
            {
                AllModifiedTime = times.Select(t => t.ToString(dateFormat)).ToList();
            }

            SelectedModifiedTime = AllModifiedTime.FirstOrDefault();
        }

        public void CountedAllCreationTime()
        {
            PathNode node = new PathNode(FullPath());

            var times = new List<DateTime> { this.CreationTime };

            if (!node.IsFile && !node.IsLost)
            {
                times.AddRange(node.GetFiles().Select(t => t.CreationTime));

                AllCreationTime = times.OrderBy(t => t).Select(t => t.ToString(dateFormat)).Distinct().ToList();
            }
            else
            {
                AllCreationTime = times.Select(t => t.ToString(dateFormat)).ToList();
            }

            SelectedCreationTime = AllCreationTime.FirstOrDefault();
        }

        public void CountedFileQuantity()
        {
            PathNode node = new PathNode(FullPath());

            if (node.IsDirectory)
            {
                StringBuilder builder = new StringBuilder();

                void CountedT()
                {
                    var tCount = node.GetDirectories().Count();

                    if (tCount > 0)
                    {
                        builder.Append($"{tCount}T");
                    }
                }

                void CountedQuantity()
                {
                    var counters = RenamerSettings.GetCounters();

                    if (counters.Any())
                    {
                        long length = 0;

                        foreach (var counter in counters)
                        {
                            var files = node.GetFiles(counter.Value, true).ToList();

                            if (files.Any())
                            {
                                builder.Append($"{files.Count}{counter.Key}");

                                foreach (var file in files)
                                {
                                    length += file.Size.HasValue ? file.Size.Value : 0;
                                }
                            }
                        }

                        if (length > 0)
                        {
                            builder.Append($"-{ConvertSize(length)}");
                        }
                    }
                }

                //if (node.IsCollection())
                //{
                //    CountedT();

                //    CountedQuantity();
                //}
                //else if (node.IsDirectory)
                //{
                //    CountedQuantity();
                //}

                CountedQuantity();

                FileQuantity = builder.ToString();
            }
        }

        private string ConvertSize(long fileSize)
        {
            string outFileSize = string.Empty;

            if (fileSize < 1024)
            {
                outFileSize = String.Format("{0}B", fileSize);
            }
            else if (fileSize < 1024 * 1024)
            {
                outFileSize = String.Format("{0}KB", fileSize / 1024);
            }
            else if (fileSize < 1024 * 1024 * 1024)
            {
                outFileSize = String.Format("{0}MB", fileSize / (1024 * 1024));
            }
            else if (fileSize < 1024 * 1024 * 1024 * 1024L)
            {
                outFileSize = String.Format("{0}GB", fileSize / (1024 * 1024 * 1024));
            }
            else
            {
                outFileSize = String.Format("{0}TB", fileSize / (1024 * 1024 * 1024));
            }

            return outFileSize;
        }

        #endregion
    }
}
