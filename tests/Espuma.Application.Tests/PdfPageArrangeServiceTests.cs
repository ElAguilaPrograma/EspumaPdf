using Espuma.Application.Common;
using Espuma.Application.Services;

namespace Espuma.Application.Tests
{
    public class PdfPageArrangeServiceTests
    {
        private readonly PdfPageArrangeService _service = new();

        [Fact]
        public async Task Arrange_ReordersAndRotatesPages()
        {
            using var input = TestFiles.CreatePdf(3);
            using var output = new MemoryStream();

            await _service.ArrangeAsync(input,
                [new(2), new(0, PageRotation.Clockwise90), new(1, PageRotation.Clockwise270)],
                output, TestContext.Current.CancellationToken);

            Assert.Equal([(202, 0), (200, 90), (201, 270)], TestFiles.ReadPages(output));
        }

        [Fact]
        public async Task Arrange_AddsRotationToExistingPageRotation()
        {
            using var input = TestFiles.CreatePdf(2, (page, i) => page.Rotate = i == 0 ? 270 : 0);
            using var output = new MemoryStream();

            await _service.ArrangeAsync(input, [new(0, PageRotation.Rotate180), new(1, PageRotation.Clockwise90)],
                output, TestContext.Current.CancellationToken);

            Assert.Equal([90, 90], TestFiles.ReadPages(output).Select(p => p.Rotate));
        }

        [Fact]
        public async Task Arrange_WithUnknownPage_ThrowsInvalidRange()
        {
            using var input = TestFiles.CreatePdf(2);

            var ex = await Assert.ThrowsAsync<PdfOperationException>(() =>
                _service.ArrangeAsync(input, [new(0), new(2)], new MemoryStream(), cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(PdfErrorCode.InvalidRange, ex.Code);
        }

        [Fact]
        public async Task Arrange_WithoutPages_ThrowsNoPagesSelected()
        {
            using var input = TestFiles.CreatePdf(2);

            var ex = await Assert.ThrowsAsync<PdfOperationException>(() =>
                _service.ArrangeAsync(input, [], new MemoryStream(), cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(PdfErrorCode.NoPagesSelected, ex.Code);
        }

        [Fact]
        public async Task ExtractRange_KeepsInclusiveRange()
        {
            using var input = TestFiles.CreatePdf(5);
            using var output = new MemoryStream();

            await _service.ExtractRangeAsync(input, 1, 3, output, TestContext.Current.CancellationToken);

            Assert.Equal([201, 202, 203], TestFiles.ReadPages(output).Select(p => p.Width));
        }

        [Theory]
        [InlineData(-1, 2)]
        [InlineData(0, 5)]
        [InlineData(3, 1)]
        public async Task ExtractRange_WithInvalidRange_Throws(int first, int last)
        {
            using var input = TestFiles.CreatePdf(5);

            var ex = await Assert.ThrowsAsync<PdfOperationException>(() =>
                _service.ExtractRangeAsync(input, first, last, new MemoryStream(), cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(PdfErrorCode.InvalidRange, ex.Code);
        }

        [Fact]
        public async Task DeletePages_RemovesSelectedPages()
        {
            using var input = TestFiles.CreatePdf(4);
            using var output = new MemoryStream();

            await _service.DeletePagesAsync(input, [0, 2], output, TestContext.Current.CancellationToken);

            Assert.Equal([201, 203], TestFiles.ReadPages(output).Select(p => p.Width));
        }

        [Fact]
        public async Task DeletePages_AllPages_ThrowsNoPagesSelected()
        {
            using var input = TestFiles.CreatePdf(2);

            var ex = await Assert.ThrowsAsync<PdfOperationException>(() =>
                _service.DeletePagesAsync(input, [0, 1], new MemoryStream(), cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(PdfErrorCode.NoPagesSelected, ex.Code);
        }
    }
}
