using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Espuma.Application.Services;
using Espuma.Presentation.Services;

namespace Espuma.Presentation.ViewModels
{
    public sealed partial class ImageToPdfViewModel(ToolDescriptor descriptor, AppServices services)
        : ToolViewModel(descriptor, services)
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(OriginalSize))]
        public partial bool FitToA4 { get; set; } = true;

        public bool OriginalSize
        {
            get => !FitToA4;
            set => FitToA4 = !value;
        }

        public string ActionText => Files.Count > 1 ? $"Crear PDF con {Files.Count} imágenes" : "Crear PDF";

        private bool CanConvert => Files.Count > 0;

        protected override void OnFilesChanged()
        {
            OnPropertyChanged(nameof(ActionText));
            ConvertCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanConvert))]
        private Task ConvertAsync()
        {
            var inputs = Files.Select(file => file.File).ToList();
            var layout = FitToA4 ? ImagePageLayout.FitToA4 : ImagePageLayout.OriginalSize;
            var name = inputs.Count == 1 ? $"{BaseName(inputs[0].Name)}.pdf" : "imagenes.pdf";

            return SaveResultAsync(FileKind.Pdf, name, "Creando el PDF…", inputs, async (output, progress, cancellationToken) =>
            {
                var streams = new List<Stream>();
                try
                {
                    foreach (var input in inputs)
                        streams.Add(await input.OpenReadAsync());

                    await Services.ImageToPdf.ConvertAsync(streams, output, layout, progress, cancellationToken);
                }
                finally
                {
                    foreach (var stream in streams)
                        await stream.DisposeAsync();
                }
            });
        }
    }
}
