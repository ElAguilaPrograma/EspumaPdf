using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using Espuma.Application.Common;
using Espuma.Application.Rendering;
using Espuma.Presentation.Services;

namespace Espuma.Presentation.ViewModels
{
    public sealed partial class SelectedFile(IInputFile file) : ObservableObject, IDisposable
    {
        private const int ThumbnailWidth = 96;

        public IInputFile File { get; } = file;
        public string Name => File.Name;

        [ObservableProperty]
        public partial string Detail { get; private set; } = "";

        [ObservableProperty]
        public partial Bitmap? Thumbnail { get; private set; }

        [ObservableProperty]
        public partial string? Problem { get; private set; }

        public int PageCount { get; private set; }

        /// <summary>Reads the page count and a small preview of the first page or image.</summary>
        public async Task LoadDetailsAsync(FileKind kind, IPdfRenderer renderer)
        {
            var size = await TryGetSizeAsync();
            try
            {
                await using var stream = await File.OpenReadAsync();
                var (thumbnail, pages) = await Task.Run(() => kind == FileKind.Pdf
                    ? RenderFirstPage(stream, renderer)
                    : (Bitmap.DecodeToWidth(stream, ThumbnailWidth, BitmapInterpolationMode.MediumQuality), 0));

                Thumbnail = thumbnail;
                PageCount = pages;
                Detail = pages > 0 ? $"{pages} {(pages == 1 ? "página" : "páginas")} · {size}" : size;
            }
            catch (PdfOperationException ex)
            {
                Problem = ErrorMessages.TitleFor(ex.Code);
                Detail = size;
            }
            catch (Exception)
            {
                Problem = kind == FileKind.Pdf ? "No se pudo leer el PDF" : "No se pudo leer la imagen";
                Detail = size;
            }
        }

        private static (Bitmap, int) RenderFirstPage(Stream stream, IPdfRenderer renderer)
        {
            using var document = renderer.Open(stream);
            var (width, height) = document.GetPageSize(0).ToPixels(ThumbnailWidth);
            var bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96),
                PixelFormat.Bgra8888, AlphaFormat.Opaque);
            using (var frame = bitmap.Lock())
                document.Render(0, frame.Address, width, height, frame.RowBytes);
            return (bitmap, document.PageCount);
        }

        private async Task<string> TryGetSizeAsync()
        {
            try
            {
                return FormatSize(await File.GetSizeAsync() ?? 0);
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static string FormatSize(ulong bytes) => bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024d:0.#} KB",
            _ => $"{bytes / 1024d / 1024d:0.#} MB"
        };

        public void Dispose()
        {
            Thumbnail?.Dispose();
            Thumbnail = null;
        }
    }
}
