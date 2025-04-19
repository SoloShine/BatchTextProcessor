
using System.Windows;
using BatchTextProcessor.ViewModels;

namespace BatchTextProcessor.Views
{
    public partial class PreviewWindow : Window
    {
        public PreviewWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
