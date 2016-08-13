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
			if (result == true)
			{
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
			}
		}
	}
}
