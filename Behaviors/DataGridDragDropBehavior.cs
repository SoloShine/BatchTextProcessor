
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using System.Windows.Media;
using System.Collections;

namespace BatchTextProcessor.Behaviors
{
    public class DataGridDragDropBehavior : Behavior<DataGrid>
    {
        private object draggedItem;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove += OnPreviewMouseMove;
            AssociatedObject.Drop += OnDrop;
            AssociatedObject.AllowDrop = true;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            AssociatedObject.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
            AssociatedObject.PreviewMouseMove -= OnPreviewMouseMove;
            AssociatedObject.Drop -= OnDrop;
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var row = FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject);
            if (row != null)
            {
                draggedItem = row.Item;
            }
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            draggedItem = null;
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && draggedItem != null)
            {
                DragDrop.DoDragDrop(AssociatedObject, draggedItem, DragDropEffects.Move);
            }
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (!(sender is DataGrid dataGrid)) return;
            if (!(FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject)?.Item is { } targetItem)) return;
            if (!(e.Data.GetData(targetItem.GetType()) is { } sourceItem)) return;
            if (!(dataGrid.ItemsSource is IList itemsSource)) return;

            int sourceIndex = itemsSource.IndexOf(sourceItem);
            int targetIndex = itemsSource.IndexOf(targetItem);

            if (sourceIndex != -1 && targetIndex != -1 && sourceIndex != targetIndex)
            {
                itemsSource.RemoveAt(sourceIndex);
                itemsSource.Insert(targetIndex, sourceItem);
                dataGrid.SelectedItem = sourceItem;
                
                if (dataGrid.DataContext is BatchTextProcessor.ViewModels.MainWindowViewModel vm)
                {
                    vm.RefreshIndices();
                }
            }
        }

        private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
