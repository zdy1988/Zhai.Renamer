using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using Zhai.Famil.Converters;
using Zhai.Renamer.Model;

namespace Zhai.Renamer.Converters
{
    internal class RenameNodeFullPathConverter : ConverterMarkupExtensionBase<RenameNodeFullPathConverter>, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RenameNode node)
            {
                return node.FullPath();
            }

            return "Unknown Path ...";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
