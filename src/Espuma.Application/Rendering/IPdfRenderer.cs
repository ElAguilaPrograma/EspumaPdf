using Espuma.Application.Common;

namespace Espuma.Application.Rendering
{
    /// <summary>Page size in points (1/72 inch), already including the page's own rotation.</summary>
    public readonly record struct PageSize(double Width, double Height)
    {
        public PageSize Rotate(PageRotation rotation) =>
            rotation is PageRotation.Clockwise90 or PageRotation.Clockwise270 ? new(Height, Width) : this;

        /// <summary>Pixel size that keeps the aspect ratio for the given width.</summary>
        public (int Width, int Height) ToPixels(int width) =>
            (width, Math.Max(1, (int)Math.Round(width * Height / Width)));
    }

    public interface IPdfRenderer
    {
        /// <summary>Opens a document for rendering. The stream must stay open until the document is disposed.</summary>
        IPdfRenderDocument Open(Stream pdf);
    }

    public interface IPdfRenderDocument : IDisposable
    {
        int PageCount { get; }

        PageSize GetPageSize(int pageIndex);

        /// <summary>Renders a page as opaque BGRA pixels into a caller-owned buffer.</summary>
        void Render(int pageIndex, nint buffer, int width, int height, int stride,
            PageRotation rotation = PageRotation.None);
    }
}
