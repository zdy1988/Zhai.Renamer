using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zhai.Famil.Common.Mvvm;

namespace Zhai.Renamer.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class RenameCounter : ViewModelBase
    {
        private string flag;
        [JsonProperty]
        public string Flag
        {
            get { return flag; }
            set  {  Set(() => Flag, ref flag, value);  }
        }

        private string formats;
        [JsonProperty]
        public string Formats
        {
            get { return formats; }
            set  {  Set(() => Formats, ref formats, value);   }
        }

        private bool isUsed;
        [JsonProperty]
        public bool IsUsed
        {
            get { return isUsed; }
            set { Set(() => IsUsed, ref isUsed, value); }
        }

        private bool isRecursived;
        [JsonProperty]
        public bool IsRecursived
        {
            get { return isRecursived; }
            set { Set(() => IsRecursived, ref isRecursived, value); }
        }

        public string DIRFormat
        {
            get
            {
                return "*DIR*";
            }
        }

        public bool IsDIR
        {
            get
            {
                return Formats == DIRFormat;
            }
        }
    }
}
