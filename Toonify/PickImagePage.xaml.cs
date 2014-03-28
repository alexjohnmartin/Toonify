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
using Microsoft.Phone;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.IO;

namespace Toonify
{
    public class ImageItem
    {
        public WriteableBitmap Image { get; set; }
        public String Name { get; set; }
    }

    public partial class PickImagePage : PhoneApplicationPage
    {
        private const int MaxImages = 100;
        private List<BitmapImage> lstBitmapImage;
        private ObservableCollection<ImageItem> _listOfImages = new ObservableCollection<ImageItem>();

        public ObservableCollection<ImageItem> ListOfImages
        {
            get { return _listOfImages; }

            set { _listOfImages = value; }
        }

        public PickImagePage()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

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
                            foreach (Picture p in album.Pictures)
                            {
                                BitmapImage b = new BitmapImage();
                                b.SetSource(p.GetThumbnail());
                                var wb = new WriteableBitmap(b);
                                ListOfImages.Add(new ImageItem { Image = wb, Name = p.Name });                                
                            }
                        }
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)e.OriginalSource; 
            NavigationService.Navigate(new Uri("/EditImagePage.xaml?name=" + button.CommandParameter, UriKind.Relative)); 
        }
    }
}