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
using System.Windows.Shapes;
using FF4;

namespace FF4MapEdit
{
	/// <summary>
	/// Interaction logic for WorldMapTriggersWindow.xaml
	/// </summary>
	public partial class WorldMapTriggersWindow : Window
	{
		private readonly FF4Rom _rom;

		private readonly ushort[] _pointers;
		private readonly List<WorldMapTeleport> _teleports;
		private readonly List<WorldMapEvent> _events;

		public WorldMapTriggersWindow(FF4Rom rom)
		{
			InitializeComponent();

			_rom = rom;

			var triggers = _rom.LoadWorldMapTriggers(out _pointers);
			_teleports = triggers.Where(trigger => trigger is WorldMapTeleport).Cast<WorldMapTeleport>().ToList();
			_events = triggers.Where(trigger => trigger is WorldMapEvent).Cast<WorldMapEvent>().ToList();

			TeleportsDataGrid.ItemsSource = _teleports;
			EventsDataGrid.ItemsSource = _events;
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			_rom.SaveWorldMapTriggers(_teleports.Cast<WorldMapTrigger>().Concat(_events).ToList(), _pointers);
		}
	}
}
