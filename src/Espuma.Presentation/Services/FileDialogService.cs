using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Espuma.Presentation.Services
{
    public interface IFileDialogService
    {
        Task<IReadOnlyList<IInputFile>> OpenAsync(FileKind kind, bool allowMultiple);

        /// <returns>The chosen file, or null when the user cancels.</returns>
        Task<IStorageFile?> SaveAsync(FileKind kind, string suggestedName);
    }

    public sealed class FileDialogService(Func<TopLevel?> topLevel) : IFileDialogService
    {
        public async Task<IReadOnlyList<IInputFile>> OpenAsync(FileKind kind, bool allowMultiple)
        {
            var provider = GetProvider();
            var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = allowMultiple ? "Elige los archivos" : "Elige un archivo",
                AllowMultiple = allowMultiple,
                FileTypeFilter = [kind.PickerType()]
            });
            return files.Select(file => (IInputFile)new StorageInputFile(file)).ToList();
        }

        public async Task<IStorageFile?> SaveAsync(FileKind kind, string suggestedName)
        {
            var provider = GetProvider();
            return await provider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Guardar como",
                SuggestedFileName = suggestedName,
                DefaultExtension = kind.Extension(),
                FileTypeChoices = [kind.PickerType()],
                ShowOverwritePrompt = true
            });
        }

        private IStorageProvider GetProvider() =>
            topLevel()?.StorageProvider ?? throw new InvalidOperationException("La ventana principal no está disponible.");
    }
}
