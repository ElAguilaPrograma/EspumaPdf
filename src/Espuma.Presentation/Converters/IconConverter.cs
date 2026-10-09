using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Espuma.Presentation.Converters
{
    /// <summary>Converts an icon key such as "Merge" into the "Icon.Merge" geometry resource.</summary>
    public sealed class IconConverter : IValueConverter
    {
        public static IconConverter Instance { get; } = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var app = Avalonia.Application.Current;
            return value is string key && app is not null && app.TryGetResource("Icon." + key, app.ActualThemeVariant, out var resource)
                ? resource as Geometry
                : null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
