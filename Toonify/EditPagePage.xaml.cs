using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using Nokia.Graphics.Imaging;
using System.IO;

namespace Toonify
{
    public partial class EditPagePage : PhoneApplicationPage
    {
        //768 x 1000 = page size = 8" x 10.5" at 96dpi
        private const int DefaultWidth = 768;
        private const int DefaultHeight = 1000;
        private const int DefaultPageMargin = 20;
        
        private int top = 0;
        private int left = 0;
        private int width = 0;
        private int height = 0;
        private PageLayout _layout;
        private WriteableBitmap _pageImage;

        public EditPagePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var newPage = string.Empty; 
            NavigationContext.QueryString.TryGetValue("new", out newPage);

            //var selectedImageName = string.Empty;
            //NavigationContext.QueryString.TryGetValue("selectedimagename", out selectedImageName);

            if (newPage.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                var layout = string.Empty;
                NavigationContext.QueryString.TryGetValue("layout", out layout);
                _layout = ParseLayoutString(layout); 

                //start new page
                _pageImage = new WriteableBitmap(DefaultWidth, DefaultHeight);
                DrawBlankPage(); 

                if (!string.IsNullOrEmpty(App.ViewModel.SelectedImageName))
                {
                    //var strInt = string.Empty; 
                    //NavigationContext.QueryString.TryGetValue("top", out strInt);
                    //top = int.Parse(strInt);
                    //NavigationContext.QueryString.TryGetValue("left", out strInt);
                    //left = int.Parse(strInt);
                    //NavigationContext.QueryString.TryGetValue("width", out strInt);
                    //width = int.Parse(strInt);
                    //NavigationContext.QueryString.TryGetValue("height", out strInt);
                    //height = int.Parse(strInt);

                    AddImageToPage(App.ViewModel.SelectedImageName); 
                }
            }
            else
            {
                //load previously made page
                MessageBox.Show("cannot edit an existing page yet", "Error", MessageBoxButton.OK);
                NavigationService.GoBack();
                return; 
            }

            PageImage.Source = _pageImage; 
        }

        private void DrawBlankPage()
        {
            switch (_layout)
            {
                case PageLayout.Single:
                    DrawBlankSinglePage();
                    break; 
                default:
                    throw new NotImplementedException(); 
            }
        }

        private void DrawBlankSinglePage()
        {
            var centreX = DefaultWidth/2; 
            var centreY = DefaultHeight/2; 
            _pageImage.FillRectangle(0, 0, DefaultWidth, DefaultHeight, Colors.White);
            _pageImage.DrawRectangle(DefaultPageMargin, DefaultPageMargin, DefaultWidth - DefaultPageMargin, DefaultHeight - DefaultPageMargin, Colors.Black);
            DrawAddIcon(centreX, centreY);
        }

        private void DrawAddIcon(int centreX, int centreY)
        {
            _pageImage.FillEllipseCentered(centreX, centreY, 70, 70, Colors.Gray);
            _pageImage.FillRectangle(centreX - 10, centreY - 50, centreX + 10, centreY + 50, Colors.White);
            _pageImage.FillRectangle(centreX - 50, centreY - 10, centreX + 50, centreY + 10, Colors.White);
        }

        private void PageImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //get which panel has been tapped
            var tapPosition = e.GetPosition(PageImage);
            switch(_layout)
            {
                case PageLayout.Single:
                    top = DefaultPageMargin;
                    left = DefaultPageMargin;
                    width = DefaultWidth - (2 * DefaultPageMargin); 
                    height = DefaultHeight- (2 * DefaultPageMargin);
                    break;
                default:
                    MessageBox.Show("cannot add to this page layout yet", "Error", MessageBoxButton.OK);
                    NavigationService.GoBack();
                    break;
            }

            //show image selector
            App.ViewModel.SelectedImageName = string.Empty; 
            NavigationService.Navigate(new Uri(
                                            string.Format("/PickImagePage.xaml?new=true&top={0}&left={1}&width={2}&height={3}&layout={4}",
                                                top, left, width, height, _layout), 
                                            UriKind.Relative
                                        )); 
        }

        private PageLayout ParseLayoutString(string layout)
        {
            switch (layout)
            {
                case "Single":
                    return PageLayout.Single;
                default:
                    MessageBox.Show("cannot edit this page layout yet", "Error", MessageBoxButton.OK);
                    NavigationService.GoBack();
                    return PageLayout.Single;
            }
        }

        private IEnumerable<ImageItem> LoadImageItems()
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                foreach (var file in store.GetFileNames("image_*"))
                {
                    var bitmap = new BitmapImage();
                    bitmap.SetSource(store.OpenFile(file, System.IO.FileMode.Open));
                    yield return new ImageItem { Name = file, Image = new WriteableBitmap(bitmap) };
                }
            }
        }

        private async void AddImageToPage(string imageName)
        {
            //image selected
            var selectedImage = App.ViewModel.ImageItems.FirstOrDefault(i => i.Name.Equals(imageName, StringComparison.InvariantCultureIgnoreCase)).Image;
            if (selectedImage == null)
            {
                MessageBox.Show("cannot find selected image", "Error", MessageBoxButton.OK);
                NavigationService.GoBack();
            }

            //TODO: allow selective cropping of the image

            //add cropped image to main image
            //TODO - select a region of the image
            try
            {
                var offsetTop = 0;
                var offsetLeft = 0;
                var selectedImageWidth = selectedImage.PixelWidth;
                var selectedImageHeight = selectedImage.PixelHeight; 
                var aspectRatioOriginal = (double)width / (double)height;
                var aspectRatioImport = (double)selectedImage.PixelWidth / (double)selectedImage.PixelHeight;
                if (aspectRatioImport > aspectRatioOriginal)
                {
                    var zoom = (double)selectedImageHeight / (double)height;
                    selectedImageWidth = (int)(width * zoom); 
                    offsetLeft = (selectedImage.PixelWidth - selectedImageWidth) / 2;
                }
                else
                {
                    var zoom = (double)selectedImageWidth / (double)width;
                    selectedImageHeight = (int)(height * zoom);
                    offsetTop = (selectedImage.PixelHeight - selectedImageHeight) / 2;
                }

                _pageImage.Blit(new Rect(left, top, width, height),
                                selectedImage,
                                new Rect(offsetLeft, offsetTop, selectedImageWidth, selectedImageHeight));
                PageImage.Source = _pageImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);               
            } 
        }
    }
}

//crop image
//// Create effect collection with the source stream
//var fileStream = new MemoryStream();
//selectedImage.SaveJpeg(fileStream, selectedImage.PixelWidth, selectedImage.PixelHeight, 100, 100);
//fileStream.Seek(0, SeekOrigin.Begin); 
//var target = new WriteableBitmap(width, height);
//using (var source = new StreamImageSource(fileStream))
//{
//    using (var filters = new FilterEffect(source))
//    {
//        // Initialize the filter 
//        var sampleFilter = new CropFilter(new Windows.Foundation.Rect(0, 0, width, height));

//        // Add the filter to the FilterEffect collection
//        filters.Filters = new IFilter[] { sampleFilter };

//        // Create a target where the filtered image will be rendered to

//        // Create a new renderer which outputs WriteableBitmaps
//        using (var renderer = new WriteableBitmapRenderer(filters, target))
//        {
//            // Render the image with the filter(s)
//            await renderer.RenderAsync();
//        }
//    }
//}
