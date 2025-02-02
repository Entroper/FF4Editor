using FF4Lib;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace FF4Editor.Drawing
{
    public static class Maps
    {
        public static DrawingImage LoadWorldMapTileset(WorldTileset tileset)
        {
            var tileGroup = new DrawingGroup();

            for (int i = 0; i < FF4Rom.MapTileCount; i++)
            {
                tileGroup.Children.Add(new ImageDrawing(
                    BitmapSource.Create(16, 16, 72, 72, PixelFormats.Bgr555, null, tileset[i], 16 * 2),
                    new Rect(new Point(16 * (i % 16), 16 * (i / 16)), new Size(16, 16))));
            }

            return new DrawingImage(tileGroup);
        }

        public static (DrawingImage image, WriteableBitmap[] rowBitmaps) LoadWorldMapTiles(WorldMap map, WorldTileset tileset)
        {
            var rowGroup = new DrawingGroup();
            rowGroup.Open();

            var rowBitmaps = new WriteableBitmap[map.Height];
            for (int y = 0; y < map.Height; y++)
            {
                rowBitmaps[y] = new WriteableBitmap(16 * map.Width, 16, 72, 72, PixelFormats.Bgr555, null);
                rowBitmaps[y].Lock();
                for (int x = 0; x < map.Width; x++)
                {
                    var tile = map[y, x];
                    rowBitmaps[y].WritePixels(new Int32Rect(16 * x, 0, 16, 16), tileset[tile], 16 * 2, 0);
                }

                rowBitmaps[y].Unlock();

                rowGroup.Children.Add(new ImageDrawing(rowBitmaps[y],
                    new Rect(new Point(0, 16 * y), new Size(16 * map.Width, 16))));
            }

            return (new DrawingImage(rowGroup), rowBitmaps);
        }
    }
}
