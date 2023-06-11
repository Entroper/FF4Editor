using System;
using System.Collections.Generic;
using System.Windows;
using FF4Lib;
using Microsoft.Win32;

namespace FF4Editor;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	private FF4Rom? _rom;
	private string _filename;

	private WorldMap[] _worldMaps;
	private WorldTileset[] _tilesets;
	private List<WorldMapTrigger> _triggers;
	private ushort[] _triggerPointers;

	public MainWindow()
	{
		_filename = string.Empty;

		_worldMaps = new WorldMap[0];
		_tilesets = new WorldTileset[0];
		_triggers = new List<WorldMapTrigger>();
		_triggerPointers = new ushort[0];

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
			return;

		_rom = new FF4Rom(openFileDialog.FileName);
		if (!_rom.Validate())
			MessageBox.Show("ROM does not appear to be a US version 1.1 ROM.  Proceed at your own risk.", "Validation Failed");

		_filename = openFileDialog.FileName;
		FilenameLabel.Content = _filename;

		_worldMaps = new[]
		{
            _rom.LoadWorldMap(MapType.Overworld),
            _rom.LoadWorldMap(MapType.Underworld),
            _rom.LoadWorldMap(MapType.Moon)
		};
		_tilesets = new[]
		{
			_rom.LoadWorldMapTileset(MapType.Overworld),
			_rom.LoadWorldMapTileset(MapType.Underworld),
			_rom.LoadWorldMapTileset(MapType.Moon)
		};
		_triggers = _rom.LoadWorldMapTriggers(out _triggerPointers);
	}

	private void SaveButton_Click(object sender, RoutedEventArgs e)
	{
		if (_rom == null)
			return;

		try
		{
            _rom.SaveWorldMap(_worldMaps[(int)MapType.Overworld]);
            _rom.SaveWorldMap(_worldMaps[(int)MapType.Underworld]);
            _rom.SaveWorldMap(_worldMaps[(int)MapType.Moon]);

            _rom.SaveWorldMapTileset(MapType.Overworld, _tilesets[(int)MapType.Overworld]);
            _rom.SaveWorldMapTileset(MapType.Underworld, _tilesets[(int)MapType.Underworld]);
            _rom.SaveWorldMapTileset(MapType.Moon, _tilesets[(int)MapType.Moon]);

            _rom.SaveWorldMapTriggers(_triggers, _triggerPointers);
        }
        catch (IndexOutOfRangeException ex) when(ex.Message.StartsWith("World map data is too big"))
        {
            MessageBox.Show(ex.Message, "Error while saving");
        }

        _rom.Save(_filename);
	}

    private void WorldMapButton_Click(object sender, RoutedEventArgs e)
    {
		if ( _rom == null)
			return;

		new WorldMapWindow(_worldMaps, _tilesets, _triggers, _triggerPointers).ShowDialog();
    }
}
