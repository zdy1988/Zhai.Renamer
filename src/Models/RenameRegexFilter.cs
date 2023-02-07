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
    internal class RenameRegexFilter : ViewModelBase
    {
        private string name;
        [JsonProperty]
        public string Name
        {
            get { return name; }
            set { Set(() => Name, ref name, value); }
        }

        private string regex;
        [JsonProperty]
        public string Regex
        {
            get { return regex; }
            set { Set(() => Regex, ref regex, value); }
        }
    }
}
