using Espuma.Application.Common;
using Espuma.Application.Services;

namespace Espuma.Application.Tests
{
    public class PdfPageNumberServiceTests
    {
        private readonly PdfPageNumberService _service = new();

        [Fact]
        public async Task AddPageNumbers_FromFirstPage_NumbersEveryPage()
        {
            using var input = TestFiles.CreatePdf(3);
            using var output = new MemoryStream();

            await _service.AddPageNumbersAsync(input, output, new PageNumberOptions(),
                cancellationToken: TestContext.Current.CancellationToken);

            for (var i = 0; i < 3; i++)
                Assert.True(BottomRightInk(output, i) > 0, $"La página {i + 1} no tiene número.");
        }

        [Fact]
        public async Task AddPageNumbers_SkipsPagesBeforeFirstPageIndex()
        {
            using var input = TestFiles.CreatePdf(4);
            using var output = new MemoryStream();

            await _service.AddPageNumbersAsync(input, output, new PageNumberOptions(FirstPageIndex: 2),
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(0, BottomRightInk(output, 0));
            Assert.Equal(0, BottomRightInk(output, 1));
            Assert.True(BottomRightInk(output, 2) > 0);
            Assert.True(BottomRightInk(output, 3) > 0);
        }

        [Theory]
        [InlineData(90)]
        [InlineData(180)]
        [InlineData(270)]
        [InlineData(-90)]
        public async Task AddPageNumbers_OnRotatedPage_DrawsAtVisualBottomRight(int rotate)
        {
            using var input = TestFiles.CreatePdf(1, (page, _) => page.Rotate = rotate);
            using var output = new MemoryStream();

            await _service.AddPageNumbersAsync(input, output, new PageNumberOptions(),
                cancellationToken: TestContext.Current.CancellationToken);

            var page = TestFiles.Render(output, 0, 300);
            Assert.True(page.InkIn(0.5, 0.75, 1, 1) > 0);
            Assert.Equal(0, page.InkIn(0, 0, 1, 0.5));
            Assert.Equal(0, page.InkIn(0, 0.5, 0.5, 1));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(3)]
        public async Task AddPageNumbers_WithInvalidFirstPage_ThrowsInvalidRange(int firstPageIndex)
        {
            using var input = TestFiles.CreatePdf(3);

            var ex = await Assert.ThrowsAsync<PdfOperationException>(() =>
                _service.AddPageNumbersAsync(input, new MemoryStream(), new PageNumberOptions(firstPageIndex),
                    cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(PdfErrorCode.InvalidRange, ex.Code);
        }

        private static int BottomRightInk(Stream pdf, int pageIndex) =>
            TestFiles.Render(pdf, pageIndex, 200).InkIn(0.5, 0.75, 1, 1);
    }
}
