using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Espuma.Presentation.Services;

namespace Espuma.Presentation.ViewModels
{
    public abstract partial class ToolViewModel : ViewModelBase
    {
        protected ToolViewModel(ToolDescriptor descriptor, AppServices services)
        {
            Descriptor = descriptor;
            Services = services;
            Files.CollectionChanged += OnFilesCollectionChanged;
        }

        public ToolDescriptor Descriptor { get; }
        protected AppServices Services { get; }

        public ObservableCollection<SelectedFile> Files { get; } = [];
        public bool HasFiles => Files.Count > 0;
        public bool AllowMultiple => Descriptor.AllowMultiple;

        public string DropTitle => (Descriptor.InputKind, AllowMultiple) switch
        {
            (FileKind.Image, _) => "Arrastra tus imágenes aquí",
            (_, true) => "Arrastra tus PDF aquí",
            _ => "Arrastra tu PDF aquí"
        };

        public string DropHint => AllowMultiple ? "o haz clic para elegirlos" : "o haz clic para elegirlo";

        public static ToolViewModel Create(ToolDescriptor descriptor, AppServices services) => descriptor.Kind switch
        {
            ToolKind.Merge => new MergeViewModel(descriptor, services),
            ToolKind.ExtractRange => new ExtractRangeViewModel(descriptor, services),
            ToolKind.DeletePages => new DeletePagesViewModel(descriptor, services),
            ToolKind.NumberPages => new NumberPagesViewModel(descriptor, services),
            ToolKind.ImageToPdf => new ImageToPdfViewModel(descriptor, services),
            ToolKind.PdfToImage => new PdfToImageViewModel(descriptor, services),
            ToolKind.Organize => new OrganizeViewModel(descriptor, services),
            _ => throw new ArgumentOutOfRangeException(nameof(descriptor))
        };

        public void AddFiles(IReadOnlyList<IInputFile> files)
        {
            var accepted = files.Where(file => Descriptor.InputKind.Matches(file.Name)).ToList();
            var ignored = files.Count - accepted.Count;
            if (ignored > 0)
            {
                var kind = Descriptor.InputKind == FileKind.Image ? "imagen" : "PDF";
                var message = ignored == 1
                    ? $"1 archivo no es {kind}."
                    : $"{ignored} archivos no son {(kind == "imagen" ? "imágenes" : kind)}.";
                Services.Notifications.Info("Archivos ignorados", message);
            }

            if (accepted.Count == 0)
                return;

            if (!AllowMultiple)
            {
                ClearFiles();
                accepted = accepted.Take(1).ToList();
            }

            foreach (var file in accepted)
            {
                var selected = new SelectedFile(file);
                Files.Add(selected);
                _ = LoadDetailsAsync(selected);
            }
        }

        protected virtual Task LoadDetailsAsync(SelectedFile file) =>
            file.LoadDetailsAsync(Descriptor.InputKind, Services.Renderer);

        protected virtual void OnFilesChanged()
        {
        }

        /// <summary>
        /// Asks where to save, runs <paramref name="write"/> with progress and cancellation, and removes the
        /// incomplete file when the operation fails or is cancelled.
        /// </summary>
        protected async Task SaveResultAsync(FileKind kind, string suggestedName, string busyMessage,
            IEnumerable<IInputFile> inputs, Func<Stream, IProgress<double>, CancellationToken, Task> write)
        {
            var output = await Services.FileDialogs.SaveAsync(kind, suggestedName);
            if (output is null)
                return;

            if (inputs.Any(input => input.Path is not null && input.Path == output.Path))
            {
                Services.Notifications.Error("Elige otro nombre",
                    "No se puede guardar encima de un archivo que se está usando como entrada.");
                return;
            }

            var succeeded = await Services.Runner.RunAsync(busyMessage, async (progress, cancellationToken) =>
            {
                await using var stream = await output.OpenWriteAsync();
                if (stream.CanSeek)
                    stream.SetLength(0);

                await write(stream, progress, cancellationToken);
            });

            if (succeeded)
            {
                Services.Notifications.Success("Listo", $"Se guardó {output.Name}", "Abrir", async () =>
                {
                    if (!await Services.OpenFile(output))
                        Services.Notifications.Error("No se pudo abrir el archivo", output.Name);
                });
                return;
            }

            try
            {
                await output.DeleteAsync();
            }
            catch (Exception)
            {
                Services.Notifications.Error("No se pudo borrar el archivo incompleto", output.Name);
            }
        }

        protected static string BaseName(string fileName) => Path.GetFileNameWithoutExtension(fileName);

        [RelayCommand]
        private async Task BrowseAsync()
        {
            try
            {
                AddFiles(await Services.FileDialogs.OpenAsync(Descriptor.InputKind, AllowMultiple));
            }
            catch (Exception ex)
            {
                Services.Notifications.Error("No se pudo abrir el selector de archivos", ex.Message);
            }
        }

        [RelayCommand]
        private void AddDroppedFiles(IReadOnlyList<IStorageFile> files) =>
            AddFiles(files.Select(file => (IInputFile)new StorageInputFile(file)).ToList());

        [RelayCommand]
        private void RemoveFile(SelectedFile file)
        {
            Files.Remove(file);
            file.Dispose();
        }

        [RelayCommand]
        private void ClearFiles()
        {
            var removed = Files.ToList();
            Files.Clear();
            foreach (var file in removed)
                file.Dispose();
        }

        private void OnFilesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(HasFiles));
            OnFilesChanged();
        }
    }
}
