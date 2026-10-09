using CommunityToolkit.Mvvm.ComponentModel;
using Espuma.Presentation.Services;

namespace Espuma.Presentation.ViewModels
{
    public sealed partial class SelectedFile(IInputFile file) : ObservableObject
    {
        public IInputFile File { get; } = file;
        public string Name => File.Name;

        [ObservableProperty]
        public partial string SizeText { get; private set; } = "";

        public async Task LoadSizeAsync()
        {
            try
            {
                SizeText = FormatSize(await File.GetSizeAsync() ?? 0);
            }
            catch (Exception)
            {
                SizeText = "";
            }
        }

        private static string FormatSize(ulong bytes) => bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024d:0.#} KB",
            _ => $"{bytes / 1024d / 1024d:0.#} MB"
        };
    }
}
