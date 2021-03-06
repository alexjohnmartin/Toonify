﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Media.PhoneExtensions;
using Nokia.Graphics.Imaging;
using BugSense;
using Nokia.InteropServices.WindowsRuntime;

namespace Toonify
{
    public partial class EditPagePage : PhoneApplicationPage
    {
        //768 x 1000 = page size = 8" x 10.5" at 96dpi
        //private const int DefaultWidth = 768;
        //private const int DefaultHeight = 1000;
        private const int DefaultWidth = 600;
        private const int DefaultHeight = 800;
        private const int DefaultPageMargin = 20;
        private const int DefaultFontsize = 20;

        private int top = 0;
        private int left = 0;
        private int width = 0;
        private int height = 0;
        private PageLayout _layout;
        private WriteableBitmap _pageImage;
        private bool _addSpeechBubble = false; 
        private string _pageFileName = string.Empty;
        private string _speechBubbleWatermark = string.Empty;

        public EditPagePage()
        {
            InitializeComponent();
            ApplicationBar = new ApplicationBar();
            _speechBubbleWatermark = Toonify.Resources.AppResources.EnterSpeechBubbleTextWatermark;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            BugSenseHandler.Instance.LeaveBreadCrumb("EditPagePage - navigated to");
            AddImage_Click(null, null); 

            var dummy = string.Empty; 
            var newPage = NavigationContext.QueryString.TryGetValue("new", out dummy);

            if (!string.IsNullOrEmpty(App.ViewModel.SelectedImageName))
            {
                BugSenseHandler.Instance.LeaveBreadCrumb("EditPagePage - adding image to page");
                AddImageToPage(App.ViewModel.SelectedImageName);
                SavePage();
                BuildApplicationBar();
                App.ViewModel.SelectedImageName = string.Empty; 
            }
            else if (newPage)
            {
                BugSenseHandler.Instance.LeaveBreadCrumb("EditPagePage - new page");
                PageLayoutPanel.Visibility = System.Windows.Visibility.Visible;
                PagePanel.Visibility = System.Windows.Visibility.Collapsed;
                TextDialog.Visibility = System.Windows.Visibility.Collapsed;
                _pageFileName = string.Empty;
                _pageImage = new WriteableBitmap(DefaultWidth, DefaultHeight);
                _addSpeechBubble = false; 
            }
            else if (NavigationContext.QueryString.TryGetValue("edit", out _pageFileName))
            {
                BugSenseHandler.Instance.LeaveBreadCrumb("EditPagePage - edit existing page");
                //load previously made page
                BuildApplicationBar();

                //get layout-type from file name
                var parts = _pageFileName.Split('_');
                _layout = (PageLayout)Enum.Parse(typeof(PageLayout), parts[1]); 

                LoadImageFromIsolatedStorage(); 

                PageLayoutPanel.Visibility = System.Windows.Visibility.Collapsed;
                PagePanel.Visibility = System.Windows.Visibility.Visible;
                TextDialog.Visibility = System.Windows.Visibility.Collapsed;
                _addSpeechBubble = false;
                PageImage.Source = new WriteableBitmap(_pageImage);
            }

            PageImage.Source = new WriteableBitmap(_pageImage);
        }

        protected void AddSpeech_Click(object sender, EventArgs e)
        {
            _addSpeechBubble = true;
            //MessageBox.Show("tap on image to place a speech bubble", "Speech bubble", MessageBoxButton.OK); 
            //TitleText.Text = "add text"; 
            AddSpeechButton.IsEnabled = false;
            AddImageButton.IsEnabled = true; 
        }

        protected void AddImage_Click(object sender, EventArgs e)
        {
            _addSpeechBubble = false;
            AddSpeechButton.IsEnabled = true;
            AddImageButton.IsEnabled = false; 
        }

        private void Export_Click(object sender, EventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("EditPagePage - export");
            SaveImageToMediaLibrary();
            MessageBox.Show(Toonify.Resources.AppResources.PageHasBeenExported, Toonify.Resources.AppResources.ExportTitle, MessageBoxButton.OK);
        }

        private void Share_Click(object sender, EventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("EditPagePage - share");
            var shareTask = new ShareMediaTask();
            shareTask.FilePath = SaveImageToMediaLibrary();
            shareTask.Show(); 
        }

        private void PageImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("EditPagePage - image tap");
            var tapPosition = e.GetPosition(PageImage);

