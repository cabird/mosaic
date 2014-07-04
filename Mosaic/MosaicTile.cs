using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mosaic
{
    public class MosaicTileLocation
    {
        public CellGrid cellGrid;
        public int TileX;
        public int TileY;
        public int Width;
        public int Height;
        public Tile ImageTile;

        public MosaicTileLocation()
        {

        }

        public void ComputeCellColors(BitmapData bitmapData, byte[] data)
        {
            Rectangle rect = new Rectangle(Width * TileX, Height * TileY, Width, Height);
            cellGrid = new CellGrid();
            cellGrid.ComputeCellColors(bitmapData, data, rect);
        }

        public Bitmap GenerateCellBitmap()
        {
            return cellGrid.GenerateCellBitmap(Width, Height);
        }
    }
}
