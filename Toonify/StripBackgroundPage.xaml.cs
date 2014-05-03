using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Nokia.Graphics.Imaging;
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework.Media;
using System.IO;
using Windows.UI;
using System.Windows.Media;
using BugSense;

namespace Toonify
{
    public partial class StripBackgroundPage : PhoneApplicationPage
    {
        private WriteableBitmap _cartoonImageBitmap = null;
        private WriteableBitmap _thumbnailImageBitmap = null;

        public StripBackgroundPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            BugSenseHandler.Instance.LeaveBreadCrumb("StripBackgroundPage - navigated to");

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
                                BugSenseHandler.Instance.LeaveBreadCrumb("StripBackgroundPage - showing image");
                                _thumbnailImageBitmap = new WriteableBitmap(pic.Width, pic.Height);
                                _thumbnailImageBitmap.SetSource(pic.GetImage());
                                BeginImage.Source = _thumbnailImageBitmap;

                                //CreateCartoonImage(pic.GetImage(), pic.Width, pic.Height, Colors.Blue);
                            }
                        }
                    }
                }
            }
        }

        private async void CreateCartoonImage(Stream chosenPhoto, int width, int height, Windows.UI.Color color)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("StripBackgroundPage - stripping background");
            _cartoonImageBitmap = new WriteableBitmap(width, height);

            try
            {
                // Rewind stream to start.                     
                chosenPhoto.Position = 0;

                // A cartoon effect is initialized with selected image stream as source.
                var imageStream = new StreamImageSource(chosenPhoto);
                var cartoonEffect = new FilterEffect(imageStream);

                // Add the cartoon filter as the only filter for the effect.
                var cartoonFilter = new ChromaKeyFilter(color, 0.25, 0.1, false); 
                cartoonEffect.Filters = new[] { cartoonFilter };

                // Render the image to a WriteableBitmap.
                var renderer = new WriteableBitmapRenderer(cartoonEffect, _cartoonImageBitmap);
                await renderer.RenderAsync();

                // Set the rendered image as source for the cartoon image control.
                EndImage.Source = _cartoonImageBitmap;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return;
            }
        }

        private void BeginImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("StripBackgroundPage - image tapped");
            var tapPosition = e.GetPosition(BeginImage);
            double zoom = GetZoomLevelOfOriginalImage();
            double x = (double)tapPosition.X;
            double y = (double)tapPosition.Y; 
            var color = _thumbnailImageBitmap.GetPixel((int)Math.Floor(x * zoom), (int)Math.Floor(y * zoom));
            FilterColor.Fill = new SolidColorBrush(color); 

            var fileStream = new MemoryStream();
            _thumbnailImageBitmap.SaveJpeg(fileStream, _thumbnailImageBitmap.PixelWidth, _thumbnailImageBitmap.PixelHeight, 100, 100);
            fileStream.Seek(0, SeekOrigin.Begin);
            CreateCartoonImage(fileStream, _thumbnailImageBitmap.PixelWidth, _thumbnailImageBitmap.PixelHeight, Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B)); 
        }

        private double GetZoomLevelOfOriginalImage()
        {
            var aspectRatioOfOriginalImage = (double)_thumbnailImageBitmap.PixelWidth / (double)_thumbnailImageBitmap.PixelHeight;
            var aspectRatioOfDisplayControl = 480/350;

            if (aspectRatioOfDisplayControl > aspectRatioOfOriginalImage)
            {
                return (double)_thumbnailImageBitmap.PixelHeight / (double)350; 
            }
            else
            {
                return (double)_thumbnailImageBitmap.PixelWidth / (double)480; 
            }
        }
    }
}