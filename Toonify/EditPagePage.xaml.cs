using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using Nokia.Graphics.Imaging;
using System.IO;

namespace Toonify
{
    public partial class EditPagePage : PhoneApplicationPage
    {
        //768 x 1000 = page size = 8" x 10.5" at 96dpi
        private const int DefaultWidth = 768;
        private const int DefaultHeight = 1000;
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

        public EditPagePage()
        {
            InitializeComponent();
            ApplicationBar = new ApplicationBar(); 
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var dummy = string.Empty; 
            var newPage = NavigationContext.QueryString.TryGetValue("new", out dummy);

            if (!string.IsNullOrEmpty(App.ViewModel.SelectedImageName))
            {
                AddImageToPage(App.ViewModel.SelectedImageName);
                SavePage();
                BuildApplicationBar();
                App.ViewModel.SelectedImageName = string.Empty; 
            }
            else if (newPage)
            {
                PageLayoutPanel.Visibility = System.Windows.Visibility.Visible;
                PageImage.Visibility = System.Windows.Visibility.Collapsed;
                TextDialog.Visibility = System.Windows.Visibility.Collapsed;
                _pageFileName = string.Empty;
                _pageImage = new WriteableBitmap(DefaultWidth, DefaultHeight);
                _addSpeechBubble = false; 
            }
            else if (NavigationContext.QueryString.TryGetValue("edit", out _pageFileName))
            {
                //load previously made page
                BuildApplicationBar();

                //get layout-type from file name
                var parts = _pageFileName.Split('_');
                _layout = (PageLayout)Enum.Parse(typeof(PageLayout), parts[1]); 

                LoadImageFromIsolatedStorage(); 

                PageLayoutPanel.Visibility = System.Windows.Visibility.Collapsed;
                PageImage.Visibility = System.Windows.Visibility.Visible;
                TextDialog.Visibility = System.Windows.Visibility.Collapsed;
                _addSpeechBubble = false;
                PageImage.Source = _pageImage; 
            }

            PageImage.Source = _pageImage; 
        }

        private void LoadImageFromIsolatedStorage()
        {
            try
            {
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
            var addSpeechButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.chat.png", UriKind.Relative));
            addSpeechButton.Text = "add speech bubble"; //MeetMeHere.Support.Resources.AppResources.AppBarRefreshButtonText;
            addSpeechButton.Click += AddSpeech_Click;
            ApplicationBar.Buttons.Add(addSpeechButton);
        }

        private void AddSpeech_Click(object sender, EventArgs e)
        {
            _addSpeechBubble = true;
            MessageBox.Show("tap on image to place a speech bubble", "Speech bubble", MessageBoxButton.OK); 
        }

        private void SavePage()
        {
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
                App.ViewModel.PageItems.Add(new ImageItem { Name = _pageFileName, Image = _pageImage });
            else
                imageInStore.Image = _pageImage; 
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
                default:
                    throw new NotImplementedException(); 
            }
        }

        private void DrawBlankSinglePage()
        {
            var centreX = DefaultWidth/2; 
            var centreY = DefaultHeight/2; 
            _pageImage.FillRectangle(0, 0, DefaultWidth, DefaultHeight, Colors.White);
            _pageImage.DrawRectangle(DefaultPageMargin, DefaultPageMargin, DefaultWidth - DefaultPageMargin, DefaultHeight - DefaultPageMargin, Colors.Black);
            DrawAddIcon(centreX, centreY);
        }

        private void DrawAddIcon(int centreX, int centreY)
        {
            _pageImage.FillEllipseCentered(centreX, centreY, 70, 70, Colors.Gray);
            _pageImage.FillRectangle(centreX - 10, centreY - 50, centreX + 10, centreY + 50, Colors.White);
            _pageImage.FillRectangle(centreX - 50, centreY - 10, centreX + 50, centreY + 10, Colors.White);
        }

        private void PageImage_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
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

        private PageLayout ParseLayoutString(string layout)
        {
            switch (layout)
            {
                case "Single":
                    return PageLayout.Single;
                default:
                    MessageBox.Show("cannot edit this page layout yet", "Error", MessageBoxButton.OK);
                    NavigationService.GoBack();
                    return PageLayout.Single;
            }
        }

        private IEnumerable<ImageItem> LoadImageItems()
        {
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

                _pageImage.Blit(new Rect(left, top, width, height),
                                selectedImage,
                                new Rect(offsetLeft, offsetTop, selectedImageWidth, selectedImageHeight));
                PageImage.Source = _pageImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK);               
            } 
        }

        private void SpeechOkButton_Click(object sender, RoutedEventArgs e)
        {
            TextDialog.Visibility = System.Windows.Visibility.Collapsed;
            DrawSpeechBubble();
            SavePage(); 
        }

        private void DrawSpeechBubble()
        {
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
            _pageImage.FillEllipseCentered(centreX, centreY, bubbleWidth/2, bubbleHeight/2, Colors.Black);
            _pageImage.FillEllipseCentered(centreX, centreY, (bubbleWidth / 2)-2, (bubbleHeight / 2)-2, Colors.White);
            _pageImage.Invalidate();

            _pageImage.DrawText(SpeechBubbleTextbox.Text, Colors.Black, DefaultFontsize, centreX - (TextWidth(SpeechBubbleTextbox.Text)/2), (int)(centreY - DefaultFontsize*zoom / 2));
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

        private void SpeechCancelButton_Click(object sender, RoutedEventArgs e)
        {
            TextDialog.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void LayoutButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)e.OriginalSource;
            _layout = ParseLayoutString(button.CommandParameter.ToString());
            PageLayoutPanel.Visibility = System.Windows.Visibility.Collapsed;
            PageImage.Visibility = System.Windows.Visibility.Visible;

            //start new page
            _pageFileName = GeneratePageFileName();
            DrawBlankPage();
        }
    }

    internal static class WritableBitmapExtensions
    {
        internal static void DrawText(this WriteableBitmap wbm, string text, Color color, int fontSize, int x, int y)
        {
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
