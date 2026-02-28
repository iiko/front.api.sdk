using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Resto.Front.Api.SamplePlugin.WpfHelpers
{
    internal sealed class FontStyleToWpfFontStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var apiFontStyle = (Data.View.FontStyle)value;
            return apiFontStyle.HasFlag(Data.View.FontStyle.Italic) ? FontStyles.Italic : FontStyles.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class FontStyleToWpfFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var apiFontStyle = (Data.View.FontStyle)value;
            return apiFontStyle.HasFlag(Data.View.FontStyle.Bold) ? FontWeights.Bold : FontWeights.Regular;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class FontStyleToWpfTextDecorationsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var apiFontStyle = (Data.View.FontStyle)value;
            var textDecorations = new List<string>();
            if (apiFontStyle.HasFlag(Data.View.FontStyle.Underline))
                textDecorations.Add("Underline");
            if (apiFontStyle.HasFlag(Data.View.FontStyle.Strikeout))
                textDecorations.Add("Strikethrough");

            return string.Join(", ", textDecorations);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}