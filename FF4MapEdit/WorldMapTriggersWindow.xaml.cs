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
		private readonly List<WorldMapTrigger> _overworldTriggers;
		private readonly List<WorldMapTrigger> _underworldTriggers;
		private readonly List<WorldMapTrigger> _moonTriggers;

		public WorldMapTriggersWindow(FF4Rom rom)
		{
			InitializeComponent();

			_rom = rom;

			var triggers = _rom.LoadWorldMapTriggers(out _pointers);
			var overworldTriggerCount = _pointers[1] / FF4Rom.WorldMapTriggerSize;
			var underworldTriggerCount = _pointers[2] / FF4Rom.WorldMapTriggerSize - overworldTriggerCount;
			var moonTriggerCount = FF4Rom.WorldMapTriggerCount - overworldTriggerCount - underworldTriggerCount;
			_overworldTriggers = triggers.GetRange(0, overworldTriggerCount);
			_underworldTriggers = triggers.GetRange(overworldTriggerCount, underworldTriggerCount);
			_moonTriggers = triggers.GetRange(overworldTriggerCount + underworldTriggerCount, moonTriggerCount);

			OverworldListView.ItemsSource = _overworldTriggers;
			UnderworldListView.ItemsSource = _underworldTriggers;
			MoonListView.ItemsSource = _moonTriggers;
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			_rom.SaveWorldMapTriggers(_overworldTriggers.Concat(_underworldTriggers).Concat(_moonTriggers).ToList(), _pointers);
		}
	}

	public class WorldMapTriggerDataTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate
			SelectTemplate(object item, DependencyObject container)
		{
			if (container is FrameworkElement element && item != null)
			{
				if (item is WorldMapTeleport)
					return element.FindResource("WorldMapTeleportTemplate") as DataTemplate;
				else
					return element.FindResource("WorldMapEventTemplate") as DataTemplate;
			}

			return null;
		}
	}
}
