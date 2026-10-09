using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Espuma.Presentation.Services
{
    public enum ToastKind
    {
        Info,
        Success,
        Error
    }

    public sealed partial class Toast(ToastKind kind, string title, string message, Action<Toast> dismiss) : ObservableObject
    {
        public ToastKind Kind { get; } = kind;
        public string Title { get; } = title;
        public string Message { get; } = message;
        public bool HasMessage => Message.Length > 0;
        public bool IsSuccess => Kind == ToastKind.Success;
        public bool IsError => Kind == ToastKind.Error;
        public string IconKey => Kind switch
        {
            ToastKind.Success => "Success",
            ToastKind.Error => "Error",
            _ => "Info"
        };

        [RelayCommand]
        private void Dismiss() => dismiss(this);
    }

    public sealed class NotificationService
    {
        private const int MaxVisible = 3;
        private static readonly TimeSpan Duration = TimeSpan.FromSeconds(5);

        public ObservableCollection<Toast> Toasts { get; } = [];

        public void Info(string title, string message = "") => Show(ToastKind.Info, title, message);

        public void Success(string title, string message = "") => Show(ToastKind.Success, title, message);

        public void Error(string title, string message = "") => Show(ToastKind.Error, title, message);

        private void Show(ToastKind kind, string title, string message)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var toast = new Toast(kind, title, message, Remove);
                Toasts.Add(toast);
                while (Toasts.Count > MaxVisible)
                    Toasts.RemoveAt(0);

                DispatcherTimer.RunOnce(() => Remove(toast), Duration);
            });
        }

        private void Remove(Toast toast) => Toasts.Remove(toast);
    }
}
