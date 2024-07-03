using CommunityToolkit.Maui.Views;
using OCRScannerMauiApp;
using OCRScannerMauiApp.ViewModel;
using OCRScannerMauiApp.Services;
using SkiaSharp;
using Syncfusion.OCRProcessor;
using Syncfusion.Pdf;

namespace OCRScannerMauiApp
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        MainViewModel viewModel;
        PopupPage popup;
        public MainPage(MainViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm;
            viewModel = vm;
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

        }

        private void CounterBtn_Clicked(object sender, EventArgs e)
        {
            PickPhoto();
        }

        private void CameraBtn_Clicked(object sender, EventArgs e)
        {
            TakePhoto();
        }

        private void ConvertBtn_Clicked(object sender, EventArgs e)
        {
            //popup = new PopupPage();
            //this.ShowPopup(popup);

            ConvertToPDF();
        }
        public void ConvertToPDF()
        {
            Task.Run(async () =>
            {

                PdfDocument finalDcoument = new PdfDocument();
                if (viewModel.Items.Count == 0)
                    return;

                //Initialize OCR processor
                using (OCRProcessor processor = new OCRProcessor())
                {
                    //Set extrenal azure OCR engine
                    processor.ExternalEngine = new AzureOcrEngine();
                    foreach (var item in viewModel.Items)
                    {
                        FileStream imageStream = new FileStream(item.ImagePath, FileMode.Open);
                        //Perform OCR on image and get the PDF document object
                        PdfDocument document = processor.PerformOCR(imageStream);
                        MemoryStream saveStream = new MemoryStream();
                        document.Save(saveStream);
                        document.Close(true);
                        //Merge the PDF document.
                        PdfDocument.Merge(finalDcoument, saveStream);
                    }
                }

                MemoryStream fileSave = new MemoryStream();
                finalDcoument.Save(fileSave);
                fileSave.Position = 0;
                finalDcoument.Close(true);
                SaveService service = new SaveService();
                service.SaveAndView("MAUI_scanned.pdf", "application/pdf", fileSave);

            });
        }
        //Open camera and take photo.
        public async void TakePhoto()
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                FileResult photo = await MediaPicker.Default.CapturePhotoAsync();

                if (photo != null)
                {
                    // save the file into local storage
                    string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

                    using Stream sourceStream = await photo.OpenReadAsync();
                    using SKBitmap sourceBitmap = SKBitmap.Decode(sourceStream);
                    int height = Math.Min(794, sourceBitmap.Height);
                    int width = Math.Min(794, sourceBitmap.Width);

                    using SKBitmap scaledBitmap = sourceBitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.Medium);
                    using SKImage scaledImage = SKImage.FromBitmap(scaledBitmap);

                    using (SKData data = scaledImage.Encode())
                    {
                        File.WriteAllBytes(localFilePath, data.ToArray());
                    }


                    ImageModel model = new ImageModel() { ImagePath = localFilePath, Title = "sample", Description = "Cool" };
                    viewModel.Items.Add(model);
                }
            }
        }
        public async void PickPhoto()
        {
            if (MediaPicker.Default.IsCaptureSupported)
            {
                FileResult photo = await MediaPicker.Default.PickPhotoAsync();

                if (photo != null)
                {
                    // save the file into local storage
                    string localFilePath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

                    using Stream sourceStream = await photo.OpenReadAsync();
                    // using FileStream localFileStream = File.OpenWrite(localFilePath);
                    using FileStream localFileStream = new FileStream(localFilePath, FileMode.Create);

                    await sourceStream.CopyToAsync(localFileStream);
                    localFileStream.Position = 0;

                    ImageModel model = new ImageModel() { ImagePath = localFilePath, Title = "sample", Description = "Cool" };
                    viewModel.Items.Add(model);
                }
            }
        }
    }

}
