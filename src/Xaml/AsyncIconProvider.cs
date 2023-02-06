using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using Zhai.Famil.Common.Threads;
using Zhai.Renamer.Model;

namespace Zhai.Renamer
{
    public static class AsyncIconProvider
    {
        public static readonly DependencyProperty RenameNodeProperty = DependencyProperty.RegisterAttached("RenameNode", typeof(RenameNode), typeof(AsyncIconProvider), new PropertyMetadata(new PropertyChangedCallback(OnRenamerFilePropertyChangedCallback)));
        public static RenameNode GetRenameNode(DependencyObject obj) => (RenameNode)obj.GetValue(RenameNodeProperty);
        public static void SetRenameNode(DependencyObject obj, object value) => obj.SetValue(RenameNodeProperty, value);

        private static void OnRenamerFilePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (!(sender is System.Windows.Controls.Image image))
            {
                return;
            }

            if (image.IsLoaded)
            {
                return;
            }

            BindSource(image);
        }

        private static void BindSource(System.Windows.Controls.Image image)
        {
            var node = GetRenameNode(image);

            if (node == null) return;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                ApplicationDispatcher.InvokeOnUIThread(() =>
                {
                    try
                    {
                        var fullPath = node.FullPath();

                        if (System.IO.File.Exists(fullPath))
                        {
                            //Icon icon = Icon.ExtractAssociatedIcon(node.FullPath());

                            //image.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle, new Int32Rect(0, 0, icon.Width, icon.Height), BitmapSizeOptions.FromEmptyOptions());
                            image.Source = new BitmapImage(new Uri("pack://application:,,,/Zhai.Renamer;component/Resources/Notes.png"));
                        }
                        else if (System.IO.Directory.Exists(fullPath))
                        {
                            image.Source = new BitmapImage(new Uri("pack://application:,,,/Zhai.Renamer;component/Resources/Folder.png"));
                        }
                        else
                        {
                            image.Source = new BitmapImage(new Uri("pack://application:,,,/Zhai.Renamer;component/Resources/Losted.png"));
                        }
                    }
                    catch
                    {
                        image.Source = new BitmapImage(new Uri("pack://application:,,,/Zhai.Renamer;component/Resources/Losted.png"));
                    }
                });
            });
        }
    }
}
