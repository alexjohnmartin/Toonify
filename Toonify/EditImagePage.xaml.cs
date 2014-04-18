using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Nokia.Graphics.Imaging;
using Nokia.InteropServices.WindowsRuntime;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media;
using System.IO.IsolatedStorage;

namespace Toonify
{
    public partial class EditImagePage : PhoneApplicationPage
    {
        //private const int DefaultWidth = 768;
        //private const int DefaultHeight = 1000;
        private const int DefaultWidth = 600;
        private const int DefaultHeight = 800;

        //private bool _pageLoaded = false; 
        private string _filename = string.Empty;
        private IsolatedStorageSettings _settings;
        private WriteableBitmap _finalImageBitmap = null;
        private WriteableBitmap _sketchImageBitmap = null;
        private WriteableBitmap _cartoonImageBitmap = null;
        //private WriteableBitmap _thumbnailImageBitmap = null;
        
        public EditImagePage()
        {
            InitializeComponent();
            DataContext = this; 
            _settings = IsolatedStorageSettings.ApplicationSettings;
            if (!_settings.Contains("EffectListIndex")) _settings.Add("EffectListIndex", 0); 
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //EffectList.SelectedIndex = (int)_settings["EffectListIndex"]; 

            _filename = string.Empty;
            if (!NavigationContext.QueryString.TryGetValue("name", out _filename)) return;

            OkButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
            LoadingPanel.Visibility = System.Windows.Visibility.Visible;
            ConvertAndDisplayImage();
            //_pageLoaded = true;
        }

