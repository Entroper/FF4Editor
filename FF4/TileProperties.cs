using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF4
{
	[Flags]
	public enum WorldTileProperties
	{
		Walk = 0x0001,
		ChocoboWalk = 0x0002,
		BlackChocoboFly = 0x0004,
		BlackChocoboLand = 0x0008,
		Hovercraft = 0x0010,
		AirshipFly = 0x0020,
		WalkPlateau = 0x0040,
		BigWhaleFly = 0x0080,
		ObscuresHalf = 0x0800,
		AirshipLand = 0x1000,
		EnemyEncounters = 0x4000,
		Trigger = 0x8000
	}

	[Flags]
	public enum AreaTileProperties
	{
		Walk = 0x0001,
		SavePoint = 0x0008,
		Damage = 0x0100,
		ObscuresAll = 0x0400,
		ObscuresHalf = 0x0800,
		Exit = 0x1000,
		Trigger = 0x8000
	}
}
