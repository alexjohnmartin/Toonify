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
using System.IO.IsolatedStorage;

namespace Toonify
{
    public partial class PickImagePage : PhoneApplicationPage
    {
        private const int MaxImages = 100;
        private List<BitmapImage> lstBitmapImage;
        private string _querystring = string.Empty; 

        private ObservableCollection<ImageItem> _listOfImages = new ObservableCollection<ImageItem>();
        public ObservableCollection<ImageItem> ListOfImages
        {
            get { return _listOfImages; }
            set { _listOfImages = value; }
        }

        public PickImagePage()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            //TODO:save querystring params so we can send them back
            if (e.Uri.OriginalString.Contains('?'))
                _querystring = e.Uri.OriginalString.Substring(e.Uri.OriginalString.IndexOf('?') + 1);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)e.OriginalSource;
            NavigationService.Navigate(new Uri("/EditPagePage.xaml?selectedimagename=" + button.CommandParameter + "&" + _querystring, UriKind.Relative));
        }
    }
}