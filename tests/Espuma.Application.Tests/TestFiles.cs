using System.Runtime.InteropServices;
using Espuma.Application.Rendering;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using SkiaSharp;

namespace Espuma.Application.Tests
{
    internal static class TestFiles
    {
        /// <summary>Each page is 200 + index points wide, so page order can be checked through widths.</summary>
        public static MemoryStream CreatePdf(int pageCount, Action<PdfPage, int>? configure = null)
        {
            using var document = new PdfDocument();
            for (var i = 0; i < pageCount; i++)
            {
                var page = document.AddPage();
                page.Width = XUnit.FromPoint(WidthOf(i));
                page.Height = XUnit.FromPoint(300);
                configure?.Invoke(page, i);
            }

            var stream = new MemoryStream();
            document.Save(stream, false);
            stream.Position = 0;
            return stream;
        }

        public static double WidthOf(int pageIndex) => 200 + pageIndex;

        public static MemoryStream CreateEncryptedPdf()
        {
            using var document = new PdfDocument();
            document.AddPage();
            document.SecuritySettings.UserPassword = "secret";
            document.SecuritySettings.OwnerPassword = "owner";
            var stream = new MemoryStream();
            document.Save(stream, false);
            stream.Position = 0;
            return stream;
        }

        public static MemoryStream CreateCorruptedPdf()
        {
            var bytes = "%PDF-1.7\n"u8.ToArray().Concat(Enumerable.Repeat((byte)'x', 2048)).ToArray();
            return new MemoryStream(bytes);
        }

        public static (double Width, int Rotate)[] ReadPages(Stream pdf)
        {
            pdf.Position = 0;
            using var document = PdfReader.Open(pdf, PdfDocumentOpenMode.Import);
            return document.Pages.Cast<PdfPage>()
                .Select(page => (Math.Round(page.Width.Point), page.Rotate))
                .ToArray();
        }

        /// <summary>Image whose left half is red and right half is blue.</summary>
        public static MemoryStream CreateImage(SKEncodedImageFormat format, int width = 300, int height = 200)
        {
            using var bitmap = new SKBitmap(width, height);
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.Blue);
                using var paint = new SKPaint { Color = SKColors.Red };
                canvas.DrawRect(0, 0, width / 2f, height, paint);
            }

            var stream = new MemoryStream();
            bitmap.Encode(stream, format, 95);
            stream.Position = 0;
            return stream;
        }

        /// <summary>JPEG with an EXIF orientation tag (6 = displayed rotated 90° clockwise).</summary>
        public static MemoryStream CreateJpegWithOrientation(ushort orientation)
        {
            var jpeg = CreateImage(SKEncodedImageFormat.Jpeg).ToArray();
            byte[] exif =
            [
                0xFF, 0xE1, 0x00, 0x22,
                (byte)'E', (byte)'x', (byte)'i', (byte)'f', 0x00, 0x00,
                (byte)'M', (byte)'M', 0x00, 0x2A, 0x00, 0x00, 0x00, 0x08,
                0x00, 0x01,
                0x01, 0x12, 0x00, 0x03, 0x00, 0x00, 0x00, 0x01, (byte)(orientation >> 8), (byte)orientation, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00
            ];

            var result = new MemoryStream();
            result.Write(jpeg, 0, 2);
            result.Write(exif);
            result.Write(jpeg, 2, jpeg.Length - 2);
            result.Position = 0;
            return result;
        }

        /// <summary>Renders a page and returns its BGRA pixels.</summary>
        public static RenderedPage Render(Stream pdf, int pageIndex, int width)
        {
            pdf.Position = 0;
            using var document = new PdfiumRenderer().Open(pdf);
            var (w, h) = document.GetPageSize(pageIndex).ToPixels(width);
            var pixels = new byte[w * h * 4];
            var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            try
            {
                document.Render(pageIndex, handle.AddrOfPinnedObject(), w, h, w * 4);
            }
            finally
            {
                handle.Free();
            }

            return new RenderedPage(pixels, w, h);
        }
    }

    internal sealed record RenderedPage(byte[] Bgra, int Width, int Height)
    {
        public (byte R, byte G, byte B) PixelAt(int x, int y)
        {
            var offset = (y * Width + x) * 4;
            return (Bgra[offset + 2], Bgra[offset + 1], Bgra[offset]);
        }

        /// <summary>Counts non-white pixels inside a region given as fractions of the page size.</summary>
        public int InkIn(double left, double top, double right, double bottom)
        {
            var ink = 0;
            for (var y = (int)(top * Height); y < (int)(bottom * Height); y++)
            for (var x = (int)(left * Width); x < (int)(right * Width); x++)
            {
                var (r, g, b) = PixelAt(x, y);
                if (r + g + b < 600) ink++;
            }

            return ink;
        }
    }

    /// <summary>Wraps a stream hiding its ability to seek, like some Android content streams.</summary>
    internal sealed class NonSeekableStream(Stream inner) : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
