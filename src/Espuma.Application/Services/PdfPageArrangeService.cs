using Espuma.Application.Common;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Espuma.Application.Services
{
    /// <summary>Builds a new PDF from a selection of pages of a source PDF. Page indexes are 0-based.</summary>
    public interface IPdfPageArrangeService
    {
        Task ArrangeAsync(Stream input, IReadOnlyList<PageSelection> pages, Stream output,
            CancellationToken cancellationToken = default);

        Task ExtractRangeAsync(Stream input, int firstPageIndex, int lastPageIndex, Stream output,
            CancellationToken cancellationToken = default);

        Task DeletePagesAsync(Stream input, IReadOnlyCollection<int> pageIndexes, Stream output,
            CancellationToken cancellationToken = default);
    }

    public sealed class PdfPageArrangeService : IPdfPageArrangeService
    {
        public Task ArrangeAsync(Stream input, IReadOnlyList<PageSelection> pages, Stream output,
            CancellationToken cancellationToken = default) =>
            RunAsync(input, output, _ => pages, cancellationToken);

        public Task ExtractRangeAsync(Stream input, int firstPageIndex, int lastPageIndex, Stream output,
            CancellationToken cancellationToken = default) =>
            RunAsync(input, output, pageCount =>
            {
                if (firstPageIndex < 0 || lastPageIndex >= pageCount)
                    throw new PdfOperationException(PdfErrorCode.InvalidRange,
                        $"Rango inválido, este PDF tiene {pageCount} páginas.");

                if (lastPageIndex < firstPageIndex)
                    throw new PdfOperationException(PdfErrorCode.InvalidRange,
                        "La página final no puede ser menor que la inicial.");

                return Enumerable.Range(firstPageIndex, lastPageIndex - firstPageIndex + 1)
                    .Select(index => new PageSelection(index))
                    .ToList();
            }, cancellationToken);

        public Task DeletePagesAsync(Stream input, IReadOnlyCollection<int> pageIndexes, Stream output,
            CancellationToken cancellationToken = default) =>
            RunAsync(input, output, pageCount =>
            {
                var toDelete = pageIndexes.ToHashSet();
                return Enumerable.Range(0, pageCount)
                    .Where(index => !toDelete.Contains(index))
                    .Select(index => new PageSelection(index))
                    .ToList();
            }, cancellationToken);

        private static Task RunAsync(Stream input, Stream output,
            Func<int, IReadOnlyList<PageSelection>> selectPages, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                var stream = PdfInput.EnsureSeekable(input, out var ownsStream);
                try
                {
                    using var inputDocument = PdfInput.Open(stream, PdfDocumentOpenMode.Import);
                    var pageCount = inputDocument.PageCount;
                    var pages = selectPages(pageCount);

                    if (pages.Count == 0)
                        throw new PdfOperationException(PdfErrorCode.NoPagesSelected,
                            "El resultado no tendría páginas.");

                    using var outputDocument = new PdfDocument();
                    foreach (var selection in pages)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (selection.SourceIndex < 0 || selection.SourceIndex >= pageCount)
                            throw new PdfOperationException(PdfErrorCode.InvalidRange,
                                $"La página {selection.SourceIndex + 1} no existe, este PDF tiene {pageCount} páginas.");

                        var page = outputDocument.AddPage(inputDocument.Pages[selection.SourceIndex]);
                        if (selection.Rotation != PageRotation.None)
                            page.Rotate = ((page.Rotate + (int)selection.Rotation) % 360 + 360) % 360;
                    }

                    outputDocument.Save(output, false);
                }
                finally
                {
                    if (ownsStream) stream.Dispose();
                }
            }, cancellationToken);
        }
    }
}
