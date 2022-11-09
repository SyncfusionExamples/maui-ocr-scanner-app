

using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Graphics;
using OCRScanner.Services;
using OCRScanner.ViewModel;
using SkiaSharp;
using Syncfusion.OCRProcessor;
using Syncfusion.Pdf;

namespace OCRScanner;

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
        PickPhoto();
    }

    private void OnConvertClicked(object sender, EventArgs e)
    {
        popup = new PopupPage();
        this.ShowPopup(popup);
       
        ConvertToPDF();
    }

    /// <summary>
    /// OCR the image using external OCR services and converted to PDF with text
    /// </summary>
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
                FileStream imageStream = new FileStream(item.ImagePath,FileMode.Open);
                //Perform OCR on image and get the PDF document object
                PdfDocument document = processor.PerformOCR(imageStream);
                MemoryStream saveStream = new MemoryStream();
                document.Save(saveStream);
                document.Close(true);
                //Merge the PDF document.
                PdfDocument.Merge(finalDcoument,saveStream);
            }

        }

            MemoryStream fileSave = new MemoryStream();
            finalDcoument.Save(fileSave);
            fileSave.Position = 0;
            finalDcoument.Close(true);
            
            Dispatcher.Dispatch(() =>
            {
                popup.Close();
                SaveService service = new SaveService();
                service.SaveAndView("MAUI_scanned.pdf", "application/pdf", fileSave);
            });

        });
    }

    //Select images from gallery
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
                using FileStream localFileStream = File.OpenWrite(localFilePath);

                await sourceStream.CopyToAsync(localFileStream);

                ImageModel model = new ImageModel() { ImagePath = localFilePath, Title = "sample", Description = "Cool" };
                viewModel.Items.Add(model);
            }
        }
    }

    private void OnCameraClicked(object sender, EventArgs e)
    {
        TakePhoto();
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
                using SKBitmap sourceBitmap=SKBitmap.Decode(sourceStream);
                int height = Math.Min(794, sourceBitmap.Height);
                int width = Math.Min(794, sourceBitmap.Width);

                using SKBitmap scaledBitmap = sourceBitmap.Resize(new SKImageInfo(width, height),SKFilterQuality.Medium);
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
}

