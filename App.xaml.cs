
namespace BatchTextProcessor
{
    using Prism.DryIoc;
    using Prism.Ioc;
    using System.Windows;
    using BatchTextProcessor.Views;
    using BatchTextProcessor.Services;

    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            // 修改为返回您实际需要启动的窗口
            // 例如: return Container.Resolve<YourActualWindow>();
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ILogger, LoggerService>();
            containerRegistry.Register<FileScannerService>();
            containerRegistry.Register<FileMergeService>();
        }
    }
}
