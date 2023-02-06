using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Zhai.Famil.Common.Threads;
using Zhai.Famil.Controls;
using Zhai.Renamer.Model;

namespace Zhai.Renamer
{
    public partial class RenamerWindow : GlassesWindow
    {
        internal RenamerWindowViewModel ViewModel => DataContext as RenamerWindowViewModel;

        public RenamerWindow()
        {
            InitializeComponent();

            AllowDrop = true;

            Drop += async (sender, e) =>
            {
                if (ViewModel.IsRenameNodeListLoaded)
                {
                    var filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);

                    var filePathNodeList = filePaths.Select(filePath => new PathNode(filePath)).ToList();

                    await ViewModel.AddRenameNodeToListAsync(filePathNodeList);
                }
            };
        }
    }
}
