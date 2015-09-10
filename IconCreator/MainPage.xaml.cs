using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IconCreator
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private StorageFile _oriFile;

        public static List<uint> SizesList = new List<uint>()
        {
            600,500,300,284,256,225,200,188,176,150,142,107,100,89,88,75,71,66,63,55,50,48,44,24,16
        };

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".png");
            var pickedFile = await picker.PickSingleFileAsync();

            if(pickedFile!= null)
            {
                _oriFile = pickedFile;

                using (var stream =await pickedFile.OpenStreamForReadAsync())
                {
                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream.AsRandomAccessStream());

                    this.PreviewImage.Source = bitmap;
                }
            }
        }

        private async void CreateIconsClick(object sender,RoutedEventArgs e)
        {
            if(_oriFile== null)
            {
                await new MessageDialog("Please pick a file fist ;-)").ShowAsync();
                return;
            }

            try
            {
                var folder = await KnownFolders.SavedPictures.CreateFolderAsync(_oriFile.DisplayName, CreationCollisionOption.GenerateUniqueName);

                foreach (var size in SizesList)
                {
                    using (var stream = await _oriFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        stream.Seek(0);
                        var decoder = await BitmapDecoder.CreateAsync(stream);
                        
                        var pixels = await decoder.GetPixelDataAsync();

                        var file = await folder.CreateFileAsync($"{_oriFile.DisplayName}_{size}.png");
                        using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                        {

                            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);

                            encoder.BitmapTransform.ScaledWidth = size;
                            encoder.BitmapTransform.ScaledHeight = size;

                            encoder.SetPixelData(
                                decoder.BitmapPixelFormat,
                                decoder.BitmapAlphaMode,
                                decoder.PixelWidth, decoder.PixelHeight,
                                decoder.DpiX, decoder.DpiY,
                                pixels.DetachPixelData());

                            encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;

                            await encoder.FlushAsync();
                        }
                    }
                }
                await new MessageDialog($"All saved in {folder.Path} folder").ShowAsync();

            }
            catch (Exception e1)
            {
               await new MessageDialog(e1.Message).ShowAsync();
            }
           
        }
    }
}
