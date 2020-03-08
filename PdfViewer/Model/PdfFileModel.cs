using System;
using System.ComponentModel;

namespace PdfViewer.Model
{
    public class PdfFileModel : INotifyPropertyChanged
    {
        private string _fullFilePath;
        private DateTime _lastTimeOpened;
        private bool _isFavorite;

        public string FullFilePath
        {
            get { return _fullFilePath; }
            set
            {
                if (_fullFilePath != value)
                {
                    _fullFilePath = value;
                    RaisePropertyChanged("Filename");
                }
            }
        }

        public DateTime LastTimeOpened
        {
            get { return _lastTimeOpened; }
            set
            {
                if (_lastTimeOpened != value)
                {
                    _lastTimeOpened = value;
                    RaisePropertyChanged("LastTimeOpened");
                }
            }
        }

        public bool IsFavorite
        {
            get { return _isFavorite; }
            set
            {
                if (_isFavorite != value)
                {
                    _isFavorite = value;
                    RaisePropertyChanged("IsFavorite");
                }
            }
        }

        public string FileToken { get; set; }

        public string Filename => System.IO.Path.GetFileNameWithoutExtension(_fullFilePath);

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
