using System.Collections;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Espuma.Presentation.Behaviors
{
    /// <summary>A request to move the item at <see cref="From"/> to <see cref="To"/>.</summary>
    public sealed record ReorderMove(int From, int To);

    /// <summary>
    /// Lets the user reorder the items of an <see cref="ItemsControl"/> by dragging them. Moves are sent to
    /// <c>MoveCommand</c> as a <see cref="ReorderMove"/> when it is set; otherwise the items source, which must be an
    /// <see cref="IList"/>, is changed directly. Dragging does not start on buttons, so actions inside the items keep
    /// working.
    /// </summary>
    public static class Reorder
    {
        private const double DragThreshold = 6;
        private const string DropTargetClass = "droptarget";
        private static readonly DataFormat<string> ItemIndexFormat = DataFormat.CreateInProcessFormat<string>("espuma-reorder-index");

        public static readonly AttachedProperty<bool> IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<ItemsControl, bool>("IsEnabled", typeof(Reorder));

        public static readonly AttachedProperty<ICommand?> MoveCommandProperty =
            AvaloniaProperty.RegisterAttached<ItemsControl, ICommand?>("MoveCommand", typeof(Reorder));

        private static readonly AttachedProperty<DragStart?> DragStartProperty =
            AvaloniaProperty.RegisterAttached<ItemsControl, DragStart?>("DragStart", typeof(Reorder));

        private static readonly AttachedProperty<Control?> DropTargetProperty =
            AvaloniaProperty.RegisterAttached<ItemsControl, Control?>("DropTarget", typeof(Reorder));

        static Reorder()
        {
            IsEnabledProperty.Changed.AddClassHandler<ItemsControl>(OnIsEnabledChanged);
        }

        public static bool GetIsEnabled(ItemsControl control) => control.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(ItemsControl control, bool value) => control.SetValue(IsEnabledProperty, value);

        public static ICommand? GetMoveCommand(ItemsControl control) => control.GetValue(MoveCommandProperty);

        public static void SetMoveCommand(ItemsControl control, ICommand? value) => control.SetValue(MoveCommandProperty, value);

        private static void OnIsEnabledChanged(ItemsControl control, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is not true)
                return;

            DragDrop.SetAllowDrop(control, true);
            control.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
            control.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, RoutingStrategies.Tunnel);
            control.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
            control.AddHandler(DragDrop.DragOverEvent, OnDragOver);
            control.AddHandler(DragDrop.DragLeaveEvent, OnDragLeave);
            control.AddHandler(DragDrop.DropEvent, OnDrop);
        }

        private static void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (sender is not ItemsControl control || !e.GetCurrentPoint(control).Properties.IsLeftButtonPressed)
                return;

            if (e.Source is Visual source && source.FindAncestorOfType<Button>(includeSelf: true) is not null)
                return;

            var index = IndexAt(control, e.Source as Visual);
            control.SetValue(DragStartProperty, index >= 0 ? new DragStart(e, index, e.GetPosition(control)) : null);
        }

        private static async void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (sender is not ItemsControl control || control.GetValue(DragStartProperty) is not { } start)
                return;

            var delta = e.GetPosition(control) - start.Position;
            if (Math.Abs(delta.X) < DragThreshold && Math.Abs(delta.Y) < DragThreshold)
                return;

            control.SetValue(DragStartProperty, null);
            var data = new DataTransfer();
            data.Add(DataTransferItem.Create(ItemIndexFormat, start.Index.ToString()));
            await DragDrop.DoDragDropAsync(start.PressedArgs, data, DragDropEffects.Move);
            SetDropTarget(control, null);
        }

        private static void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (sender is ItemsControl control)
                control.SetValue(DragStartProperty, null);
        }

        private static void OnDragOver(object? sender, DragEventArgs e)
        {
            if (sender is not ItemsControl control || !e.DataTransfer.Contains(ItemIndexFormat))
                return;

            e.DragEffects = DragDropEffects.Move;
            var index = IndexAt(control, e.Source as Visual);
            SetDropTarget(control, index >= 0 ? control.ContainerFromIndex(index) : null);
            e.Handled = true;
        }

        private static void OnDragLeave(object? sender, DragEventArgs e)
        {
            if (sender is ItemsControl control)
                SetDropTarget(control, null);
        }

        private static void OnDrop(object? sender, DragEventArgs e)
        {
            if (sender is not ItemsControl control)
                return;

            SetDropTarget(control, null);
            if (e.DataTransfer.TryGetValue(ItemIndexFormat) is not { } text || !int.TryParse(text, out var from))
                return;

            var to = IndexAt(control, e.Source as Visual);
            if (to < 0 || from == to || from >= control.ItemCount)
                return;

            e.Handled = true;
            if (GetMoveCommand(control) is { } command)
            {
                var move = new ReorderMove(from, to);
                if (command.CanExecute(move))
                    command.Execute(move);
                return;
            }

            if (control.ItemsSource is IList items)
            {
                var item = items[from];
                items.RemoveAt(from);
                items.Insert(to, item);
            }
        }

        private static int IndexAt(ItemsControl control, Visual? visual)
        {
            for (var current = visual; current is not null && current != control; current = current.GetVisualParent())
            {
                if (current is Control candidate && control.IndexFromContainer(candidate) is var index and >= 0)
                    return index;
            }

            return -1;
        }

        private static void SetDropTarget(ItemsControl control, Control? target)
        {
            var previous = control.GetValue(DropTargetProperty);
            if (previous == target)
                return;

            previous?.Classes.Remove(DropTargetClass);
            target?.Classes.Add(DropTargetClass);
            control.SetValue(DropTargetProperty, target);
        }

        private sealed record DragStart(PointerPressedEventArgs PressedArgs, int Index, Point Position);
    }
}
