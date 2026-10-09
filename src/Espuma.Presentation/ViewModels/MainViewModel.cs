using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Espuma.Presentation.Services;

namespace Espuma.Presentation.ViewModels
{
    public sealed partial class MainViewModel : ViewModelBase
    {
        public const string RepositoryUrl = "https://github.com/ElAguilaPrograma/EspumaPdf";

        private readonly AppServices _services;
        private readonly HomeViewModel _home;
        private readonly Dictionary<ToolKind, ToolViewModel> _tools = [];

        public MainViewModel(AppServices services)
        {
            _services = services;
            _home = new HomeViewModel(tool => SelectedTool = tool);
            CurrentPage = _home;
        }

        public IReadOnlyList<ToolDescriptor> Tools => ToolDescriptor.All;
        public OperationRunner Runner => _services.Runner;
        public NotificationService Notifications => _services.Notifications;
        public string Version { get; } = "v" + (typeof(MainViewModel).Assembly.GetName().Version?.ToString(3) ?? "0.0.0");

        [ObservableProperty]
        public partial ViewModelBase CurrentPage { get; private set; }

        [ObservableProperty]
        public partial ToolDescriptor? SelectedTool { get; set; }

        [ObservableProperty]
        public partial bool IsSidebarCompact { get; set; }

        public bool IsHomeSelected => SelectedTool is null;

        partial void OnSelectedToolChanged(ToolDescriptor? value)
        {
            OnPropertyChanged(nameof(IsHomeSelected));

            if (value is null)
            {
                CurrentPage = _home;
                return;
            }

            if (!_tools.TryGetValue(value.Kind, out var tool))
            {
                tool = ToolViewModel.Create(value, _services);
                _tools[value.Kind] = tool;
            }

            CurrentPage = tool;
        }

        [RelayCommand]
        private void GoHome() => SelectedTool = null;

        [RelayCommand]
        private async Task OpenRepositoryAsync()
        {
            if (!await _services.OpenUri(new Uri(RepositoryUrl)))
                _services.Notifications.Error("No se pudo abrir el navegador", RepositoryUrl);
        }
    }
}
