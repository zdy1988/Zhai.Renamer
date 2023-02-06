using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Documents;
using Zhai.Renamer.Model;

namespace Zhai.Renamer.Core
{
    internal class ProfileManager
    {
        private IList<RenameProfile> ProfileList { get; }

        private string LastProfileName = "上次使用过的规则";

        internal ProfileManager(IList<RenameProfile> profiles)
        {
            if (!Directory.Exists(RenamerSettings.ProfileDataPath))
                Directory.CreateDirectory(RenamerSettings.ProfileDataPath);

            // load profiles
            ProfileList = profiles;

            string[] files = Directory.GetFiles(RenamerSettings.ProfileDataPath, "*.json");

            ProfileList.Clear();

            if (files.Any())
            {
                var list = files.Select(file => new RenameProfile(file)).OrderBy(file => file.Name);

                foreach (var item in list)
                {
                    ProfileList.Add(item);
                }

                // 置顶上次使用的规则
                var lastProfileName = ProfileList.FirstOrDefault(t => t.Name == LastProfileName);

                if (lastProfileName != null)
                {
                    var index = ProfileList.IndexOf(lastProfileName);

                    ProfileList.Insert(0, lastProfileName);
                    ProfileList.RemoveAt(index + 1);
                }
            }
        }

        #region Methods

        internal bool TrySaveProfile(int index, bool overwrite, out string message)
        {
            if (!ProfileList[index].TrySave(overwrite, out message))
            {
                return false;
            }

            return true;
        }

        internal bool TrySaveProfile(RenameProfile profile, bool overwrite, out string message)
        {
            var index = FindProfile(profile.Name);

            if (index != -1)
            {
                return TrySaveProfile(index, overwrite, out message);
            }

            message = "配置文件丢失！";

            return false;
        }

        internal bool TryUpdateProfile(int index, IList<RenameModifier> modifiers, out string message)
        {
            ProfileList[index].Modifiers.Clear();

            ProfileList[index].Modifiers = new List<RenameModifier>(modifiers);

            return TrySaveProfile(index, true, out message);
        }

        internal bool TryUpdateProfile(RenameProfile profile, IList<RenameModifier> modifiers, out string message)
        {
            var index = FindProfile(profile.Name);

            if (index != -1)
            {
                return TryUpdateProfile(index, modifiers, out message);
            }

            message = "配置文件丢失！";

            return false;
        }

        internal bool TryAddProfile(string name, IList<RenameModifier> modifiers, out string message)
        {
            ProfileList.Add(new RenameProfile(name, modifiers));

            int index = ProfileList.Count - 1;

            if (!TrySaveProfile(index, false, out message))
            {
                ProfileList.RemoveAt(index);

                return false;
            }

            return true;
        }

        internal bool TryDeleteProfile(int index, out string message)
        {
            if (ProfileList[index].TryDelete(out message))
            {
                ProfileList.RemoveAt(index);

                return true;
            }

            return false;
        }

        internal bool TryDeleteProfile(RenameProfile profile, out string message)
        {
            var index = FindProfile(profile.Name);

            if (index != -1)
            {
                return TryDeleteProfile(index, out message);
            }

            message = "配置文件丢失！";

            return false;
        }

        internal int FindProfile(string name)
        {
            for (int i = 0; i < ProfileList.Count; i++)
            {
                if (name == ProfileList[i].Name) return i;
            }

            return -1;
        }

        internal bool TrySaveLastProfile(IList<RenameModifier> modifiers, out string message)
        {
            if (modifiers.Count == 0)
            {
                message = "";

                return false;
            }

            var index = FindProfile(LastProfileName);

            if (index == -1)
            {
                return TryAddProfile(LastProfileName, modifiers, out message);
            }

            ProfileList[index].Modifiers.Clear();

            ProfileList[index].Modifiers = new List<RenameModifier>(modifiers);

            return ProfileList[index].TrySave(true, out message);
        }

        #endregion
    }
}