        private void ConvertAndDisplayImage()
        {
            try
            {
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
                                var pic = album.Pictures.FirstOrDefault(p => p.Name.Equals(_filename, StringComparison.InvariantCultureIgnoreCase));
                                if (pic != null)
                                {
                                    //switch (EffectList.SelectedIndex)
                                    //{
                                    //    case 0:
                                    //        CreateCartoonImage(pic.GetImage(), pic.Width, pic.Height);
                                    //        break;
                                    //    case 1:
                                    //        CreateInkedImage(pic.GetImage(), pic.Width, pic.Height);
                                    //        break;
                                    //    case 2:
                                    CreateCombinedImage(pic.GetImage(), pic.Width, pic.Height);
                                    //        break;
                                    //}
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Error loading image", MessageBoxButton.OK);
                LoadingPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        //private async void CreateCartoonImage(Stream chosenPhoto, int width, int height)
        //{   
        //    try
        //    {
        //        // Rewind stream to start.                     
        //        chosenPhoto.Position = 0;
        //        var imageStream = new StreamImageSource(chosenPhoto);

        //        //var originalImage = new WriteableBitmap(width, height);
        //        //originalImage.SetSource(chosenPhoto); 

        //        var selectedImageWidth = DefaultWidth;
        //        var selectedImageHeight = DefaultHeight;
        //        var aspectRatioOriginal = (double)width / (double)height;
        //        var aspectRatioImport = (double)DefaultWidth / (double)DefaultHeight;
        //        if (aspectRatioImport < aspectRatioOriginal)
        //        {
        //            var zoom = (double)height / (double)DefaultHeight;
        //            selectedImageWidth = (int)(width / zoom);
        //        }
        //        else
        //        {
        //            var zoom = (double)width / (double)DefaultWidth;
        //            selectedImageHeight = (int)(height / zoom);
        //        }

        //        //_finalImageBitmap = new WriteableBitmap(selectedImageWidth, selectedImageHeight);
        //        //_sketchImageBitmap = new WriteableBitmap(selectedImageWidth, selectedImageHeight);
        //        _cartoonImageBitmap = new WriteableBitmap(selectedImageWidth, selectedImageHeight);
        //        //_thumbnailImageBitmap = new WriteableBitmap(selectedImageWidth, selectedImageHeight);

        //        //_thumbnailImageBitmap.Blit(new Rect(0, 0, selectedImageWidth, selectedImageHeight),
        //        //                originalImage,
        //        //                new Rect(0, 0, width, height));

        //        // A cartoon effect is initialized with selected image stream as source.
        //        //var sketchEffect = await RenderSketchImage(imageStream);
        //        var cartoonEffect = await RenderCartoonImage(imageStream);
        //        //await RenderFinalImage(sketchEffect, cartoonEffect);

        //        //ImageDisplay.Source = _finalImageBitmap;
        //        //SketchDisplay.Source = _sketchImageBitmap;
        //        CartoonDisplay.Source = _cartoonImageBitmap;
        //        //OriginalDisplay.Source = _thumbnailImageBitmap;
        //    }
        //    catch (OutOfMemoryException)
        //    {
        //        MessageBox.Show("Out of memory", "Error", MessageBoxButton.OK);
        //        NavigateBackToHomeScreen();
        //    }
        //    catch (Exception)
        //    {
        //        MessageBox.Show("Out of memory", "Error", MessageBoxButton.OK);
        //        NavigationService.GoBack(); 
        //    }
        //}

        //private async void CreateInkedImage(Stream chosenPhoto, int width, int height)
        //{
        //    try
        //    {
        //        // Rewind stream to start.                     
        //        chosenPhoto.Position = 0;
        //        var imageStream = new StreamImageSource(chosenPhoto);

        //        //var originalImage = new WriteableBitmap(width, height);
        //        //originalImage.SetSource(chosenPhoto); 

        //        var selectedImageWidth = DefaultWidth;
        //        var selectedImageHeight = DefaultHeight;
        //        var aspectRatioOriginal = (double)width / (double)height;
        //        var aspectRatioImport = (double)DefaultWidth / (double)DefaultHeight;
        //        if (aspectRatioImport < aspectRatioOriginal)
        //        {
        //            var zoom = (double)height / (double)DefaultHeight;
        //            selectedImageWidth = (int)(width / zoom);
        //        }
        //        else
        //        {
        //            var zoom = (double)width / (double)DefaultWidth;
        //            selectedImageHeight = (int)(height / zoom);
        //        }

        //        //_finalImageBitmap = new WriteableBitmap(selectedImageWidth, selectedImageHeight);
        //        _sketchImageBitmap = new WriteableBitmap(selectedImageWidth, selectedImageHeight);
        //        //_cartoonImageBitmap = new WriteableBitmap(selectedImageWidth, selectedImageHeight);
        //        //_thumbnailImageBitmap = new WriteableBitmap(selectedImageWidth, selectedImageHeight);

        //        //_thumbnailImageBitmap.Blit(new Rect(0, 0, selectedImageWidth, selectedImageHeight),
        //        //                originalImage,
        //        //                new Rect(0, 0, width, height));

        //        // A cartoon effect is initialized with selected image stream as source.
        //        var sketchEffect = await RenderSketchImage(imageStream);
        //        //var cartoonEffect = await RenderCartoonImage(imageStream);
        //        //await RenderFinalImage(sketchEffect, cartoonEffect);

        //        //ImageDisplay.Source = _finalImageBitmap;
        //        CartoonDisplay.Source = _sketchImageBitmap;
        //        //CartoonDisplay.Source = _cartoonImageBitmap;
        //        //OriginalDisplay.Source = _thumbnailImageBitmap;
        //    }
        //    catch (OutOfMemoryException)
        //    {
        //        MessageBox.Show("Out of memory", "Error", MessageBoxButton.OK);
        //        NavigateBackToHomeScreen();
        //    }
        //    catch (Exception)
        //    {
        //        MessageBox.Show("Out of memory", "Error", MessageBoxButton.OK);
        //        NavigationService.GoBack();
        //    }
        //}

        private async void CreateCombinedImage(Stream chosenPhoto, int width, int height)
        {
            try
            {
                // Rewind stream to start.                     
                chosenPhoto.Position = 0;
                var imageStream = new StreamImageSource(chosenPhoto);

                var selectedImageWidth = DefaultWidth;
                var selectedImageHeight = DefaultHeight;
                var aspectRatioOriginal = (double)width / (double)height;
                var aspectRatioImport = (double)DefaultWidth / (double)DefaultHeight;
                if (aspectRatioImport < aspectRatioOriginal)
                {
                    var zoom = (double)height / (double)DefaultHeight;
                    selectedImageWidth = (int)(width / zoom);
                }
                else
                {
                    var zoom = (double)width / (double)DefaultWidth;
                    selectedImageHeight = (int)(height / zoom);
                }

                _finalImageBitmap = new WriteableBitmap(selectedImageWidth, selectedImageHeight);
                _sketchImageBitmap = new WriteableBitmap(selectedImageWidth, selectedImageHeight);
                _cartoonImageBitmap = new WriteableBitmap(selectedImageWidth, selectedImageHeight);

                // A cartoon effect is initialized with selected image stream as source.
                var sketchEffect = await RenderSketchImage(imageStream);
                var cartoonEffect = await RenderCartoonImage(imageStream);
                await RenderFinalImage(sketchEffect, cartoonEffect);

                CartoonDisplay.Source = _finalImageBitmap;
                OkButton.IsEnabled = true;
                CancelButton.IsEnabled = true;
                LoadingPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
            catch (OutOfMemoryException)
            {
                MessageBox.Show("Out of memory, your device may not be powerful enough", "Error", MessageBoxButton.OK);
                NavigateBackToHomeScreen();
            }
            catch (Exception)
            {
                MessageBox.Show("Error converting image", "Error", MessageBoxButton.OK);
                NavigationService.GoBack();
            }
        }

        private async System.Threading.Tasks.Task RenderFinalImage(FilterEffect sketchEffect, FilterEffect cartoonEffect)
        {
            var blendFilter = new BlendFilter(cartoonEffect, BlendFunction.Multiply);
            var blendEffect = new FilterEffect(sketchEffect);
            blendEffect.Filters = new[] { blendFilter };
            var finalRenderer = new WriteableBitmapRenderer(blendEffect, _finalImageBitmap);
            await finalRenderer.RenderAsync();
        }

        private async System.Threading.Tasks.Task<FilterEffect> RenderSketchImage(StreamImageSource imageStream)
        {
            //var sketchFilter = new SketchFilter(SketchMode.Gray);
            var sketchFilter = new StampFilter(5, 0.3);
            var sketchEffect = new FilterEffect(imageStream);
            sketchEffect.Filters = new[] { sketchFilter };
            var sketchRenderer = new WriteableBitmapRenderer(sketchEffect, _sketchImageBitmap);
            await sketchRenderer.RenderAsync();
            return sketchEffect;
        }

        private async System.Threading.Tasks.Task<FilterEffect> RenderCartoonImage(StreamImageSource imageStream)
        {
            var cartoonEffect = new FilterEffect(imageStream);
            var cartoonFilter = new CartoonFilter();
            cartoonEffect.Filters = new[] { cartoonFilter };
            var cartoonRenderer = new WriteableBitmapRenderer(cartoonEffect, _cartoonImageBitmap);
            await cartoonRenderer.RenderAsync();
            return cartoonEffect; 
        }

        private void NavigateBackToHomeScreen()
        {
            var lastStackItem = NavigationService.BackStack.FirstOrDefault(); 
            if (lastStackItem.Source.OriginalString.Contains("ImportImagePage.xaml"))
                NavigationService.RemoveBackEntry();

            NavigationService.GoBack(); 
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            //_settings["EffectListIndex"] = EffectList.SelectedIndex;
            NavigateBackToHomeScreen();
            //switch (EffectList.SelectedIndex)
            //{ 
            //    case 0:
            //        SaveImage(_cartoonImageBitmap);
            //        break;
            //    case 1:
            //        SaveImage(_sketchImageBitmap);
            //        break;
            //    case 2:
                    SaveImage(_finalImageBitmap);
            //        break; 
            //}
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateBackToHomeScreen();
        }

        //private void EffectList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (_pageLoaded)
        //    {
        //        LoadingPanel.Visibility = System.Windows.Visibility.Visible;
        //        ConvertAndDisplayImage();
        //    }
        //}

        private void SaveImage(WriteableBitmap bitmap)
        {
            var fileStream = new MemoryStream();
            bitmap.SaveJpeg(fileStream, bitmap.PixelWidth, bitmap.PixelHeight, 100, 100);
            fileStream.Seek(0, SeekOrigin.Begin);

            var filename = "image_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var stream = store.CreateFile(filename);

                var reader = new StreamReader(fileStream);
                byte[] contents;
                using (BinaryReader bReader = new BinaryReader(reader.BaseStream))
                {
                    contents = bReader.ReadBytes((int)reader.BaseStream.Length);
                }
                stream.Write(contents, 0, contents.Length);
                stream.Close();
            }

            App.ViewModel.AddImageItem(new ImageItem { Name = filename, Image = bitmap });
        }
    }
}