            if (_addSpeechBubble)
            {
                top = (int)tapPosition.Y;
                left = (int)tapPosition.X;
                width = 0;
                height = 0;
                ShowTextInputBox(); 
            }
            else
            {
                //get which panel has been tapped
                switch (_layout)
                {
                    case PageLayout.Single:
                        top = DefaultPageMargin;
                        left = DefaultPageMargin;
                        width = DefaultWidth - (2 * DefaultPageMargin);
                        height = DefaultHeight - (2 * DefaultPageMargin);
                        break;
                    case PageLayout.DoubleSimple:
                        left = DefaultPageMargin;
                        width = DefaultWidth - (2 * DefaultPageMargin);
                        height = (DefaultHeight - (3 * DefaultPageMargin)) / 2;
                        if (tapPosition.Y < PageImage.ActualHeight / 2)
                            top = DefaultPageMargin;
                        else
                            top = (DefaultPageMargin*2) + height;
                        break;
                    case PageLayout.TripleSimple:
                        left = DefaultPageMargin;
                        width = DefaultWidth - (2 * DefaultPageMargin);
                        height = (DefaultHeight - (4 * DefaultPageMargin)) / 3;
                        if (tapPosition.Y < PageImage.ActualHeight / 3)
                            top = DefaultPageMargin;
                        else if (tapPosition.Y < (PageImage.ActualHeight / 3) * 2)
                            top = (DefaultPageMargin*2) + (height);
                        else
                            top = (DefaultPageMargin*3) + (height * 2);
                        break; 
                    case PageLayout.FourSimple:
                        width = (DefaultWidth - (3 * DefaultPageMargin)) / 2;
                        height = (DefaultHeight - (3 * DefaultPageMargin)) / 2;
                        if (tapPosition.Y < PageImage.ActualHeight / 2)
                            top = DefaultPageMargin;
                        else
                            top = (DefaultPageMargin*2) + height;
                        if (tapPosition.X < PageImage.ActualWidth / 2)
                            left = DefaultPageMargin;
                        else
                            left = DefaultPageMargin * 2 + width; 
                        break;
                    default:
                        MessageBox.Show("cannot add to this page layout yet", "Error", MessageBoxButton.OK);
                        NavigationService.GoBack();
                        break;
                }

                //show image selector
                App.ViewModel.SelectedImageName = string.Empty;
                NavigationService.Navigate(new Uri(
                                                string.Format("/PickImagePage.xaml?new=true&top={0}&left={1}&width={2}&height={3}&layout={4}",
                                                    top, left, width, height, _layout),
                                                UriKind.Relative
                                            ));
            }
        }

        private void ShowTextInputBox()
        {
            SpeechBubbleTextbox.Text = string.Empty; 
            TextDialog.Visibility = System.Windows.Visibility.Visible; 
        }

        private void SpeechOkButton_Click(object sender, RoutedEventArgs e)
        {
            TextDialog.Visibility = System.Windows.Visibility.Collapsed;
            DrawSpeechBubble();
            SavePage(); 
        }

        private void SpeechCancelButton_Click(object sender, RoutedEventArgs e)
        {
            TextDialog.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void LayoutButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)e.OriginalSource;
            _layout = ParseLayoutString(button.CommandParameter.ToString());
            PageLayoutPanel.Visibility = System.Windows.Visibility.Collapsed;
            PagePanel.Visibility = System.Windows.Visibility.Visible;

