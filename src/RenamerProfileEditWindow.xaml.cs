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
using System.Windows.Shapes;
using Zhai.Famil.Controls;

namespace Zhai.Renamer
{
    /// <summary>
    /// RenamerProfileEditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RenamerProfileEditWindow : TransparentWindow
    {
        internal RenamerWindowViewModel ViewModel => DataContext as RenamerWindowViewModel;

        public RenamerProfileEditWindow()
        {
            InitializeComponent();

            Owner = App.Current.MainWindow;

            ViewModel.ProfileName = String.Empty;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string name = ViewModel.ProfileName;

            if (String.IsNullOrWhiteSpace(name))
            {
                return;
            }

            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
