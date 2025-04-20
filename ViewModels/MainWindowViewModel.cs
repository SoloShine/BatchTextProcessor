
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ookii.Dialogs.Wpf;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using BatchTextProcessor.Services;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Windows;
using System.Diagnostics;
using BatchTextProcessor.Utils;
using BatchTextProcessor.Models;

namespace BatchTextProcessor.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly FileScannerService _fileScanner = new();
        private readonly FileMergeService _fileMergeService;
        private readonly FileSplitService _fileSplitService = new();
        private readonly ProjectFileService _projectFileService;
        private readonly ILogger _logger = new LoggerService();

        public MainWindowViewModel()
        {
            _fileMergeService = new FileMergeService(_logger);
            _projectFileService = new ProjectFileService(_logger);
            SplitFileViewModel = new SplitFileViewModel();
            InitializeCommand = new RelayCommand(OnInitialize);
            
            OpenProjectCommand = new RelayCommand(OnOpenProject);
            SaveProjectCommand = new RelayCommand(OnSaveProject, CanSaveProject);
            ExitCommand = new RelayCommand(OnExit);
            AboutCommand = new RelayCommand(OnAbout);
            OpenFileLocationCommand = new RelayCommand<string>(OnOpenFileLocation);
        }

        public IRelayCommand InitializeCommand { get; }
        public IRelayCommand OpenProjectCommand { get; }
        public IRelayCommand SaveProjectCommand { get; }
        public IRelayCommand ExitCommand { get; }
        public IRelayCommand AboutCommand { get; }
        public IRelayCommand<string> OpenFileLocationCommand { get; }

        private void OnInitialize()
        {
            ClearList();
        }

        public SplitFileViewModel SplitFileViewModel { get; }

        [ObservableProperty]
        private ObservableCollection<TextFileItem> _fileItems = new();
        [ObservableProperty]
        private bool _canExport = true;

        [ObservableProperty]
        private string _minMatchChars = "5";

        [ObservableProperty] 
        private string _maxMatchChars = "20";

        partial void OnMinMatchCharsChanged(string value)
        {
            if(int.TryParse(value, out int num) && num >= 1 && num <= 50)
            {
                RefreshMergeNamesCommand.Execute(null);
            }
        }

        partial void OnMaxMatchCharsChanged(string value)
        {
            if(int.TryParse(value, out int num) && num >= 1 && num <= 50)
            {
                RefreshMergeNamesCommand.Execute(null);
            }
        }

        public void RefreshIndices()
        {
            for (int i = 0; i < FileItems.Count; i++)
            {
                FileItems[i].Index = i + 1;
            }
        }

        public ObservableCollection<TextFileItem> GetFilesForPreview(string previewItem)
        {
            if (string.IsNullOrEmpty(previewItem))
                return new ObservableCollection<TextFileItem>();

            var mergeName = previewItem.Split(':').LastOrDefault()?.Trim();
            if (string.IsNullOrEmpty(mergeName))
                return new ObservableCollection<TextFileItem>();

            var files = FileItems
                .Where(f => f.MergedName == mergeName && f.ShouldExport)
                .ToList();

            return new ObservableCollection<TextFileItem>(files);
        }

        [RelayCommand]
        private void PasteToSelectedItems(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            
            var selectedItems = FileItems.Where(f => f.IsSelectedForDeletion).ToList();
            if (selectedItems.Count == 0) return;

            foreach (var item in selectedItems)
            {
                item.MergedName = text;
            }
            
            CanExport = FileItems.Any(f => f.ShouldExport && !string.IsNullOrEmpty(f.MergedName));
        }

        private bool CanSaveProject()
        {
            return FileItems.Any();
        }

        private void OnOpenProject()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "工程文件|*.btp",
                Title = "打开工程文件"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var items = _projectFileService.LoadProject(dialog.FileName);
                    FileItems.Clear();
                    foreach (var item in items)
                    {
                        FileItems.Add(item);
                    }
                    //从工程文件中加载的时候，不需要自动设置合并名称，因为工程文件已经包含了这些信息
                    //AutoSetMergeNames();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"打开工程文件失败: {ex.Message}");
                    MessageBox.Show("打开工程文件失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            SaveProjectCommand.NotifyCanExecuteChanged();
        }

        private void OnSaveProject()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "工程文件|*.btp",
                Title = "保存工程文件"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _projectFileService.SaveProject(dialog.FileName, FileItems);
                    MessageBox.Show("工程文件保存成功", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"保存工程文件失败: {ex.Message}");
                    MessageBox.Show("保存工程文件失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OnExit()
        {
            Application.Current.Shutdown();
        }

        private void OnAbout()
        {
            var aboutWindow = new Views.AboutWindow();
            aboutWindow.ShowDialog();
        }

        private void OnOpenFileLocation(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    var args = $"/select,\"{filePath}\"";
                    Process.Start("explorer.exe", args);
                }
                else
                {
                    _logger.LogWarning($"尝试打开不存在的文件位置: {filePath} (文件不存在)");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"打开文件位置失败: {ex.Message}");
            }
        }

        [RelayCommand]
        private void AddFiles()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "文本文件|*.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                var comparer = new NaturalSortComparer();
                var sortedFiles = dialog.FileNames
                    .OrderBy(f => Path.GetFileName(f), comparer)
                    .ToList();

                int startIndex = FileItems.Count + 1;
                var files = sortedFiles
                    .Select((f, i) => new TextFileItem
                    {
                        FileName = Path.GetFileName(f),
                        FullPath = f,
                        Index = startIndex + i,
                        MergedName = string.Empty,
                        ShouldExport = true
                    });

                foreach (var file in files)
            {
                FileItems.Add(file);
            }

            AutoSetMergeNames();
            CanExport = FileItems.Any(f => f.ShouldExport && !string.IsNullOrEmpty(f.MergedName));
            SaveProjectCommand.NotifyCanExecuteChanged();
            
            }
        }

        [RelayCommand]
        private void AddFolders()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Folder Selection",
                ValidateNames = false
            };

            if (dialog.ShowDialog() == true)
            {
                var folders = dialog.FileNames.Select(Path.GetDirectoryName).Distinct();
                int startIndex = FileItems.Count + 1;
                var comparer = new NaturalSortComparer();
                
                foreach (var folder in folders)
                {
                    var files = _fileScanner.ScanTextFiles(folder ?? string.Empty)
                        .OrderBy(f => Path.GetFileName(f), comparer)
                        .Select((f, i) => new TextFileItem
                        {
                            FileName = Path.GetFileName(f),
                            FullPath = f,
                            Index = startIndex + i,
                            MergedName = string.Empty,
                            ShouldExport = true
                        });

                    foreach (var file in files)
                    {
                        FileItems.Add(file);
                    }
                }

                AutoSetMergeNames();
                CanExport = FileItems.Any(f => f.ShouldExport && !string.IsNullOrEmpty(f.MergedName));
            }
        }

        [RelayCommand]
        private void RemoveSelected()
        {
            var selectedItems = FileItems.Where(f => f.IsSelectedForDeletion).ToList();
            foreach (var item in selectedItems)
            {
                FileItems.Remove(item);
            }
            AutoSetMergeNames();
            CanExport = FileItems.Any(f => f.ShouldExport && !string.IsNullOrEmpty(f.MergedName));
            SaveProjectCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        private void ClearList()
        {
            FileItems.Clear();
            PreviewItems.Clear();
            CanExport = false;
            SaveProjectCommand.NotifyCanExecuteChanged();
        }

        [RelayCommand]
        private void RefreshMergeNames()
        {
            AutoSetMergeNames();
            CanExport = FileItems.Any(f => f.ShouldExport && !string.IsNullOrEmpty(f.MergedName));
        }

        private void AutoSetMergeNames()
        {
            var groups = FileItems
                .GroupBy(f => {
                    var baseName = Regex.Replace(
                        f.FileName, 
                        @"[\(（]\d+[\)）]", 
                        "");
                    
                    return _fileScanner.FindCommonPrefix(
                        new[] { baseName }, 
                        int.TryParse(MinMatchChars, out var min) ? min : 5,
                        int.TryParse(MaxMatchChars, out var max) ? max : 20);
                })
                .Where(g => !string.IsNullOrEmpty(g.Key));

            foreach (var group in groups)
            {
                foreach (var file in group)
                {
                    file.MergedName = group.Key;
                    if (!file.MergedName.EndsWith(".txt"))
                    {
                        file.MergedName += ".txt";
                    }
                    if (!file.MergedName.Contains("(合并)"))
                    {
                        file.MergedName = file.MergedName.Replace(".txt", "(合并).txt");
                    }
                }
            }
            CanExport = FileItems.Any(f => f.ShouldExport && !string.IsNullOrEmpty(f.MergedName));
        }

        public class PreviewItem
        {
            public int Index { get; set; }
            public string? MergedName { get; set; }
            public int FileCount { get; set; }
            public IEnumerable<TextFileItem> Files { get; set; } = Enumerable.Empty<TextFileItem>();
        }

        [ObservableProperty]
        private ObservableCollection<PreviewItem> _previewItems = new();

        [RelayCommand]
        private void PreviewExport()
        {
            PreviewItems.Clear();
            var groups = FileItems
                .Where(f => f.ShouldExport && !string.IsNullOrEmpty(f.MergedName))
                .GroupBy(f => f.MergedName);

            int i = 1;
            foreach (var group in groups)
            {
                PreviewItems.Add(new PreviewItem
                {
                    Index = i++,
                    MergedName = group.Key,
                    FileCount = group.Count(),
                    Files = group.ToList()
                });
            }
        }

        [RelayCommand]
        private void ShowPreviewWindow()
        {
            PreviewExport();
            var window = new Views.PreviewWindow(this);
            window.ShowDialog();
        }

        [RelayCommand(CanExecute = nameof(CanExport))]
        private void ExportFiles()
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog() == true)
            {
                var groups = FileItems
                    .Where(f => f.ShouldExport && !string.IsNullOrEmpty(f.MergedName))
                    .GroupBy(f => f.MergedName);

                int successCount = 0;
                foreach (var group in groups)
                {
                    if (_fileMergeService.MergeFiles(
                        group.Select(f => f.FullPath),
                        dialog.SelectedPath,
                        group.Key))
                    {
                        successCount++;
                    }
                }

                PreviewItems.Clear();
                MessageBox.Show($"导出完成，成功合并{successCount}个文件", "导出结果");
            }
        }
    }
}
