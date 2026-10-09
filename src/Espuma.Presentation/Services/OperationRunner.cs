using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Espuma.Application.Common;

namespace Espuma.Presentation.Services
{
    /// <summary>
    /// Runs one long operation at a time, exposing its progress for the busy overlay and reporting the
    /// outcome through notifications.
    /// </summary>
    public sealed partial class OperationRunner(NotificationService notifications) : ObservableObject
    {
        private CancellationTokenSource? _cancellation;

        [ObservableProperty]
        public partial bool IsBusy { get; private set; }

        [ObservableProperty]
        public partial string Message { get; private set; } = "";

        [ObservableProperty]
        public partial double Progress { get; private set; }

        [ObservableProperty]
        public partial bool IsIndeterminate { get; private set; }

        /// <returns>True when the operation finished successfully.</returns>
        public async Task<bool> RunAsync(string message, Func<IProgress<double>, CancellationToken, Task> operation)
        {
            if (IsBusy)
                return false;

            using var cancellation = new CancellationTokenSource();
            _cancellation = cancellation;
            Message = message;
            Progress = 0;
            IsIndeterminate = true;
            IsBusy = true;

            var progress = new Progress<double>(value =>
            {
                IsIndeterminate = false;
                Progress = value * 100;
            });

            try
            {
                await operation(progress, cancellation.Token);
                return true;
            }
            catch (OperationCanceledException)
            {
                notifications.Info("Operación cancelada");
            }
            catch (PdfOperationException ex)
            {
                notifications.Error(ErrorMessages.TitleFor(ex.Code), ex.Message);
            }
            catch (Exception ex)
            {
                notifications.Error("Algo salió mal", ex.Message);
            }
            finally
            {
                IsBusy = false;
                _cancellation = null;
            }

            return false;
        }

        [RelayCommand]
        private void Cancel() => _cancellation?.Cancel();
    }
}
