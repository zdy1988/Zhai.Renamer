using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zhai.Renamer.Models;

namespace Zhai.Renamer.Core
{
    internal class RenameManager
    {
        private IList<RenameNode> RenameNodeList { get; }

        private List<RenameNode> RenameNodeListPrevSnapshot { get; }

        private List<RenameNode> RenameNodeListOriginalSnapshot { get; }

        internal RenameManager(IList<RenameNode> renameNodes)
        {
            RenameNodeList = renameNodes ?? new List<RenameNode>();

            RenameNodeListPrevSnapshot = new List<RenameNode>();

            RenameNodeListOriginalSnapshot = new List<RenameNode>();

            InitMatchTimer();
        }

        private volatile bool isRenameWorkPaused = false;

        private bool isRenameWorkCanceled = false;

        #region Methods

        internal Task RenameAsync(bool isBackup, string outputPath = "")
        {
            RenameStarted?.Invoke(this, null);

            bool isOutputToOtherDirectory = !String.IsNullOrWhiteSpace(outputPath);

            if (!isOutputToOtherDirectory && isBackup)
            {
                //不输出到其他目录时，备份命名
                TakePrevSnapshot();
            }

            return Task.Run(async () =>
            {
                double step = 100.0 / RenameNodeList.Count;

                double sum = 0;

                async Task DoRename(RenameNode renameNode)
                {
                    while (isRenameWorkPaused)
                    {
                        await Task.Delay(100);
                    }

                    if (isRenameWorkCanceled)
                    {
                        throw new TaskCanceledException();
                    }

                    if (!renameNode.TryRename(outputPath))
                    {
                        
                    }
                    else if (!isOutputToOtherDirectory)
                    {
                        renameNode.OriginalName = renameNode.ModifiedName;
                    }

                    RenameProgress?.Invoke(this, new RenameProgressEventArgs((int)(sum += step), renameNode));
                }

                try
                {
                    /*
                        最先重命名地址最短的，因为短地址可能会是长地址的上级目录
                        重命名前先查看父节点地址是否改变，如果改变则更新当前目录地址
                    */

                    var shortestPathFirstOrderedNodes = RenameNodeList.OrderBy(t => t.FullPath().Length).AsEnumerable();

                    foreach (var node in shortestPathFirstOrderedNodes)
                    {
                        if (node.ParentNode != null && node.ParentNode.FullPath() != node.Directory)
                        {
                            node.Directory = node.ParentNode.FullPath();
                        }

                        await DoRename(node);

                        if (!node.IsFile)
                        {
                            var retryCount = 0;

                            // 确保文件夹重命名成功，但只重试 10 次
                            while (retryCount <= 10 || !Directory.Exists(node.FullPath()))
                            {
                                await Task.Delay(100);

                                retryCount++;
                            }
                        }
                    }
                }
                catch { }

                isRenameWorkCanceled = false;

                RenameProgress?.Invoke(this, new RenameProgressEventArgs(100));

                RenameFinished?.Invoke(this, new RenameFinishedEventArgs());
            });
        }

        internal void PauseRenameWork()
        {
            isRenameWorkPaused = true;
        }

        internal void ContinueRenameWork()
        {
            isRenameWorkPaused = false;
        }

        internal void CancelRenameWork()
        {
            isRenameWorkCanceled = true;

            isRenameWorkPaused = false;

            RenameCanceled?.Invoke(this, null);
        }

        internal void TakePrevSnapshot()
        {
            RenameNodeListPrevSnapshot.Clear();

            foreach (var file in RenameNodeList)
            {
                RenameNodeListPrevSnapshot.Add(new RenameNode(file, true));
            }

            RenameBackuped?.Invoke(this, null);
        }

