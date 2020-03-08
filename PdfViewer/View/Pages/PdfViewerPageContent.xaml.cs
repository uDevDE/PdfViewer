using System;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using PdfViewer.Model;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PdfViewer.View.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PdfViewerPageContent : Page
    {
        public delegate void PdfViewerButtonToggledClickedHandler(PdfViewerSource pdfViewer, PdfFileModel pdfFile);
        public event PdfViewerButtonToggledClickedHandler PdfViewerButtonToggledClicked;

        public List<TabViewItem> TabViewItems { get; private set; }

        public PdfViewerPageContent()
        {
            this.InitializeComponent();
        }

        public bool SelectFileIfOpen(string fullFilePath)
        {
            foreach (var item in PivotControl.Items)
            {
                if (item is PivotItem pivotItem)
                {
                    if (pivotItem.Content is Frame frame)
                    {
                        if (frame.Content is PdfViewerSource pdfViewer)
                        {
                            if (pdfViewer.PdfFile.FullFilePath == fullFilePath)
                            {
                                PivotControl.SelectedItem = item;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public void LoadPdfViewer(StorageFile file, PdfFileModel pdfFile)
        {
            PivotItem newItem = new PivotItem
            {
                Header = file.DisplayName,
                //IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Document }
            };

            Frame frame = new Frame();

            frame.Navigated += Frame_Navigated;
            frame.Navigate(typeof(PdfViewerSource), new object[] { file, pdfFile });
            newItem.Content = frame;

            PivotControl.Items.Add(newItem);
            PivotControl.SelectedItem = newItem;
        }

        private void Frame_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if (e.Content is PdfViewerSource pdfViewer)
            {
                pdfViewer.ButtonCloseClicked += OnPdfViewerCloseButtonClicked;
                pdfViewer.ButtonFavoriteToggled += OnButtonFavoriteToggled;
            }
        }

        private void OnButtonFavoriteToggled(PdfViewerSource pdfViewer, PdfFileModel pdfFile)
        {
            PdfViewerButtonToggledClicked?.Invoke(pdfViewer, pdfFile);
        }

        private void OnPdfViewerCloseButtonClicked(PdfViewerSource pdfViewer)
        {
            var item = PivotControl.Items.Single(x => (((x as PivotItem).Content as Frame).Content as PdfViewerSource) == pdfViewer);
            PivotControl.Items.Remove(item as PivotItem);
        }

    }
}
