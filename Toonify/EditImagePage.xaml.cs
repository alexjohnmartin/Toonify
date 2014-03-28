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

namespace Toonify
{
    public partial class EditImagePage : PhoneApplicationPage
    {
        private FilterEffect _cartoonEffect = null;
        private WriteableBitmap _cartoonImageBitmap = null;
        private WriteableBitmap _thumbnailImageBitmap = null;

        public EditImagePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var imageName = string.Empty;
            if (!NavigationContext.QueryString.TryGetValue("name", out imageName)) return; 

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
                            var pic = album.Pictures.Where(p => p.Name.Equals(imageName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault(); 
                            if (pic != null)
                            {
                                //BitmapImage b = new BitmapImage();
                                //b.SetSource(pic.GetImage());
                                //ImageDisplay.Source = b; 
                                CreateCartoonImage(pic.GetImage()); 
                            }
                        }
                    }
                }
            }
        }

        private async void CreateCartoonImage(Stream chosenPhoto)
        {
            _cartoonImageBitmap = new WriteableBitmap((int)ImageDisplay.Width, (int)ImageDisplay.Height);
            _thumbnailImageBitmap = new WriteableBitmap((int)ImageDisplay.Width, (int)ImageDisplay.Height);

            try
            {
                // Show thumbnail of original image.
                _thumbnailImageBitmap.SetSource(chosenPhoto);
                //ImageDisplay.Source = _thumbnailImageBitmap;

                // Rewind stream to start.                     
                chosenPhoto.Position = 0;

                // A cartoon effect is initialized with selected image stream as source.
                var imageStream = new StreamImageSource(chosenPhoto);
                _cartoonEffect = new FilterEffect(imageStream);

                // Add the cartoon filter as the only filter for the effect.
                var cartoonFilter = new CartoonFilter();
                _cartoonEffect.Filters = new[] { cartoonFilter };

                // Render the image to a WriteableBitmap.
                var renderer = new WriteableBitmapRenderer(_cartoonEffect, _thumbnailImageBitmap);
                _cartoonImageBitmap = await renderer.RenderAsync();

                // Set the rendered image as source for the cartoon image control.
                ImageDisplay.Source = _cartoonImageBitmap;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return;
            }
        }
    }
}