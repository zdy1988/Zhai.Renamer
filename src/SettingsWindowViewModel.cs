using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zhai.Famil.Common.Mvvm;
using Zhai.Famil.Common.Mvvm.Command;
using Zhai.Renamer.Models;
using Zhai.Renamer.Properties;

namespace Zhai.Renamer
{
    internal class SettingsWindowViewModel : ViewModelBase
    {
        private bool isWindowDarked = Properties.Settings.Default.IsWindowDarked;
        public bool IsWindowDarked
        {
            get => isWindowDarked;
            set
            {
                if (Set(() => IsWindowDarked, ref isWindowDarked, value))
                {
                    Properties.Settings.Default.IsWindowDarked = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private bool isWindowTransparency = Properties.Settings.Default.IsWindowTransparency;
        public bool IsWindowTransparency
        {
            get => isWindowTransparency;
            set
            {
                if (Set(() => IsWindowTransparency, ref isWindowTransparency, value))
                {
                    Properties.Settings.Default.IsWindowTransparency = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private bool isSaveLastProfile = Settings.Default.IsSaveLastProfile;
        public bool IsSaveLastProfile
        {
            get { return isSaveLastProfile; }
            set
            {
                if (Set(() => IsSaveLastProfile, ref isSaveLastProfile, value))
                {
                    Settings.Default.IsSaveLastProfile = value;
                    Settings.Default.Save();
                }
            }
        }

        private bool isRegisterAtWindowsContextMenu = Settings.Default.IsRegisterAtWindowsContextMenu;
        public bool IsRegisterAtWindowsContextMenu
        {
            get { return isRegisterAtWindowsContextMenu; }
            set
            {
                if (Set(() => IsRegisterAtWindowsContextMenu, ref isRegisterAtWindowsContextMenu, value))
                {
                    if (value)
                    {
                        value = RenamerSettings.RegistryWindowsContextMenu();
                    }
                    else if(RenamerSettings.IsRegistryWindowsContextMenu())
                    {
                        RenamerSettings.UnRegistryWindowsContextMenu();
                    }

                    Settings.Default.IsRegisterAtWindowsContextMenu = value;
                    Settings.Default.Save();
                }
            }
        }

        private ObservableCollection<RenameCounter> counters = new ObservableCollection<RenameCounter>(RenamerSettings.GetCounters());
        public ObservableCollection<RenameCounter> Counters
        {
            get { return counters; }
            set { Set(() => Counters, ref counters, value); }
        }

        private ObservableCollection<RenameRegexFilter> regexFilters = new ObservableCollection<RenameRegexFilter>(RenamerSettings.GetRegexFilters());
        public ObservableCollection<RenameRegexFilter> RegexFilters
        {
            get { return regexFilters; }
            set { Set(() => RegexFilters, ref regexFilters, value); }
        }

        #region Commands

        public RelayCommand<RenameCounter> ExecuteRemoveCounterCommand => new Lazy<RelayCommand<RenameCounter>>(() => new RelayCommand<RenameCounter>((counter) =>
        {
            counters.Remove(counter);

        })).Value;

        public RelayCommand ExecuteAddCounterCommand => new Lazy<RelayCommand>(() => new RelayCommand(() =>
        {
            counters.Add(new RenameCounter());

        })).Value;

        public RelayCommand<RenameRegexFilter> ExecuteRemoveRegexFilterCommand => new Lazy<RelayCommand<RenameRegexFilter>>(() => new RelayCommand<RenameRegexFilter>((regexFilter) =>
        {
            RegexFilters.Remove(regexFilter);

        })).Value;

        public RelayCommand ExecuteAddRegexFilterCommand => new Lazy<RelayCommand>(() => new RelayCommand(() =>
        {
            RegexFilters.Add(new RenameRegexFilter());

        })).Value;

        #endregion
    }
}
