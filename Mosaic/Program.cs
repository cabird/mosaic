using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mosaic
{
    class Program
    {

        static string DirBase = @"C:\Development\MarvelImages\images\comic\";

        //static string ImagePath = @"C:\Development\MarvelImages\images\nachi-head.jpg";
        //static string ImagePath = @"C:\Users\cbird\OneDrive\Amy's Family Pix\BestFamPic.jpg";
        static string ImagePath = @"C:\Development\MarvelImages\images\cbird_profile.jpg";
        static void Main(string[] args)
        {
            TileManager tileManager = new TileManager();
            tileManager.AddDirectory(DirBase);

            MosaicBuilder builder = new MosaicBuilder();
            MosaicImage mosaicImage = new MosaicImage(ImagePath, 25, tileManager.GetAvgTileSize());
            mosaicImage.Analyze();
            builder.BuildMosaic(tileManager, mosaicImage);
            Bitmap bmp = mosaicImage.DrawMosaic(5000, adjust: 0.25f);

            string newPath = ImagePath.Replace(".jpg", "-mosaic.jpg");

            bmp.Save(newPath, ImageFormat.Jpeg);  
        }

        static void SerializeDirectory(string dir)
        {

          
        }
    }
}
