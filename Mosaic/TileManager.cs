using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mosaic
{
    public class TileManager
    {
        List<TileDirectory> TileDirectories;

        IEnumerable<Tile> Tiles { get { return TileDirectories.SelectMany(td => td.Tiles); } }

        public TileManager()
        {
            TileDirectories = new List<TileDirectory>();
        }



        public void AddDirectory(string directory)
        {
            TileDirectory tileDir = new TileDirectory(directory);
            tileDir.Load();
            tileDir.Save();
            TileDirectories.Add(tileDir);
        }

        public void SetAllTilesAvailable()
        {
            foreach (Tile tile in Tiles)
            {
                tile.IsAvailable = true;
            }
        }

        public Size GetAvgTileSize()
        {
            int width = (int)Tiles.Select(t => t.Width).Average();
            int height = (int)Tiles.Select(t => t.Height).Average();
            return new Size(width, height);
        }

        public Tile FindBestTile(MosaicTileLocation mosaicTile)
        {
            Int64 bestDistance = 0;
            Tile bestTile = null;

            foreach(Tile tile in TileDirectories.SelectMany(td => td.Tiles).Where(t => t.IsAvailable))
            {
                Int64 distance = tile.cellGrid.DistanceFrom(mosaicTile.cellGrid);
                if (bestTile == null || bestDistance > distance)
                {
                    bestDistance = distance;
                    bestTile = tile;
                }
            }
            return bestTile;
        }

    }
}
