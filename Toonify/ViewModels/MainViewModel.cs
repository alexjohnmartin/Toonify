using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using Toonify.Resources;

namespace Toonify.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            this.PageItems = new ObservableCollection<ImageItem>();
            this.ImageItems = new ObservableCollection<ImageItem>();
        }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<ImageItem> PageItems { get; private set; }
        public ObservableCollection<ImageItem> ImageItems { get; private set; }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                foreach (var file in store.GetFileNames("image_*"))
                {
                    var bitmap = new BitmapImage();
                    using (var stream = store.OpenFile(file, FileMode.Open, FileAccess.Read))
                    {
                        bitmap.SetSource(stream);
                    }
                    ImageItems.Add(new ImageItem { Name = file, Image = new WriteableBitmap(bitmap) });
                }
            }

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                foreach (var file in store.GetFileNames("page_*"))
                {
                    var bitmap = new BitmapImage();
                    using (var stream = store.OpenFile(file, FileMode.Open, FileAccess.Read))
                    {
                        bitmap.SetSource(stream);
                    }
                    PageItems.Add(new ImageItem { Name = file, Image = new WriteableBitmap(bitmap) });
                }
            }

            this.IsDataLoaded = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string SelectedImageName { get; set; }
    }
}