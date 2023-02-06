using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Zhai.Renamer.Core;

namespace Zhai.Renamer.Model
{
    public class RenameProfile
    {
        public string Name { get; set; }
        public string FileName { get; set; }
        public List<RenameModifier> Modifiers { get; set; }

        public RenameProfile(string file)
        {
            Name = Path.GetFileNameWithoutExtension(file);

            FileName = file;

            try
            {
                string json = File.ReadAllText(FileName);

                var modifierSettings = JsonConvert.DeserializeObject<List<RenameModifier>>(json);

                Modifiers = modifierSettings.Select(t => new RenameModifier(t.ModifierKind, t.FirstArgument, t.SecondArgument)).ToList();
            }
            catch
            {
                Modifiers = new List<RenameModifier>();
            }
        }

        public RenameProfile(string name, IList<RenameModifier> list)
        {
            Name = name.Clean();

            FileName = Path.Combine(RenamerSettings.ProfileDataPath, $"{Name}.json");

            Modifiers = new List<RenameModifier>(list);
        }

        public void ReplaceTo(IList<RenameModifier> list)
        {
            if (Modifiers.Count > 0)
            {
                list = list ?? new List<RenameModifier>();

                list.Clear();

                foreach (var modifier in Modifiers)
                {
                    list.Add(modifier);
                }
            }
        }

        public bool TrySave(bool overwrite, out string message)
        {
            if (!overwrite && File.Exists(FileName))
            {
                message = "配置文件名称已存在，请使用其他名称！";

                return false;
            }

            if (String.IsNullOrWhiteSpace(Name))
            {
                message = "配置文件的名称未填写！";

                return false;
            }

            TryDelete(out _);

            try
            {
                var writer = new StreamWriter(FileName);
                writer.Write(GetJson());
                writer.Close();

                message = "";
                return true;
            }
            catch (Exception exception)
            {
                message = exception.Message;
                return false;
            }
        }

        public bool TrySaveAs(string newName, out string message)
        {
            FileName = newName;

            return TrySave(false, out message);
        }

        public bool TryRename(string newName, out string message)
        {
            message = "";

            try
            {
                File.Move(FileName, newName);

                FileName = newName;

                return true;
            }
            catch (Exception exception)
            {
                message = exception.Message;

                return false;
            }
        }

        public bool TryDelete(out string message)
        {
            try
            {
                message = "";

                File.Delete(FileName);

                return true;
            }
            catch (Exception exception)
            {
                message = exception.Message;

                return false;
            }
        }

        private string GetJson()
        {
            return JsonConvert.SerializeObject(Modifiers);
        }
    }
}
