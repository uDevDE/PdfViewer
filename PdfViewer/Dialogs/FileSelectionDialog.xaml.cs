using PdfViewer.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PdfViewer.Dialogs
{
    public sealed partial class FileSelectionDialog : ContentDialog
    {
        public ObservableCollection<PdfFileModel> PdfFiles { get; private set; }

        public PdfFileModel Result { get; private set; }

        public FileSelectionDialog(string title, List<PdfFileModel> pdfFiles)
        {
            this.InitializeComponent();
            this.Title = title;
            PdfFiles = new ObservableCollection<PdfFileModel>(pdfFiles);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            ErrorText.Visibility = Visibility.Collapsed;

            if (FilesListBox.SelectedItem is PdfFileModel pdfFile)
            {
                Result = pdfFile;
            }
            else
            {
                ErrorText.Visibility = Visibility.Visible;
                args.Cancel = true;
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
