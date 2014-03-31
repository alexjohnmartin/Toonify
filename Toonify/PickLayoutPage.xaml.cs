using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Toonify
{
    public partial class PickLayoutPage : PhoneApplicationPage
    {
        public PickLayoutPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)e.OriginalSource; 
            NavigationService.Navigate(new Uri("/EditPagePage.xaml?new=true&layout=" + button.CommandParameter, UriKind.Relative)); 
        }
    }
}