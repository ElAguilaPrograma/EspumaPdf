using Espuma.Application.Common;
using Espuma.Application.Services;
using SkiaSharp;

namespace Espuma.Application.Tests
{
    public class PdfImageToPdfServiceTests
    {
        private readonly PdfImageToPdfService _service = new();

        [Theory]
        [InlineData(SKEncodedImageFormat.Jpeg)]
        [InlineData(SKEncodedImageFormat.Png)]
        [InlineData(SKEncodedImageFormat.Webp)]
        public async Task Convert_OriginalSize_UsesImageSize(SKEncodedImageFormat format)
        {
            using var image = TestFiles.CreateImage(format, 300, 200);
            using var output = new MemoryStream();

            await _service.ConvertAsync([image], output, ImagePageLayout.OriginalSize,
                cancellationToken: TestContext.Current.CancellationToken);

            var rendered = TestFiles.Render(output, 0, 300);
            Assert.Equal(200, rendered.Height);
            AssertRed(rendered.PixelAt(10, 100));
            AssertBlue(rendered.PixelAt(290, 100));
        }

        [Fact]
        public async Task Convert_FitToA4_UsesA4Pages()
        {
            using var first = TestFiles.CreateImage(SKEncodedImageFormat.Jpeg);
            using var second = TestFiles.CreateImage(SKEncodedImageFormat.Png);
            using var output = new MemoryStream();

            await _service.ConvertAsync([first, second], output, ImagePageLayout.FitToA4,
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal([595, 595], TestFiles.ReadPages(output).Select(p => p.Width));
        }

        [Fact]
        public async Task Convert_AppliesExifOrientation()
        {
            using var image = TestFiles.CreateJpegWithOrientation(6);
            using var output = new MemoryStream();

            await _service.ConvertAsync([image], output, ImagePageLayout.OriginalSize,
                cancellationToken: TestContext.Current.CancellationToken);

            var rendered = TestFiles.Render(output, 0, 200);
            Assert.Equal(300, rendered.Height);
            AssertRed(rendered.PixelAt(100, 10));
            AssertBlue(rendered.PixelAt(100, 290));
        }

        [Fact]
        public async Task Convert_WithInvalidImage_ThrowsInvalidImage()
        {
            using var notAnImage = new MemoryStream("hello"u8.ToArray());

            var ex = await Assert.ThrowsAsync<PdfOperationException>(() =>
                _service.ConvertAsync([notAnImage], new MemoryStream(), ImagePageLayout.OriginalSize,
                    cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(PdfErrorCode.InvalidImage, ex.Code);
        }

        private static void AssertRed((byte R, byte G, byte B) pixel) =>
            Assert.True(pixel.R > 200 && pixel.B < 60, $"Se esperaba rojo y se obtuvo {pixel}.");

        private static void AssertBlue((byte R, byte G, byte B) pixel) =>
            Assert.True(pixel.B > 200 && pixel.R < 60, $"Se esperaba azul y se obtuvo {pixel}.");
    }
}
