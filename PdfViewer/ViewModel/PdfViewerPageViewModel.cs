using PdfViewer.View.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PdfViewer.ViewModel
{
    public class PdfViewerPageViewModel : INotifyPropertyChanged
    {
        private PdfViewerPageControl _currentPage;
        private ObservableCollection<PdfViewerPageControl> _pages { get; set; }
        public PdfViewerPageControl GetLastPage() => _pages.LastOrDefault();
        public PdfViewerPageControl GetPage(uint index) => _pages.FirstOrDefault(p => p.Page.Index == (index - 1));
        public PdfViewerPageControl GetFirstPage() => _pages.FirstOrDefault();
        public PdfViewerPageControl CurrentPage
        {
            get
            {
                return _currentPage == null ? _pages?.FirstOrDefault() : _currentPage;
            }
            set
            {
                _currentPage = value;
            }
        }

        public PdfViewerPageControl GetCurrentPage()
        {
            double val = double.MaxValue;
            PdfViewerPageControl result = null;
            foreach (var page in _pages)
            {
                if (page.PageLoaded && page.DistanceY < val)
                {
                    val = page.DistanceY;
                    result = page;
                }
            }

            CurrentPage = result;
            return result;
        }

        public ObservableCollection<PdfViewerPageControl> Pages
        {
            get { return _pages; }
            set
            {
                if (_pages != value)
                {
                    _pages = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PdfViewerPageViewModel() => _pages = new ObservableCollection<PdfViewerPageControl>();

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
