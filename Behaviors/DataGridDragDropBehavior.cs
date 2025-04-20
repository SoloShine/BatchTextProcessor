
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
        private object? draggedItem;

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

        private Point? dragStartPoint;
        private const double DragThreshold = 5.0; // 最小拖动距离阈值

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 检查是否在编辑状态（通过KeyboardFocus或EditingControl）
            if (Keyboard.FocusedElement is TextBox || 
                Keyboard.FocusedElement is ComboBox ||
                Keyboard.FocusedElement is DatePicker)
            {
                draggedItem = null;
                return;
            }

            var row = FindAncestor<DataGridRow>(e.OriginalSource as DependencyObject);
            if (row != null)
            {
                draggedItem = row.Item;
                dragStartPoint = e.GetPosition(AssociatedObject);
            }
        }

        private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            draggedItem = null;
            dragStartPoint = null;
        }

        private void OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || 
                draggedItem == null || 
                dragStartPoint == null)
            {
                return;
            }

            // 检查是否达到最小拖动距离
            var currentPoint = e.GetPosition(AssociatedObject);
            if (Math.Abs(currentPoint.X - dragStartPoint.Value.X) < DragThreshold &&
                Math.Abs(currentPoint.Y - dragStartPoint.Value.Y) < DragThreshold)
            {
                return;
            }

            // 检查是否在可拖动区域
            var hitTest = VisualTreeHelper.HitTest(AssociatedObject, currentPoint);
            if (hitTest == null || FindAncestor<DataGridRow>(hitTest.VisualHit) == null)
            {
                return;
            }

            DragDrop.DoDragDrop(AssociatedObject, draggedItem, DragDropEffects.Move);
            dragStartPoint = null;
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
