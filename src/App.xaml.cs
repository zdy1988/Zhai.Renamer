using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Zhai.Renamer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static ViewModelLocator ViewModelLocator { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            App.ViewModelLocator = FindResource("Locator") as ViewModelLocator;

            if (e.Args.Any())
            {
                var nodes = e.Args.Select(t => new Models.PathNode(t)).Where(t => !t.IsLost);

                if (nodes.Any())
                {
                    await App.ViewModelLocator.RenamerWindow.AddRenameNodeToListAsync(e.Args.Select(t => new Models.PathNode(t)));
                }
            }

            base.OnStartup(e);
        }
    }
}
