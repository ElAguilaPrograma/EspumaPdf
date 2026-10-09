using Avalonia.Media;
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

        public ITransform RotationTransform => new RotateTransform(RotationAngle);

        private bool IsSideways => Rotation is PageRotation.Clockwise90 or PageRotation.Clockwise270;

        /// <summary>Size of the page as displayed after <see cref="Rotation"/>, fitted in the thumbnail cell.</summary>
        public double BoxWidth => FittedBox().Width;

        public double BoxHeight => FittedBox().Height;

        /// <summary>Size of the unrotated image that, once rotated, fills the box.</summary>
        public double ImageWidth => IsSideways ? BoxHeight : BoxWidth;

        public double ImageHeight => IsSideways ? BoxWidth : BoxHeight;

        [ObservableProperty]
        public partial Bitmap? Thumbnail { get; internal set; }

        [ObservableProperty]
        public partial bool IsSelected { get; set; }

        [ObservableProperty]
        public partial bool IsHighlighted { get; set; }

        [ObservableProperty]
        public partial string? Badge { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(RotationAngle), nameof(RotationTransform), nameof(BoxWidth), nameof(BoxHeight), nameof(ImageWidth), nameof(ImageHeight))]
        public partial PageRotation Rotation { get; set; }

        internal bool IsInView { get; private set; }

        private (double Width, double Height) FittedBox()
        {
            var aspect = IsSideways ? 1 / AspectRatio : AspectRatio;
            var height = DisplayWidth * aspect;
            return height <= MaxDisplayHeight ? (DisplayWidth, height) : (MaxDisplayHeight / aspect, MaxDisplayHeight);
        }

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
