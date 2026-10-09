using Espuma.Application.Common;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Espuma.Application.Services
{
    /// <param name="FirstPageIndex">0-based index of the first page that gets a number.</param>
    /// <param name="Format">Composite format: {0} is the page number, {1} the total of pages.</param>
    public sealed record PageNumberOptions(int FirstPageIndex = 0, string Format = "{0} | {1}", double FontSize = 16);

    public interface IPdfPageNumberService
    {
        Task AddPageNumbersAsync(Stream input, Stream output, PageNumberOptions options,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default);
    }

    public sealed class PdfPageNumberService : IPdfPageNumberService
    {
        private const double Margin = 35;

        public Task AddPageNumbersAsync(Stream input, Stream output, PageNumberOptions options,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            EmbeddedFontResolver.EnsureRegistered();

            return Task.Run(() =>
            {
                var stream = PdfInput.EnsureSeekable(input, out var ownsStream);
                try
                {
                    using var document = PdfInput.Open(stream, PdfDocumentOpenMode.Modify);
                    var totalPages = document.PageCount;

                    if (options.FirstPageIndex < 0 || options.FirstPageIndex >= totalPages)
                        throw new PdfOperationException(PdfErrorCode.InvalidRange,
                            $"La página inicial debe estar entre 1 y {totalPages}.");

                    var font = new XFont(EmbeddedFontResolver.Roboto, options.FontSize);
                    for (var i = options.FirstPageIndex; i < totalPages; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var page = document.Pages[i];
                        using var gfx = XGraphics.FromPdfPage(page);
                        var visualSize = UseVisualCoordinates(gfx, page.Rotate);
                        var text = string.Format(options.Format, i + 1, totalPages);
                        var size = gfx.MeasureString(text, font);
                        var position = new XPoint(visualSize.Width - size.Width - Margin, visualSize.Height - Margin);
                        gfx.DrawString(text, font, XBrushes.Black, position);

                        progress?.Report((i + 1d - options.FirstPageIndex) / (totalPages - options.FirstPageIndex));
                    }

                    document.Save(output, false);
                }
                finally
                {
                    if (ownsStream) stream.Dispose();
                }
            }, cancellationToken);
        }

        /// <summary>
        /// Makes the drawing coordinates match the page as it is displayed, taking its /Rotate into account.
        /// Returns the displayed page size.
        /// </summary>
        private static XSize UseVisualCoordinates(XGraphics gfx, int rotate)
        {
            var width = gfx.PageSize.Width;
            var height = gfx.PageSize.Height;

            switch ((rotate % 360 + 360) % 360)
            {
                case 90:
                    gfx.TranslateTransform(0, height);
                    gfx.RotateTransform(-90);
                    return new XSize(height, width);
                case 180:
                    gfx.TranslateTransform(width, height);
                    gfx.RotateTransform(180);
                    return new XSize(width, height);
                case 270:
                    gfx.TranslateTransform(width, 0);
                    gfx.RotateTransform(90);
                    return new XSize(height, width);
                default:
                    return new XSize(width, height);
            }
        }
    }
}
