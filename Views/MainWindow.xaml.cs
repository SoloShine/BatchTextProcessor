
using System.Windows;
using System.Windows.Controls;

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
