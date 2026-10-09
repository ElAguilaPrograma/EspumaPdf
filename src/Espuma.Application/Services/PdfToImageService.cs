using System.IO.Compression;
using Espuma.Application.Common;
using Espuma.Application.Rendering;
using SkiaSharp;

namespace Espuma.Application.Services
{
    public enum ImageExportFormat
    {
        Png,
        Jpeg
    }

    /// <param name="Scale">Pixels per point: 1 = 72 DPI, 2 = 144 DPI.</param>
    public sealed record PdfToImageOptions(double Scale = 2, ImageExportFormat Format = ImageExportFormat.Png, int JpegQuality = 90);

    public interface IPdfToImageService
    {
        /// <summary>Writes every page as an image into a ZIP archive.</summary>
        Task ExportToZipAsync(Stream pdf, Stream zipOutput, PdfToImageOptions options,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default);
    }

    public sealed class PdfToImageService : IPdfToImageService
    {
        private const int PngCompressionLevel = 4;
        private static readonly int BatchSize = Math.Clamp(Environment.ProcessorCount, 1, 4);

        private readonly IPdfRenderer _renderer;

        public PdfToImageService(IPdfRenderer renderer)
        {
            _renderer = renderer;
        }

        public Task ExportToZipAsync(Stream pdf, Stream zipOutput, PdfToImageOptions options,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(options.Scale);

            return Task.Run(() =>
            {
                using var document = _renderer.Open(pdf);
                using var archive = new ZipArchive(zipOutput, ZipArchiveMode.Create, leaveOpen: true);
                var extension = options.Format == ImageExportFormat.Png ? "png" : "jpg";
                var encoded = new byte[BatchSize][];

                for (var start = 0; start < document.PageCount; start += BatchSize)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var count = Math.Min(BatchSize, document.PageCount - start);
                    var bitmaps = new SKBitmap[count];

                    try
                    {
                        for (var i = 0; i < count; i++)
                            bitmaps[i] = RenderPage(document, start + i, options.Scale);

                        Parallel.For(0, count, new ParallelOptions { CancellationToken = cancellationToken },
                            i => encoded[i] = Encode(bitmaps[i], options));
                    }
                    finally
                    {
                        foreach (var bitmap in bitmaps)
                            bitmap?.Dispose();
                    }

                    for (var i = 0; i < count; i++)
                    {
                        // Stored as is: the images are already compressed.
                        var entry = archive.CreateEntry($"page_{start + i + 1}.{extension}", CompressionLevel.NoCompression);
                        using var entryStream = entry.Open();
                        entryStream.Write(encoded[i]);
                        encoded[i] = [];
                    }

                    progress?.Report((double)(start + count) / document.PageCount);
                }
            }, cancellationToken);
        }

        private static SKBitmap RenderPage(IPdfRenderDocument document, int pageIndex, double scale)
        {
            var size = document.GetPageSize(pageIndex);
            var width = Math.Max(1, (int)Math.Round(size.Width * scale));
            var (_, height) = size.ToPixels(width);
            var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque));
            document.Render(pageIndex, bitmap.GetPixels(), width, height, bitmap.RowBytes);
            return bitmap;
        }

        private static byte[] Encode(SKBitmap bitmap, PdfToImageOptions options)
        {
            using var pixmap = bitmap.PeekPixels();
            using var data = options.Format == ImageExportFormat.Png
                ? pixmap.Encode(new SKPngEncoderOptions(SKPngEncoderFilterFlags.AllFilters, PngCompressionLevel))
                : pixmap.Encode(new SKJpegEncoderOptions(options.JpegQuality, SKJpegEncoderDownsample.Downsample420,
                    SKJpegEncoderAlphaOption.Ignore));
            return data?.ToArray() ?? throw new InvalidOperationException("No se pudo codificar la imagen.");
        }
    }
}
