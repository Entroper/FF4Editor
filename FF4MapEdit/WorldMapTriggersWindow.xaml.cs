using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
		private readonly ObservableCollection<WorldMapTrigger> _overworldTriggers;
		private readonly ObservableCollection<WorldMapTrigger> _underworldTriggers;
		private readonly ObservableCollection<WorldMapTrigger> _moonTriggers;

		public WorldMapTriggersWindow(FF4Rom rom)
		{
			InitializeComponent();

			_rom = rom;

			var triggers = _rom.LoadWorldMapTriggers(out _pointers);
			var overworldTriggerCount = _pointers[1] / FF4Rom.WorldMapTriggerSize;
			var underworldTriggerCount = _pointers[2] / FF4Rom.WorldMapTriggerSize - overworldTriggerCount;
			var moonTriggerCount = FF4Rom.WorldMapTriggerCount - overworldTriggerCount - underworldTriggerCount;
			_overworldTriggers = new ObservableCollection<WorldMapTrigger>(triggers.GetRange(0, overworldTriggerCount));
			_underworldTriggers = new ObservableCollection<WorldMapTrigger>(triggers.GetRange(overworldTriggerCount, underworldTriggerCount));
			_moonTriggers = new ObservableCollection<WorldMapTrigger>(triggers.GetRange(overworldTriggerCount + underworldTriggerCount, moonTriggerCount));

			OverworldListView.ItemsSource = _overworldTriggers;
			UnderworldListView.ItemsSource = _underworldTriggers;
			MoonListView.ItemsSource = _moonTriggers;
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			_rom.SaveWorldMapTriggers(_overworldTriggers.Concat(_underworldTriggers).Concat(_moonTriggers).ToList(), _pointers);
		}

		private void TriggerType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var comboBox = (ComboBox)sender;
			var selectedType = (WorldMapTriggerType)comboBox.SelectedValue;

			var parent = VisualTreeHelper.GetParent(comboBox);
			while (!(parent is ListViewItem))
			{
				parent = VisualTreeHelper.GetParent(parent);
			}
			var listViewItem = (ListViewItem)parent;

			while (!(parent is ListView))
			{
				parent = VisualTreeHelper.GetParent(parent);
			}
			var listView = (ListView)parent;

			var trigger = (WorldMapTrigger)listViewItem.DataContext;
			var selectedIndex = listView.Items.IndexOf(trigger);
			var items = (ObservableCollection<WorldMapTrigger>)listView.ItemsSource;
			if (trigger is WorldMapTeleport && selectedType == WorldMapTriggerType.Event)
			{
				items.RemoveAt(selectedIndex);
				items.Insert(selectedIndex, new WorldMapEvent(trigger.Bytes));
			}
			else if (trigger is WorldMapEvent && selectedType == WorldMapTriggerType.Teleport)
			{
				items.RemoveAt(selectedIndex);
				items.Insert(selectedIndex, new WorldMapTeleport(trigger.Bytes));
			}
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
