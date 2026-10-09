using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace Espuma.Presentation.Thumbnails
{
    /// <summary>
    /// Container for a page thumbnail that tells its <see cref="PageThumbnailViewModel"/> whether it is inside
    /// (or close to) the visible area of the scroll viewer.
    /// </summary>
    public sealed class ThumbnailHost : Border
    {
        private const double Preload = 400;

        public ThumbnailHost()
        {
            EffectiveViewportChanged += OnEffectiveViewportChanged;
            DetachedFromVisualTree += (_, _) => (DataContext as PageThumbnailViewModel)?.SetInView(false);
        }

        /// <summary>Page of the thumbnail that contains the given element, if any.</summary>
        public static PageThumbnailViewModel? FindPage(object? source) =>
            (source as Visual)?.FindAncestorOfType<ThumbnailHost>(includeSelf: true)?.DataContext as PageThumbnailViewModel;

        private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
        {
            if (DataContext is not PageThumbnailViewModel page)
                return;

            var viewport = e.EffectiveViewport.Inflate(Preload);
            page.SetInView(viewport.Intersects(new Rect(Bounds.Size)));
        }
    }
}
