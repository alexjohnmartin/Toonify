using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Toonify.Resources;

namespace Toonify.ViewModels
{
    public class PairOfItems
    {
        public ImageItem Item1 { get; set; }
        public ImageItem Item2 { get; set; }
        public Brush Item1BackgroundColor { get { return _backgroundColor; } }
        public Brush Item2BackgroundColor { get { return Item2 != null ? _backgroundColor : new SolidColorBrush(); } }
        private Brush _backgroundColor { get; set; }

        public PairOfItems()
        {
            _backgroundColor = new SolidColorBrush((Color)Application.Current.Resources["PhoneAccentColor"]);
        }
    }

    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            PageItems = new ObservableCollection<ImageItem>();
            ImageItems = new ObservableCollection<ImageItem>();
        }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<ImageItem> PageItems { get; private set; }
        public ObservableCollection<ImageItem> ImageItems { get; private set; }

        public ObservableCollection<PairOfItems> PageItemPairs
        {
            get
            {
                var collection = new ObservableCollection<PairOfItems>();
                var pair = new PairOfItems();
                foreach (var item in PageItems)
                {
                    if (pair.Item1 == null)
                        pair.Item1 = item;
                    else if (pair.Item2 == null)
                        pair.Item2 = item;
                    else
                    {
                        collection.Add(pair);
                        pair = new PairOfItems();
                        pair.Item1 = item;
                    }
                }
                if (pair.Item1 != null)
                    collection.Add(pair); 

                return collection; 
            }
        }

        public ObservableCollection<PairOfItems> ImageItemPairs
        {
            get
            {
                var collection = new ObservableCollection<PairOfItems>();
                var pair = new PairOfItems();
                foreach (var item in ImageItems)
                {
                    if (pair.Item1 == null)
                        pair.Item1 = item;
                    else if (pair.Item2 == null)
                        pair.Item2 = item;
                    else
                    {
                        collection.Add(pair);
                        pair = new PairOfItems();
                        pair.Item1 = item;
                    }
                }
                if (pair.Item1 != null)
                    collection.Add(pair); 

                return collection;
            }
        }

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

        internal void RemoveImageItem(ImageItem item)
        {
            ImageItems.Remove(item);
            NotifyPropertyChanged("ImageItems");
            NotifyPropertyChanged("ImageItemPairs"); 
        }

        internal void AddImageItem(ImageItem item)
        {
            ImageItems.Add(item);
            NotifyPropertyChanged("ImageItems");
            NotifyPropertyChanged("ImageItemPairs");
        }

        internal void RemovePageItem(ImageItem item)
        {
            PageItems.Remove(item);
            NotifyPropertyChanged("PageItems");
            NotifyPropertyChanged("PageItemPairs");
        }

        internal void AddPageItem(ImageItem item)
        {
            PageItems.Add(item);
            NotifyPropertyChanged("PageItems");
            NotifyPropertyChanged("PageItemPairs");
        }

        internal void UpdatePageItem(ImageItem item, WriteableBitmap image)
        {
            item.Image = image;
            NotifyPropertyChanged("PageItems");
            NotifyPropertyChanged("PageItemPairs");
        }
    }
}