using System.Collections.ObjectModel;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Espuma.Presentation.Services;

namespace Espuma.Presentation.ViewModels
{
    public partial class ToolViewModel : ViewModelBase
    {
        protected ToolViewModel(ToolDescriptor descriptor, AppServices services)
        {
            Descriptor = descriptor;
            Services = services;
            Files.CollectionChanged += (_, _) =>
            {
                OnPropertyChanged(nameof(HasFiles));
                OnFilesChanged();
            };
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

        public static ToolViewModel Create(ToolDescriptor descriptor, AppServices services) =>
            new(descriptor, services);

        protected virtual void OnFilesChanged()
        {
        }

        [RelayCommand]
        private async Task BrowseAsync()
        {
            var files = await Services.FileDialogs.OpenAsync(Descriptor.InputKind, AllowMultiple);
            AddFiles(files);
        }

        [RelayCommand]
        private void AddDroppedFiles(IReadOnlyList<IStorageFile> files) =>
            AddFiles(files.Select(file => (IInputFile)new StorageInputFile(file)).ToList());

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
                Files.Clear();
                accepted = accepted.Take(1).ToList();
            }

            foreach (var file in accepted)
            {
                var selected = new SelectedFile(file);
                Files.Add(selected);
                _ = selected.LoadSizeAsync();
            }
        }

        [RelayCommand]
        private void RemoveFile(SelectedFile file) => Files.Remove(file);

        [RelayCommand]
        private void ClearFiles() => Files.Clear();
    }
}
