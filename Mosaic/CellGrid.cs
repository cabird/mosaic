using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mosaic
{
    class CellGrid
    {
        public readonly int Rows = 7;
        public readonly int Columns = 7;
        public Color[,] CellColors;

        public CellGrid()
        {
            CellColors = new Color[Columns, Rows];
        }

        public Int64 DistanceFrom(CellGrid cellGrid)
        {
            if (cellGrid.Rows != Rows || cellGrid.Columns != Columns)
            {
                throw new Exception("rows and columns are not consistent");
            }

            Int64 rSum = 0;
            Int64 gSum = 0;
            Int64 bSum = 0;

            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    Color c1 = CellColors[x, y];
                    Color c2 = cellGrid.CellColors[x, y];
                    rSum += (c1.R - c2.R) * (c1.R - c2.R);
                    gSum += (c1.G - c2.G) * (c1.G - c2.G);
                    bSum += (c1.B - c2.B) * (c1.B - c2.B);
                }
            }

            return rSum + gSum + bSum;
        }

        public void ComputeCellColors(BitmapData bitmapData, byte[] data, Rectangle rect)
        {

            if (bitmapData.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new Exception("need to fix this to handle different pixel formats");
            }
            byte bytesPerPixel = 4;
            int stride = Math.Abs(bitmapData.Stride);

            /* for each column of cells */
            for (int cellY = 0; cellY < Rows; cellY++)
            {
                int yStart = rect.Y + (int)Math.Floor(rect.Height * (double)cellY / Rows);
                int yEnd = rect.Y + (int)Math.Floor(rect.Height * (double)(cellY + 1) / Rows);

                /* for each cell in that column (one per row) */
                for (int cellX = 0; cellX < Columns; cellX++)
                {
                    int xStart = rect.X + (int)Math.Floor(rect.Width * (double)cellX / Columns);
                    int xEnd = rect.X + (int)Math.Floor(rect.Width * (double)(cellX + 1) / Columns);

                    int r = 0;
                    int g = 0;
                    int b = 0;
                    int pixels = 0;
                    for (int y = yStart; y < yEnd; y++)
                    {
                        for (int x = xStart; x < xEnd; x++)
                        {
                            int offset = bytesPerPixel * x + (stride * y);
                            /* this may kill us with endianness on different platforms...  the format is ARGB, but it's switched so we read it as BGRA
                             * This is super specific to the pixel format*/
                            b += data[offset];
                            g += data[offset + 1];
                            r += data[offset + 2];
                            pixels++;
                        }
                    }
                    CellColors[cellX, cellY] = Color.FromArgb(r / pixels, g / pixels, b / pixels);
                }
            }
        }

        public Bitmap GenerateCellBitmap(int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height);
            /* for each column of cells */
            for (int cellX = 0; cellX < Columns; cellX++)
            {
                int xStart = (int)Math.Floor(bitmap.Width * (double)cellX / Columns);
                int xEnd = (int)Math.Floor(bitmap.Width * (double)(cellX + 1) / Columns);

                /* for each cell in that column (one per row) */
                for (int cellY = 0; cellY < Rows; cellY++)
                {
                    int yStart = (int)Math.Floor(bitmap.Height * (double)cellY / Rows);
                    int yEnd = (int)Math.Floor(bitmap.Height * (double)(cellY + 1) / Rows);

                    for (int y = yStart; y < yEnd; y++)
                    {
                        for (int x = xStart; x < xEnd; x++)
                        {
                            bitmap.SetPixel(x, y, CellColors[cellX, cellY]);
                        }
                    }
                }
            }
            return bitmap;
        }


    }
}
