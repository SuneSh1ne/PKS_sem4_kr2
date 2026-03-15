using System.Configuration;
using System.Data;
using System.Windows;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PKS_sem4_kr2;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
}

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}