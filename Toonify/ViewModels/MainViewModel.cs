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
            //this.MediaLibraryItems = new ObservableCollection<ImageItemViewModel>();
        }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<ImageItem> PageItems { get; private set; }
        public ObservableCollection<ImageItem> ImageItems { get; private set; }
        //public ObservableCollection<ImageItemViewModel> MediaLibraryItems { get; private set; }

        private string _sampleProperty = "Sample Runtime Property Value";
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding
        /// </summary>
        /// <returns></returns>
        public string SampleProperty
        {
            get
            {
                return _sampleProperty;
            }
            set
            {
                if (value != _sampleProperty)
                {
                    _sampleProperty = value;
                    NotifyPropertyChanged("SampleProperty");
                }
            }
        }

        /// <summary>
        /// Sample property that returns a localized string
        /// </summary>
        public string LocalizedSampleProperty
        {
            get
            {
                return AppResources.SampleProperty;
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

            //PictureAlbum cameraRoll = null;
            //foreach (MediaSource source in MediaSource.GetAvailableMediaSources())
            //{
            //    if (source.MediaSourceType == MediaSourceType.LocalDevice)
            //    {
            //        var mediaLibrary = new MediaLibrary(source);
            //        PictureAlbumCollection allAlbums = mediaLibrary.RootPictureAlbum.Albums;
            //        foreach (PictureAlbum album in allAlbums)
            //        {
            //            if (album.Name == "Camera Roll")
            //            {
            //                cameraRoll = album;
            //            }
            //        }
            //    }
            //}

            //if (cameraRoll != null)
            //{
            //    //List<BitmapImage> lstBitmapImage = new List<BitmapImage>();
            //    foreach (Picture p in cameraRoll.Pictures)
            //    {
            //        BitmapImage b = new BitmapImage();
            //        b.SetSource(p.GetThumbnail());
            //        //lstBitmapImage.Add(b);
            //        MediaLibraryItems.Add(new ImageItemViewModel { Filename = p.Name, ImageUri = "test" });
            //    }
            //}

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