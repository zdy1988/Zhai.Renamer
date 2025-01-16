using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Zhai.Famil.Common.ExtensionMethods;
using Zhai.Famil.Common.Mvvm;
using Zhai.Renamer.Core;

namespace Zhai.Renamer.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RenameModifier : ViewModelBase
    {
        //()
        //(alt, [pos])
        //(text1)
        //(text1, pos)
        //(pos)
        //(text1, text2)
        public RenameModifier(ModifierKind kind, object x = null, object y = null)
        {
            ModifierKind = kind;

            FirstArgument = (x == null ? "" : x.ToString());
            SecondArgument = (y == null ? "" : y.ToString());

            if (ModifierKind == ModifierKind.AppendBefore || ModifierKind == ModifierKind.AppendAfter ||
                ModifierKind == ModifierKind.Regex ||
                ModifierKind == ModifierKind.FormatTimeString)
            {
                text1 = FirstArgument;
            }

            else if (ModifierKind == ModifierKind.AppendAtPosition ||
                     ModifierKind == ModifierKind.AppendFromDirectory || ModifierKind == ModifierKind.AppendFromTextFile ||
                     ModifierKind == ModifierKind.AppendCountingFileQuantity || ModifierKind == ModifierKind.AppendCountingCreationTime || ModifierKind == ModifierKind.AppendCountingModifiedTime ||
                     ModifierKind == ModifierKind.PreserveFromLeftCharacter || ModifierKind == ModifierKind.PreserveFromRightCharacter || ModifierKind == ModifierKind.TrimFromLeftCharacter || ModifierKind == ModifierKind.TrimFromRightCharacter)
            {
                text1 = FirstArgument;
                position1 = y == null || y.ToString() == "" ? 0 : Convert.ToInt32(y);
            }

            else if (ModifierKind == ModifierKind.NumberByDirectories || ModifierKind == ModifierKind.SwapOrder ||
                     ModifierKind == ModifierKind.PreserveFromLeft || ModifierKind == ModifierKind.PreserveFromRight ||
                     ModifierKind == ModifierKind.TrimFromLeft || ModifierKind == ModifierKind.TrimFromRight ||
                     ModifierKind == ModifierKind.ParentDirectory || ModifierKind == ModifierKind.OriginalFileName ||
                     ModifierKind == ModifierKind.KeepTimeString)
            {
                position1 = x == null || x.ToString() == "" ? 0 : Convert.ToInt32(x);
            }

            else if (ModifierKind == ModifierKind.RegexReplace ||
                     ModifierKind == ModifierKind.ReplaceString || ModifierKind == ModifierKind.ReplaceCaseInsensitive)
            {
                text1 = FirstArgument;
                text2 = SecondArgument;
            }

            else if (ModifierKind == ModifierKind.AddNumbering || ModifierKind == ModifierKind.AddMultipleNumbering ||
                     ModifierKind == ModifierKind.Substring || ModifierKind == ModifierKind.RemoveSubstring)
            {
                position1 = x == null || x.ToString() == "" ? 0 : Convert.ToInt32(x);
                position2 = y == null || y.ToString() == "" ? 0 : Convert.ToInt32(y);
            }

            if (ModifierKind == ModifierKind.AppendFromTextFile)
            {
                //Read text file and store its lines
                lines = new List<string>();

                try
                {
                    using (var reader = new StreamReader(text1))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            lines.Add(line);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message, ex);
                }
            }
            else if (ModifierKind == ModifierKind.AppendFromDirectory)
            {
                lines = new List<string>();

                var filePaths = Array.Empty<string>();

                try
                {
                    filePaths = Directory.GetFiles(text1);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message, ex);
                }

                //Array.Sort(filePaths, Comparers);

                foreach (var fileName in filePaths)
                {
                    lines.Add(Path.GetFileName(fileName));
                }
            }
        }

        public RenameModifier(ModifierKind kind)
        {
            ModifierKind = kind;
        }

        public RenameModifier()
        {

        }

        #region Properties

        [JsonProperty]
        [JsonConverter(typeof(StringEnumConverter))]
        public ModifierKind ModifierKind { get; set; }

        private bool isUsed;
        public bool IsUsed
        {
            get
            {
                return isUsed;
            }
            set
            {
                if (Set(() => IsUsed, ref isUsed, value))
                {
                    RenamerModifierUpdated?.Invoke(this, null);
                }
            }
        }

        private string firstArgument;
        [JsonProperty]
        public string FirstArgument
        {
            get { return firstArgument; }
            set
            {
                if (Set(() => FirstArgument, ref firstArgument, value))
                {
                    RenamerModifierUpdated?.Invoke(this, null);
                }
            }
        }

        private string secondArgument;
        [JsonProperty]
        public string SecondArgument
        {
            get { return secondArgument; }
            set
            {
                if (Set(() => SecondArgument, ref secondArgument, value))
                {
                    RenamerModifierUpdated?.Invoke(this, null);
                }
            }
        }

        public string Note
        {
            get
            {
                return String.Format(ModifierExplainer.Explain(ModifierKind), FirstArgument, SecondArgument).Replace("位置  ", "位置 0 ").Replace("  个", " 0 个").Replace("起始于  ", "起始于 1 ");
            }
        }

        #endregion

        #region Methods

        public string GetFunctionName()
        {
            return ModifierKind.ToString().SplitPascalCase();
        }

        //(base 0) index is only required for AddNumbering (and Swap Order) and AppendFromTextFile
        //max is the number of files, it's used to fill with zeros if ModifierType is AddNumbering 
        //previously used file name
        private RenameNode previousFileName = null;

        //file count in the current directory
        private int fileCount = 0;

        public void Reset()
        {
            if (ModifierKind == ModifierKind.AddMultipleNumbering)
            {
                fileCount = 0;
            }
        }

        private readonly int position1;
        private readonly int position2;
        private readonly string text1;
        private readonly string text2;

        private readonly List<string> lines;

        public bool TryApply(RenameNode renamerFile, int index = 0, int max = 0)
        {
            //the input will be the latest output in this case, the modified name of the current file
            string input = renamerFile.ModifiedName;

            try
            {
                switch (ModifierKind)
                {
                    case ModifierKind.Clear:
                        renamerFile.ModifiedName = input.Clear();
                        break;

                    case ModifierKind.AddNumbering:
                        var startIndex = position2 > 0 ? position2 - 1 : position2;
                        var number = (index + 1 + startIndex).CompleteZeros(max + startIndex);
                        renamerFile.ModifiedName = input.AppendAtPosition(number, position1);
                        break;

                    case ModifierKind.NumberByDirectories:
                        if (previousFileName != null && previousFileName.ParentDirectory() != renamerFile.ParentDirectory())
                            fileCount = 0;

                        previousFileName = renamerFile;
                        fileCount++;

                        renamerFile.ModifiedName = input.AppendAtPosition((fileCount).CompleteZeros(max), position1);
                        break;

                    case ModifierKind.AddMultipleNumbering:
                        string numbers = "";
                        max *= position2;

                        for (int i = 0; i < position2; i++)
                        {
                            //fileCount += 1 + i;
                            numbers += (index + 1 + i + fileCount).CompleteZeros(max) + " ";
                        }

                        fileCount += position2 - 1;

                        renamerFile.ModifiedName = input.AppendAtPosition(numbers, position1);
                        break;

                    case ModifierKind.SwapOrder:
                        var order = index;
                        if (index % 2 == 0) order += 2;
                        renamerFile.ModifiedName = input.AppendAtPosition(order.CompleteZeros(max), position1);
                        break;

                    case ModifierKind.AppendBefore:
                        renamerFile.ModifiedName = input.AppendBefore(text1);
                        break;

                    case ModifierKind.AppendAfter:
                        renamerFile.ModifiedName = input.AppendAfter(text1);
                        break;

                    case ModifierKind.AppendFromDirectory:
                    case ModifierKind.AppendFromTextFile:
                        if (index + 1 > lines.Count)
                            renamerFile.ModifiedName = input;
                        renamerFile.ModifiedName = input.AppendAtPosition(lines[index], position1);
                        break;

                    case ModifierKind.AppendAtPosition:
                        renamerFile.ModifiedName = input.AppendAtPosition(text1, position1);
                        break;

                    case ModifierKind.keepChinese:
                        renamerFile.ModifiedName = input.ExtractChinese();
                        break;

                    case ModifierKind.KeepNumeric:
                        renamerFile.ModifiedName = input.ExtractNumeric();
                        break;

                    case ModifierKind.KeepAlphanumeric:
                        renamerFile.ModifiedName = input.ExtractAlphanumeric();
                        break;

                    case ModifierKind.RemoveInvalidCharacters:
                        renamerFile.ModifiedName = input.Clean();
                        break;

                    case ModifierKind.PreserveFromLeft:
                        renamerFile.ModifiedName = input.KeepLeft(position1);
                        break;

                    case ModifierKind.PreserveFromRight:
                        renamerFile.ModifiedName = input.KeepRight(position1);
                        break;

                    case ModifierKind.TrimFromLeft:
                        renamerFile.ModifiedName = input.TrimLeft(position1);
                        break;

                    case ModifierKind.TrimFromRight:
                        renamerFile.ModifiedName = input.TrimRight(position1);
                        break;

                    case ModifierKind.Substring:
                        renamerFile.ModifiedName = position2 != 0 ? input.TrimLeft(position1).KeepLeft(position2) : input.TrimLeft(position1);
                        break;

                    case ModifierKind.RemoveSubstring:
                        var partA = input.KeepLeft(position1);
                        var partB = input.TrimLeft(position1 + position2);
                        renamerFile.ModifiedName = partA + partB;
                        break;

                    case ModifierKind.PreserveFromLeftCharacter:
                        var charIndex = text1.IsNullOrEmpty() ? -1 : input.IndexOf(text1);
                        if (charIndex != -1)
                        {
                            var sub = input.Substring(charIndex + 1);
                            renamerFile.ModifiedName = position1 == 0 ? sub : sub.KeepLeft(position1);
                        }
                        break;

                    case ModifierKind.PreserveFromRightCharacter:
                        charIndex = text1.IsNullOrEmpty() ? -1 : input.IndexOf(text1);
                        if (charIndex != -1)
                        {
                            var sub = input.Substring(0, charIndex);
                            renamerFile.ModifiedName = position1 == 0 ? sub : sub.KeepRight(position1);
                        }
                        break;

                    case ModifierKind.TrimFromLeftCharacter:
                        charIndex = text1.IsNullOrEmpty() ? -1 : input.IndexOf(text1);
                        if (charIndex != -1)
                        {
                            var sub = input.Substring(0, charIndex + 1);
                            var sub2 = input.Substring(charIndex + 1);
                            renamerFile.ModifiedName = position1 == 0 ? sub : sub + sub2.TrimLeft(position1);
                        }
                        break;

                    case ModifierKind.TrimFromRightCharacter:
                        charIndex = text1.IsNullOrEmpty() ? -1 : input.IndexOf(text1);
                        if (charIndex != -1)
                        {
                            var sub = input.Substring(0, charIndex);
                            var sub2 = input.Substring(charIndex);
                            renamerFile.ModifiedName = position1 == 0 ? sub2 : sub.TrimRight(position1) + sub2;
                        }
                        break;

                    case ModifierKind.CapitalizeEachWord:
                        renamerFile.ModifiedName = input.CapitalizeEachWord();
                        break;

                    case ModifierKind.UpperCase:
                        renamerFile.ModifiedName = input.ToUpper();
                        break;

                    case ModifierKind.LowerCase:
                        renamerFile.ModifiedName = input.ToLower();
                        break;

                    case ModifierKind.SentenceCase:
                        renamerFile.ModifiedName = input.SentenceCase();
                        break;

                    case ModifierKind.Regex:
                        try
                        {
                            var match = new Regex(text1).Match(input);
                            if (match.Success)
                                renamerFile.ModifiedName = match.Value;
                        }
                        catch (ArgumentException ex)
                        {
                            renamerFile.ModifiedName = string.Empty;
                            Debug.WriteLine(ex.Message, ex);
                            return false;
                        }
                        break;

                    case ModifierKind.RegexReplace:
                        try
                        {
                            renamerFile.ModifiedName = Regex.Replace(input, text1, text2);
                        }
                        catch (ArgumentException ex)
                        {
                            renamerFile.ModifiedName = string.Empty;
                            Debug.WriteLine(ex.Message, ex);
                            return false;
                        }
                        break;

                    case ModifierKind.ReplaceString:
                        if (text1 == "")
                            renamerFile.ModifiedName = input;
                        renamerFile.ModifiedName = input.Replace(text1, text2);
                        break;

                    case ModifierKind.ReplaceCaseInsensitive:
                        renamerFile.ModifiedName = input.ReplaceInsensitive(text1, text2);
                        break;

                    case ModifierKind.AddExtension:
                        renamerFile.ModifiedName = input + renamerFile.GetExtension();
                        break;

                    case ModifierKind.RemoveExtension:
                        renamerFile.ModifiedName = renamerFile.GetModifiedNameWithoutExtension();
                        break;

                    case ModifierKind.ParentDirectory:
                        renamerFile.ModifiedName = input.AppendAtPosition(renamerFile.ParentDirectory(), position1);
                        break;

                    case ModifierKind.OriginalFileName:
                        renamerFile.ModifiedName = input.AppendAtPosition(renamerFile.OriginalName, position1);
                        break;

                    case ModifierKind.RemoveTimeString:
                        if (input.RegexMatchTime(out string time))
                        {
                            renamerFile.ModifiedName = input.Replace(time, "");
                        }
                        break;

                    case ModifierKind.FormatTimeString:
                        if (input.RegexMatchTime(out string time1))
                        {
                            if (DateTime.TryParse(time1, out DateTime dateTime) && !string.IsNullOrWhiteSpace(text1))
                            {
                                renamerFile.ModifiedName = input.Replace(time1, dateTime.ToString(text1));
                            }
                        }
                        break;

                    case ModifierKind.KeepTimeString:
                        if (input.RegexMatchTime(out string time2))
                        {
                            renamerFile.ModifiedName = input.Replace(time2, "").AppendAtPosition(time2, position1);
                        }
                        break;

                    case ModifierKind.AppendCountingFileQuantityAfter:
                        renamerFile.ModifiedName = input.AppendAfter(renamerFile.FileQuantity);
                        break;

                    case ModifierKind.AppendCountingFileQuantity:
                        renamerFile.ModifiedName = input.AppendAtPosition(renamerFile.FileQuantity, position1);
                        break;

                    case ModifierKind.AppendCountingCreationTimeAfter:
                        renamerFile.ModifiedName = input.AppendAfter(renamerFile.SelectedCreationTime);
                        break;

                    case ModifierKind.AppendCountingCreationTime:
                        renamerFile.ModifiedName = input.AppendAtPosition(renamerFile.SelectedCreationTime, position1);
                        break;

                    case ModifierKind.AppendCountingModifiedTimeAfter:
                        renamerFile.ModifiedName = input.AppendAfter(renamerFile.SelectedModifiedTime);
                        break;

                    case ModifierKind.AppendCountingModifiedTime:
                        renamerFile.ModifiedName = input.AppendAtPosition(renamerFile.SelectedModifiedTime, position1);
                        break;

                    default:
                        renamerFile.ModifiedName = string.Empty;
                        break;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Events

        public event EventHandler RenamerModifierUpdated;

        #endregion
    }
}
