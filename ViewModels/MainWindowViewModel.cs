
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
using BatchTextProcessor.Utils;

namespace BatchTextProcessor.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly FileScannerService _fileScanner = new();
        private readonly FileMergeService _fileMergeService;
        private readonly FileSplitService _fileSplitService = new();
        private readonly ILogger _logger = new LoggerService();

        public MainWindowViewModel()
        {
            _fileMergeService = new FileMergeService(_logger);
            SplitFileViewModel = new SplitFileViewModel();
            InitializeCommand = new RelayCommand(OnInitialize);
        }

        public IRelayCommand InitializeCommand { get; }

        private void OnInitialize()
        {
            // 窗口初始化完成时执行的操作
            // 由用户手动添加具体实现
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
            var selectedItems = FileItems.Where(f => f.ShouldExport).ToList();
            foreach (var item in selectedItems)
            {
                FileItems.Remove(item);
            }
            AutoSetMergeNames();
            CanExport = FileItems.Any(f => f.ShouldExport && !string.IsNullOrEmpty(f.MergedName));
        }

        [RelayCommand]
        private void ClearList()
        {
            FileItems.Clear();
            PreviewItems.Clear();
            CanExport = false;
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
                    // 使用正则表达式移除任意数字的修饰符
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
                var window = new BatchTextProcessor.Views.PreviewWindow(this);
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

    public class TextFileItem : ObservableObject
    {
        private int _index;
        private string _fileName = string.Empty;
        private string _fullPath = string.Empty;
        private bool _shouldExport = true;
        private string _mergedName = string.Empty;

        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        public string FullPath
        {
            get => _fullPath;
            set => SetProperty(ref _fullPath, value);
        }

        public bool ShouldExport
        {
            get => _shouldExport;
            set => SetProperty(ref _shouldExport, value);
        }

        public string MergedName
        {
            get => _mergedName;
            set => SetProperty(ref _mergedName, value);
        }
    }
}
