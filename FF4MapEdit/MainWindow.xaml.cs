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
using FF4;
using Microsoft.Win32;

namespace FF4MapEdit
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private FF4Rom _rom;
		private string _filename;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void OpenButton_Click(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new OpenFileDialog
			{
				Filter = "SNES ROM files (*.sfc;*.smc)|*.sfc;*.smc|All Files (*.*)|*.*"
			};

			var result = openFileDialog.ShowDialog(this);
			if (result == false)
			{
				return;
			}

			_rom = new FF4Rom(openFileDialog.FileName);
			if (_rom.Validate())
			{
				MessageBox.Show("ROM appears to be valid.");
			}
			else
			{
				MessageBox.Show("ROM does not appear to be valid.  Proceed at your own risk.");
			}

			_filename = openFileDialog.FileName;

			LoadOverworld();
		}

		private void LoadOverworld()
		{
			LoadOverworldTileset();
			LoadOverworldTiles();
		}

		private void LoadOverworldTileset()
		{
			var tileGroup = new DrawingGroup();
			tileGroup.Open();

			var subTiles = _rom.GetOverworldSubTiles();
			var palette = _rom.GetOverworldPalette();
			FixPalette(palette);
			var offsets = _rom.GetOverworldSubTilePaletteOffsets();
			var formations = _rom.GetOverworldTileFormations();

			var tileCount = FF4Rom.MapTileCount;
			var tiles = new ImageDrawing[tileCount];
			for (int i = 0; i < tileCount; i++)
			{
				var tileBytes = new ushort[16*16];
				var subTileIndex = formations[i];
				CopySubTile(subTiles, 32*subTileIndex, tileBytes, 0, palette, offsets[subTileIndex]);
				subTileIndex = formations[i + tileCount];
				CopySubTile(subTiles, 32*subTileIndex, tileBytes, 8, palette, offsets[subTileIndex]);
				subTileIndex = formations[i + 2*tileCount];
				CopySubTile(subTiles, 32*subTileIndex, tileBytes, 8*16, palette, offsets[subTileIndex]);
				subTileIndex = formations[i + 3*tileCount];
				CopySubTile(subTiles, 32*subTileIndex, tileBytes, 8*16 + 8, palette, offsets[subTileIndex]);

				tileGroup.Children.Add(new ImageDrawing(
					BitmapSource.Create(16, 16, 72, 72, PixelFormats.Bgr555, null, tileBytes, 16*2),
					new Rect(new Point(16*(i%16), 16*(i/16)), new Size(16, 16))));
			}

			Tileset.Source = new DrawingImage(tileGroup);
		}

		private void CopySubTile(byte[] subTiles, int subTilesOffset, ushort[] tile, int tileOffset, ushort[] palette, int paletteOffset)
		{
			for (int y = 0; y < 8; y++)
			{
				for (int x = 0; x < 4; x++)
				{
					byte twoPixels = subTiles[subTilesOffset + 4 * y + x];

					byte pixel = (byte)(twoPixels & 0x0F);
					tile[tileOffset + 16*y + 2*x] = palette[paletteOffset + pixel];

					pixel = (byte)((twoPixels & 0xF0) >> 4);
					tile[tileOffset + 16*y + 2*x + 1] = palette[paletteOffset + pixel];
				}
			}
		}

		private void FixPalette(ushort[] palette)
		{
			for (int i = 0; i < palette.Length; i++)
			{
				palette[i] = (ushort)(
					((palette[i] & 0x001F) << 10) |
					((palette[i] & 0x03E0)) |
					((palette[i] & 0x7C00) >> 10));
			}
		}

		private void LoadOverworldTiles()
		{
			var tiles = ((DrawingGroup)((DrawingImage)Tileset.Source).Drawing).Children;

			var rowGroup = new DrawingGroup();
			rowGroup.Open();

			var rows = _rom.GetOverworldRows();
			for (int y = 0; y < FF4Rom.OverworldRowCount; y++)
			{
				var tileGroup = new DrawingGroup { Transform = new TranslateTransform(0, 16*y) };
				tileGroup.Open();
				for (int x = 0; x < FF4Rom.OverworldRowLength; x++)
				{
					tileGroup.Children.Add(new ImageDrawing(
						((ImageDrawing)tiles[rows[FF4Rom.OverworldRowLength*y + x]]).ImageSource,
						new Rect(new Point(16*x, 0), new Size(16, 16))));
				}

				rowGroup.Children.Add(tileGroup);
			}

			Map.Source = new DrawingImage(rowGroup);
			Map.Stretch = Stretch.None;
		}
	}
}
