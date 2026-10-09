using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace Espuma.Presentation.Controls
{
    /// <summary>Draws a 24x24 stroke icon geometry scaled to <see cref="Size"/>.</summary>
    public sealed class Icon : Control
    {
        private const double DesignSize = 24;

        public static readonly StyledProperty<Geometry?> DataProperty =
            AvaloniaProperty.Register<Icon, Geometry?>(nameof(Data));

        public static readonly StyledProperty<double> SizeProperty =
            AvaloniaProperty.Register<Icon, double>(nameof(Size), 20);

        public static readonly StyledProperty<double> StrokeThicknessProperty =
            AvaloniaProperty.Register<Icon, double>(nameof(StrokeThickness), 2);

        public static readonly StyledProperty<IBrush?> ForegroundProperty =
            TextElement.ForegroundProperty.AddOwner<Icon>();

        static Icon()
        {
            AffectsMeasure<Icon>(SizeProperty);
            AffectsRender<Icon>(DataProperty, SizeProperty, StrokeThicknessProperty, ForegroundProperty);
        }

        public Geometry? Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public double Size
        {
            get => GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        public double StrokeThickness
        {
            get => GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public IBrush? Foreground
        {
            get => GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize) => new(Size, Size);

        public override void Render(DrawingContext context)
        {
            if (Data is null || Foreground is null)
                return;

            var pen = new Pen(Foreground, StrokeThickness, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
            var scale = Size / DesignSize;
            using (context.PushTransform(Matrix.CreateScale(scale, scale)))
                context.DrawGeometry(null, pen, Data);
        }
    }
}
