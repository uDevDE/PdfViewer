using Newtonsoft.Json;
using AutoMapper;
using PdfViewer.Core;
using PdfViewer.View.Pages;
using PdfViewer.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using PdfViewer.Model;
using PdfViewer.Core.Enums;
using PdfViewer.Dialogs;
using Windows.Storage.AccessCache;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PdfViewer
{
    // CHECK DUPLICATE FILES IN PIVOT

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;

        private static Mapper Mapper;
        private static List<PdfFile> PdfFiles;
        private bool contentLoaded;

        public MainPage()
        {
            this.InitializeComponent();

            var configuration = new MapperConfiguration(cfg => {
                cfg.CreateMap<PdfFile, PdfFileModel>().ReverseMap();
            });

            Mapper = new Mapper(configuration);
            Current = this;
        }

        private readonly static Dictionary<string, Type> _pages = new Dictionary<string, Type>
        {
            { "PdfViewerPageContent", typeof(PdfViewerPageContent) }
        };

        private async void Navigator_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            try
            {
                if (args.SelectedItem is NavigationViewItem navigationViewItem)
                {
                    var tag = navigationViewItem.Tag;
                    if (tag is string typeName)
                    {
                        if (!string.IsNullOrEmpty(typeName))
                        {
                            if (_pages.TryGetValue(typeName, out Type type))
                            {
                                if (type == typeof(PdfViewerPageContent))
                                {
                                    var file = await PickFile();
                                    if (file != null)
                                    {
                                        if (IsFileOpen(file.Path))
                                        {
                                            Navigator.SelectedItem = null;
                                            return;
                                        }

                                        var token = StorageApplicationPermissions.FutureAccessList.Add(file);

                                        //var tempFile = await CopyFileToDevice(file);
                                        var pdfFileExists = PdfFiles.Find(x => x.FullFilePath == file.Path);
                                        var tempFile = pdfFileExists != null ? await OpenFile(Mapper.Map<PdfFileModel>(pdfFileExists)) : await CopyFileToDevice(file);
                                        if (tempFile != null)
                                        {
                                            if (ContentFrame.Content is PdfViewerPageContent content)
                                            {
                                                PdfFile pdfFile = null;
                                                pdfFile = PdfFiles.Find(x => x.FullFilePath == file.Path);
                                                if (pdfFile == null)
                                                {
                                                    pdfFile = new PdfFile() { FullFilePath = file.Path, IsFavorite = false, LastTimeOpened = DateTime.Now, FileToken = token };
                                                }
                                                var pdfFileModel = Mapper.Map<PdfFileModel>(pdfFile);
                                                if (!PdfFiles.Exists(x => x.FullFilePath == pdfFile.FullFilePath))
                                                {
                                                    await RemovePdfFileLastUsedAsync();
                                                    PdfFiles.Add(pdfFile);
                                                    await SaveSettings(PdfFiles);                       
                                                }


                                                content.LoadPdfViewer(tempFile, pdfFileModel);
                                            }
                                        }
                                    }
                                    Navigator.SelectedItem = null;
                                }
                            }
                            else if (typeName == "FavoriteDialog")
                            {
                                await OpenFavoriteFileDialog();
                                Navigator.SelectedItem = null;
                            }
                            else if (typeName == "LastTimeUsedDialog")
                            {
                                await OpenLastTimeUsedFilesDialog();
                                Navigator.SelectedItem = null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(ex.Message, "Fehler");
                await dialog.ShowAsync();
            }
        }


        private async Task RemovePdfFileLastUsedAsync()
        {
            var pdfFiles = PdfFiles.Where(x => !x.IsFavorite).ToList();
            if (pdfFiles.Count > 19)
            {
                var dateTime = pdfFiles.Max(x => x.LastTimeOpened);
                var pdfFile = pdfFiles.First(x => x.LastTimeOpened == dateTime);
                if (pdfFile != null)
                {
                    PdfFiles.Remove(pdfFile);
                    await SaveSettings(PdfFiles);
                }
            }
        }

        private async Task<StorageFile> OpenFile(PdfFileModel pdfFile)
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(pdfFile.FullFilePath);
                if (file != null)
                {
                    var tempFile = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(pdfFile.FileToken);
                    if (tempFile != null)
                    {
                        return await Task.FromResult(tempFile);
                    }
                }
                return await Task.FromResult<StorageFile>(null);
            }
            catch (Exception)
            {
                return await Task.FromResult<StorageFile>(null);
            }
        }

        private async Task OpenFavoriteFileDialog()
        {
            try
            {
                var files = PdfFiles.Where(x => x.IsFavorite == true).ToList().OrderBy(x => x.LastTimeOpened).ToList();
                if (files.Count == 0)
                {
                    var infoDialog = new MessageDialog("Die Favoritenliste ist noch leer", "Information");
                    await infoDialog.ShowAsync();
                    return;
                }
                var models = Mapper.Map<List<PdfFile>, List<PdfFileModel>>(files);
                var dialog = new FileSelectionDialog("Favoritenliste", models);
                var dialogResult = await dialog.ShowAsync();
                if (dialogResult == ContentDialogResult.Primary)
                {
                    if (dialog.Result != null)
                    {
                        var pdfFile = dialog.Result;
                        if (IsFileOpen(pdfFile.FullFilePath))
                        {
                            return;
                        }

                        PdfFiles.Where(x => x.FullFilePath == pdfFile.FullFilePath).FirstOrDefault().LastTimeOpened = DateTime.Now;
                        await SaveSettings(PdfFiles);
                        if (ContentFrame.Content is PdfViewerPageContent content)
                        {
                            var file = await OpenFile(pdfFile);
                            if (file != null)
                            {
                                content.LoadPdfViewer(file, pdfFile);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                var errorDialog = new MessageDialog(ex.Message);
                await errorDialog.ShowAsync();
            }
        }

        private async Task OpenLastTimeUsedFilesDialog()
        {
            try
            {
                var files = PdfFiles.Where(x => x.IsFavorite == false).ToList().OrderBy(x => x.LastTimeOpened).ToList();
                if (files.Count == 0)
                {
                    var infoDialog = new MessageDialog("Es gibt noch keine Pdf-Dokumente, die schon einmal geöffnet wurden", "Information");
                    await infoDialog.ShowAsync();
                    return;
                }
                var models = Mapper.Map<List<PdfFile>, List<PdfFileModel>>(files);
                var dialog = new FileSelectionDialog("Zuletzt verwendete Dateien", models);
                var dialogResult = await dialog.ShowAsync();
                if (dialogResult == ContentDialogResult.Primary)
                {
                    if (dialog.Result != null)
                    {
                        var pdfFile = dialog.Result;
                        if (IsFileOpen(pdfFile.FullFilePath))
                        {
                            return;
                        }

                        PdfFiles.Where(x => x.FullFilePath == pdfFile.FullFilePath).FirstOrDefault().LastTimeOpened = DateTime.Now;
                        await SaveSettings(PdfFiles);
                        if (ContentFrame.Content is PdfViewerPageContent content)
                        {
                            var file = await OpenFile(pdfFile);
                            if (file != null)
                            {
                                content.LoadPdfViewer(file, pdfFile);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var errorDialog = new MessageDialog(ex.Message);
                await errorDialog.ShowAsync();
            }
        }

        private IAsyncOperation<StorageFile> PickFile()
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail
            };
            picker.FileTypeFilter.Add(".pdf");

            return picker.PickSingleFileAsync();
        }

        private IAsyncOperation<StorageFile> CopyFileToDevice(StorageFile file)
        {
            var tempFolder = ApplicationData.Current.TemporaryFolder;
            return file.CopyAsync(tempFolder, file.Name, NameCollisionOption.ReplaceExisting);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await CreateSettingsFile();
            await LoadSettings();
            //await DeleteSettingsFile();

            ContentFrame.Navigated += ContentFrame_Navigated;
            ContentFrame.Navigate(typeof(PdfViewerPageContent));
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is PdfViewerPageContent content)
            {
                content.PdfViewerButtonToggledClicked += Content_PdfViewerButtonToggledClicked;
                contentLoaded = true;
            }
        }

        private async void Content_PdfViewerButtonToggledClicked(PdfViewerSource pdfViewer, PdfFileModel pdfFile)
        {
            if (PdfFiles.Exists(x => x.FullFilePath == pdfFile.FullFilePath))
            {
                PdfFiles.Where(x => x.FullFilePath == pdfFile.FullFilePath).FirstOrDefault().IsFavorite = pdfFile.IsFavorite;
            }

            await SaveSettings(PdfFiles);
        }

        private async Task DeleteSettingsFile()
        {
            var storageFolder = ApplicationData.Current.LocalFolder;
            var file = await storageFolder.GetFileAsync("settings.json");
            await file.DeleteAsync();
        }

        private async Task CreateSettingsFile()
        {
            try
            {
                var storageFolder = ApplicationData.Current.LocalFolder;
                await storageFolder.CreateFileAsync("settings.json", CreationCollisionOption.FailIfExists);

            }
            catch (Exception) { }
        }

        private async Task LoadSettings()
        {
            try
            {
                var storageFolder = ApplicationData.Current.LocalFolder;
                var file = await storageFolder.GetFileAsync("settings.json");
                if (file != null)
                {
                    var json = await FileIO.ReadTextAsync(file);
                    PdfFiles = JsonConvert.DeserializeObject<List<PdfFile>>(json);

                    if (PdfFiles == null)
                    {
                        PdfFiles = new List<PdfFile>();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private async Task SaveSettings(List<PdfFile> pdfFiles)
        {
            var storageFolder = ApplicationData.Current.LocalFolder;
            var file = await storageFolder.GetFileAsync("settings.json");
            if (file != null)
            {
                var json = JsonConvert.SerializeObject(pdfFiles, Formatting.Indented);
                await FileIO.WriteTextAsync(file, json);
            }
        }


        public async Task OpenPdfFileFromActivation(Windows.ApplicationModel.Activation.FileActivatedEventArgs args)
        {
            while (!contentLoaded)
            {
                await Task.Delay(50);
            }

            try
            {
                foreach (var file in args.Files)
                {
                    if (IsFileOpen(file.Path))
                    {
                        continue;
                    }

                    if (ContentFrame.Content is PdfViewerPageContent content)
                    {
                        if (file is StorageFile tempFile)
                        {
                            var tempFolder = ApplicationData.Current.TemporaryFolder;
                            var storageFile = await tempFile.CopyAsync(tempFolder, file.Name, NameCollisionOption.ReplaceExisting);

                            if (storageFile != null)
                            {
                                PdfFile pdfFile = null;
                                if (PdfFiles.Exists(x => x.FullFilePath == file.Path))
                                {
                                    pdfFile = PdfFiles.Find(x => x.FullFilePath == file.Path);
                                    pdfFile.LastTimeOpened = DateTime.Now;
                                    await SaveSettings(PdfFiles);
                                }
                                else
                                {
                                    var token = StorageApplicationPermissions.FutureAccessList.Add(tempFile);
                                    pdfFile = new PdfFile() { FullFilePath = file.Path, IsFavorite = false, LastTimeOpened = DateTime.Now, FileToken = token };
                                    await RemovePdfFileLastUsedAsync();
                                    PdfFiles.Add(pdfFile);
                                    await SaveSettings(PdfFiles);
                                }

                                var pdfFileModel = Mapper.Map<PdfFileModel>(pdfFile);
                                content.LoadPdfViewer(storageFile, pdfFileModel);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog(ex.Message);
                await dialog.ShowAsync();
            }
        }

        private bool IsFileOpen(string fullFilePath)
        {
            if (ContentFrame.Content is PdfViewerPageContent content)
            {
                return content.SelectFileIfOpen(fullFilePath);
            }

            return false;
        }

    }
}
