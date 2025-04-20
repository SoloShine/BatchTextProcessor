
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using static ImTools.ImMap;
using static System.Net.Mime.MediaTypeNames;

namespace BatchTextProcessor.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            
            // 添加键盘事件处理
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
        }

        #region 调用WindosAPI C++动态库 的剪切板相关函数来解决无法写入剪切板的问题。如有软件定时扫描剪切板（迅雷、百度网盘、向日葵等）的时候，wpf的剪切板会写入失败
        [DllImport("User32")]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("User32")]
        private static extern bool CloseClipboard();

        [DllImport("User32")]
        private static extern bool EmptyClipboard();

        [DllImport("User32")]
        private static extern bool IsClipboardFormatAvailable(int format);

        [DllImport("User32")]
        private static extern IntPtr GetClipboardData(int uFormat);

        [DllImport("User32", CharSet = CharSet.Unicode)]
        private static extern IntPtr SetClipboardData(int uFormat, IntPtr hMem);
        #endregion

        private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                if (e.Key == System.Windows.Input.Key.C)
                {
                    CopyCellContent();
                    e.Handled = true;
                }
                else if (e.Key == System.Windows.Input.Key.V)
                {
                    PasteToCell();
                    e.Handled = true;
                }
                else if (e.Key == System.Windows.Input.Key.S)
                {
                    var vm = DataContext as ViewModels.MainWindowViewModel;
                    vm?.SaveProjectCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }

        private void CopyCellContent()
        {
            try
            {                
                if (FilesDataGrid?.SelectedItem is Models.TextFileItem selectedItem && 
                    !string.IsNullOrEmpty(selectedItem.MergedName))
                {
                    if (!OpenClipboard(IntPtr.Zero))
                    {
                        CopyCellContent();
                        return;
                    }
                    EmptyClipboard();
                    //System.Windows.Clipboard.Clear();
                    //System.Windows.Clipboard.SetText(selectedItem.MergedName);
                    SetClipboardData(13, Marshal.StringToHGlobalUni(selectedItem.MergedName));
                    CloseClipboard();
                }
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                MessageBox.Show("无法访问剪切板，请稍后重试："+e.Message, "剪切板错误", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void PasteToCell()
        {
            var text = System.Windows.Clipboard.GetText();
            if (string.IsNullOrEmpty(text)) return;
            //粘贴给多个选中的单元
            foreach (var item in FilesDataGrid.SelectedItems)
            {
                if (item is Models.TextFileItem textFileItem)
                {
                    textFileItem.MergedName = text;
                }
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.MainWindowViewModel viewModel)
            {
                if (viewModel.InitializeCommand.CanExecute(null))
                {
                    viewModel.InitializeCommand.Execute(null);
                }
            }
        }
    }
}
