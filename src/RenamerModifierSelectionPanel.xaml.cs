using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Zhai.Renamer
{
    /// <summary>
    /// RenamerFilterSetting.xaml 的交互逻辑
    /// </summary>
    public partial class RenamerModifierSelectionPanel : UserControl
    {
        internal RenamerWindowViewModel ViewModel => DataContext as RenamerWindowViewModel;

        public RenamerModifierSelectionPanel()
        {
            InitializeComponent();
        }

        private void AppendFromDirectoryFilesButton_Click(object sender, RoutedEventArgs e)
        {
            if (Zhai.Famil.Win32.CommonDialog.OpenFolderDialog(out string filename))
            {
                ViewModel.ModifierSettings[33].FirstArgument = filename;
            }
        }

        private void AppendFromTextFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (Zhai.Famil.Win32.CommonDialog.OpenFileDialog("文本文件(*.txt)|*.txt", out string filename))
            {
                ViewModel.ModifierSettings[34].FirstArgument = filename;
            }
        }
    }
}
