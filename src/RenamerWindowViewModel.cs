using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Zhai.Famil.Common.Mvvm;
using Zhai.Famil.Common.Mvvm.Command;
using Zhai.Famil.Common.Threads;
using Zhai.Renamer.Core;
using Zhai.Renamer.Models;
using Zhai.Renamer.Properties;

namespace Zhai.Renamer
{
    internal partial class RenamerWindowViewModel : ViewModelBase
    {
        public RenamerWindowViewModel()
        {
            ProfileManager = new ProfileManager(ProfileList);
            ModifierManager = new ModifierManager(RenameNodeList, ModifierList);
            RenameManager = new RenameManager(RenameNodeList);

            RenameManager.RenameProgress += RenameManager_RenameProgress;
            RenameManager.RenameStarted += RenameManager_RenameStarted;
            RenameManager.RenameBackuped += RenameManager_RenameBackuped;
            RenameManager.RenameFinished += RenameManager_RenameFinished;
            RenameManager.RenameMatched += RenameManager_RenameMatched;
            RenameManager.RenameMatching += RenameManager_RenameMatching;
            ModifierManager.ModifierPreviewing += ModifierManager_ModifierPreviewing;
            ModifierManager.ModifierPreviewed += ModifierManager_ModifierPreviewed;

            QuickModifiers = RenamerSettings.GetQuickModifiers();
            RegexFilters = RenamerSettings.GetRegexFilters();
            RenamerSettings.Load();
        }

        internal ModifierManager ModifierManager { get; }

        internal RenameManager RenameManager { get; }

        internal ProfileManager ProfileManager { get; }

        public List<RenameModifier> ModifierSettings => ModifierManager.ModifierSettings;

        #region Properties

        private bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            set { Set(() => IsBusy, ref isBusy, value); }
        }

        private bool isRenameWorking;
        public bool IsRenameWorking
        {
            get { return isRenameWorking; }
            set { Set(() => IsRenameWorking, ref isRenameWorking, value); }
        }


        private string inputRegex;
        public string InputRegex
        {
            get { return inputRegex; }
            set
            {
                Set(() => InputRegex, ref inputRegex, value);

                MatchFileList();
            }
        }

        private List<RenameRegexFilter> regexFilters;
        public List<RenameRegexFilter> RegexFilters
        {
            get { return regexFilters; }
            set { Set(() => RegexFilters, ref regexFilters, value); }
        }



        private Dictionary<String, List<RenameModifier>> quickModifiers;
        public Dictionary<String, List<RenameModifier>> QuickModifiers
        {
            get { return quickModifiers; }
            set { Set(() => QuickModifiers, ref quickModifiers, value); }
        }

        private bool isModifierSetting;
        public bool IsModifierSetting
        {
            get { return isModifierSetting; }
            set
            {
                Set(() => IsModifierSetting, ref isModifierSetting, value);

                ModifierManager.ResetModifierSettings();
            }
        }

        private bool isModifierListFound = false;
        public bool IsModifierListFound
        {
            get { return isModifierListFound; }
            set { Set(() => IsModifierListFound, ref isModifierListFound, value); }
        }

        private ConcurrentObservableCollection<RenameModifier> modifierList = new ConcurrentObservableCollection<RenameModifier>();
        public ConcurrentObservableCollection<RenameModifier> ModifierList
        {
            get { return modifierList; }
            set { Set(() => ModifierList, ref modifierList, value); }
        }



        private List<PathNode> renameNodeListSnapshot = new List<PathNode>();

        private bool isRenameNodeListLoaded = true;
        public bool IsRenameNodeListLoaded
        {
            get { return isRenameNodeListLoaded; }
            set { Set(() => IsRenameNodeListLoaded, ref isRenameNodeListLoaded, value); }
        }

        private bool isRenameNodeListFound = false;
        public bool IsRenameNodeListFound
        {
            get { return isRenameNodeListFound; }
            set { Set(() => IsRenameNodeListFound, ref isRenameNodeListFound, value); }
        }

