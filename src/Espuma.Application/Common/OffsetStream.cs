namespace Espuma.Application.Common
{
    /// <summary>Read-only view of a seekable stream that starts at the given offset.</summary>
    internal sealed class OffsetStream(Stream inner, long offset) : Stream
    {
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => inner.Length - offset;

        public override long Position
        {
            get => inner.Position - offset;
            set => inner.Position = value + offset;
        }

        public override int Read(byte[] buffer, int index, int count) => inner.Read(buffer, index, count);

        public override int Read(Span<byte> buffer) => inner.Read(buffer);

        public override long Seek(long position, SeekOrigin origin) => origin switch
        {
            SeekOrigin.Begin => inner.Seek(position + offset, SeekOrigin.Begin) - offset,
            SeekOrigin.Current => inner.Seek(position, SeekOrigin.Current) - offset,
            _ => inner.Seek(position, SeekOrigin.End) - offset
        };

        public override void Flush() { }
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int index, int count) => throw new NotSupportedException();
    }
}
