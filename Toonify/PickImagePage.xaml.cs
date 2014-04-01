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
        public PickImagePage()
        {
            InitializeComponent();
            DataContext = App.ViewModel;
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var image = (Image)e.OriginalSource;
            App.ViewModel.SelectedImageName = image.Tag.ToString(); 
            NavigationService.GoBack();
        }
    }
}