        private ConcurrentObservableCollection<RenameNode> renameNodeList = new ConcurrentObservableCollection<RenameNode>();
        public ConcurrentObservableCollection<RenameNode> RenameNodeList
        {
            get { return renameNodeList; }
            set { Set(() => RenameNodeList, ref renameNodeList, value); }
        }



        private string renameNodeListSortKind;
        public string RenameNodeListSortKind
        {
            get { return renameNodeListSortKind; }
            set { Set(() => RenameNodeListSortKind, ref renameNodeListSortKind, value); }
        }

        private bool isRenameNodeListSortByAsc;
        public bool IsRenameNodeListSortByAsc
        {
            get { return isRenameNodeListSortByAsc; }
            set { Set(() => IsRenameNodeListSortByAsc, ref isRenameNodeListSortByAsc, value); }
        }



        private bool isRenameNodeListContainFile = true;
        public bool IsRenameNodeListContainFile
        {
            get { return isRenameNodeListContainFile; }
            set { Set(() => IsRenameNodeListContainFile, ref isRenameNodeListContainFile, value); }
        }

        private bool isRenameNodeListContainDirectory = true;
        public bool IsRenameNodeContainDirectory
        {
            get { return isRenameNodeListContainDirectory; }
            set { Set(() => IsRenameNodeContainDirectory, ref isRenameNodeListContainDirectory, value); }
        }



        private bool isBackuped = false;
        public bool IsBackuped
        {
            get { return isBackuped; }
            set { Set(() => IsBackuped, ref isBackuped, value); }
        }



        private int renameProgressValue = 0;
        public int RenameProgressValue
        {
            get { return renameProgressValue; }
            set { Set(() => RenameProgressValue, ref renameProgressValue, value); }
        }

        private string renamingNodeName;
        public string RenamingNodeName
        {
            get { return renamingNodeName; }
            set { Set(() => RenamingNodeName, ref renamingNodeName, value); }
        }



        private string outputFolderPath;
        public string OutputFolderPath
        {
            get { return outputFolderPath; }
            set { Set(() => OutputFolderPath, ref outputFolderPath, value); }
        }



        private bool isRecursiveLoad = false;
        public bool IsRecursiveLoad
        {
            get { return isRecursiveLoad; }
            set { Set(() => IsRecursiveLoad, ref isRecursiveLoad, value); }
        }



        private bool isDragDropEnabled = false;
        public bool IsDragDropEnabled
        {
            get { return isDragDropEnabled; }
            set { Set(() => IsDragDropEnabled, ref isDragDropEnabled, value); }
        }



        private bool isCountingFileQuantity = false;
        public bool IsCountingFileQuantity
        {
            get { return isCountingFileQuantity; }
            set { Set(() => IsCountingFileQuantity, ref isCountingFileQuantity, value); }
        }

        private bool isCountingCreationTime = false;
        public bool IsCountingCreationTime
        {
            get { return isCountingCreationTime; }
            set { Set(() => IsCountingCreationTime, ref isCountingCreationTime, value); }
        }

        private bool isCountingModifiedTime = false;
        public bool IsCountingModifiedTime
        {
            get { return isCountingModifiedTime; }
            set { Set(() => IsCountingModifiedTime, ref isCountingModifiedTime, value); }
        }

        #endregion

        #region Event Handlers

        private void RenameManager_RenameStarted(object sender, EventArgs e)
        {
            ApplicationDispatcher.InvokeOnUIThread(() =>
            {
                IsRenameWorking = true;

                RenameProgressValue = 0;
            });
        }

        private void RenameManager_RenameBackuped(object sender, EventArgs e)
        {
            IsBackuped = true;
        }

        private void RenameManager_RenameFinished(object sender, RenameManager.RenameFinishedEventArgs e)
        {
            ApplicationDispatcher.InvokeOnUIThread(() =>
            {
                IsRenameWorking = false;

                ModifierManager.PreviewModifier();

                this.SendNotificationMessage("命名已完成！");
            });
        }

