using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Espuma.Application.Common;

namespace Espuma.Presentation.Thumbnails
{
    public sealed partial class PageThumbnailViewModel : ObservableObject
    {
        public const double DisplayWidth = 132;
        private const double MaxDisplayHeight = 200;

        private readonly ThumbnailLoader _loader;

        public PageThumbnailViewModel(ThumbnailLoader loader, int pageIndex, double aspectRatio)
        {
            _loader = loader;
            PageIndex = pageIndex;
            AspectRatio = aspectRatio;
        }

        public int PageIndex { get; }
        public int PageNumber => PageIndex + 1;

        /// <summary>Height divided by width of the page as displayed without extra rotation.</summary>
        public double AspectRatio { get; }

        public double DisplayHeight => Math.Min(DisplayWidth * AspectRatio, MaxDisplayHeight);

        public double RotationAngle => (int)Rotation;

        [ObservableProperty]
        public partial Bitmap? Thumbnail { get; internal set; }

        [ObservableProperty]
        public partial bool IsSelected { get; set; }

        [ObservableProperty]
        public partial bool IsHighlighted { get; set; }

        [ObservableProperty]
        public partial string? Badge { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RotationAngle))]
        public partial PageRotation Rotation { get; set; }

        internal bool IsInView { get; private set; }

        public void SetInView(bool inView)
        {
            if (IsInView == inView)
                return;

            IsInView = inView;
            if (inView)
                _loader.Request(this);
            else
                _loader.Release(this);
        }
    }
}
