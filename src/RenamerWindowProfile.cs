﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using Zhai.Renamer.Models;
using System.Windows;
using Zhai.Famil.Common.Mvvm;
using Zhai.Famil.Common.Mvvm.Command;

namespace Zhai.Renamer
{
    internal partial class RenamerWindowViewModel : ViewModelBase
    {
        #region Properties

        private ConcurrentObservableCollection<RenameProfile> profileList = new ConcurrentObservableCollection<RenameProfile>();
        public ConcurrentObservableCollection<RenameProfile> ProfileList
        {
            get { return profileList; }
            set { Set(() => ProfileList, ref profileList, value); }
        }

        private RenameProfile selectedProfile;
        public RenameProfile SelectedProfile
        {
            get { return selectedProfile; }
            set
            {
                if (selectedProfile != value)
                {
                    Set(() => SelectedProfile, ref selectedProfile, value);

                    SelectedProfile?.ReplaceTo(ModifierList);

                    IsModifierListFound = ModifierList.Any();

                    ModifierManager.PreviewModifier();
                }
            }
        }

        private string profileName;
        public string ProfileName
        {
            get { return profileName; }
            set { Set(() => ProfileName, ref profileName, value); }
        }

        #endregion

        #region Methods

        private void SaveProfile()
        {
            if (SelectedProfile != null)
            {
                if (ProfileManager.TryUpdateProfile(SelectedProfile, ModifierList, out string message))
                {
                    this.SendNotificationMessage("规则保存完成！");

                    return;
                }

                this.SendNotificationMessage(message);
            }
            else
            {
                SaveAsProfile();
            }
        }

        private void SaveAsProfile()
        {
            if (ModifierList.Any())
            {
                var renamerProfileEditWindow = new RenamerProfileEditWindow();

                if (renamerProfileEditWindow.ShowDialog() == true)
                {
                    if (ProfileManager.TryAddProfile(this.ProfileName, ModifierList, out string message))
                    {
                        this.SendNotificationMessage("规则保存完成！");

                        return;
                    }

                    this.SendNotificationMessage(message);
                }
            }
            else
            {
                this.SendNotificationMessage("未发现待保存规则条目！");
            }
        }

        private void RenameProfile()
        {
            if (SelectedProfile != null)
            {
                var renamerProfileEditWindow = new RenamerProfileEditWindow();

                if (renamerProfileEditWindow.ShowDialog() == true)
                {
                    if (ProfileManager.TryDeleteProfile(this.SelectedProfile, out string message))
                    {
                        if (ProfileManager.TryAddProfile(this.ProfileName, ModifierList, out message))
                        {
                            this.SendNotificationMessage("规则保存完成！");

                            return;
                        }

                        this.SendNotificationMessage(message);
                    }
                }
            }
        }

        private void DeleteProfile()
        {
            if (SelectedProfile != null)
            {
                if (MessageBox.Show("确认删除当前规则？", "确认提示", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                {
                    if (ProfileManager.TryDeleteProfile(SelectedProfile, out string message))
                    {
                        SelectedProfile = null;

                        this.SendNotificationMessage("规则已删除！");

                        return;
                    }

                    this.SendNotificationMessage(message);
                }
            }
        }

        #endregion

        #region Commands

        public RelayCommand<String> ExecuteSaveProfileCommand => new Lazy<RelayCommand<String>>(() => new RelayCommand<String>((action) =>
        {
            switch (action)
            {
                case "Save":

                    SaveProfile();
                    break;

                case "SaveAs":

                    SaveAsProfile();
                    break;

                case "Rename":

                    RenameProfile();
                    break;
            }

        })).Value;

        public RelayCommand ExecuteRemoveProfileCommand => new Lazy<RelayCommand>(() => new RelayCommand(() =>
        {
            DeleteProfile();

        })).Value;

        public RelayCommand<RenameProfile> ExecuteSelectedProfileCommand => new Lazy<RelayCommand<RenameProfile>>(() => new RelayCommand<RenameProfile>((profile) =>
        {
            this.SelectedProfile = profile;

        })).Value;

        public RelayCommand ExecuteCancelSelectedProfileCommand => new Lazy<RelayCommand>(() => new RelayCommand(() =>
        {
            this.SelectedProfile = null;

            ModifierList.Clear();

        })).Value;

        #endregion
    }
}
