using PdfViewer.View.Controls;
using PdfViewer.ViewModel;
using System;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using PdfViewer.Core.Enums;
using PdfViewer.Model;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PdfViewer.View.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PdfViewerSource : Page
    {
        public delegate void ButtonCloseClickedHandler(PdfViewerSource pdfViewer);
        public event ButtonCloseClickedHandler ButtonCloseClicked;

        public delegate void ButtonFavoriteToggledHandler(PdfViewerSource pdfViewer, PdfFileModel pdfFile);
        public event ButtonFavoriteToggledHandler ButtonFavoriteToggled;

        private bool ignoreEvent;

        public PdfViewerPageViewModel ViewerPageViewModel { get; set; }
        public StorageFile File { get; private set; }
        public PdfFileModel PdfFile { get; set; }
        public bool PdfLoaded { get; private set; }
        public PdfViewerViewType PdfViewerViewType { get; private set; }

        public PdfViewerSource()
        {
            this.InitializeComponent();

            ViewerPageViewModel = new PdfViewerPageViewModel();
            PdfViewerViewType = PdfViewerViewType.Normal;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameters = e.Parameter as object[];
            File = parameters[0] as StorageFile;
            PdfFile = parameters[1] as PdfFileModel;

            base.OnNavigatedTo(e);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (File != null)
            {
                var pdfDocument = await PdfDocument.LoadFromFileAsync(File);
                var pageCount = pdfDocument.PageCount;
                NumberBoxPageNumber.Maximum = pageCount;

                if (pageCount == 1)
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { LoadingControl.IsLoading = true; });
                }

                for (uint i = 0; i < pdfDocument.PageCount; i++)
                {
                    var page = pdfDocument.GetPage(i);
                    if (page != null)
                    {
                        var pdfPageControl = new PdfViewerPageControl();
                        await pdfPageControl.Load(page);
                        ViewerPageViewModel.Pages.Add(pdfPageControl);
                    }
                }

                if (pageCount == 1)
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => { LoadingControl.IsLoading = false; });
                }
                PdfLoaded = true;
            }
        }

        private void GoToPage(uint pageIndex)
        {
            var page = ViewerPageViewModel?.GetPage(pageIndex);
            if (page != null)
            {
                page.StartBringIntoView(new BringIntoViewOptions() { AnimationDesired = true });
                PdfViewScrollViewer.UpdateLayout();
            }
        }

        private void PdfViewScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                var zoomFactor = scrollViewer.ZoomFactor;
                if (zoomFactor < 0.2f && PdfViewerViewType == PdfViewerViewType.Normal)
                {

                }

                var page = ViewerPageViewModel?.GetCurrentPage();
                if (page != null)
                {
                    ignoreEvent = true;
                    NumberBoxPageNumber.Value = Convert.ToDouble(page.Page.Index + 1);
                    ignoreEvent = false;
                }

            }
        }

        private void ButtonScrollToFirst_Click(object sender, RoutedEventArgs e) => 
            PdfViewScrollViewer.ChangeView(PdfViewScrollViewer.HorizontalOffset, 0, PdfViewScrollViewer.ZoomFactor);

        private void ButtonScrollToLast_Click(object sender, RoutedEventArgs e)
        {
            var page = ViewerPageViewModel.GetLastPage();
            if (page != null)
            {
                page.StartBringIntoView();
                PdfViewScrollViewer.UpdateLayout();
            }
        }

        private void NumberBoxPageNumber_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (ignoreEvent)
                return;

            GoToPage(Convert.ToUInt32(args.NewValue));
        }

        private void ButtonZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (PdfViewScrollViewer.ZoomFactor < PdfViewScrollViewer.MaxZoomFactor)
                PdfViewScrollViewer.ChangeView(PdfViewScrollViewer.HorizontalOffset, PdfViewScrollViewer.VerticalOffset, PdfViewScrollViewer.ZoomFactor + 0.2f);
        }

        private void ButtonZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (PdfViewScrollViewer.ZoomFactor >= 0.4f)
                PdfViewScrollViewer.ChangeView(PdfViewScrollViewer.HorizontalOffset, PdfViewScrollViewer.VerticalOffset, PdfViewScrollViewer.ZoomFactor - 0.2f);
        }

        private void ButtonRotateLeft_Click(object sender, RoutedEventArgs e)
        {
            var page = ViewerPageViewModel.CurrentPage;
            if (page != null)
            {
                page.RotateLeft();
                PdfViewScrollViewer.UpdateLayout();
            }
        }

        private void ButtonRotateRight_Click(object sender, RoutedEventArgs e)
        {
            var page = ViewerPageViewModel.CurrentPage;
            if (page != null)
            {
                page.RotateRight();
                PdfViewScrollViewer.UpdateLayout();
            }
        }

        private void ButtonGoBack_Click(object sender, RoutedEventArgs e)
        {
            var pageIndex = Convert.ToUInt32(NumberBoxPageNumber.Value);
            if (pageIndex > 1)
            {
                var newPageIndex = pageIndex - 1;
                GoToPage(newPageIndex);
            }
        }

        private void ButtonGoNext_Click(object sender, RoutedEventArgs e)
        {
            var pageIndex = Convert.ToUInt32(NumberBoxPageNumber.Value);
            if (pageIndex < ViewerPageViewModel.Pages.Count)
            {
                var newPageIndex = pageIndex + 1;
                GoToPage(newPageIndex);
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e) => ButtonCloseClicked?.Invoke(this);

        private void ButtonFavorite_Checked(object sender, RoutedEventArgs e)
        {
            PdfFile.IsFavorite = true;
            ButtonFavoriteToggled?.Invoke(this, PdfFile);
        }

        private void ButtonFavorite_Unchecked(object sender, RoutedEventArgs e)
        {
            PdfFile.IsFavorite = false;
            ButtonFavoriteToggled?.Invoke(this, PdfFile);
        }
    }
}
