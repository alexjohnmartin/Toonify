using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Media.PhoneExtensions;

namespace Toonify
{
    public partial class ViewImagePage : PhoneApplicationPage
    {
        private ImageItem _imageItem; 

        public ViewImagePage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var filename = string.Empty;
            if (!NavigationContext.QueryString.TryGetValue("name", out filename)) NavigationService.GoBack();

            _imageItem = App.ViewModel.ImageItems.FirstOrDefault(i => i.Name.Equals(filename, StringComparison.InvariantCultureIgnoreCase));
            if (_imageItem == null) NavigationService.GoBack();

            ImageDisplay.Source = _imageItem.Image;
            BuildApplicationBar(); 
        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar();

            var exportButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.save.png", UriKind.Relative));
            exportButton.Text = "export"; //MeetMeHere.Support.Resources.AppResources.AppBarRefreshButtonText;
            exportButton.Click += Export_Click;
            ApplicationBar.Buttons.Add(exportButton);

            var shareButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.message.send.png", UriKind.Relative));
            shareButton.Text = "share"; //MeetMeHere.Support.Resources.AppResources.AppBarRefreshButtonText;
            shareButton.Click += Share_Click;
            ApplicationBar.Buttons.Add(shareButton);
        }

        private void Export_Click(object sender, EventArgs e)
        {
            SaveImageToMediaLibrary();
            MessageBox.Show("Image has been exported to your device's photos", "Export", MessageBoxButton.OK);
        }

        private void Share_Click(object sender, EventArgs e)
        {
            var shareTask = new ShareMediaTask();
            shareTask.FilePath = SaveImageToMediaLibrary();
            shareTask.Show();
        }

        private string SaveImageToMediaLibrary()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                _imageItem.Image.SaveJpeg(stream, _imageItem.Image.PixelWidth, _imageItem.Image.PixelHeight, 0, 100);
                stream.Seek(0, SeekOrigin.Begin);

                foreach (MediaSource source in MediaSource.GetAvailableMediaSources())
                {
                    if (source.MediaSourceType == MediaSourceType.LocalDevice)
                    {
                        var mediaLibrary = new MediaLibrary(source);
                        var picture = mediaLibrary.SavePicture(_imageItem.Name, stream);
                        return picture.GetPath();
                    }
                }
            }

            return string.Empty;
        }
    }
}