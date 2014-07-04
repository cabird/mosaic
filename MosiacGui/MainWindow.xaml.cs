using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Mosaic;
using System.IO;
using System.Drawing.Imaging;
using System.Windows.Threading;

namespace MosiacGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        MosaicImage mosaicImage;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenImage_Click(object sender, RoutedEventArgs e)
        {
            OpenImage();
        }

        private void OpenImage()
        {
            // Create OpenFileDialog
            OpenFileDialog dlg = new OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Image Files (*.bmp, *.jpg, *.png)|*.bmp;*.jpg;*.jpeg;*.png";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;

                mosaicImage = new MosaicImage(filename);
                mosaicImage.bitmapUpdatedDelegate = new MosaicImage.BitmapUpdated(UpdateUIBitmap);
                UpdateUIBitmap();
            }
        }

        void UpdateUIBitmap()
        {
            var bitmap = new System.Windows.Media.Imaging.BitmapImage();
            bitmap.BeginInit();
            MemoryStream memoryStream = new MemoryStream();
            mosaicImage.Bitmap.Save(memoryStream, ImageFormat.Bmp);
            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
            bitmap.StreamSource = memoryStream;
            bitmap.EndInit();
            MainImage.Source = bitmap;
            MainImage.Width = bitmap.Width;
            MainImage.Height = bitmap.Height;
            AllowUIToUpdate();
        }

        protected void AllowUIToUpdate()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate(object parameter)
            {
                frame.Continue = false;
                return null;

            }), null);
            Dispatcher.PushFrame(frame);
        }

        private void GenerateMosaic_Click(object sender, RoutedEventArgs e)
        {
            GenerateMosaic();
        }

        private void GenerateMosaic()
        {
            TileManager tileManager = new TileManager();
            tileManager.AddDirectory(@"E:\MarvelCovers\");
            MosaicBuilder builder = new MosaicBuilder();

            mosaicImage.Analyze(10, tileManager.GetAvgTileSize());
            mosaicImage.PrepareToDrawMosaic();
            builder.BuildMosaic(tileManager, mosaicImage);
        }

    }
}
