using CommunityToolkit.Mvvm.Input;
using Espuma.Presentation.Services;

namespace Espuma.Presentation.ViewModels
{
    public sealed partial class MergeViewModel(ToolDescriptor descriptor, AppServices services)
        : ToolViewModel(descriptor, services)
    {
        public string ActionText => Files.Count >= 2 ? $"Unir {Files.Count} PDF" : "Unir PDF";
        public bool NeedsMoreFiles => Files.Count == 1;

        private bool CanMerge => Files.Count >= 2;

        protected override void OnFilesChanged()
        {
            OnPropertyChanged(nameof(ActionText));
            OnPropertyChanged(nameof(NeedsMoreFiles));
            MergeCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand(CanExecute = nameof(CanMerge))]
        private Task MergeAsync()
        {
            var inputs = Files.Select(file => file.File).ToList();
            return SaveResultAsync(FileKind.Pdf, $"{BaseName(inputs[0].Name)}-combinado.pdf",
                $"Uniendo {inputs.Count} archivos…", inputs, async (output, progress, cancellationToken) =>
                {
                    var streams = new List<Stream>();
                    try
                    {
                        foreach (var input in inputs)
                            streams.Add(await input.OpenReadAsync());

                        await Services.Merge.MergeAsync(streams, output, progress, cancellationToken);
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
