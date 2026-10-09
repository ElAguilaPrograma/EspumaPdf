using CommunityToolkit.Mvvm.Input;

namespace Espuma.Presentation.ViewModels
{
    public sealed partial class HomeViewModel(Action<ToolDescriptor> openTool) : ViewModelBase
    {
        public IReadOnlyList<ToolDescriptor> Tools => ToolDescriptor.All;

        [RelayCommand]
        private void OpenTool(ToolDescriptor tool) => openTool(tool);
    }
}
