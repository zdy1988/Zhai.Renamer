using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zhai.Renamer
{
    internal class ViewModelLocator
    {
        public RenamerWindowViewModel RenamerWindow { get; } = new RenamerWindowViewModel();

        public SettingsWindowViewModel SettingsWindow { get; } = new SettingsWindowViewModel();
    }
}