            //start new page
            _pageFileName = GeneratePageFileName();
            DrawBlankPage();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Toonify.Resources.AppResources.DeletePageAreYouSure, Toonify.Resources.AppResources.DeleteTitle, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                BugSenseHandler.Instance.LeaveBreadCrumb("EditPagePage - delete image");
                //delete image from isostore
                using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (iso.FileExists(_pageFileName))
                        iso.DeleteFile(_pageFileName);
                }

                //remove image from viewmodel
                var item = App.ViewModel.PageItems.FirstOrDefault(i => i.Name.Equals(_pageFileName, StringComparison.InvariantCultureIgnoreCase));
                App.ViewModel.RemovePageItem(item);

                NavigationService.GoBack();
            }
        }

        private void WatermarkTB_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SpeechBubbleTextbox.Text == _speechBubbleWatermark)
            {
                SpeechBubbleTextbox.Text = "";
                SolidColorBrush Brush1 = new SolidColorBrush();
                Brush1.Color = Colors.Magenta;
                SpeechBubbleTextbox.Foreground = Brush1;
            }
        }

        private void WatermarkTB_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SpeechBubbleTextbox.Text == String.Empty)
            {
                SpeechBubbleTextbox.Text = _speechBubbleWatermark;
                SolidColorBrush Brush2 = new SolidColorBrush();
                Brush2.Color = Colors.Blue;
                SpeechBubbleTextbox.Foreground = Brush2;
            }
        }

        private string SaveImageToMediaLibrary()
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("EditPagePage - save image to media library");
            using (MemoryStream stream = new MemoryStream())
            {
                _pageImage.SaveJpeg(stream, _pageImage.PixelWidth, _pageImage.PixelHeight, 0, 100);
                stream.Seek(0, SeekOrigin.Begin);

                foreach (MediaSource source in MediaSource.GetAvailableMediaSources())
                {
                    if (source.MediaSourceType == MediaSourceType.LocalDevice)
                    {
                        var mediaLibrary = new MediaLibrary(source);
                        var picture = mediaLibrary.SavePicture(_pageFileName, stream);
                        return picture.GetPath();
                    }
                }
            }

            return string.Empty;
        }

        private void LoadImageFromIsolatedStorage()
        {
            try
            {
                BugSenseHandler.Instance.LeaveBreadCrumb("EditPagePage - load image from iso storage");
                using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (iso.FileExists(_pageFileName))
                    {
                        //using (var stream = new IsolatedStorageFileStream(_pageFileName, FileMode.Open, iso))
                        using (var stream = iso.OpenFile(_pageFileName, FileMode.Open, FileAccess.Read))
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.SetSource(stream);
                            _pageImage = new WriteableBitmap(bitmap);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Image not found: " + _pageFileName, "Error loading image", MessageBoxButton.OK);
                        NavigationService.GoBack();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error loading image", MessageBoxButton.OK);
                NavigationService.GoBack();
            }
        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from MeetMeHere.Support.MeetMeHereResources.
            //var addSpeechButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.chat.png", UriKind.Relative));
            //addSpeechButton.Text = "add speech bubble"; //MeetMeHere.Support.Resources.AppResources.AppBarRefreshButtonText;
            //addSpeechButton.Click += AddSpeech_Click;
            //ApplicationBar.Buttons.Add(addSpeechButton);

            //var addImageButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.image.beach.png", UriKind.Relative));
            //addImageButton.Text = "add image"; //MeetMeHere.Support.Resources.AppResources.AppBarRefreshButtonText;
            //addImageButton.Click += AddImage_Click;
            //ApplicationBar.Buttons.Add(addImageButton);

            var exportButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.save.png", UriKind.Relative));
            exportButton.Text = Toonify.Resources.AppResources.ExportTitle;
            exportButton.Click += Export_Click;
            ApplicationBar.Buttons.Add(exportButton);

            var shareButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.message.send.png", UriKind.Relative));
            shareButton.Text = Toonify.Resources.AppResources.ShareTitle;
            shareButton.Click += Share_Click;
            ApplicationBar.Buttons.Add(shareButton);

            var deleteButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.delete.png", UriKind.Relative));
            deleteButton.Text = Toonify.Resources.AppResources.DeleteTitle;
            deleteButton.Click += Delete_Click;
            ApplicationBar.Buttons.Add(deleteButton);
        }

        private PageLayout ParseLayoutString(string layout)
        {
            switch (layout)
            {
                case "Single":
                    return PageLayout.Single;
                case "DoubleSimple":
                    return PageLayout.DoubleSimple;
                case "TripleSimple":
                    return PageLayout.TripleSimple;
                case "FourSimple":
                    return PageLayout.FourSimple;
                default:
                    MessageBox.Show("cannot edit this page layout yet", "Error", MessageBoxButton.OK);
                    NavigationService.GoBack();
                    return PageLayout.Single;
            }
        }

        private IEnumerable<ImageItem> LoadImageItems()
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("EditPagePage - load image items");
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                foreach (var file in store.GetFileNames("image_*"))
                {
                    var bitmap = new BitmapImage();
                    bitmap.SetSource(store.OpenFile(file, System.IO.FileMode.Open));
                    yield return new ImageItem { Name = file, Image = new WriteableBitmap(bitmap) };
                }
            }
        }

        private async void AddImageToPage(string imageName)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("EditPagePage - add image to page");
            //image selected
            var selectedImage = App.ViewModel.ImageItems.FirstOrDefault(i => i.Name.Equals(imageName, StringComparison.InvariantCultureIgnoreCase)).Image;
            if (selectedImage == null)
            {
                MessageBox.Show("cannot find selected image", "Error", MessageBoxButton.OK);
                NavigationService.GoBack();
            }

            //TODO: allow selective cropping of the image

            //add cropped image to main image
            //TODO - select a region of the image
            try
            {
                var offsetTop = 0;
                var offsetLeft = 0;
                var selectedImageWidth = selectedImage.PixelWidth;
                var selectedImageHeight = selectedImage.PixelHeight;
                var aspectRatioOriginal = (double)width / (double)height;
                var aspectRatioImport = (double)selectedImage.PixelWidth / (double)selectedImage.PixelHeight;
                if (aspectRatioImport > aspectRatioOriginal)
                {
                    var zoom = (double)selectedImageHeight / (double)height;
                    selectedImageWidth = (int)(width * zoom);
                    offsetLeft = (selectedImage.PixelWidth - selectedImageWidth) / 2;
                }
                else
                {
                    var zoom = (double)selectedImageWidth / (double)width;
                    selectedImageHeight = (int)(height * zoom);
                    offsetTop = (selectedImage.PixelHeight - selectedImageHeight) / 2;
                }

                //re-frame the selected image to correct proportions
                var reframedPic = new WriteableBitmap(width, height);
                using (var imageSource = new BitmapImageSource(selectedImage.AsBitmap()))
                using (var reframeEffect = new FilterEffect(imageSource))
                {
                    var imageInfo = await imageSource.GetInfoAsync();
                    var filter = new ReframingFilter();

                    filter.ReframingArea = new Windows.Foundation.Rect(offsetLeft, offsetTop, selectedImageWidth, selectedImageHeight);
                    filter.Angle = 0;
                    reframeEffect.Filters = new IFilter[] { filter };
                    var finalRenderer = new WriteableBitmapRenderer(reframeEffect, reframedPic);
                    await finalRenderer.RenderAsync();
                }

                using (var backgroundSource = new BitmapImageSource(_pageImage.AsBitmap()))
                using (var foregroundImageSource = new BitmapImageSource(reframedPic.AsBitmap()))
                using (var filterEffect = new FilterEffect(backgroundSource))
                using (var blendFilter = new BlendFilter(foregroundImageSource))
                {
                    blendFilter.BlendFunction = BlendFunction.Normal;
                    blendFilter.TargetArea = new Windows.Foundation.Rect(
                        ((double)left) / ((double)_pageImage.PixelWidth),
                        ((double)top) / ((double)_pageImage.PixelHeight),
                        ((double)width) / ((double)_pageImage.PixelWidth),
                        ((double)height) / ((double)_pageImage.PixelHeight));
                    blendFilter.TargetAreaRotation = 0;
                    blendFilter.TargetOutputOption = OutputOption.Stretch;

                    filterEffect.Filters = new IFilter[] { blendFilter };

                    var finalRenderer = new WriteableBitmapRenderer(filterEffect, _pageImage);
                    await finalRenderer.RenderAsync();
                }

                //_pageImage.Blit(new Rect(left, top, width, height),
                //                selectedImage,
                //                new Rect(offsetLeft, offsetTop, selectedImageWidth, selectedImageHeight));
                Deployment.Current.Dispatcher.BeginInvoke(() => { PageImage.Source = _pageImage; });
            }
            catch (Exception ex)
            {
                BugSenseHandler.Instance.LogException(ex);
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);
            }
        }

        private void SavePage()
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("EditPagePage - save page");
            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (iso.FileExists(_pageFileName))
                    iso.DeleteFile(_pageFileName);
            }
            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var isostream = iso.CreateFile(_pageFileName))
                {
                    Extensions.SaveJpeg(_pageImage, isostream, _pageImage.PixelWidth, _pageImage.PixelHeight, 0, 100);
                    isostream.Close();
                }
            }

            var imageInStore = App.ViewModel.PageItems.FirstOrDefault(p => p.Name.Equals(_pageFileName));
            if (imageInStore == null)
                App.ViewModel.AddPageItem(new ImageItem { Name = _pageFileName, Image = _pageImage });
            else
                App.ViewModel.UpdatePageItem(imageInStore, _pageImage);
        }

        private string GeneratePageFileName()
        {
            return "page_" + _layout.GetHashCode() + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";
        }

        private void DrawBlankPage()
        {
            switch (_layout)
            {
                case PageLayout.Single:
                    DrawBlankSinglePage();
                    break;
                case PageLayout.DoubleSimple:
                    DrawBlankDoublePage();
                    break;
                case PageLayout.TripleSimple:
                    DrawBlankTriplePage();
                    break;
                case PageLayout.FourSimple:
                    DrawBlankFourPage();
                    break;
                default:
                    throw new NotImplementedException();
            }
            Deployment.Current.Dispatcher.BeginInvoke(() => { PageImage.Source = _pageImage; });
        }

        private void DrawBlankFourPage()
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("EditPagePage - draw 4 panel page");
            var centreX = DefaultWidth / 2;
            var centreY = DefaultHeight / 2;
            var halfMargin = DefaultPageMargin / 2;
            _pageImage.FillRectangle(0, 0, DefaultWidth, DefaultHeight, Colors.White);
            _pageImage.DrawRectangle(DefaultPageMargin+5, DefaultPageMargin+5, centreX - halfMargin - 5, centreY - halfMargin - 5, Colors.Black);
            _pageImage.DrawRectangle(centreX + halfMargin + 5, DefaultPageMargin+5, DefaultWidth - DefaultPageMargin - 5, centreY - halfMargin - 5, Colors.Black);
            _pageImage.DrawRectangle(DefaultPageMargin+5, centreY + halfMargin + 5, centreX - halfMargin - 5, DefaultHeight - DefaultPageMargin - 5, Colors.Black);
            _pageImage.DrawRectangle(centreX + halfMargin + 5, centreY + halfMargin + 5, DefaultWidth - DefaultPageMargin - 5, DefaultHeight - DefaultPageMargin - 5, Colors.Black);
            DrawAddIcon(DefaultWidth / 4, DefaultHeight / 4);
            DrawAddIcon(DefaultWidth / 4 * 3, DefaultHeight / 4);
            DrawAddIcon(DefaultWidth / 4, DefaultHeight / 4 * 3);
            DrawAddIcon(DefaultWidth / 4 * 3, DefaultHeight / 4 * 3);
        }

        private void DrawBlankTriplePage()
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("EditPagePage - draw triple panel page");
            var centreX = DefaultWidth / 2;
            var thirdHeight = DefaultHeight / 3;
            _pageImage.FillRectangle(0, 0, DefaultWidth, DefaultHeight, Colors.White);
            _pageImage.DrawRectangle(DefaultPageMargin+5, DefaultPageMargin+5, DefaultWidth - DefaultPageMargin - 5, thirdHeight - DefaultPageMargin / 2 - 5, Colors.Black);
            _pageImage.DrawRectangle(DefaultPageMargin+5, thirdHeight + DefaultPageMargin / 2 + 5, DefaultWidth - DefaultPageMargin - 5, DefaultHeight - thirdHeight - DefaultPageMargin / 2 - 5, Colors.Black);
            _pageImage.DrawRectangle(DefaultPageMargin+5, DefaultHeight - thirdHeight + DefaultPageMargin / 2 + 5, DefaultWidth - DefaultPageMargin - 5, DefaultHeight - DefaultPageMargin - 5, Colors.Black);
            DrawAddIcon(centreX, thirdHeight / 2);
            DrawAddIcon(centreX, thirdHeight + (thirdHeight / 2));
            DrawAddIcon(centreX, thirdHeight * 2 + (thirdHeight / 2));
        }

        private void DrawBlankDoublePage()
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("EditPagePage - draw double panel page");
            var centreX = DefaultWidth / 2;
            var centreY = DefaultHeight / 2;
            _pageImage.FillRectangle(0, 0, DefaultWidth, DefaultHeight, Colors.White);
            _pageImage.DrawRectangle(DefaultPageMargin+5, DefaultPageMargin+5, DefaultWidth - DefaultPageMargin - 5, DefaultHeight - centreY - DefaultPageMargin / 2 - 5, Colors.Black);
            _pageImage.DrawRectangle(DefaultPageMargin+5, DefaultHeight - centreY + DefaultPageMargin / 2 + 5, DefaultWidth - DefaultPageMargin - 5, DefaultHeight - DefaultPageMargin - 5, Colors.Black);
            DrawAddIcon(centreX, centreY / 2);
            DrawAddIcon(centreX, centreY / 2 + centreY);
        }

        private void DrawBlankSinglePage()
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("EditPagePage - draw single panel page");
            var centreX = DefaultWidth / 2;
            var centreY = DefaultHeight / 2;
            _pageImage.FillRectangle(0, 0, DefaultWidth, DefaultHeight, Colors.White);
            _pageImage.DrawRectangle(DefaultPageMargin+5, DefaultPageMargin+5, DefaultWidth - DefaultPageMargin - 5, DefaultHeight - DefaultPageMargin - 5, Colors.Black);
            DrawAddIcon(centreX, centreY);
        }

        private void DrawAddIcon(int centreX, int centreY)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("EditPagePage - draw add icon");
            _pageImage.FillEllipseCentered(centreX, centreY, 70, 70, Colors.Gray);
            _pageImage.FillRectangle(centreX - 10, centreY - 50, centreX + 10, centreY + 50, Colors.White);
            _pageImage.FillRectangle(centreX - 50, centreY - 10, centreX + 50, centreY + 10, Colors.White);
        }

        private void DrawSpeechBubble()
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("EditPagePage - draw speech bubble");
            var zoom = 0d;
            var aspectRatioOriginal = (double)width / (double)height;
            var aspectRatioImport = (double)_pageImage.PixelWidth / (double)_pageImage.PixelHeight;
            if (aspectRatioImport > aspectRatioOriginal)
                zoom = (double)_pageImage.PixelHeight / (double)PageImage.ActualHeight;
            else
                zoom = (double)_pageImage.PixelWidth / (double)PageImage.ActualWidth;

            var textWidth = (int)(TextWidth(SpeechBubbleTextbox.Text) * zoom);
            var centreY = (int)(top * zoom);
            var centreX = (int)(left * zoom);
            var bubbleWidth = textWidth + 4;
            var bubbleHeight = (int)((DefaultFontsize + 20) * zoom);
            _pageImage.FillEllipseCentered(centreX, centreY, bubbleWidth / 2, bubbleHeight / 2, Colors.Black);
            _pageImage.FillEllipseCentered(centreX, centreY, (bubbleWidth / 2) - 2, (bubbleHeight / 2) - 2, Colors.White);
            _pageImage.Invalidate();

            _pageImage.DrawText(SpeechBubbleTextbox.Text.Trim(), Colors.Black, DefaultFontsize, centreX - (TextWidth(SpeechBubbleTextbox.Text) / 2), (int)(centreY - DefaultFontsize * zoom / 2));
            _pageImage.Invalidate();
        }

        public int TextWidth(string text)
        {
            TextBlock t = new TextBlock();
            t.Text = text;
            //t.FontFamily = ...
            t.FontSize = DefaultFontsize;
            t.FontWeight = FontWeights.ExtraBold;
            return (int)Math.Ceiling(t.ActualWidth);
        }
    }

    internal static class WritableBitmapExtensions
    {
        internal static void DrawText(this WriteableBitmap wbm, string text, Color color, int fontSize, int x, int y)
        {
            BugSenseHandler.Instance.LeaveBreadCrumb("EditPagePage - WritableBitmapExtensions - draw text");
            TextBlock tb = new TextBlock();
            tb.FontSize = fontSize;
            tb.FontWeight = FontWeights.ExtraBold;
            tb.Foreground = new SolidColorBrush(color);
            tb.Text = text;

            // TranslateTransform 
            TranslateTransform tf = new TranslateTransform();
            tf.X = x;
            tf.Y = y;
            wbm.Render(tb, tf);
        }
    }
}

//crop image
//// Create effect collection with the source stream
//var fileStream = new MemoryStream();
//selectedImage.SaveJpeg(fileStream, selectedImage.PixelWidth, selectedImage.PixelHeight, 100, 100);
//fileStream.Seek(0, SeekOrigin.Begin); 
//var target = new WriteableBitmap(width, height);
//using (var source = new StreamImageSource(fileStream))
//{
//    using (var filters = new FilterEffect(source))
//    {
//        // Initialize the filter 
//        var sampleFilter = new CropFilter(new Windows.Foundation.Rect(0, 0, width, height));

//        // Add the filter to the FilterEffect collection
//        filters.Filters = new IFilter[] { sampleFilter };

//        // Create a target where the filtered image will be rendered to

//        // Create a new renderer which outputs WriteableBitmaps
//        using (var renderer = new WriteableBitmapRenderer(filters, target))
//        {
//            // Render the image with the filter(s)
//            await renderer.RenderAsync();
//        }
//    }
//}
