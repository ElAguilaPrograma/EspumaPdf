using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;

namespace Espuma.Presentation.Controls
{
    /// <summary>
    /// Area that accepts files dragged from the file manager and opens the file picker when clicked.
    /// Dropped files are passed to <see cref="FilesDroppedCommand"/> as an <see cref="IReadOnlyList{T}"/> of
    /// <see cref="IStorageFile"/>.
    /// </summary>
    public sealed class DropZone : Button
    {
        public static readonly StyledProperty<string?> TitleProperty =
            AvaloniaProperty.Register<DropZone, string?>(nameof(Title));

        public static readonly StyledProperty<string?> HintProperty =
            AvaloniaProperty.Register<DropZone, string?>(nameof(Hint));

        public static readonly StyledProperty<Geometry?> IconProperty =
            AvaloniaProperty.Register<DropZone, Geometry?>(nameof(Icon));

        public static readonly StyledProperty<ICommand?> FilesDroppedCommandProperty =
            AvaloniaProperty.Register<DropZone, ICommand?>(nameof(FilesDroppedCommand));

        public DropZone()
        {
            DragDrop.SetAllowDrop(this, true);
            AddHandler(DragDrop.DragEnterEvent, OnDragEnter);
            AddHandler(DragDrop.DragOverEvent, OnDragOver);
            AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
            AddHandler(DragDrop.DropEvent, OnDrop);
        }

        protected override Type StyleKeyOverride => typeof(DropZone);

        public string? Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string? Hint
        {
            get => GetValue(HintProperty);
            set => SetValue(HintProperty, value);
        }

        public Geometry? Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public ICommand? FilesDroppedCommand
        {
            get => GetValue(FilesDroppedCommandProperty);
            set => SetValue(FilesDroppedCommandProperty, value);
        }

        private void OnDragEnter(object? sender, DragEventArgs e)
        {
            var accepts = e.DataTransfer.Contains(DataFormat.File);
            e.DragEffects = accepts ? DragDropEffects.Copy : DragDropEffects.None;
            PseudoClasses.Set(":dragover", accepts);
        }

        private void OnDragOver(object? sender, DragEventArgs e) =>
            e.DragEffects = e.DataTransfer.Contains(DataFormat.File) ? DragDropEffects.Copy : DragDropEffects.None;

        private void OnDragLeave(object? sender, RoutedEventArgs e) => PseudoClasses.Set(":dragover", false);

        private void OnDrop(object? sender, DragEventArgs e)
        {
            PseudoClasses.Set(":dragover", false);
            var files = e.DataTransfer.TryGetFiles()?.OfType<IStorageFile>().ToList();
            if (files is { Count: > 0 } && FilesDroppedCommand?.CanExecute(files) == true)
                FilesDroppedCommand.Execute(files);
        }
    }
}
