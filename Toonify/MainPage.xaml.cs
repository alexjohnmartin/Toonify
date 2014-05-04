using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.Windows.Media;
using System.Xml.Linq;
using BugSense;
using RateMyApp.Helpers;

namespace Toonify
{
    public partial class MainPage : PhoneApplicationPage
    {
        CameraCaptureTask cameraCaptureTask = new CameraCaptureTask();

        public MainPage()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
            cameraCaptureTask.Completed += new EventHandler<PhotoResult>(cameraCaptureTask_Completed);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - navigated to");
            if (!App.ViewModel.IsDataLoaded)
            {
                BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - loading data");
                App.ViewModel.LoadData();
            }

            UpdateTutorialText();
            UpdateButtonColor();
        }

        protected void cameraCaptureTask_Completed(object sender, PhotoResult e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - photo taken");
            if (e.TaskResult == TaskResult.OK)
            {
                var fullPath = e.OriginalFileName;
                var filename = fullPath.Contains('\\') ? fullPath.Substring(fullPath.LastIndexOf('\\') + 1) : fullPath; 
                NavigationService.Navigate(new Uri("/EditImagePage.xaml?name=" + filename, UriKind.Relative));
            }
        }

        private void TakePhotoButton_Click(object sender, RoutedEventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - start camera task");
            cameraCaptureTask.Show(); 
        }

        private void ImportImageButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/ImportImagePage.xaml", UriKind.Relative)); 
        }

        private void NewPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/EditPagePage.xaml?new=true", UriKind.Relative));
        }

        public void TwitterButton_Click(object sender, EventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - twitter");
            var task = new WebBrowserTask
            {
                Uri = new Uri("https://twitter.com/AlexJohnMartin", UriKind.Absolute)
            };
            task.Show();
        }

        public void StoreButton_Click(object sender, EventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - store");
            var currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var task = new WebBrowserTask
            {
                Uri = new Uri(string.Format("http://www.windowsphone.com/{0}/store/publishers?publisherId=nocturnal%2Btendencies&appId=63cb6767-4940-4fa1-be8c-a7f58e455c3b", currentCulture.Name), UriKind.Absolute)
            };
            task.Show();
        }

        public void ReviewButton_Click(object sender, EventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - review");
            FeedbackHelper.Default.Reviewed();
            var marketplace = new MarketplaceReviewTask();
            marketplace.Show();
        }

        public void EmailButton_Click(object sender, EventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - email");
            var email = new EmailComposeTask();
            email.To = "alexmartin9999@hotmail.com";
            email.Subject = "Feedback for the Calendar Tile application";
            email.Show();
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var image = (FrameworkElement)e.OriginalSource;
            NavigationService.Navigate(new Uri("/ViewImagePage.xaml?name=" + image.Tag, UriKind.Relative));
        }

        private void PageImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var image = (FrameworkElement)e.OriginalSource; 
            NavigationService.Navigate(new Uri("/EditPagePage.xaml?edit=" + image.Tag, UriKind.Relative));
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - export");
            var menuItem = (MenuItem)sender;
            var imageItem = (ImageItem)menuItem.Tag;
            ImageHelper.SaveImageToMediaLibrary(imageItem); 
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - delete");
            var menuItem = (MenuItem)sender;
            var imageItem = (ImageItem)menuItem.Tag; 
            ImageHelper.Delete(imageItem); 
        }

        private void Share_Click(object sender, RoutedEventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - share");
            var menuItem = (MenuItem)sender;
            var imageItem = (ImageItem)menuItem.Tag;
            ImageHelper.Share(imageItem); 
        }

        private void UpdateButtonColor()
        {
            VersionTextBox.Text = "v" + XDocument.Load("WMAppManifest.xml").Root.Element("App").Attribute("Version").Value;
            ReviewButton.Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneAccentColor"]);
            EmailButton.Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneAccentColor"]);
            StoreButton.Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneAccentColor"]);
            PinButton.Background = new SolidColorBrush((Color)Application.Current.Resources["PhoneAccentColor"]);
        }

        private void UpdateTutorialText()
        {
            bool anyPages = App.ViewModel.PageItems.Any();
            bool anyImages = App.ViewModel.ImageItems.Any();

            NewPage.IsEnabled = anyImages; 
            PageTutorialText.Visibility = anyPages && anyImages ? Visibility.Collapsed : Visibility.Visible;
            ImageTutorialText.Visibility = anyPages && anyImages ? Visibility.Collapsed : Visibility.Visible;

            if (!anyImages)
            {
                PageTutorialText.Text = Toonify.Resources.AppResources.TutorialTextImportImages;
                ImageTutorialText.Text = Toonify.Resources.AppResources.TutorialTextImportImages;
            }
            else if (!anyPages)
            {
                PageTutorialText.Text = Toonify.Resources.AppResources.TutorialTextCreatePages;
                ImageTutorialText.Text = Toonify.Resources.AppResources.TutorialTextCreatePages;
            }
        }

        private void AppButton_Click(object sender, RoutedEventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - other app...");
            var button = (Button)sender;
            var task = new MarketplaceDetailTask();
            task.ContentIdentifier = button.Tag.ToString();
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - ..." + button.Tag.ToString());
            task.ContentType = MarketplaceContentType.Applications;
            task.Show();
        }

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - pin to home screen");
            var tile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString().Contains("MainPage.xaml"));
            if (tile == null) ShellTile.Create(new Uri("/MainPage.xaml?test=true", UriKind.Relative), GetTileData(), true);
        }

        private ShellTileData GetTileData()
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("MainPage - get tile data");
            var data = new FlipTileData();
            data.BackgroundImage = new Uri(@"/Assets/Tiles/NormalTile.png", UriKind.Relative);
            data.WideBackgroundImage = new Uri(@"/Assets/Tiles/WideTile.png", UriKind.Relative);
            data.Title = Toonify.Resources.AppResources.ApplicationTitle;
            return data;
        }
    }
}