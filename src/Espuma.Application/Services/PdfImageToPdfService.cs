using Espuma.Application.Common;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Espuma.Application.Services
{
    public enum ImagePageLayout
    {
        /// <summary>Each page takes the size of its image (1 pixel = 1 point).</summary>
        OriginalSize,

        /// <summary>A4 pages with the image scaled to fit and centered.</summary>
        FitToA4
    }

    public interface IPdfImageToPdfService
    {
        Task ConvertAsync(IReadOnlyList<Stream> images, Stream output, ImagePageLayout layout,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default);
    }

    public sealed class PdfImageToPdfService : IPdfImageToPdfService
    {
        /// <summary>Largest page side most PDF viewers accept (200 inches).</summary>
        private const double MaxPageSide = 14400;

        public Task ConvertAsync(IReadOnlyList<Stream> images, Stream output, ImagePageLayout layout,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            if (images.Count == 0)
                throw new PdfOperationException(PdfErrorCode.NotEnoughFiles, "No se proporcionaron imágenes.");

            return Task.Run(() =>
            {
                var ownedStreams = new List<Stream>();
                try
                {
                    using var document = new PdfDocument();

                    for (var i = 0; i < images.Count; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var prepared = ImageInput.PrepareForPdf(images[i], out var ownsStream);
                        if (ownsStream) ownedStreams.Add(prepared);

                        using var image = XImage.FromStream(prepared);
                        var page = document.AddPage();

                        if (layout == ImagePageLayout.FitToA4)
                        {
                            page.Size = PageSize.A4;
                            DrawFitted(page, image);
                        }
                        else
                        {
                            var scale = Math.Min(1, MaxPageSide / Math.Max(image.PixelWidth, image.PixelHeight));
                            page.Width = XUnit.FromPoint(image.PixelWidth * scale);
                            page.Height = XUnit.FromPoint(image.PixelHeight * scale);
                            using var gfx = XGraphics.FromPdfPage(page);
                            gfx.DrawImage(image, 0, 0, page.Width.Point, page.Height.Point);
                        }

                        progress?.Report((i + 1d) / images.Count);
                    }

                    document.Save(output, false);
                }
                finally
                {
                    foreach (var stream in ownedStreams)
                        stream.Dispose();
                }
            }, cancellationToken);
        }

        private static void DrawFitted(PdfPage page, XImage image)
        {
            using var gfx = XGraphics.FromPdfPage(page);
            var pageWidth = page.Width.Point;
            var pageHeight = page.Height.Point;
            var scale = Math.Min(pageWidth / image.PixelWidth, pageHeight / image.PixelHeight);
            var width = image.PixelWidth * scale;
            var height = image.PixelHeight * scale;
            gfx.DrawImage(image, (pageWidth - width) / 2, (pageHeight - height) / 2, width, height);
        }
    }
}
