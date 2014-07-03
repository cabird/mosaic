using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mosaic
{
    class MosaicBuilder
    {
        TileManager tileManager;
        MosaicImage mosaicImage;

        public void BuildMosaic(TileManager tileManager, MosaicImage mosaicImage)
        {
            this.tileManager = tileManager;
            this.mosaicImage = mosaicImage;
            Bitmap bmp = new Bitmap(mosaicImage.Width, mosaicImage.Height);
            mosaicImage.Analyze();

            /* build and shuffle the list of tile locations that need to be filled in */
            List<MosaicTile> mosaicTiles = new List<MosaicTile>();
            foreach (var tile in mosaicImage.TileGrid)
            {
                mosaicTiles.Add(tile);
            }
            mosaicTiles.Shuffle();

            var start = DateTime.Now;

            int lastSeconds = 0;
            int seconds = 0;

            int completedTiles = 0;
            /* for each tile location, find the best image time that goes in that image */
            foreach (var mosaicTile in mosaicTiles)
            {
                SetUnavailableImageTiles(mosaicTile);
                Tile tile = tileManager.FindBestTile(mosaicTile);
                mosaicTile.ImageTile = tile;

                completedTiles++;
                seconds = (int)(DateTime.Now - start).TotalSeconds;
                if (seconds > lastSeconds)
                {
                    Debug.WriteLine("Completed {0} tiles {1:0.0}% in {2} seconds", completedTiles, completedTiles * 100 / mosaicTiles.Count, seconds);
                    lastSeconds = seconds;
                }
            }
        }

        public void SetUnavailableImageTiles(MosaicTile mosaicTile)
        {
            tileManager.SetAllTilesAvailable();

            int yMin = Math.Max(0,mosaicTile.TileY-Constants.RepeatDistance);
            int yMax = Math.Min(mosaicImage.TilesHigh, mosaicTile.TileY+Constants.RepeatDistance);
            int xMin = Math.Max(0, mosaicTile.TileX-Constants.RepeatDistance);
            int xMax = Math.Min(mosaicImage.TilesWide, mosaicTile.TileX+Constants.RepeatDistance);

            for (int y=yMin; y < yMax; y++)
            {
                for (int x=xMin; x < xMax; x++)
                {
                    if (mosaicImage.TileGrid[x, y] != null && mosaicImage.TileGrid[x, y].ImageTile != null)
                    {
                        mosaicImage.TileGrid[x, y].ImageTile.IsAvailable = false;
                    }
                }
            }

        }
    }
}
