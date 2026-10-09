using Avalonia.Platform.Storage;

namespace Espuma.Presentation.Services
{
    /// <summary>A file chosen by the user, independent of where it comes from.</summary>
    public interface IInputFile
    {
        string Name { get; }

        /// <summary>Location of the file, when the platform exposes one.</summary>
        Uri? Path { get; }

        Task<Stream> OpenReadAsync();

        Task<ulong?> GetSizeAsync();
    }

    public sealed class StorageInputFile(IStorageFile file) : IInputFile
    {
        public string Name => file.Name;

        public Uri? Path => file.Path;

        public Task<Stream> OpenReadAsync() => file.OpenReadAsync();

        public async Task<ulong?> GetSizeAsync() => (await file.GetBasicPropertiesAsync()).Size;
    }
}
