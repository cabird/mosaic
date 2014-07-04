using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.IO;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace Mosaic
{
    [Serializable]
    public class Tile
    {

        protected Tile()
        { }

        public Tile(string path)
        {
            Path = path;
        }

        public int Width;
        public int Height;
        public string Path;
        public int FileSize;
        public CellGrid cellGrid;
        public bool IsAvailable;

        Bitmap cachedBitmap;
        
        public Bitmap LoadTile()
        {
            
            if (cachedBitmap == null)
            {
                cachedBitmap = new Bitmap(Path);
            }
            Width = cachedBitmap.Width;
            Height = cachedBitmap.Height;
            FileSize = (int)(new FileInfo(Path)).Length;
            return cachedBitmap;
        }
        
        public void ReleaseImage()
        {
            if (cachedBitmap != null)
            {
                cachedBitmap.Dispose();
                cachedBitmap = null;
            }
        }

        public void ComputeCellColors()
        {
            /* create a copy that we'll lock */
            Bitmap bitmap = new Bitmap(LoadTile());

            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

            BitmapData bData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);

            if (bData.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new Exception("need to fix this to handle different pixel formats");
            }

            int size = bData.Stride * bData.Height;
            byte[] data = new byte[size];
            System.Runtime.InteropServices.Marshal.Copy(bData.Scan0, data, 0, size);

            cellGrid = new CellGrid();
            cellGrid.ComputeCellColors(bData, data, rect);

            bitmap.Dispose();
        }

        public Bitmap GenerateCellBitmap()
        {
            return cellGrid.GenerateCellBitmap(Width, Height);
        }

        public void Serialize(string path)
        {
            using (Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Serialize(stream);
            }
        }

        public void Serialize(Stream stream)
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                Serialize(writer);
            }
        }

        public void Serialize(BinaryWriter bwriter)
        {
            bwriter.Write(Path.ToLower());
            bwriter.Write(FileSize);
            bwriter.Write(Width);
            bwriter.Write(Height);
            bwriter.Write(cellGrid.Rows);
            bwriter.Write(cellGrid.Columns);
            for (int y = 0; y < cellGrid.Rows; y++)
            {
                for (int x = 0; x < cellGrid.Columns; x++)
                {
                    bwriter.Write(cellGrid.CellColors[x, y].ToArgb());
                }
            }
        }

        public static Tile Deserialize(string path)
        {
            using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return Deserialize(stream);
            }
        }

        public static Tile Deserialize(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                return Deserialize(reader);
            }
        }

        public static Tile Deserialize(BinaryReader reader)
        {
            Tile tile = new Tile();

            tile.Path = reader.ReadString().ToLower();
            tile.FileSize = reader.ReadInt32();

            /* check that we actually have the file and that the size matches.  Otherwise the tile is invalid and shouldn't
             * be deserialized */
            var fileInfo = new FileInfo(tile.Path);
            if (!fileInfo.Exists || fileInfo.Length != tile.FileSize)
            {
                Debug.WriteLine("Actual Tile ile does not exist or does not match size in serialized form");
                return null;
            }

            tile.Width = reader.ReadInt32();
            tile.Height = reader.ReadInt32();
            int rows = reader.ReadInt32();
            int columns = reader.ReadInt32();

            tile.cellGrid = new CellGrid();

            if (rows != tile.cellGrid.Rows || columns != tile.cellGrid.Columns)
            {
                Debug.WriteLine("The number of rows and columns in the serialized form does not match");
                return null;
            }
            
            for (int y = 0; y < tile.cellGrid.Rows; y++)
            {
                for (int x = 0; x < tile.cellGrid.Columns; x++)
                {
                    tile.cellGrid.CellColors[x, y] = Color.FromArgb(reader.ReadInt32());
                }
            }
            return tile;
        }
    }
}
