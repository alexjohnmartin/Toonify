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
using BugSense;

//Handling swipe events
//http://stackoverflow.com/questions/21399514/implement-swipe-event-on-wp8

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
            BugSenseHandler.Instance.LeaveBreadCrumb("ViewImagePage - navigated to");
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
            BugSenseHandler.Instance.LeaveBreadCrumb("ViewImagePage - delete");
            if (ImageHelper.Delete(_imageItem))
                NavigationService.GoBack();
        }

        private void Export_Click(object sender, EventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("ViewImagePage - export");
            ImageHelper.SaveImageToMediaLibrary(_imageItem);
            MessageBox.Show("Image has been exported to your device's photos", "Export", MessageBoxButton.OK);
        }

        private void Share_Click(object sender, EventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("ViewImagePage - share");
            ImageHelper.Share(_imageItem); 
        }

        private void ImageDisplay_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("ViewImagePage - swipe");
            if (e.FinalVelocities.LinearVelocity.X < 0)
                LoadNextPage();
            if (e.FinalVelocities.LinearVelocity.X > 0)
                LoadPreviousPage();
        }

        private void LoadPreviousPage()
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("ViewImagePage - load prev");
            var index = App.ViewModel.ImageItems.IndexOf(_imageItem);
            if (index > 0) index--;
            _imageItem = App.ViewModel.ImageItems[index];
            ImageDisplay.Source = _imageItem.Image;
        }

        private void LoadNextPage()
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("ViewImagePage - load next");
            var index = App.ViewModel.ImageItems.IndexOf(_imageItem);
            if (index < App.ViewModel.ImageItems.Count - 1) index++;
            _imageItem = App.ViewModel.ImageItems[index];
            ImageDisplay.Source = _imageItem.Image;
        }
    }
}