using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using System.Windows.Media.Imaging;

namespace Toonify
{
    public partial class EditImagePage : PhoneApplicationPage
    {
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
                                BitmapImage b = new BitmapImage();
                                b.SetSource(pic.GetImage());
                                ImageDisplay.Source = b; 
                            }
                        }
                    }
                }
            }
        }
    }
}