
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using BatchTextProcessor.Services;

namespace BatchTextProcessor.ViewModels
{
    public partial class SplitFileViewModel : ObservableObject
    {
        private readonly FileSplitService _fileSplitService = new();
        private readonly ILogger _logger = new LoggerService();

        [ObservableProperty]
        private ObservableCollection<SplitFileItem> _fileItems = new();

        [ObservableProperty]
        private int _linesPerFile = 1000;

        [ObservableProperty]
        private double _sizePerFileMB = 1.0;

        [ObservableProperty]
        private bool _useSizeSplit = false;

        partial void OnUseSizeSplitChanged(bool value)
        {
            if (value) LinesPerFile = 0;
        }

        partial void OnLinesPerFileChanged(int value)
        {
            if (value > 0) UseSizeSplit = false;
        }

        [RelayCommand]
        private void AddFiles()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Text files (*.txt)|*.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                int index = FileItems.Count + 1;
                foreach (var file in dialog.FileNames)
                {
                    var fileInfo = new FileInfo(file);
                    var content = File.ReadAllText(file);
                    
                    // 更精确的字数统计：统计所有中文字符和单词
                    int chineseCount = Regex.Matches(content, @"[\u4e00-\u9fff]").Count;
                    int englishWordCount = Regex.Matches(content, @"\b\w+\b").Count;
                    int wordCount = chineseCount + englishWordCount;
                    
                    FileItems.Add(new SplitFileItem
                    {
                        Index = index++,
                        FileName = Path.GetFileName(file),
                        FullPath = file,
                        FileSize = fileInfo.Length,
                        WordCount = wordCount
                    });
                }
            }
        }

        [RelayCommand]
        private void RemoveFile(SplitFileItem item)
        {
            if (item != null)
            {
                FileItems.Remove(item);
                // 重新编号
                for (int i = 0; i < FileItems.Count; i++)
                {
                    FileItems[i].Index = i + 1;
                }
            }
        }

        [RelayCommand]
        private void ClearFiles()
        {
            FileItems.Clear();
        }

        [RelayCommand]
        private void SplitFiles()
        {
            if (FileItems.Count == 0)
            {
                MessageBox.Show("请先添加要拆分的文件", "操作提示");
                return;
            }

            foreach (var item in FileItems)
            {
                if (UseSizeSplit)
                {
                    _fileSplitService.SplitTextFileBySize(
                        item.FullPath,
                        (long)(SizePerFileMB * 1024 * 1024),
                        _logger);
                }
                else
                {
                    _fileSplitService.SplitTextFileByLines(
                        item.FullPath,
                        LinesPerFile,
                        _logger);
                }
            }
            MessageBox.Show($"拆分完成，共处理{FileItems.Count}个文件", "操作完成");
        }
    }

    public class SplitFileItem : ObservableObject
    {
        private int _index;
        private string _fileName = string.Empty;
        private string _fullPath = string.Empty;
        private long _fileSize;
        private int _wordCount;

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

        public long FileSize
        {
            get => _fileSize;
            set => SetProperty(ref _fileSize, value);
        }

        public int WordCount
        {
            get => _wordCount;
            set => SetProperty(ref _wordCount, value);
        }
    }
}
