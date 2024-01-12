using System;
using System.Windows.Data;

namespace GmailFilterWpf;

public class DebuggerConverter : IValueConverter
{
    #region IValueConverter Members

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        // Set breakpoint here
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        // Set breakpoint here
        return value;
    }

    #endregion
}