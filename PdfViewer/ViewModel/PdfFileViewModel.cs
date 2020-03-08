using PdfViewer.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PdfViewer.ViewModel
{
    public class PdfFileViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<PdfFileModel> _files;

        public ObservableCollection<PdfFileModel> Files
        {
            get { return _files; }
            set
            {
                if (_files != value)
                {
                    _files = value;
                    RaisePropertyChanged("Files");
                }
            }
        }

        public PdfFileViewModel() => _files = new ObservableCollection<PdfFileModel>();

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
