using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Espuma.Application.Services;
using Espuma.Presentation.Services;

namespace Espuma.Presentation.ViewModels
{
    public sealed record ImageQualityOption(string Label, double Scale);

    public sealed record ImageFormatOption(string Label, ImageExportFormat Format);

    public sealed partial class PdfToImageViewModel : SinglePdfToolViewModel
    {
        public PdfToImageViewModel(ToolDescriptor descriptor, AppServices services) : base(descriptor, services)
        {
            SelectedQuality = Qualities[1];
            SelectedFormat = ImageFormats[0];
        }

        public IReadOnlyList<ImageQualityOption> Qualities { get; } =
        [
            new("Normal (72 ppp)", 1),
            new("Alta (144 ppp)", 2),
            new("Muy alta (216 ppp)", 3)
        ];

        public IReadOnlyList<ImageFormatOption> ImageFormats { get; } =
        [
            new("PNG (sin pérdida)", ImageExportFormat.Png),
            new("JPEG (más ligero)", ImageExportFormat.Jpeg)
        ];

        [ObservableProperty]
        public partial ImageQualityOption SelectedQuality { get; set; }

        [ObservableProperty]
        public partial ImageFormatOption SelectedFormat { get; set; }

        public string ActionText => PageCount switch
        {
            0 => "Exportar imágenes",
            1 => "Exportar 1 imagen",
            var n => $"Exportar {n} imágenes"
        };

        protected override void OnDocumentChanged()
        {
            OnPropertyChanged(nameof(ActionText));
            ExportCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(HasDocument))]
        private Task ExportAsync()
        {
            var options = new PdfToImageOptions(SelectedQuality.Scale, SelectedFormat.Format);

            return SaveResultAsync(FileKind.Zip, $"{BaseName(File!.Name)}-imagenes.zip", "Exportando páginas…",
                [File.File], async (output, progress, cancellationToken) =>
                {
                    await using var input = await OpenInputAsync();
                    await Services.PdfToImage.ExportToZipAsync(input, output, options, progress, cancellationToken);
                });
        }
    }
}
