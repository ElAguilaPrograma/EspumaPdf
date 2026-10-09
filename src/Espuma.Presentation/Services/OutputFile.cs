using Avalonia.Platform.Storage;

namespace Espuma.Presentation.Services
{
    /// <summary>A file chosen in the save dialog.</summary>
    public interface IOutputFile
    {
        string Name { get; }

        Uri? Path { get; }

        Task<Stream> OpenWriteAsync();

        Task DeleteAsync();
    }

    public sealed class StorageOutputFile(IStorageFile file) : IOutputFile
    {
        public IStorageFile File { get; } = file;

        public string Name => File.Name;

        public Uri? Path => File.Path;

        public Task<Stream> OpenWriteAsync() => File.OpenWriteAsync();

        public Task DeleteAsync() => File.DeleteAsync();
    }
}
