using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Espuma.Presentation.Controls
{
    /// <summary>Layout shared by every tool: compact header (icon, title, description) above the content.</summary>
    public sealed class ToolPage : ContentControl
    {
        public static readonly StyledProperty<Geometry?> IconProperty =
            AvaloniaProperty.Register<ToolPage, Geometry?>(nameof(Icon));

        public static readonly StyledProperty<string?> TitleProperty =
            AvaloniaProperty.Register<ToolPage, string?>(nameof(Title));

        public static readonly StyledProperty<string?> DescriptionProperty =
            AvaloniaProperty.Register<ToolPage, string?>(nameof(Description));

        public static readonly StyledProperty<object?> ActionsProperty =
            AvaloniaProperty.Register<ToolPage, object?>(nameof(Actions));

        public Geometry? Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public string? Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string? Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        /// <summary>Content shown at the right of the header, usually the main action button.</summary>
        public object? Actions
        {
            get => GetValue(ActionsProperty);
            set => SetValue(ActionsProperty, value);
        }
    }
}
