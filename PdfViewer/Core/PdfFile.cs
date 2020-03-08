using System;

namespace PdfViewer.Core
{
    public class PdfFile
    {
        public string FullFilePath { get; set; }
        public DateTime LastTimeOpened { get; set; }
        public string FileToken { get; set; }
        public bool IsFavorite { get; set; }
    }
}
