using Espuma.Application.Common;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Espuma.Application.Services
{
    public interface IPdfMergeService
    {
        Task MergeAsync(IReadOnlyList<Stream> inputs, Stream output,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default);
    }

    public sealed class PdfMergeService : IPdfMergeService
    {
        public Task MergeAsync(IReadOnlyList<Stream> inputs, Stream output,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            if (inputs.Count < 2)
                throw new PdfOperationException(PdfErrorCode.NotEnoughFiles, "Se necesitan al menos dos archivos para unirlos.");

            return Task.Run(() =>
            {
                using var outputDocument = new PdfDocument();

                for (var i = 0; i < inputs.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var stream = PdfInput.EnsureSeekable(inputs[i], out var ownsStream);
                    try
                    {
                        using var inputDocument = PdfInput.Open(stream, PdfDocumentOpenMode.Import);
                        for (var page = 0; page < inputDocument.PageCount; page++)
                            outputDocument.AddPage(inputDocument.Pages[page]);
                    }
                    finally
                    {
                        if (ownsStream) stream.Dispose();
                    }

                    progress?.Report((i + 1d) / inputs.Count);
                }

                cancellationToken.ThrowIfCancellationRequested();
                outputDocument.Save(output, false);
            }, cancellationToken);
        }
    }
}
