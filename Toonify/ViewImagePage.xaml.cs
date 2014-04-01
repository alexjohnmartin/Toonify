using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO;
using System.IO.IsolatedStorage;
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

            var deleteButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.delete.png", UriKind.Relative));
            deleteButton.Text = "delete"; //MeetMeHere.Support.Resources.AppResources.AppBarRefreshButtonText;
            deleteButton.Click += Delete_Click;
            ApplicationBar.Buttons.Add(deleteButton);
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (ImageHelper.Delete(_imageItem))
                NavigationService.GoBack();
        }

        private void Export_Click(object sender, EventArgs e)
        {
            ImageHelper.SaveImageToMediaLibrary(_imageItem);
            MessageBox.Show("Image has been exported to your device's photos", "Export", MessageBoxButton.OK);
        }

        private void Share_Click(object sender, EventArgs e)
        {
            ImageHelper.Share(_imageItem); 
        }
    }
}