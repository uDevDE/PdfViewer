using PdfViewer.View.Pages;
using System;
using System.Threading.Tasks;
using Windows.Data.Pdf;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PdfViewer.View.Controls
{
    public sealed partial class PdfViewerPageControl : UserControl
    {
        public double ImageRotation { get; private set; }

        public PdfPage Page { get; private set; }
        public double DistanceX { get; private set; }
        public double DistanceY { get; private set; }
        public bool PageLoaded { get; private set; }
        public bool IsFirstPage => Page?.Index == 0 ? true : false;

        public PdfViewerPageControl()
        {
            this.InitializeComponent();
        }

        public async Task Load(PdfPage page)
        {
            using (var stream = new InMemoryRandomAccessStream())
            {
                Page = page;
                var bitmap = new BitmapImage();
                await page.RenderToStreamAsync(stream);
                await bitmap.SetSourceAsync(stream);
                PdfImage.Source = bitmap;
            }
        }

        private void UserControl_EffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
        {
            DistanceX = args.BringIntoViewDistanceX;
            DistanceY = args.BringIntoViewDistanceY;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            PageLoaded = true;
        }

        public void RotateRight()
        {
            var newValue = ImageRotation + 90;
            if (newValue > 360)
            {
                newValue = 90;
            }
            ImageRotation = newValue;

            var rotateTransform = new RotateTransform()
            {
                CenterX = PdfImage.ActualWidth / 2,
                CenterY = PdfImage.ActualHeight / 2,
                Angle = ImageRotation
            };
            PdfImage.RenderTransform = rotateTransform;
        }

        public void RotateLeft()
        {
            var newValue = ImageRotation - 90;
            if (newValue < 0)
            {
                newValue = 270;
            }
            ImageRotation = newValue;

            var rotateTransform = new RotateTransform()
            {
                CenterX = PdfImage.ActualWidth / 2,
                CenterY = PdfImage.ActualHeight / 2,
                Angle = ImageRotation
            };
            PdfImage.RenderTransform = rotateTransform;
        }

    }
}
