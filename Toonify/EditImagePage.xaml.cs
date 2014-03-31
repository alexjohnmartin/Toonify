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
                                //BitmapImage b = new BitmapImage();
                                //b.SetSource(pic.GetImage());
                                //ImageDisplay.Source = b; 
                                CreateCartoonImage(pic.GetImage(), pic.Width, pic.Height); 
                            }
                        }
                    }
                }
            }
        }

        private async void CreateCartoonImage(Stream chosenPhoto, int width, int height)
        {
            _finalImageBitmap = new WriteableBitmap(width, height);
            _sketchImageBitmap = new WriteableBitmap(width, height);
            _cartoonImageBitmap = new WriteableBitmap(width, height);
            _thumbnailImageBitmap = new WriteableBitmap(width, height);

            try
            {
                // Show thumbnail of original image.
                _thumbnailImageBitmap.SetSource(chosenPhoto);
                //ImageDisplay.Source = _thumbnailImageBitmap;

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
            catch (Exception exception)
            {
                //MessageBox.Show(exception.Message);
                LoadingMessage.Text = "Error, please try another image";
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
            SaveImage(_finalImageBitmap);
            NavigationService.GoBack();
        }

        private void SketchButton_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(_finalImageBitmap);
            NavigationService.GoBack(); 
        }

        private void CartoonButton_Click(object sender, RoutedEventArgs e)
        {
            SaveImage(_finalImageBitmap);
            NavigationService.GoBack();
        }

        private void SaveImage(WriteableBitmap bitmap)
        {
            var fileStream = new MemoryStream();
            bitmap.SaveJpeg(fileStream, bitmap.PixelWidth, bitmap.PixelHeight, 100, 100);
            fileStream.Seek(0, SeekOrigin.Begin); 

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var filename = "image_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg"; 
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
        }
    }
}