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

namespace Toonify
{
    public partial class MainPage : PhoneApplicationPage
    {
        CameraCaptureTask cameraCaptureTask = new CameraCaptureTask();

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;

            cameraCaptureTask.Completed += new EventHandler<PhotoResult>(cameraCaptureTask_Completed);
        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }

            UpdateTutorialText(); 
        }

        protected void cameraCaptureTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                var fullPath = e.OriginalFileName;
                var filename = fullPath.Contains('\\') ? fullPath.Substring(fullPath.LastIndexOf('\\') + 1) : fullPath; 
                NavigationService.Navigate(new Uri("/EditImagePage.xaml?name=" + filename, UriKind.Relative));
            }
        }

        private void TakePhotoButton_Click(object sender, RoutedEventArgs e)
        {
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
            var task = new WebBrowserTask
            {
                Uri = new Uri("https://twitter.com/AlexJohnMartin", UriKind.Absolute)
            };
            task.Show();
        }

        public void StoreButton_Click(object sender, EventArgs e)
        {
            var currentCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var task = new WebBrowserTask
            {
                Uri = new Uri(string.Format("http://www.windowsphone.com/{0}/store/publishers?publisherId=nocturnal%2Btendencies&appId=63cb6767-4940-4fa1-be8c-a7f58e455c3b", currentCulture.Name), UriKind.Absolute)
            };
            task.Show();
        }

        public void ReviewButton_Click(object sender, EventArgs e)
        {
            //FeedbackHelper.Default.Reviewed();
            var marketplace = new MarketplaceReviewTask();
            marketplace.Show();
        }

        public void EmailButton_Click(object sender, EventArgs e)
        {
            var email = new EmailComposeTask();
            email.Subject = "Feedback for the Calendar Tile application";
            email.Show();
        }

        //private void PageButton_Click(object sender, RoutedEventArgs e)
        //{
        //    var button = (Button)e.OriginalSource; 
        //    NavigationService.Navigate(new Uri("/EditPagePage.xaml?edit=" + button.CommandParameter, UriKind.Relative));
        //}

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var image = (Image)e.OriginalSource;
            NavigationService.Navigate(new Uri("/ViewImagePage.xaml?name=" + image.Tag, UriKind.Relative));
        }

        private void PageImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var image = (Image)e.OriginalSource; 
            NavigationService.Navigate(new Uri("/EditPagePage.xaml?edit=" + image.Tag, UriKind.Relative));
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var imageItem = (ImageItem)menuItem.Tag;
            ImageHelper.SaveImageToMediaLibrary(imageItem); 
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var imageItem = (ImageItem)menuItem.Tag; 
            ImageHelper.Delete(imageItem); 
        }

        private void Share_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var imageItem = (ImageItem)menuItem.Tag;
            ImageHelper.Share(imageItem); 
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
    }
}