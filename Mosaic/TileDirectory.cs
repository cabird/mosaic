using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mosaic
{
    class TileDirectory
    {

        public string Directory;
        Dictionary<string, Tile> tileDictionary;

        public IEnumerable<Tile> Tiles { get { return tileDictionary == null ? new List<Tile>() : tileDictionary.Values.ToList(); } }

        public TileDirectory(string directoy)
        {
            Directory = directoy;
        }

        public void Load()
        {
            Debug.WriteLine("Loading Directory");
            tileDictionary = new Dictionary<string, Tile>();
            Deserialize();
            Debug.WriteLine("Loaded {0} tiles", tileDictionary.Count);

            int newLoadedFiles = 0;

            DirectoryInfo dirInfo = new DirectoryInfo(Directory);
            foreach (string extension in Constants.ImageExtensions)
            {
                foreach (FileInfo fileInfo in dirInfo.EnumerateFiles("*" + extension))
                {
                    if (!tileDictionary.ContainsKey(fileInfo.FullName.ToLower()))
                    {
                        try
                        {
                            Tile tile = new Tile(fileInfo.FullName);
                            tile.ComputeCellColors();
                            tile.ReleaseImage();
                            tileDictionary[tile.Path] = tile;
                            Debug.WriteLine("Loading tile {0} {1}", ++newLoadedFiles, fileInfo.Name);
                        } catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }
                    }
                }
            }
        }

        public void Save()
        {
            Serialize();
        }

        void Serialize()
        {
            using (Stream stream = new FileStream(Path.Combine(Directory, Constants.SerializedDirectoryFile), FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (BinaryWriter bwriter = new BinaryWriter(stream))
                {
                    bwriter.Write(Directory);
                    bwriter.Write(tileDictionary.Count);
                    foreach (Tile tile in tileDictionary.Values)
                    {
                        tile.Serialize(bwriter);
                    }
                }
            }
        }

        void Deserialize()
        {
            string binFile = Path.Combine(Directory, Constants.SerializedDirectoryFile);
            var fileInfo = new FileInfo(binFile);
            if (!fileInfo.Exists)
            {
                Debug.WriteLine("There is no serialized directory file");
                return;
            }

            using (Stream stream = new FileStream(binFile, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    string directory = reader.ReadString();
                    if (directory != Directory)
                    {
                        throw new Exception("invalid directory name in serialized file");
                    }
                    int count = reader.ReadInt32();
                    for (int i = 0; i < count; i++)
                    {
                        Tile tile = Tile.Deserialize(reader);
                        if (tile != null)
                        {
                            tileDictionary[tile.Path] = tile;
                        }
                    }
                }
            }
        }

    }
}
