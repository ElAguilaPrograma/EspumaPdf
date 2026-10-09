using System.IO.Compression;
using Espuma.Application.Rendering;
using Espuma.Application.Services;
using SkiaSharp;

namespace Espuma.Application.Tests
{
    public class PdfToImageServiceTests
    {
        private readonly PdfToImageService _service = new(new PdfiumRenderer());

        [Theory]
        [InlineData(ImageExportFormat.Png, "png", SKEncodedImageFormat.Png)]
        [InlineData(ImageExportFormat.Jpeg, "jpg", SKEncodedImageFormat.Jpeg)]
        public async Task Export_WritesOneImagePerPageInOrder(ImageExportFormat format, string extension,
            SKEncodedImageFormat expected)
        {
            using var pdf = TestFiles.CreatePdf(6);
            using var zip = new MemoryStream();

            await _service.ExportToZipAsync(pdf, zip, new PdfToImageOptions(Scale: 2, Format: format),
                cancellationToken: TestContext.Current.CancellationToken);

            zip.Position = 0;
            using var archive = new ZipArchive(zip, ZipArchiveMode.Read);
            Assert.Equal(Enumerable.Range(1, 6).Select(i => $"page_{i}.{extension}"), archive.Entries.Select(e => e.FullName));

            for (var i = 0; i < 6; i++)
            {
                using var entry = archive.Entries[i].Open();
                using var buffer = new MemoryStream();
                entry.CopyTo(buffer);
                buffer.Position = 0;
                using var codec = SKCodec.Create(buffer);
                Assert.Equal(expected, codec.EncodedFormat);
                Assert.Equal((int)(TestFiles.WidthOf(i) * 2), codec.Info.Width);
            }
        }

        [Fact]
        public async Task Export_WhenCancelled_Throws()
        {
            using var pdf = TestFiles.CreatePdf(3);
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                _service.ExportToZipAsync(pdf, new MemoryStream(), new PdfToImageOptions(), cancellationToken: cts.Token));
        }
    }
}