        private void RenameManager_RenameProgress(object sender, RenameManager.RenameProgressEventArgs e)
        {
            ApplicationDispatcher.InvokeOnUIThread(() =>
            {
                RenamingNodeName = e.CurrentFile != null ? e.CurrentFile.ModifiedName : "";

                RenameProgressValue = e.Value;

            });
        }

        private void RenameManager_RenameMatched(object sender, EventArgs e)
        {
            ModifierManager.PreviewModifier();
        }

        private void RenameManager_RenameMatching(object sender, EventArgs e)
        {
            IsBusy = true;
        }

        private void ModifierManager_ModifierPreviewed(object sender, EventArgs e)
        {
            IsBusy = false;
        }

        private void ModifierManager_ModifierPreviewing(object sender, EventArgs e)
        {
            IsBusy = true;
        }

        #endregion

        #region Methods

        public override void Cleanup()
        {
            base.Cleanup();

            // 关闭时，保存最后一次规则设置
            if (Settings.Default.IsSaveLastProfile)
            {
                ProfileManager.TrySaveLastProfile(ModifierList, out _);
            }

            // 关闭时，重置设置，删除快照以及其他
            Reset(true);
        }

        public void Reset(bool isDeep = false)
        {
            // 删除快照
            RenameManager.CleanOriginalSnapshot();

            IsRenameWorking = false;

            InputRegex = String.Empty;

            IsModifierSetting = false;
            ModifierList.Clear();
            IsModifierListFound = false;
            RenameNodeList.Clear();
            IsRenameNodeListFound = false;

            if (isDeep)
            {
                IsRenameNodeListContainFile = true;
                IsRenameNodeContainDirectory = true;

                IsRecursiveLoad = false;
            }

            IsBackuped = false;

            RenameProgressValue = 0;
            RenamingNodeName = String.Empty;

            OutputFolderPath = String.Empty;

            SelectedProfile = null;

            IsCountingFileQuantity = false;
            IsCountingCreationTime = false;
            IsCountingModifiedTime = false;
        }

        private CancellationTokenSource loadFileListTokenSource;

