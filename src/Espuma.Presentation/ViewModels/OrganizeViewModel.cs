using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Espuma.Application.Common;
using Espuma.Presentation.Behaviors;
using Espuma.Presentation.Services;
using Espuma.Presentation.Thumbnails;

namespace Espuma.Presentation.ViewModels
{
    public sealed partial class OrganizeViewModel(ToolDescriptor descriptor, AppServices services)
        : SinglePdfToolViewModel(descriptor, services)
    {
        private const int MaxHistory = 100;

        private readonly Stack<Snapshot> _undo = new();
        private readonly Stack<Snapshot> _redo = new();
        private PageThumbnailViewModel? _anchor;

        /// <summary>Pages in their new order, with their rotation. Removed pages are not in the list.</summary>
        public ObservableCollection<PageThumbnailViewModel> Items { get; } = [];

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasSelection), nameof(StatusText))]
        [NotifyCanExecuteChangedFor(nameof(RotateSelectionLeftCommand), nameof(RotateSelectionRightCommand), nameof(DeleteSelectionCommand))]
        public partial int SelectedCount { get; private set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StatusText))]
        [NotifyCanExecuteChangedFor(nameof(UndoCommand), nameof(RedoCommand), nameof(ResetCommand), nameof(SaveCommand))]
        public partial bool HasChanges { get; private set; }

        public bool HasSelection => SelectedCount > 0;
        private bool CanUndo => _undo.Count > 0;
        private bool CanRedo => _redo.Count > 0;
        private bool CanSave => HasDocument && Items.Count > 0;

        public string StatusText
        {
            get
            {
                var pages = Items.Count == 1 ? "1 página" : $"{Items.Count} páginas";
                var selected = SelectedCount switch
                {
                    0 => "",
                    1 => " · 1 seleccionada",
                    var n => $" · {n} seleccionadas"
                };
                return pages + selected + (HasChanges ? " · con cambios" : "");
            }
        }

        protected override void OnDocumentLoaded(PdfPreview preview)
        {
            foreach (var page in preview.Pages)
                Items.Add(page);
        }

        protected override void OnDocumentChanged()
        {
            if (!HasDocument)
                Items.Clear();

            _undo.Clear();
            _redo.Clear();
            _anchor = null;
            RefreshState();
        }

        /// <summary>Click selects one page, Ctrl toggles it and Shift selects every page from the last clicked one.</summary>
        public void SelectPage(PageThumbnailViewModel page, bool toggle, bool extend)
        {
            if (extend && _anchor is not null && Items.IndexOf(_anchor) is var from and >= 0)
            {
                var to = Items.IndexOf(page);
                if (!toggle)
                    ClearSelectionState();

                for (var i = Math.Min(from, to); i <= Math.Max(from, to); i++)
                    Items[i].IsSelected = true;
            }
            else if (toggle)
            {
                page.IsSelected = !page.IsSelected;
                _anchor = page;
            }
            else
            {
                ClearSelectionState();
                page.IsSelected = true;
                _anchor = page;
            }

            UpdateSelectedCount();
        }

        [RelayCommand]
        private void SelectAll()
        {
            foreach (var page in Items)
                page.IsSelected = true;

            UpdateSelectedCount();
        }

        [RelayCommand]
        private void ClearSelection()
        {
            ClearSelectionState();
            UpdateSelectedCount();
        }

        [RelayCommand]
        private void Move(ReorderMove move)
        {
            if (move.From < 0 || move.From >= Items.Count || move.To < 0 || move.To >= Items.Count)
                return;

            Record();
            Items.Move(move.From, move.To);
            RefreshState();
        }

        [RelayCommand]
        private void RotateLeft(PageThumbnailViewModel page) => Rotate([page], clockwise: false);

        [RelayCommand]
        private void RotateRight(PageThumbnailViewModel page) => Rotate([page], clockwise: true);

        [RelayCommand(CanExecute = nameof(HasSelection))]
        private void RotateSelectionLeft() => Rotate(Selected(), clockwise: false);

        [RelayCommand(CanExecute = nameof(HasSelection))]
        private void RotateSelectionRight() => Rotate(Selected(), clockwise: true);

        [RelayCommand]
        private void DeletePage(PageThumbnailViewModel page) => Delete([page]);

        [RelayCommand(CanExecute = nameof(HasSelection))]
        private void DeleteSelection() => Delete(Selected());

        [RelayCommand(CanExecute = nameof(CanUndo))]
        private void Undo()
        {
            _redo.Push(Capture());
            Restore(_undo.Pop());
        }

        [RelayCommand(CanExecute = nameof(CanRedo))]
        private void Redo()
        {
            _undo.Push(Capture());
            Restore(_redo.Pop());
        }

        [RelayCommand(CanExecute = nameof(HasChanges))]
        private void Reset()
        {
            Record();
            Items.Clear();
            foreach (var page in Pages)
            {
                page.Rotation = PageRotation.None;
                Items.Add(page);
            }

            RefreshState();
        }

        [RelayCommand(CanExecute = nameof(CanSave))]
        private Task SaveAsync()
        {
            var selection = Items.Select(page => new PageSelection(page.PageIndex, page.Rotation)).ToList();

            return SaveResultAsync(FileKind.Pdf, $"{BaseName(File!.Name)}-organizado.pdf", "Guardando el PDF…",
                [File.File], async (output, _, cancellationToken) =>
                {
                    await using var input = await OpenInputAsync();
                    await Services.PageArrange.ArrangeAsync(input, selection, output, cancellationToken);
                });
        }

        private List<PageThumbnailViewModel> Selected() => Items.Where(page => page.IsSelected).ToList();

        private void Rotate(IReadOnlyList<PageThumbnailViewModel> pages, bool clockwise)
        {
            if (pages.Count == 0)
                return;

            Record();
            foreach (var page in pages)
                page.Rotation = clockwise ? page.Rotation.RotateClockwise() : page.Rotation.RotateCounterClockwise();

            RefreshState();
        }

        private void Delete(IReadOnlyList<PageThumbnailViewModel> pages)
        {
            if (pages.Count == 0)
                return;

            if (pages.Count >= Items.Count)
            {
                Services.Notifications.Info("No se pueden quitar todas las páginas", "El PDF debe conservar al menos una.");
                return;
            }

            Record();
            foreach (var page in pages)
            {
                page.IsSelected = false;
                Items.Remove(page);
            }

            if (_anchor is not null && !Items.Contains(_anchor))
                _anchor = null;

            RefreshState();
        }

        private void Record()
        {
            _undo.Push(Capture());
            _redo.Clear();
            if (_undo.Count > MaxHistory)
            {
                var kept = _undo.Take(MaxHistory).Reverse().ToList();
                _undo.Clear();
                foreach (var snapshot in kept)
                    _undo.Push(snapshot);
            }
        }

        private Snapshot Capture() => new(Items.ToArray(), Items.Select(page => page.Rotation).ToArray());

        private void Restore(Snapshot snapshot)
        {
            ClearSelectionState();
            Items.Clear();
            for (var i = 0; i < snapshot.Order.Length; i++)
            {
                snapshot.Order[i].Rotation = snapshot.Rotations[i];
                Items.Add(snapshot.Order[i]);
            }

            _anchor = null;
            RefreshState();
        }

        private void ClearSelectionState()
        {
            foreach (var page in Pages)
                page.IsSelected = false;
        }

        private void UpdateSelectedCount() => SelectedCount = Items.Count(page => page.IsSelected);

        private void RefreshState()
        {
            UpdateSelectedCount();
            HasChanges = Items.Count != Pages.Count
                || Items.Where((page, index) => page.PageIndex != index || page.Rotation != PageRotation.None).Any();
            OnPropertyChanged(nameof(StatusText));
            UndoCommand.NotifyCanExecuteChanged();
            RedoCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
        }

        private sealed record Snapshot(PageThumbnailViewModel[] Order, PageRotation[] Rotations);
    }
}
