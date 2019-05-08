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

		private Tileset _tileset;
		private Map _map;

		private int _selectedTile = -1;
		private GeometryDrawing _selectedTileDrawing = new GeometryDrawing();
		private WriteableBitmap[] _rowBitmaps;

		private bool _painting = false;

		private int SpaceUsed
		{
			set
			{
				int maxLength =
					_map.MapType == MapType.Overworld ? FF4Rom.OverworldRowDataMaxLength :
					_map.MapType == MapType.Underworld ? FF4Rom.UnderworldRowDataMaxLength :
					_map.MapType == MapType.Moon ? FF4Rom.MoonRowDataMaxLength : 0;

				SpaceUsedLabel.Content = $"Space used: {value}/{maxLength} bytes";
				SpaceUsedLabel.Foreground = value > FF4Rom.OverworldRowDataMaxLength ? Brushes.Red : Brushes.Black;
			}
		}

		public MainWindow()
		{
			InitializeComponent();

			WalkCheckBox.Checked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.Walk, true);
			ChocoboWalkCheckBox.Checked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.ChocoboWalk, true);
			BlackChocoboFlyCheckBox.Checked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.BlackChocoboFly, true);
			BlackChocoboLandCheckBox.Checked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.BlackChocoboLand, true);
			HovercraftCheckBox.Checked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.Hovercraft, true);
			AirshipFlyCheckBox.Checked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.AirshipFly, true);
			Walk2CheckBox.Checked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.Walk2, true);
			BigWhaleFlyCheckBox.Checked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.BigWhaleFly, true);
			ObscuresHalfCheckBox.Checked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.ObscuresHalf, true);
			AirshipLandCheckBox.Checked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.AirshipLand, true);
			EnemyEncountersCheckBox.Checked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.EnemyEncounters, true);
			EntranceCheckBox.Checked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.Entrance, true);

			WalkCheckBox.Unchecked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.Walk, false);
			ChocoboWalkCheckBox.Unchecked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.ChocoboWalk, false);
			BlackChocoboFlyCheckBox.Unchecked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.BlackChocoboFly, false);
			BlackChocoboLandCheckBox.Unchecked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.BlackChocoboLand, false);
			HovercraftCheckBox.Unchecked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.Hovercraft, false);
			AirshipFlyCheckBox.Unchecked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.AirshipFly, false);
			Walk2CheckBox.Unchecked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.Walk2, false);
			BigWhaleFlyCheckBox.Unchecked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.BigWhaleFly, false);
			ObscuresHalfCheckBox.Unchecked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.ObscuresHalf, false);
			AirshipLandCheckBox.Unchecked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.AirshipLand, false);
			EnemyEncountersCheckBox.Unchecked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.EnemyEncounters, false);
			EntranceCheckBox.Unchecked += (sender, e) => OnPropertyCheckBoxCheckChanged(WorldTileProperties.Entrance, false);
		}

		private void OnPropertyCheckBoxCheckChanged(WorldTileProperties property, bool isChecked)
		{
			if (isChecked)
			{
				_tileset.TileProperties[_selectedTile] |= (ushort)property;
			}
			else
			{
				_tileset.TileProperties[_selectedTile] &= (ushort)~property;
			}
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
			if (!_rom.Validate())
			{
				MessageBox.Show("ROM does not appear to be a US version 1.1 ROM.  Proceed at your own risk.", "Validation Failed");
			}

			_filename = openFileDialog.FileName;

			LoadWorldMap(MapType.Overworld);
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			if (_map != null)
			{
				try
				{
					_rom.SaveWorldMap(_map);
					_rom.SaveWorldMapTileset(_tileset);
				}
				catch (IndexOutOfRangeException ex) when (ex.Message.StartsWith("Overworld map data is too big"))
				{
					MessageBox.Show(ex.Message, "Error while saving");
				}

				_rom.Save(_filename);
			}
		}

		private void Tileset_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			GetClickedTile(sender, e, out int x, out int y);
			if (x < 0 || x >= 16 || y < 0 || y >= 8)
			{
				return;
			}

			_selectedTile = 16*y + x;

			HighlightSelectedTile(x, y);

			CheckTilePropertyBoxes();
			TilePropertiesGrid.Visibility = Visibility.Visible;
		}

		private void MapComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var mapType = (MapType)Enum.Parse(typeof(MapType), ((ComboBoxItem)MapComboBox.SelectedItem).Content.ToString());
			if (mapType != _map.MapType)
				LoadWorldMap(mapType);
		}

		private void HighlightSelectedTile(int x, int y)
		{
			var tileGroup = (DrawingGroup)((DrawingImage)Tileset.Source).Drawing;
			tileGroup.Children.Remove(_selectedTileDrawing);

			var geometry = new GeometryGroup();
			geometry.Children.Add(new RectangleGeometry(new Rect(new Point(16*x + 1, 16*y + 1), new Size(14, 14))));
			_selectedTileDrawing = new GeometryDrawing
			{
				Geometry = geometry,
				Brush = Brushes.Transparent,
				Pen = new Pen(Brushes.Aqua, 2)
			};

			tileGroup.Children.Add(_selectedTileDrawing);
		}

		private void CheckTilePropertyBoxes()
		{
			var tileProperties = (WorldTileProperties)_tileset.TileProperties[_selectedTile];

			WalkCheckBox.IsChecked = tileProperties.HasFlag(WorldTileProperties.Walk);
			ChocoboWalkCheckBox.IsChecked = tileProperties.HasFlag(WorldTileProperties.ChocoboWalk);
			BlackChocoboFlyCheckBox.IsChecked = tileProperties.HasFlag(WorldTileProperties.BlackChocoboFly);
			BlackChocoboLandCheckBox.IsChecked = tileProperties.HasFlag(WorldTileProperties.BlackChocoboLand);
			HovercraftCheckBox.IsChecked = tileProperties.HasFlag(WorldTileProperties.Hovercraft);
			AirshipFlyCheckBox.IsChecked = tileProperties.HasFlag(WorldTileProperties.AirshipFly);
			Walk2CheckBox.IsChecked = tileProperties.HasFlag(WorldTileProperties.Walk2);
			BigWhaleFlyCheckBox.IsChecked = tileProperties.HasFlag(WorldTileProperties.BigWhaleFly);
			ObscuresHalfCheckBox.IsChecked = tileProperties.HasFlag(WorldTileProperties.ObscuresHalf);
			AirshipLandCheckBox.IsChecked = tileProperties.HasFlag(WorldTileProperties.AirshipLand);
			EnemyEncountersCheckBox.IsChecked = tileProperties.HasFlag(WorldTileProperties.EnemyEncounters);
			EntranceCheckBox.IsChecked = tileProperties.HasFlag(WorldTileProperties.Entrance);
		}

		private void Map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (_selectedTile == -1)
			{
				return;
			}

			GetClickedTile(sender, e, out int  x, out int y);
			if (x < 0 || x >= _map.Width || y < 0 || y >= _map.Height)
			{
				return;
			}

			Paint(x, y);
			_painting = true;
		}

		private void Map_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			_painting = false;
		}

		private void Map_MouseMove(object sender, MouseEventArgs e)
		{
			if (_painting)
			{
				GetClickedTile(sender, e, out int x, out int y);
				Paint(x, y);
			}
		}

		private void Paint(int x, int y)
		{
			_map[y, x] = (byte)_selectedTile;

			_rowBitmaps[y].Lock();
			_rowBitmaps[y].WritePixels(new Int32Rect(16*x, 0, 16, 16), _tileset[_selectedTile], 16*2, 0);
			_rowBitmaps[y].Unlock();

			SpaceUsed = _map.CompressedSize;
		}

		private void GetClickedTile(object sender, MouseButtonEventArgs e, out int x, out int y)
		{
			var position = e.GetPosition((IInputElement)sender);
			PixelsToTile(position, out x, out y);
		}

		private void GetClickedTile(object sender, MouseEventArgs e, out int x, out int y)
		{
			var position = e.GetPosition((IInputElement)sender);
			PixelsToTile(position, out x, out y);
		}

		private static void PixelsToTile(Point position, out int x, out int y)
		{
			x = (int)position.X;
			y = (int)position.Y;
			x /= 16;
			y /= 16;
		}

		private void LoadWorldMap(MapType mapType)
		{
			_map = _rom.LoadWorldMap(mapType);
			_tileset = _rom.LoadWorldMapTileset(mapType);

			LoadWorldMapTileset();
			LoadWorldMapTiles();

			SpaceUsed = _map.CompressedSize;
		}

		private void LoadWorldMapTileset()
		{
			var tileGroup = new DrawingGroup();
			tileGroup.Children.Add(_selectedTileDrawing);

			for (int i = 0; i < FF4Rom.MapTileCount; i++)
			{
				tileGroup.Children.Add(new ImageDrawing(
					BitmapSource.Create(16, 16, 72, 72, PixelFormats.Bgr555, null, _tileset[i], 16 * 2),
					new Rect(new Point(16 * (i % 16), 16 * (i / 16)), new Size(16, 16))));
			}

			Tileset.Source = new DrawingImage(tileGroup);
		}

		private void LoadWorldMapTiles()
		{
			var rowGroup = new DrawingGroup();
			rowGroup.Open();

			_rowBitmaps = new WriteableBitmap[_map.Height];
			for (int y = 0; y < _map.Height; y++)
			{
				_rowBitmaps[y] = new WriteableBitmap(16*_map.Width, 16, 72, 72, PixelFormats.Bgr555, null);
				_rowBitmaps[y].Lock();
				for (int x = 0; x < _map.Width; x++)
				{
					var tile = _map[y, x];
					_rowBitmaps[y].WritePixels(new Int32Rect(16*x, 0, 16, 16), _tileset[tile], 16*2, 0);
				}

				_rowBitmaps[y].Unlock();

				rowGroup.Children.Add(new ImageDrawing(_rowBitmaps[y],
					new Rect(new Point(0, 16*y), new Size(16* _map.Width, 16))));
			}

			Map.Source = new DrawingImage(rowGroup);
			Map.Width = 16*_map.Width;
			Map.Height = 16*_map.Height;
		}

		private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			MapScrollViewer.ScrollToHorizontalOffset(Math.Round(MapScrollViewer.HorizontalOffset));
			MapScrollViewer.ScrollToVerticalOffset(Math.Round(MapScrollViewer.VerticalOffset));
		}
	}
}
