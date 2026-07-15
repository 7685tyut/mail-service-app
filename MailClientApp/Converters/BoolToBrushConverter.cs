using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MailClientApp.Converters
{
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                var param = parameter as string;
                
                switch (param)
                {
                    case "Read":
                        return boolValue ? Brushes.Gray : Brushes.Black;
                    case "Subject":
                        return boolValue ? Brushes.Gray : Brushes.Black;
                    case "Flag":
                        return boolValue ? Brushes.Gold : Brushes.Transparent;
                    case "Default":
                        return boolValue ? Brushes.LimeGreen : Brushes.Gray;
                    default:
                        return boolValue ? Brushes.Black : Brushes.Gray;
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility == Visibility.Visible;
            }
            return false;
        }
    }

    public class LogLevelToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Models.LogLevel level)
            {
                return level switch
                {
                    Models.LogLevel.Debug => Brushes.Gray,
                    Models.LogLevel.Info => Brushes.Blue,
                    Models.LogLevel.Warning => Brushes.Orange,
                    Models.LogLevel.Error => Brushes.Red,
                    Models.LogLevel.Critical => Brushes.DarkRed,
                    _ => Brushes.Black
                };
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PreviewTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text)
            {
                if (string.IsNullOrEmpty(text))
                    return string.Empty;
                
                // Remove HTML tags
                text = System.Text.RegularExpressions.Regex.Replace(text, "<[^>]*>", " ");
                text = System.Text.RegularExpressions.Regex.Replace(text, "\s+", " ").Trim();
                
                // Limit length
                int maxLength = 100;
                if (parameter is int paramLength)
                    maxLength = paramLength;
                
                return text.Length > maxLength ? text.Substring(0, maxLength) + "..." : text;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DateToFriendlyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTimeOffset date)
            {
                var now = DateTimeOffset.Now;
                
                if (date.Date == now.Date)
                    return date.ToString("HH:mm");
                else if (date.Date == now.Date.AddDays(-1))
                    return "Вчера, " + date.ToString("HH:mm");
                else if (date.Date > now.Date.AddDays(-7))
                    return date.ToString("ddd, HH:mm");
                else
                    return date.ToString("dd.MM.yyyy");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}