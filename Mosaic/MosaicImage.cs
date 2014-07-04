using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mosaic
{
    public class MosaicImage
    {

        string path;
        Bitmap rawBitmap;
        Bitmap scaledBitmap;
        Bitmap mosaicBitmap;

        public float ColorAdjustment { get; set; }
        public Bitmap RawBitmap { get { return rawBitmap; } }
        public Bitmap ScaledBitmap { get { return scaledBitmap; } }
        public Bitmap MosaicBitmap { get { return mosaicBitmap; } }

        public Bitmap Bitmap { get { return mosaicBitmap ?? scaledBitmap ?? rawBitmap; } }
        public Size OriginalBitmapSize { get { return rawBitmap.Size; } }

        public delegate void BitmapUpdated();

        public BitmapUpdated bitmapUpdatedDelegate;
        


        public int TilesWide = 20;
        public int TilesHigh = 20;
        int TileWidth;
        int TileHeight;

        public int Width { get { return TilesWide * TileWidth; } }
        public int Height { get { return TilesHigh * TileHeight; } }

        public MosaicTileLocation[,] TileGrid;

        public MosaicImage(string path)
        {
            Load(path);
        }

        private void Load(string path)
        {
            this.path = path;
            rawBitmap = new Bitmap(path);
        }

        public void Analyze(int tilesWide, Size tileSize)
        {
            TilesWide = tilesWide;

            int tileWidthPixels = rawBitmap.Width / tilesWide;
            int tileHeightPixels = tileWidthPixels * tileSize.Height / tileSize.Width;

            TilesHigh = rawBitmap.Height / tileHeightPixels;

            TileWidth = rawBitmap.Width  / TilesWide;
            TileHeight = rawBitmap.Height / TilesHigh;

            TileGrid = new MosaicTileLocation[TilesWide, TilesHigh];

            scaledBitmap = new Bitmap(rawBitmap, Width, Height);

            bitmapUpdatedDelegate();

            /* create a copy that we'll lock */
            Bitmap bitmap = new Bitmap(scaledBitmap);
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

            if (bitmapData.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new Exception("need to fix this to handle different pixel formats");
            }

            int size = bitmapData.Stride * bitmapData.Height;
            byte[] data = new byte[size];
            System.Runtime.InteropServices.Marshal.Copy(bitmapData.Scan0, data, 0, size);


            for (int tileY=0; tileY < TilesHigh; tileY++)
            {
                for (int tileX=0; tileX < TilesWide; tileX++)
                {
                    MosaicTileLocation tile = new MosaicTileLocation() {Width = TileWidth, Height = TileHeight, TileX = tileX, TileY = tileY};
                    tile.ComputeCellColors(bitmapData, data);
                    TileGrid[tileX, tileY] = tile;
                }
            }
        }

        public Bitmap GenerateCellGridBitmap()
        {
            Bitmap bmp = new Bitmap(TilesWide * TileWidth, TilesHigh * TileHeight);
            Graphics g = Graphics.FromImage(bmp);

            for (int tileY = 0; tileY < TilesHigh; tileY++)
            {
                for (int tileX = 0; tileX < TilesWide; tileX++)
                {
                    Bitmap tileBitmap = TileGrid[tileX, tileY].GenerateCellBitmap();

                    g.DrawImage(tileBitmap, tileX * TileWidth, tileY * TileHeight);
                }
            }
            return bmp;
        }

        public Bitmap DrawMosaic(int width, int height=-1, float adjust = 0.0f)
        {
            if (height == -1)
            {
                height = width * rawBitmap.Height / rawBitmap.Width;
            }

            ColorMatrix matrix = new ColorMatrix();
            matrix.Matrix33 = 1 - adjust; //opacity 0 = completely transparent, 1 = completely opaque

            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            TileWidth = width / TilesWide;
            TileHeight = height / TilesHigh;

            Bitmap bmp = new Bitmap(rawBitmap, TilesWide * TileWidth, TilesHigh * TileHeight);
            Graphics g = Graphics.FromImage(bmp);

            Rectangle drawRect = new Rectangle(0, 0, TileWidth, TileHeight);

            for (int tileY = 0; tileY < TilesHigh; tileY++)
            {
                drawRect.Y = tileY * TileHeight;
                for (int tileX = 0; tileX < TilesWide; tileX++)
                {
                    Bitmap tileBitmap = TileGrid[tileX, tileY].ImageTile.LoadTile();
                    drawRect.X = tileX * TileWidth;
                    g.DrawImage(tileBitmap, drawRect, 0, 0, tileBitmap.Width, tileBitmap.Height, GraphicsUnit.Pixel, attributes);
                    TileGrid[tileX, tileY].ImageTile.ReleaseImage();
                }
            }
            return bmp;
        }

        public void PrepareToDrawMosaic()
        {
            mosaicBitmap = new Bitmap(scaledBitmap, TilesWide * TileWidth, TilesHigh * TileHeight);
            bitmapUpdatedDelegate();
        }

        public void DrawTileOntoBitmap(MosaicTileLocation tileLocation)
        {
            ColorMatrix matrix = new ColorMatrix();
            matrix.Matrix33 = 1 - ColorAdjustment; //opacity 0 = completely transparent, 1 = completely opaque

            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            Graphics g = Graphics.FromImage(mosaicBitmap);
            Rectangle drawRect = new Rectangle(tileLocation.TileX * TileWidth, tileLocation.TileY * TileHeight, TileWidth, TileHeight);

            Bitmap tileBitmap = tileLocation.ImageTile.LoadTile();
            g.DrawImage(tileBitmap, drawRect, 0, 0, tileBitmap.Width, tileBitmap.Height, GraphicsUnit.Pixel, attributes);
            tileLocation.ImageTile.ReleaseImage();

            bitmapUpdatedDelegate();
        }
    }
}
