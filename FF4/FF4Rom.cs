using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RomUtilities;

namespace FF4
{
	// ReSharper disable once InconsistentNaming
	public class FF4Rom : SnesRom
	{
		public const int OverworldRowPointersOffset = 0xB0000;
		public const int OverworldRowCount = 256;
		public const int OverworldSubTileGraphicsOffset = 0xE8000;
		public const int OverworldPaletteOffset = 0xA0900;
		public const int OverworldSubTilePaletteOffsetsOffset = 0xA0600;
		public const int OverworldTileFormationsOffset = 0xA0000;
		public const int OverworldTilePropertiesOffset = 0xA0A80;

		public const int UnderworldRowPointersOffset = 0xB0200;
		public const int UnderworldRowCount = 256;
		public const int UnderworldSubTileGraphicsOffset = 0xEA000;
		public const int UnderworldPaletteOffset = 0xA0980;
		public const int UnderworldSubTilePaletteOffsetsOffset = 0xA0700;
		public const int UnderworldTileFormationsOffset = 0xA0200;
		public const int UnderworldTilePropertiesOffset = 0xA0B80;

		public const int MoonRowPointersOffset = 0xB0400;
		public const int MoonRowCount = 64;
		public const int MoonSubTileGraphicsOffset = 0xEC000;
		public const int MoonPaletteOffset = 0xA0A00;
		public const int MoonSubTilePaletteOffsetsOffset = 0xA0800;
		public const int MoonTileFormationsOffset = 0xA0400;
		public const int MoonTilePropertiesOffset = 0xA0C80;

		public const int MapSubTileCount = 256;
		public const int MapTileCount = 128;
		public const int MapPaletteLength = 64;

		public FF4Rom(string filename) : base(filename)
		{
		}

		public override bool Validate()
		{
			var title = Encoding.ASCII.GetString(Get(0x7FC0, 20));
			return title == "FINAL FANTASY 2     ";
		}

		public byte[] GetOverworldSubTiles() => Get(OverworldSubTileGraphicsOffset, 32*MapSubTileCount);
		public byte[] GetOverworldTileFormations() => Get(OverworldTileFormationsOffset, 4*MapTileCount);
		public byte[] GetOverworldSubTilePaletteOffsets() => Get(OverworldSubTilePaletteOffsetsOffset, MapSubTileCount);

		public ushort[] GetOverworldPalette()
		{
			var paletteBytes = Get(OverworldPaletteOffset, 2*64);

			var palette = new ushort[64];
			Buffer.BlockCopy(paletteBytes, 0, palette, 0, 2*64);

			return palette;
		}
	}
}