        public async Task LoadRenameNodeListAsync(IEnumerable<PathNode> nodes)
        {
            if (!IsRenameNodeListLoaded) return;

            IsRenameNodeListLoaded = false;

            if (this.loadFileListTokenSource != null)
            {
                this.loadFileListTokenSource.Cancel();
                this.loadFileListTokenSource = null;
            }

            var localTokenSource = new CancellationTokenSource();

            this.loadFileListTokenSource = localTokenSource;

            Reset();

            this.renameNodeListSnapshot = nodes.ToList();

            IsRenameNodeListFound = nodes.Any();

            if (IsRenameNodeListFound)
            {
                var task = Task.Factory.StartNew(() =>
                {
                    var renamerNodes = new List<RenameNode>();

                    void AddRenameNode(PathNode node)
                    {
                        localTokenSource.Token.ThrowIfCancellationRequested();

                        if (IsRenameNodeListContainFile && node.IsFile)
                        {
                            renamerNodes.Add(new RenameNode(node.ParentPath, node.NativeName, node.NativeName, true, node));
                        }
                        else if (IsRenameNodeContainDirectory)
                        {
                            renamerNodes.Add(new RenameNode(node.ParentPath, node.NativeName, node.NativeName, false, node));
                        }
                    }

                    try
                    {
                        foreach (var node in nodes)
                        {
                            AddRenameNode(node);

                            if (IsRecursiveLoad && Directory.Exists(node.Path) && !localTokenSource.IsCancellationRequested)
                            {
                                if (IsRenameNodeContainDirectory)
                                {
                                    var directories2 = node.GetDirectories(true);

                                    if (directories2?.Count() > 0)
                                    {
                                        foreach (var dir2 in directories2)
                                        {
                                            AddRenameNode(dir2);
                                        }
                                    }
                                }

                                if (IsRenameNodeListContainFile)
                                {
                                    var files2 = node.GetFiles(true);

                                    if (files2?.Count() > 0)
                                    {
                                        foreach (var file2 in files2)
                                        {
                                            AddRenameNode(file2);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        // 去重
                        renamerNodes = renamerNodes.Where((x, i) => renamerNodes.FindIndex(z => z.FullPath() == x.FullPath()) == i).ToList();

                        // 如果是递归加载，记录其父节点
                        if (IsRecursiveLoad)
                        {
                            foreach (var renamerNode in renamerNodes)
                            {
                                renamerNode.ParentNode = renamerNodes.Find(t => !t.IsFile && t.FullPath().ToLower() == renamerNode.Directory.ToLower());
                            }
                        }
                    }

                    return renamerNodes;

                }, this.loadFileListTokenSource.Token);

                await task.ContinueWith(t =>
                {
                    try
                    {
                        if (t.IsCanceled || localTokenSource.IsCancellationRequested)
                        {

                        }
                        else if (t.IsFaulted)
                        {

                        }
                        else
                        {
                            var renamerNodes = t.Result;

                            foreach (var renamerNode in renamerNodes)
                            {
                                if (localTokenSource.IsCancellationRequested)
                                {
                                    break;
                                }

                                RenameNodeList.Add(renamerNode);
                            }
                        }
                    }
                    finally
                    {
                        if (this.loadFileListTokenSource == localTokenSource)
                        {
                            this.loadFileListTokenSource = null;
                        }

                        localTokenSource.Dispose();
                    }

                });

                RenameManager.TakeOriginalSnapshot();
            }

            IsRenameNodeListLoaded = true;
        }

        public async Task SortFileListAsync(string sortKind)
        {
            if (!IsRenameNodeListLoaded) return;

            IsRenameNodeListLoaded = false;

            if (this.loadFileListTokenSource != null)
            {
                this.loadFileListTokenSource.Cancel();
                this.loadFileListTokenSource = null;
            }

            var localTokenSource = new CancellationTokenSource();

            this.loadFileListTokenSource = localTokenSource;

            if (RenameNodeListSortKind == sortKind)
            {
                IsRenameNodeListSortByAsc = !IsRenameNodeListSortByAsc;
            }
            else
            {
                RenameNodeListSortKind = sortKind;
            }

            if (IsRenameNodeListFound)
            {
                var task = Task.Factory.StartNew(() =>
                {
                    var kind = Enum.Parse<RenamerNodeSortKind>(RenameNodeListSortKind);

                    var list = RenameNodeList.ToList();

                    list.Sort(new RenameNodeSorter(kind, IsRenameNodeListSortByAsc));

                    return list;

                }, this.loadFileListTokenSource.Token);

                await task.ContinueWith(t =>
                {
                    try
                    {
                        if (t.IsCanceled || localTokenSource.IsCancellationRequested)
                        {

                        }
                        else if (t.IsFaulted)
                        {

                        }
                        else
                        {
                            var list = t.Result;

                            RenameNodeList.Clear();

                            foreach (var item in list)
                            {
                                if (localTokenSource.IsCancellationRequested)
                                {
                                    break;
                                }

                                RenameNodeList.Add(item);
                            }
                        }
                    }
                    finally
                    {
                        if (this.loadFileListTokenSource == localTokenSource)
                        {
                            this.loadFileListTokenSource = null;
                        }

                        localTokenSource.Dispose();
                    }

                });

                RenameManager.TakeOriginalSnapshot();
            }

            ModifierManager.PreviewModifier();

            IsRenameNodeListLoaded = true;
        }

        public async Task RefreshFileListAsync()
        {
            if (!IsRenameNodeListLoaded) return;

            IsRenameNodeListLoaded = false;

            if (this.loadFileListTokenSource != null)
            {
                this.loadFileListTokenSource.Cancel();
                this.loadFileListTokenSource = null;
            }

            var localTokenSource = new CancellationTokenSource();

            this.loadFileListTokenSource = localTokenSource;

            if (IsRenameNodeListFound)
            {
                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var list = RenameNodeList.ToList();

                        RenameNodeList.Clear();

                        foreach (var item in list)
                        {
                            if (localTokenSource.IsCancellationRequested)
                            {
                                break;
                            }

                            RenameNodeList.Add(item);
                        }
                    }
                    finally
                    {
                        if (this.loadFileListTokenSource == localTokenSource)
                        {
                            this.loadFileListTokenSource = null;
                        }

                        localTokenSource.Dispose();
                    }

                }, this.loadFileListTokenSource.Token);

                RenameManager.TakeOriginalSnapshot();
            }

            ModifierManager.PreviewModifier();

            IsRenameNodeListLoaded = true;
        }

        public async Task AddRenameNodeToListAsync(IEnumerable<PathNode> nodes)
        {
            if (!IsRenameNodeListLoaded) return;

            if (!RenameNodeList.Any())
            {
                await LoadRenameNodeListAsync(nodes);
            }
            else
            {
                renameNodeListSnapshot.AddRange(nodes);
                // 去重
                renameNodeListSnapshot = renameNodeListSnapshot.Where((x, i) => renameNodeListSnapshot.FindIndex(z => z.Path == x.Path) == i).ToList();

                await LoadRenameNodeListAsync(renameNodeListSnapshot);
            }
        }

        public void MatchFileList()
        {
            RenameManager.PreviewMatch(InputRegex);
        }

        public async Task CountingFileQuantityAsync()
        {
            if (!IsRenameNodeListLoaded || !IsRenameNodeListFound) return;

            IsRenameNodeListLoaded = false;

            await Task.Factory.StartNew(() =>
            {
                foreach (var item in RenameNodeList)
                {
                    item.CountedFileQuantity();
                }
            });

            IsRenameNodeListLoaded = true;
        }

        public async Task CountingCreationTimeAsync()
        {
            if (!IsRenameNodeListLoaded || !IsRenameNodeListFound) return;

            IsRenameNodeListLoaded = false;

            await Task.Factory.StartNew(() =>
            {
                foreach (var item in RenameNodeList)
                {
                    item.CountedAllCreationTime();
                }
            });

            IsRenameNodeListLoaded = true;
        }

        public async Task CountingModifiedTimeAsync()
        {
            if (!IsRenameNodeListLoaded || !IsRenameNodeListFound) return;

            IsRenameNodeListLoaded = false;

            await Task.Factory.StartNew(() =>
            {
                foreach (var item in RenameNodeList)
                {
                    item.CountedAllModifiedTime();
                }
            });

            IsRenameNodeListLoaded = true;
        }

        #endregion

        #region Commands

        public RelayCommand<RenameRegexFilter> ExecuteSetRegexHelperCommand => new Lazy<RelayCommand<RenameRegexFilter>>(() => new RelayCommand<RenameRegexFilter>((regexFilter) =>
        {
            if (regexFilter != null)
            {
                InputRegex = regexFilter.Regex;
            }

            IsBackuped = false;

        })).Value;

        public RelayCommand ExecuteCancelRegexHelperCommand => new Lazy<RelayCommand>(() => new RelayCommand(() =>
        {
            InputRegex = string.Empty;

        }, () => !string.IsNullOrWhiteSpace(InputRegex))).Value;


        public RelayCommand<string> ExecuteAddRenameNodeToListCommand => new Lazy<RelayCommand<string>>(() => new RelayCommand<string>(async (type) =>
        {
            if (type == "File")
            {
                if (Zhai.Famil.Win32.CommonDialog.OpenMultiFileDialog(out string[] filenames))
                {
                    if (filenames.Any())
                    {
                        var filePathNodeList = filenames.Select(fileName => new PathNode(fileName)).ToList();

                        await AddRenameNodeToListAsync(filePathNodeList);
                    }
                }
            }
            else
            {
                if (Zhai.Famil.Win32.CommonDialog.OpenMultiFolderDialog(out string[] filenames))
                {
                    var dirPathNodeList = filenames.Select(fileName => new PathNode(fileName)).ToList();

                    await AddRenameNodeToListAsync(dirPathNodeList);
                }
            }

        }, _ => IsRenameNodeListLoaded)).Value;


        public RelayCommand ExecuteReloadRenameNodeListCommand => new Lazy<RelayCommand>(() => new RelayCommand(async () =>
        {
            await LoadRenameNodeListAsync(renameNodeListSnapshot);

        }, () => IsRenameNodeListLoaded)).Value;

        public RelayCommand ExecuteRefreshedRenameNodeListCommand => new Lazy<RelayCommand>(() => new RelayCommand(async () =>
        {
            await RefreshFileListAsync();

        })).Value;

        public RelayCommand<string> ExecuteSortedRenameNodeListCommand => new Lazy<RelayCommand<string>>(() => new RelayCommand<string>(async (sortKind) =>
        {
            await SortFileListAsync(sortKind);

        }, _ => !IsDragDropEnabled)).Value;

        public RelayCommand<RenameNode> ExecuteRemoveRenameNodeCommand => new Lazy<RelayCommand<RenameNode>>(() => new RelayCommand<RenameNode>((node) =>
        {
            // 移除快照中间节点
            var snapshotRenameNode = renameNodeListSnapshot.Find(t => t.Path.ToLower() == node.FullPath().ToLower());

            if (snapshotRenameNode != null)
            {
                renameNodeListSnapshot.Remove(snapshotRenameNode);
            }

            // 移除节点
            RenameNodeList.Remove(node);

            IsRenameNodeListFound = RenameNodeList.Any();

            IsBackuped = false;

            if (!IsRenameNodeListFound)
            {
                Reset(true);
            }

        })).Value;

        public RelayCommand ExecuteRemoveAllRenameNodeCommand => new Lazy<RelayCommand>(() => new RelayCommand(() =>
        {

            Reset(true);

        })).Value;


        public RelayCommand ExecuteToggleModifierSettingsCommand => new Lazy<RelayCommand>(() => new RelayCommand(() =>
        {

            IsModifierSetting = !IsModifierSetting;

        })).Value;

        public RelayCommand ExecuteRefreshedModifierListCommand => new Lazy<RelayCommand>(() => new RelayCommand(() =>
        {
            var list = ModifierList.ToList();

            ModifierList.Clear();

            ModifierList.AddRange(list);

            ModifierManager.PreviewModifier();

        })).Value;

        public RelayCommand ExecuteAddModifierCommand => new Lazy<RelayCommand>(() => new RelayCommand(() =>
        {
            ModifierManager.AttachModifierSettings(ModifierList);

            IsModifierListFound = ModifierList.Any();

            IsModifierSetting = false;

            IsBackuped = false;

        })).Value;

        public RelayCommand<List<RenameModifier>> ExecuteAdditionalModifierCommand => new Lazy<RelayCommand<List<RenameModifier>>>(() => new RelayCommand<List<RenameModifier>>((modifiers) =>
        {
            modifiers = modifiers.Select(t => new RenameModifier(t.ModifierKind, t.FirstArgument, t.SecondArgument)).ToList();

            ModifierList.AddRange(modifiers);

            IsModifierListFound = ModifierList.Any();

            IsModifierSetting = false;

            IsBackuped = false;

            ModifierManager.PreviewModifier();

        })).Value;

        public RelayCommand<RenameModifier> ExecuteRemoveModifierCommand => new Lazy<RelayCommand<RenameModifier>>(() => new RelayCommand<RenameModifier>((modifier) =>
        {

            ModifierList.Remove(modifier);

            IsModifierListFound = ModifierList.Any();

            IsBackuped = false;

            ModifierManager.PreviewModifier();

        })).Value;

        public RelayCommand ExecuteRemoveAllModifierCommand => new Lazy<RelayCommand>(() => new RelayCommand(() =>
        {
            ModifierList.Clear();

            IsModifierListFound = ModifierList.Any();

            IsBackuped = false;

            ModifierManager.PreviewModifier();

        })).Value;


        public RelayCommand ExecuteRenamedCommand => new Lazy<RelayCommand>(() => new RelayCommand(async () =>
        {
            async Task DoRenameAsync()
            {
                // 执行重命名
                await RenameManager.RenameAsync(true, OutputFolderPath);

                // 执行重命名后取消递归加载
                IsRecursiveLoad = false;

                // 将重命名后的数据作为快照
                renameNodeListSnapshot = RenameNodeList.Select(t => new PathNode(t.FullPath())).Where(t => !t.IsLost).ToList();
            }

            if (ModifierList.Any() && RenameNodeList.Any())
            {
                bool allNamesAreValid = RenameNodeList.All(name => name.IsValidName());

                if (allNamesAreValid)
                {
                    bool allNamesAreUnique = RenameNodeList.Select(x => x.ModifiedName).Count() == RenameNodeList.Select(x => x.ModifiedName).Distinct().Count();

                    if (!allNamesAreUnique)
                    {
                        if (System.Windows.MessageBox.Show("当前待命名文件名存在重复项，它们必须是唯一的，确认要继续吗？", "确认提示", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
                        {
                            await DoRenameAsync();
                        }
                    }
                    else
                    {
                        await DoRenameAsync();
                    }
                }
                else
                {
                    this.SendNotificationMessage("某些文件名无效，请在重命名之前更正它们！");
                }
            }

        })).Value;

        public RelayCommand ExecuteCancelRenamedCommand => new Lazy<RelayCommand>(() => new RelayCommand(() =>
        {
            RenameManager.PauseRenameWork();

            if (System.Windows.MessageBox.Show("确认要终止重命名工作吗？", "确认提示", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.OK)
            {
                RenameManager.CancelRenameWork();

                IsBackuped = false;
            }
            else
            {
                RenameManager.ContinueRenameWork();
            }

        })).Value;

        public RelayCommand ExecuteRevertCommand => new Lazy<RelayCommand>(() => new RelayCommand(async () =>
        {

            IsBackuped = false;

            await RenameManager.RevertAsync();

            MatchFileList();

        })).Value;


        public RelayCommand ExecuteSelectOutputFolderPathCommand => new Lazy<RelayCommand>(() => new RelayCommand(() =>
        {
            if (Zhai.Famil.Win32.CommonDialog.OpenFolderDialog(out string filename))
            {
                var security = new DirectorySecurity(filename, AccessControlSections.Access);

                if (!security.AreAccessRulesProtected)
                {
                    OutputFolderPath = filename;
                }
                else
                {
                    SendNotificationMessage($"软件对路径：“{filename}”没有访问权限！");
                }
            }

        })).Value;


        public RelayCommand ExecuteCountingFileQuantityCommand => new Lazy<RelayCommand>(() => new RelayCommand(async () =>
        {
            if (IsCountingFileQuantity)
            {
                await CountingFileQuantityAsync();
            }
            else
            {
                ModifierSettings[31].IsUsed = false;
                ModifierSettings[32].IsUsed = false;
            }

        })).Value;

        public RelayCommand ExecuteCountingCreationTimeCommand => new Lazy<RelayCommand>(() => new RelayCommand(async () =>
        {
            if (IsCountingCreationTime)
            {
                await CountingCreationTimeAsync();
            }
            else
            {
                ModifierSettings[33].IsUsed = false;
                ModifierSettings[34].IsUsed = false;
            }

        })).Value;

        public RelayCommand ExecuteCountingModifiedTimeCommand => new Lazy<RelayCommand>(() => new RelayCommand(async () =>
        {
            if (IsCountingModifiedTime)
            {
                await CountingModifiedTimeAsync();
            }
            else
            {
                ModifierSettings[35].IsUsed = false;
                ModifierSettings[36].IsUsed = false;
            }

        })).Value;

        #endregion
    }
}
