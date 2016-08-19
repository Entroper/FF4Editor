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
		public const int OverworldRowDataOffset = 0xB0480;
		public const int OverworldRowDataMaxLength = 0x4000;
		public const int OverworldRowCount = 256;
		public const int OverworldRowLength = 256;
		public const int OverworldSubTileGraphicsOffset = 0xE8000;
		public const int OverworldPaletteOffset = 0xA0900;
		public const int OverworldSubTilePaletteOffsetsOffset = 0xA0600;
		public const int OverworldTileFormationsOffset = 0xA0000;
		public const int OverworldTilePropertiesOffset = 0xA0A80;

		public const int UnderworldRowPointersOffset = 0xB0200;
		public const int UnderworldRowCount = 256;
		public const int UnderworldRowLength = 256;
		public const int UnderworldSubTileGraphicsOffset = 0xEA000;
		public const int UnderworldPaletteOffset = 0xA0980;
		public const int UnderworldSubTilePaletteOffsetsOffset = 0xA0700;
		public const int UnderworldTileFormationsOffset = 0xA0200;
		public const int UnderworldTilePropertiesOffset = 0xA0B80;

		public const int MoonRowPointersOffset = 0xB0400;
		public const int MoonRowCount = 64;
		public const int MoonRowLength = 64;
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

		public Tileset Tileset { get; private set; }
		public Map Map { get; private set; }

		public void LoadOverworldMap()
		{
			var data = Get(OverworldRowDataOffset, OverworldRowDataMaxLength);
			var pointerBytes = Get(OverworldRowPointersOffset, OverworldRowCount * 2);
			var pointers = new ushort[OverworldRowCount];
			Buffer.BlockCopy(pointerBytes, 0, pointers, 0, pointerBytes.Length);

			Map = new Map(MapType.Overworld, data, pointers);

			var subTiles = Get(OverworldSubTileGraphicsOffset, 32 * MapSubTileCount);
			var formations = Get(OverworldTileFormationsOffset, 4 * MapTileCount);

			var paletteBytes = Get(OverworldPaletteOffset, 2 * 64);
			var palette = new ushort[64];
			Buffer.BlockCopy(paletteBytes, 0, palette, 0, 2 * 64);
			var paletteOffsets = Get(OverworldSubTilePaletteOffsetsOffset, MapSubTileCount);

			var propertyBytes = Get(OverworldTilePropertiesOffset, MapTileCount*2);
			var tileProperties = new ushort[MapTileCount];
			Buffer.BlockCopy(propertyBytes, 0, tileProperties, 0, propertyBytes.Length);

			Tileset = new Tileset(subTiles, formations, palette, paletteOffsets, tileProperties);
		}

		public void SaveOverworldMap()
		{
			var length = Map.Length;
			if (length > OverworldRowDataMaxLength)
			{
				throw new IndexOutOfRangeException($"Overworld map data is too big: {length} bytes used, {OverworldRowDataMaxLength} bytes allowed");
			}

			byte[] data;
			ushort[] pointers;
			byte[] pointerBytes = new byte[OverworldRowCount*2];
			Map.GetCompressedData(out data, out pointers);
			Buffer.BlockCopy(pointers, 0, pointerBytes, 0, pointerBytes.Length);

			byte[] propertyBytes = new byte[MapTileCount*2];
			Buffer.BlockCopy(Tileset.TileProperties, 0, propertyBytes, 0, propertyBytes.Length);

			Put(OverworldRowDataOffset, data);
			Put(OverworldRowPointersOffset, pointerBytes);
			Put(OverworldTilePropertiesOffset, propertyBytes);
		}
	}
}
