using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Espuma.Application.Rendering;

namespace Espuma.Presentation.Thumbnails
{
    /// <summary>
    /// Renders thumbnails of the pages currently in view, most recent request first, and keeps a bounded number
    /// of bitmaps alive: pages that left the view are released first when the limit is reached.
    /// </summary>
    public sealed class ThumbnailLoader : IDisposable
    {
        public const int PixelWidth = 200;
        private const int MaxLoaded = 120;

        private readonly IPdfRenderDocument _document;
        private readonly Lock _lock = new();
        private readonly List<PageThumbnailViewModel> _pending = [];
        private readonly LinkedList<PageThumbnailViewModel> _loaded = [];
        private bool _working;
        private bool _disposed;

        public ThumbnailLoader(IPdfRenderDocument document)
        {
            _document = document;
        }

        internal void Request(PageThumbnailViewModel page)
        {
            if (page.Thumbnail is not null)
            {
                Touch(page);
                return;
            }

            lock (_lock)
            {
                if (_disposed)
                    return;

                _pending.Remove(page);
                _pending.Add(page);
                if (_working)
                    return;

                _working = true;
            }

            _ = Task.Run(Work);
        }

        internal void Release(PageThumbnailViewModel page)
        {
            lock (_lock)
                _pending.Remove(page);
        }

        private void Work()
        {
            while (true)
            {
                PageThumbnailViewModel page;
                lock (_lock)
                {
                    if (_disposed || _pending.Count == 0)
                    {
                        _working = false;
                        return;
                    }

                    page = _pending[^1];
                    _pending.RemoveAt(_pending.Count - 1);
                }

                WriteableBitmap? bitmap = null;
                try
                {
                    bitmap = Render(page.PageIndex);
                }
                catch (Exception)
                {
                    // A page that cannot be rendered keeps its placeholder.
                }

                if (bitmap is not null)
                    Dispatcher.UIThread.Post(() => Attach(page, bitmap));
            }
        }

        private WriteableBitmap Render(int pageIndex)
        {
            var (width, height) = _document.GetPageSize(pageIndex).ToPixels(PixelWidth);
            var bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(96, 96),
                PixelFormat.Bgra8888, AlphaFormat.Opaque);
            using (var frame = bitmap.Lock())
                _document.Render(pageIndex, frame.Address, width, height, frame.RowBytes);
            return bitmap;
        }

        private void Attach(PageThumbnailViewModel page, WriteableBitmap bitmap)
        {
            if (_disposed || !page.IsInView || page.Thumbnail is not null)
            {
                bitmap.Dispose();
                return;
            }

            page.Thumbnail = bitmap;
            _loaded.AddLast(page);
            EvictOverflow();
        }

        private void Touch(PageThumbnailViewModel page)
        {
            _loaded.Remove(page);
            _loaded.AddLast(page);
        }

        private void EvictOverflow()
        {
            var node = _loaded.First;
            while (_loaded.Count > MaxLoaded && node is not null)
            {
                var next = node.Next;
                if (!node.Value.IsInView)
                {
                    var bitmap = node.Value.Thumbnail;
                    node.Value.Thumbnail = null;
                    bitmap?.Dispose();
                    _loaded.Remove(node);
                }

                node = next;
            }
        }

        /// <summary>Releases every bitmap. Must be called on the UI thread.</summary>
        public void Dispose()
        {
            lock (_lock)
            {
                _disposed = true;
                _pending.Clear();
            }

            foreach (var page in _loaded)
            {
                var bitmap = page.Thumbnail;
                page.Thumbnail = null;
                bitmap?.Dispose();
            }

            _loaded.Clear();
        }
    }
}
