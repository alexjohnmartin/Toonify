using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Media.PhoneExtensions;
using Microsoft.Phone.Tasks;

namespace Toonify
{
    class ImageHelper
    {
        internal static string SaveImageToMediaLibrary(ImageItem _imageItem)
        {
            var path = string.Empty; 

            using (MemoryStream stream = new MemoryStream())
            {
                _imageItem.Image.SaveJpeg(stream, _imageItem.Image.PixelWidth, _imageItem.Image.PixelHeight, 0, 100);
                stream.Seek(0, SeekOrigin.Begin);

                foreach (MediaSource source in MediaSource.GetAvailableMediaSources())
                {
                    if (source.MediaSourceType == MediaSourceType.LocalDevice)
                    {
                        var mediaLibrary = new MediaLibrary(source);
                        var picture = mediaLibrary.SavePicture(_imageItem.Name, stream);
                        path =  picture.GetPath();
                    }
                }
            }

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                foreach (var file in store.GetFileNames("*_jpg.jpg"))
                {
                    store.DeleteFile(file); 
                }
            }

            return path;
        }

        internal static bool Delete(ImageItem imageItem)
        {
            if (MessageBox.Show("Are you sure?", "Delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                //delete image from isostore
                using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (iso.FileExists(imageItem.Name))
                        iso.DeleteFile(imageItem.Name);
                }

                //remove image from viewmodel
                if (imageItem.Name.StartsWith("image_"))
                    App.ViewModel.RemoveImageItem(imageItem);
                else
                    App.ViewModel.RemovePageItem(imageItem);

                return true; 
            }

            return false; 
        }

        internal static void Share(ImageItem imageItem)
        {
            var shareTask = new ShareMediaTask();
            shareTask.FilePath = SaveImageToMediaLibrary(imageItem);
            shareTask.Show();
        }
    }
}
