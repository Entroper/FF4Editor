using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF4Rom
{
	// ReSharper disable once InconsistentNaming
	public class FF4Rom
	{
		public const int OverworldRowPointersOffset = 0xB0000;
		public const int OverworldRowCount = 256;
		public const int OverworldSubTileGraphicsOffset = 0xE8000;
		public const int OverworldTileFormationsOffset = 0xA0000;
		public const int OverworldTilePropertiesOffset = 0xA0A80;
		public const int OverworldPaletteOffset = 0xA0900;
		public const int OverworldTilePaletteOffsetsOffset = 0xA0600;

		public const int UnderworldRowPointersOffset = 0xB0200;
		public const int UnderworldRowCount = 256;
		public const int UnderworldSubTileGraphicsOffset = 0xEA000;
		public const int UnderworldTileFormationsOffset = 0xA0200;
		public const int UnderworldTilePropertiesOffset = 0xA0B80;
		public const int UnderworldPaletteOffset = 0xA0980;
		public const int UnderworldTilePaletteOffsetsOffset = 0xA0700;

		public const int MoonRowPointersOffset = 0xB0400;
		public const int MoonRowCount = 64;
		public const int MoonSubTileGraphicsOffset = 0xEC000;
		public const int MoonTileFormationsOffset = 0xA0400;
		public const int MoonTilePropertiesOffset = 0xA0C80;
		public const int MoonPaletteOffset = 0xA0A00;
		public const int MoonTilePaletteOffsetsOffset = 0xA0800;

		public const int MapSubTileCount = 256;
		public const int MapTileCount = 128;
		public const int MapPaletteLength = 64;
	}
}
