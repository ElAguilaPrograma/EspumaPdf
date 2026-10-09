using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Espuma.Presentation.Services;
using Espuma.Presentation.ViewModels;
using Espuma.Presentation.Views;

namespace Espuma.Presentation
{
    public partial class App : Avalonia.Application
    {
        public override void Initialize() => AvaloniaXamlLoader.Load(this);

        public override void OnFrameworkInitializationCompleted()
        {
            var mainView = new MainView();
            TopLevel? GetTopLevel() => TopLevel.GetTopLevel(mainView);

            var services = new AppServices(
                new FileDialogService(GetTopLevel),
                uri => GetTopLevel()?.Launcher.LaunchUriAsync(uri) ?? Task.FromResult(false),
                file => file is StorageOutputFile storage && GetTopLevel() is { } topLevel
                    ? topLevel.Launcher.LaunchFileAsync(storage.File)
                    : Task.FromResult(false));
            mainView.DataContext = new MainViewModel(services);

            switch (ApplicationLifetime)
            {
                case IClassicDesktopStyleApplicationLifetime desktop:
                    desktop.MainWindow = new MainWindow { Content = mainView };
                    break;
                case ISingleViewApplicationLifetime singleView:
                    singleView.MainView = mainView;
                    break;
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