        internal Task RevertAsync()
        {
            RenameNodeList.Clear();

            foreach (var prevFile in RenameNodeListPrevSnapshot)
            {
                var originalFile = RenameNodeListOriginalSnapshot.Find(t => t.UniqueId == prevFile.UniqueId);

                if (originalFile != null)
                {
                    originalFile.OriginalName = prevFile.OriginalName;
                    originalFile.ModifiedName = prevFile.ModifiedName;

                    RenameNodeList.Add(originalFile);
                }
                else
                {
                    RenameNodeList.Add(prevFile);
                }
            }

            return RenameAsync(false);
        }

        internal void TakeOriginalSnapshot()
        {
            RenameNodeListOriginalSnapshot.Clear();

            foreach (var node in RenameNodeList)
            {
                RenameNodeListOriginalSnapshot.Add(node);
            }
        }

        internal void CleanOriginalSnapshot()
        {
            RenameNodeListOriginalSnapshot.Clear();
        }

        #endregion

        #region Match

        private string matchingRegex;

        private System.Timers.Timer matchTimer;

        private CancellationTokenSource matchTimerTokenSource;

        private void InitMatchTimer()
        {
            matchTimer = new System.Timers.Timer(500);

            matchTimer.Elapsed += (sender, e) =>
            {
                if (this.matchTimerTokenSource != null)
                {
                    this.matchTimerTokenSource.Cancel();
                    this.matchTimerTokenSource = null;
                }

                var localTokenSource = new CancellationTokenSource();

                this.matchTimerTokenSource = localTokenSource;

                Task.Factory.StartNew(() => Match(matchingRegex, localTokenSource), this.matchTimerTokenSource.Token).Wait();

                matchTimer.Enabled = false;

                RenameMatched?.Invoke(this, null);
            };
        }

        private void Match(string inputRegex, CancellationTokenSource localTokenSource)
        {
            // 第一次加载数据时，将原始数据快照保存
            if (!RenameNodeListOriginalSnapshot.Any())
            {
                TakeOriginalSnapshot();
            }

            RenameNodeList.Clear();

            try
            {
                foreach (var node in RenameNodeListOriginalSnapshot)
                {
                    localTokenSource.Token.ThrowIfCancellationRequested();

                    if (!String.IsNullOrWhiteSpace(inputRegex))
                    {
                        if (ModifierExecutor.ValidPattern(inputRegex) && node.OriginalName.RegexMatch(inputRegex))
                        {
                            RenameNodeList.Add(node);
                        }
                    }
                    else
                    {
                        RenameNodeList.Add(node);
                    }
                }
            }
            finally
            {
                if (this.matchTimerTokenSource == localTokenSource)
                {
                    this.matchTimerTokenSource = null;
                }

                localTokenSource.Dispose();
            }
        }

        internal void PreviewMatch(string inputRegex)
        {
            if (!matchTimer.Enabled)
            {
                matchTimer.Enabled = true;
            }
            else
            {
                matchTimer.Enabled = false;
                matchTimer.Enabled = true;
            }

            matchingRegex = inputRegex;

            RenameMatching?.Invoke(this, null);
        }

        #endregion

        #region Events

        public event EventHandler RenameStarted;

        public event EventHandler RenameBackuped;

        public event EventHandler RenameCanceled;

        public event EventHandler<RenameFinishedEventArgs> RenameFinished;
        public class RenameFinishedEventArgs : EventArgs
        {
            public RenameFinishedEventArgs()
            {

            }
        }

        public event EventHandler<RenameProgressEventArgs> RenameProgress;
        public class RenameProgressEventArgs : EventArgs
        {
            public int Max { get; } 

            public int Value { get; }

            public RenameNode CurrentFile { get; }

            public RenameProgressEventArgs(int value)
            {
                Max = 100;
                Value = value;
            }

            public RenameProgressEventArgs(int value, RenameNode currentFile)
                : this(value)
            {
                CurrentFile = currentFile;
            }
        }

        public event EventHandler RenameMatching;

        public event EventHandler RenameMatched;

        #endregion
    }
}
