using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zhai.Famil.Common.Mvvm;
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

        private bool isAddToWindowsContextMenu;
        public bool IsAddToWindowsContextMenu
        {
            get { return isAddToWindowsContextMenu; }
            set
            {
                if (Set(() => IsAddToWindowsContextMenu, ref isAddToWindowsContextMenu, value))
                {
                    Settings.Default.IsAddToWindowsContextMenu = value;
                    Settings.Default.Save();

                    if (value)
                    {
                        RenamerSettings.RegistryRightClickContextMenu();
                    }
                    else
                    {
                        RenamerSettings.UnRegistryRightClickContextMenu();
                    }
                }
            }
        }
    }
}
