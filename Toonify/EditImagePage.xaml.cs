using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Nokia.Graphics.Imaging;
using Nokia.InteropServices.WindowsRuntime;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using System.IO.IsolatedStorage;

namespace Toonify
{
    public partial class EditImagePage : PhoneApplicationPage
    {
        private const int DefaultWidth = 768;
        private const int DefaultHeight = 1000;

        private string _filename = string.Empty;
        private WriteableBitmap _finalImageBitmap = null;
        private WriteableBitmap _sketchImageBitmap = null;
        private WriteableBitmap _cartoonImageBitmap = null;
        private WriteableBitmap _thumbnailImageBitmap = null;

        public EditImagePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _filename = string.Empty;
            if (!NavigationContext.QueryString.TryGetValue("name", out _filename)) return; 

            foreach (MediaSource source in MediaSource.GetAvailableMediaSources())
            {
                if (source.MediaSourceType == MediaSourceType.LocalDevice)
                {
                    var mediaLibrary = new MediaLibrary(source);    
                    PictureAlbumCollection allAlbums = mediaLibrary.RootPictureAlbum.Albums;
                    foreach (PictureAlbum album in allAlbums)
                    {
                        if (album.Name == "Camera Roll")
                        {
                            var pic = album.Pictures.Where(p => p.Name.Equals(_filename, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault(); 
                            if (pic != null)
                            {
                                CreateCartoonImage(pic.GetImage(), pic.Width, pic.Height); 
                            }
                        }
                    }
                }
            }
        }

        private async void CreateCartoonImage(Stream chosenPhoto, int width, int height)
        {   
            try
            {
                var originalImage = new WriteableBitmap(width, height);

                var selectedImageWidth = DefaultWidth;
                var selectedImageHeight = DefaultHeight;
                var aspectRatioOriginal = (double)width / (double)height;
                var aspectRatioImport = (double)DefaultWidth / (double)DefaultHeight;
                if (aspectRatioImport < aspectRatioOriginal)
                {
                    var zoom = (double)height / (double)DefaultHeight;
                    selectedImageWidth = (int)(width / zoom);
                }
                else
                {
                    var zoom = (double)width / (double)DefaultWidth;
                    selectedImageHeight = (int)(height / zoom);
                }

                _finalImageBitmap = new WriteableBitmap(selectedImageWidth, selectedImageHeight);
                _sketchImageBitmap = new WriteableBitmap(selectedImageWidth, selectedImageHeight);
                _cartoonImageBitmap = new WriteableBitmap(selectedImageWidth, selectedImageHeight);
                _thumbnailImageBitmap = new WriteableBitmap(selectedImageWidth, selectedImageHeight);

                _thumbnailImageBitmap.Blit(new Rect(0, 0, selectedImageWidth, selectedImageHeight),
                                originalImage,
                                new Rect(0, 0, width, height));

                // Show thumbnail of original image.
                //_thumbnailImageBitmap.SetSource(chosenPhoto);
                OriginalDisplay.Source = _thumbnailImageBitmap;

                // Rewind stream to start.                     
                chosenPhoto.Position = 0;

                var imageStream = new StreamImageSource(chosenPhoto);

                // A cartoon effect is initialized with selected image stream as source.
                var sketchEffect = await RenderSketchImage(imageStream);
                var cartoonEffect = await RenderCartoonImage(imageStream);
                await RenderFinalImage(sketchEffect, cartoonEffect);

                ImageDisplay.Source = _finalImageBitmap;
                SketchDisplay.Source = _sketchImageBitmap;
                CartoonDisplay.Source = _cartoonImageBitmap;

                //save resulting image
                //_cartoonImageBitmap.SaveToMediaLibrary("toonify_" + _filename); 

                LoadingPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
            catch (OutOfMemoryException)
            {
                MessageBox.Show("Out of memory", "Error", MessageBoxButton.OK);
                NavigateBackToHomeScreen();
            }
            catch (Exception)
            {
                MessageBox.Show("Out of memory", "Error", MessageBoxButton.OK);
                NavigationService.GoBack(); 
            }
        }

        private async System.Threading.Tasks.Task RenderFinalImage(FilterEffect sketchEffect, FilterEffect cartoonEffect)
        {
            var blendFilter = new BlendFilter(sketchEffect, BlendFunction.Multiply);
            var blendEffect = new FilterEffect(cartoonEffect);
            blendEffect.Filters = new[] { blendFilter };
            var finalRenderer = new WriteableBitmapRenderer(blendEffect, _finalImageBitmap);
            await finalRenderer.RenderAsync();
        }

        private async System.Threading.Tasks.Task<FilterEffect> RenderSketchImage(StreamImageSource imageStream)
        {
            //var sketchFilter = new SketchFilter(SketchMode.Gray);
            var sketchFilter = new StampFilter(5, 0.5); 
            var sketchEffect = new FilterEffect(imageStream);
            sketchEffect.Filters = new[] { sketchFilter };
            var sketchRenderer = new WriteableBitmapRenderer(sketchEffect, _sketchImageBitmap);
            await sketchRenderer.RenderAsync();
            return sketchEffect;
        }

        private async System.Threading.Tasks.Task<FilterEffect> RenderCartoonImage(StreamImageSource imageStream)
        {
            var cartoonEffect = new FilterEffect(imageStream);
            var cartoonFilter = new CartoonFilter();
            cartoonEffect.Filters = new[] { cartoonFilter };
            var cartoonRenderer = new WriteableBitmapRenderer(cartoonEffect, _cartoonImageBitmap);
            await cartoonRenderer.RenderAsync();
            return cartoonEffect; 
        }

        private void CombinedButton_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(_cartoonImageBitmap);
            NavigateBackToHomeScreen(); 
        }

        private void SketchButton_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(_sketchImageBitmap);
            NavigateBackToHomeScreen(); 
        }

        private void CartoonButton_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(_finalImageBitmap);
            NavigateBackToHomeScreen(); 
        }

        private void OriginalButton_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(_thumbnailImageBitmap);
            NavigateBackToHomeScreen();
        }

        private void NavigateBackToHomeScreen()
        {
            var lastStackItem = NavigationService.BackStack.FirstOrDefault(); 
            if (lastStackItem.Source.OriginalString.Contains("ImportImagePage.xaml"))
                NavigationService.RemoveBackEntry();

            NavigationService.GoBack(); 
        }

        private void SaveImage(WriteableBitmap bitmap)
        {
            var fileStream = new MemoryStream();
            bitmap.SaveJpeg(fileStream, bitmap.PixelWidth, bitmap.PixelHeight, 100, 100);
            fileStream.Seek(0, SeekOrigin.Begin); 
            
            var filename = "image_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg"; 
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var stream = store.CreateFile(filename);

                var reader = new StreamReader(fileStream);
                byte[] contents;
                using (BinaryReader bReader = new BinaryReader(reader.BaseStream))
                {
                    contents = bReader.ReadBytes((int)reader.BaseStream.Length);
                }
                stream.Write(contents, 0, contents.Length);
                stream.Close();
            }

            App.ViewModel.ImageItems.Add(new ImageItem { Name = filename, Image = bitmap }); 
        }
    }
}