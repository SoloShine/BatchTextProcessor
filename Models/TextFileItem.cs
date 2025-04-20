
using CommunityToolkit.Mvvm.ComponentModel;

namespace BatchTextProcessor.Models
{
    public class TextFileItem : ObservableObject
    {
        private int _index;
        private string _fileName = string.Empty;
        private string _fullPath = string.Empty;
        private bool _shouldExport = true;
        private bool _isSelectedForDeletion;
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

        public bool IsSelectedForDeletion
        {
            get => _isSelectedForDeletion;
            set => SetProperty(ref _isSelectedForDeletion, value);
        }
    }
}
