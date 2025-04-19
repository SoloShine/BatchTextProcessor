
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
        private bool _isEditing;
        private bool _isTextSelected;

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
            set
            {
                if (!_isTextSelected && _mergedName != value)
                {
                    _mergedName = value;
                    OnPropertyChanged(nameof(MergedName));
                }
            }
        }

        public bool IsSelectedForDeletion
        {
            get => _isSelectedForDeletion;
            set => SetProperty(ref _isSelectedForDeletion, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (SetProperty(ref _isEditing, value))
                {
                    if (!value) 
                    {
                        _isTextSelected = false;
                        OnPropertyChanged(nameof(MergedName));
                    }
                }
            }
        }

        public bool IsTextSelected
        {
            get => _isTextSelected;
            set => SetProperty(ref _isTextSelected, value);
        }
    }
}
