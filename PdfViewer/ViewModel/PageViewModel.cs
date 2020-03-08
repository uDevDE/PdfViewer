using PdfViewer.View.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PdfViewer.ViewModel
{
    public class PageViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<PageViewModel> _items;
        private PdfViewerPageControl _currentPage;

        public PdfViewerPageControl CurrentPage
        {
            get
            {
                if (_currentPage == null)
                {
                    return _items.FirstOrDefault()?.CurrentPage;
                }
                else
                {
                    return _currentPage;
                }
            }
            set
            {
                if (_currentPage != value)
                {
                    _currentPage = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PdfViewerPageControl GetLastPage() => _items.LastOrDefault()?.GetLastPage();

        public PdfViewerPageControl GetFirstPage() => _items.FirstOrDefault()?.GetFirstPage();

        public ObservableCollection<PageViewModel> Items
        {
            get { return _items; }
            set
            {
                if (_items != value)
                {
                    _items = value;
                    RaisePropertyChanged();
                }
            }
        }

        public PdfViewerPageControl GetCurrentPage()
        {
            double val = double.MaxValue;
            PdfViewerPageControl result = null;
            foreach (var items in _items)
            {
                foreach (var viewModel in items.Items)
                {
                    var page = viewModel.CurrentPage;
                    if (page != null)
                    {
                        if (page.PageLoaded && page.DistanceY < val)
                        {
                            val = page.DistanceY;
                            result = page;
                        }
                    }
                }
            }

            CurrentPage = result;
            return result;
        }

        public void Add(PdfViewerPageControl pageControl)
        {
        }

        public PageViewModel() => _items = new ObservableCollection<PageViewModel>();

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
