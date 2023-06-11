using System.Text;
using RomUtilities;

namespace FF4Lib;

// ReSharper disable once InconsistentNaming
public partial class FF4Rom : SnesRom
{
	public FF4Rom(string filename) : base(filename)
	{
	}

	public override bool Validate()
	{
		var title = Encoding.ASCII.GetString(Get(0x7FC0, 20));
		return title == "FINAL FANTASY 2     ";
	}
}
