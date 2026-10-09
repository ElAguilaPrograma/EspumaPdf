using Espuma.Application.Rendering;
using Espuma.Presentation.Services;

namespace Espuma.Presentation.Thumbnails
{
    /// <summary>An open PDF with one lazily rendered thumbnail per page.</summary>
    public sealed class PdfPreview : IDisposable
    {
        private readonly Stream _stream;
        private readonly IPdfRenderDocument _document;
        private readonly ThumbnailLoader _loader;

        private PdfPreview(Stream stream, IPdfRenderDocument document)
        {
            _stream = stream;
            _document = document;
            _loader = new ThumbnailLoader(document);
            Pages = Enumerable.Range(0, document.PageCount)
                .Select(index =>
                {
                    var size = document.GetPageSize(index);
                    return new PageThumbnailViewModel(_loader, index, size.Height / size.Width);
                })
                .ToList();
        }

        public IReadOnlyList<PageThumbnailViewModel> Pages { get; }
        public int PageCount => Pages.Count;

        public static async Task<PdfPreview> OpenAsync(IInputFile file, IPdfRenderer renderer)
        {
            var stream = await file.OpenReadAsync();
            try
            {
                return await Task.Run(() => new PdfPreview(stream, renderer.Open(stream)));
            }
            catch
            {
                await stream.DisposeAsync();
                throw;
            }
        }

        public void Dispose()
        {
            _loader.Dispose();
            _document.Dispose();
            _stream.Dispose();
        }
    }
}
