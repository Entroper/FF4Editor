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

		private ushort[][] _tilesetBytes;

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
			_tilesetBytes = new ushort[tileCount][];
			for (int i = 0; i < tileCount; i++)
			{
				_tilesetBytes[i] = new ushort[16*16];
				var subTileIndex = formations[i];
				CopySubTileToTile(subTiles, 32*subTileIndex, _tilesetBytes[i], 0, palette, offsets[subTileIndex]);
				subTileIndex = formations[i + tileCount];
				CopySubTileToTile(subTiles, 32*subTileIndex, _tilesetBytes[i], 8, palette, offsets[subTileIndex]);
				subTileIndex = formations[i + 2*tileCount];
				CopySubTileToTile(subTiles, 32*subTileIndex, _tilesetBytes[i], 8*16, palette, offsets[subTileIndex]);
				subTileIndex = formations[i + 3*tileCount];
				CopySubTileToTile(subTiles, 32*subTileIndex, _tilesetBytes[i], 8*16 + 8, palette, offsets[subTileIndex]);

				tileGroup.Children.Add(new ImageDrawing(
					BitmapSource.Create(16, 16, 72, 72, PixelFormats.Bgr555, null, _tilesetBytes[i], 16*2),
					new Rect(new Point(16*(i%16), 16*(i/16)), new Size(16, 16))));
			}

			Tileset.Source = new DrawingImage(tileGroup);
		}

		private void CopySubTileToTile(byte[] subTiles, int subTilesOffset, ushort[] tile, int tileOffset, ushort[] palette, int paletteOffset)
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
			var rowGroup = new DrawingGroup();
			rowGroup.Open();

			var rows = _rom.GetOverworldRows();
			var rowLength = FF4Rom.OverworldRowLength;
			for (int y = 0; y < FF4Rom.OverworldRowCount; y++)
			{
				var rowBytes = new ushort[16*16*rowLength];
				for (int x = 0; x < rowLength; x++)
				{
					CopyTileToRow(_tilesetBytes[rows[y*rowLength + x]], rowBytes, 16*x);
				}

				rowGroup.Children.Add(new ImageDrawing(
					BitmapSource.Create(16*rowLength, 16, 72, 72, PixelFormats.Bgr555, null, rowBytes, 16*rowLength*2),
					new Rect(new Point(0, 16*y), new Size(16*rowLength, 16))));
			}

			Map.Source = new DrawingImage(rowGroup);
			Map.Stretch = Stretch.None;
		}

		private void CopyTileToRow(ushort[] tile, ushort[] row, int rowOffset)
		{
			for (int y = 0; y < 16; y++)
			{
				for (int x = 0; x < 16; x++)
				{
					row[rowOffset + 16*y*FF4Rom.OverworldRowLength + x] = tile[16*y + x];
				}
			}
		}
	}
}
