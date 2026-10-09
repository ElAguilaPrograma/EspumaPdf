using Espuma.Application.Common;
using Espuma.Application.Services;

namespace Espuma.Application.Tests
{
    public class PdfMergeServiceTests
    {
        private readonly PdfMergeService _service = new();

        [Fact]
        public async Task Merge_KeepsInputOrderAndAllPages()
        {
            using var first = TestFiles.CreatePdf(2);
            using var second = TestFiles.CreatePdf(3);
            using var output = new MemoryStream();
            var progress = new List<double>();

            await _service.MergeAsync([first, second], output, new SyncProgress(progress.Add), TestContext.Current.CancellationToken);

            var widths = TestFiles.ReadPages(output).Select(p => p.Width);
            Assert.Equal([200, 201, 200, 201, 202], widths);
            Assert.Equal(1, progress[^1]);
        }

        [Fact]
        public async Task Merge_AcceptsNonSeekableStreams()
        {
            using var first = TestFiles.CreatePdf(1);
            using var second = TestFiles.CreatePdf(1);
            using var output = new MemoryStream();

            await _service.MergeAsync([new NonSeekableStream(first), new NonSeekableStream(second)], output,
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(2, TestFiles.ReadPages(output).Length);
        }

        [Fact]
        public async Task Merge_WithSingleFile_Throws()
        {
            using var only = TestFiles.CreatePdf(1);

            var ex = await Assert.ThrowsAsync<PdfOperationException>(() => _service.MergeAsync([only], new MemoryStream(), cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(PdfErrorCode.NotEnoughFiles, ex.Code);
        }

        [Fact]
        public async Task Merge_WithNonPdf_ThrowsInvalidPdf()
        {
            using var pdf = TestFiles.CreatePdf(1);
            using var image = TestFiles.CreateImage(SkiaSharp.SKEncodedImageFormat.Png);

            var ex = await Assert.ThrowsAsync<PdfOperationException>(() => _service.MergeAsync([pdf, image], new MemoryStream(), cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(PdfErrorCode.InvalidPdf, ex.Code);
        }

        [Fact]
        public async Task Merge_WithCorruptedPdf_ThrowsCorrupted()
        {
            using var pdf = TestFiles.CreatePdf(1);
            using var corrupted = TestFiles.CreateCorruptedPdf();

            var ex = await Assert.ThrowsAsync<PdfOperationException>(() => _service.MergeAsync([pdf, corrupted], new MemoryStream(), cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(PdfErrorCode.Corrupted, ex.Code);
        }

        [Fact]
        public async Task Merge_WithEncryptedPdf_ThrowsEncrypted()
        {
            using var pdf = TestFiles.CreatePdf(1);
            using var encrypted = TestFiles.CreateEncryptedPdf();

            var ex = await Assert.ThrowsAsync<PdfOperationException>(() => _service.MergeAsync([pdf, encrypted], new MemoryStream(), cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(PdfErrorCode.Encrypted, ex.Code);
        }

        [Fact]
        public async Task Merge_AcceptsHeaderAfterLeadingGarbage()
        {
            using var clean = TestFiles.CreatePdf(1);
            using var prefixed = new MemoryStream([.. "garbage\n"u8.ToArray(), .. clean.ToArray()]);
            using var output = new MemoryStream();

            await _service.MergeAsync([clean, prefixed], output, cancellationToken: TestContext.Current.CancellationToken);

            Assert.Equal(2, TestFiles.ReadPages(output).Length);
        }
    }

    /// <summary>IProgress that reports synchronously, unlike Progress&lt;T&gt;.</summary>
    internal sealed class SyncProgress(Action<double> report) : IProgress<double>
    {
        public void Report(double value) => report(value);
    }
}
