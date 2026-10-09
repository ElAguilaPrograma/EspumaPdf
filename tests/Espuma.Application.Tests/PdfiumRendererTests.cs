using Espuma.Application.Common;
using Espuma.Application.Rendering;
using PdfSharp.Drawing;

namespace Espuma.Application.Tests
{
    public class PdfiumRendererTests
    {
        private readonly PdfiumRenderer _renderer = new();

        [Fact]
        public void Open_ReadsPageCountAndSizes()
        {
            using var pdf = TestFiles.CreatePdf(3, (page, i) => page.Rotate = i == 2 ? 90 : 0);
            using var document = _renderer.Open(pdf);

            Assert.Equal(3, document.PageCount);
            Assert.Equal(new PageSize(201, 300), document.GetPageSize(1));
            Assert.Equal(new PageSize(300, 202), document.GetPageSize(2));
        }

        [Fact]
        public void Render_DrawsPageContent()
        {
            using var pdf = TestFiles.CreatePdf(1, (page, _) =>
            {
                using var gfx = XGraphics.FromPdfPage(page);
                gfx.DrawRectangle(XBrushes.Black, 0, 0, page.Width.Point / 2, page.Height.Point);
            });

            var rendered = TestFiles.Render(pdf, 0, 200);

            Assert.Equal((0, 0, 0), rendered.PixelAt(10, 150));
            Assert.Equal((255, 255, 255), rendered.PixelAt(190, 150));
        }

        [Fact]
        public void Render_IsSafeFromSeveralThreads()
        {
            using var pdf = TestFiles.CreatePdf(20);
            using var document = _renderer.Open(pdf);

            Parallel.For(0, 200, i =>
            {
                var buffer = new byte[50 * 80 * 4];
                unsafe
                {
                    fixed (byte* pointer = buffer)
                        document.Render(i % 20, (nint)pointer, 50, 80, 50 * 4);
                }
            });
        }

        [Fact]
        public void Open_WithNonPdf_ThrowsInvalidPdf()
        {
            using var image = TestFiles.CreateImage(SkiaSharp.SKEncodedImageFormat.Png);

            var ex = Assert.Throws<PdfOperationException>(() => _renderer.Open(image));
            Assert.Equal(PdfErrorCode.InvalidPdf, ex.Code);
        }

        [Fact]
        public void Open_WithEncryptedPdf_ThrowsEncrypted()
        {
            using var pdf = TestFiles.CreateEncryptedPdf();

            var ex = Assert.Throws<PdfOperationException>(() => _renderer.Open(pdf));
            Assert.Equal(PdfErrorCode.Encrypted, ex.Code);
        }

        [Fact]
        public void Open_WithNonSeekableStream_Works()
        {
            using var pdf = TestFiles.CreatePdf(2);
            using var document = _renderer.Open(new NonSeekableStream(pdf));

            Assert.Equal(2, document.PageCount);
        }
    }
}
