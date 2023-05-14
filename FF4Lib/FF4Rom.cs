using System.Text;
using RomUtilities;

namespace FF4Lib;

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
	public const int UnderworldRowDataOffset = 0xB4480;
	public const int UnderworldRowDataMaxLength = 0x1D00;
	public const int UnderworldRowCount = 256;
	public const int UnderworldRowLength = 256;
	public const int UnderworldSubTileGraphicsOffset = 0xEA000;
	public const int UnderworldPaletteOffset = 0xA0980;
	public const int UnderworldSubTilePaletteOffsetsOffset = 0xA0700;
	public const int UnderworldTileFormationsOffset = 0xA0200;
	public const int UnderworldTilePropertiesOffset = 0xA0B80;

	public const int MoonRowPointersOffset = 0xB0400;
	public const int MoonRowDataOffset = 0xB6180;
	public const int MoonRowDataMaxLength = 0xA00;
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

	public const int WorldMapTriggerOffset = 0xCFE66;
	public const int WorldMapTriggerPointersOffset = 0xCFE60;
	public const int WorldMapTriggerCount = 82;
	public const int WorldMapTriggerSize = 5;

	public FF4Rom(string filename) : base(filename)
	{
	}

	public override bool Validate()
	{
		var title = Encoding.ASCII.GetString(Get(0x7FC0, 20));
		return title == "FINAL FANTASY 2     ";
	}

	public Map LoadWorldMap(MapType mapType)
	{
		Blob data, pointerBytes;
		int rowCount;

		if (mapType == MapType.Overworld)
		{
			data = Get(OverworldRowDataOffset, OverworldRowDataMaxLength);
			pointerBytes = Get(OverworldRowPointersOffset, OverworldRowCount * 2);
			rowCount = OverworldRowCount;
		}
		else if (mapType == MapType.Underworld)
		{
			data = Get(UnderworldRowDataOffset, UnderworldRowDataMaxLength);
			pointerBytes = Get(UnderworldRowPointersOffset, UnderworldRowCount * 2);
			rowCount = UnderworldRowCount;
		}
		else if (mapType == MapType.Moon)
		{
			data = Get(MoonRowDataOffset, MoonRowDataMaxLength);
			pointerBytes = Get(MoonRowPointersOffset, MoonRowCount * 2);
			rowCount = MoonRowCount;
		}
		else
		{
			throw new ArgumentException("Invalid world map type");
		}

		var pointers = new ushort[rowCount];
		Buffer.BlockCopy(pointerBytes, 0, pointers, 0, pointerBytes.Length);

		return new Map(mapType, data, pointers);
	}

	public Tileset LoadWorldMapTileset(MapType mapType)
	{
		Blob subTiles, formations, paletteBytes, paletteOffsets, propertyBytes;
		if (mapType == MapType.Overworld)
		{
			subTiles = Get(OverworldSubTileGraphicsOffset, 32 * MapSubTileCount);
			formations = Get(OverworldTileFormationsOffset, 4 * MapTileCount);

			paletteBytes = Get(OverworldPaletteOffset, 2 * 64);
			paletteOffsets = Get(OverworldSubTilePaletteOffsetsOffset, MapSubTileCount);

			propertyBytes = Get(OverworldTilePropertiesOffset, MapTileCount * 2);
		}
		else if (mapType == MapType.Underworld)
		{
			subTiles = Get(UnderworldSubTileGraphicsOffset, 32 * MapSubTileCount);
			formations = Get(UnderworldTileFormationsOffset, 4 * MapTileCount);

			paletteBytes = Get(UnderworldPaletteOffset, 2 * 64);
			paletteOffsets = Get(UnderworldSubTilePaletteOffsetsOffset, MapSubTileCount);

			propertyBytes = Get(UnderworldTilePropertiesOffset, MapTileCount * 2);
		}
		else if (mapType == MapType.Moon)
		{
			subTiles = Get(MoonSubTileGraphicsOffset, 32 * MapSubTileCount);
			formations = Get(MoonTileFormationsOffset, 4 * MapTileCount);

			paletteBytes = Get(MoonPaletteOffset, 2 * 64);
			paletteOffsets = Get(MoonSubTilePaletteOffsetsOffset, MapSubTileCount);

			propertyBytes = Get(MoonTilePropertiesOffset, MapTileCount * 2);
		}
		else
		{
			throw new ArgumentException("Invalid world map type");
		}

		var palette = new ushort[64];
		Buffer.BlockCopy(paletteBytes, 0, palette, 0, 2 * 64);

		var tileProperties = new ushort[MapTileCount];
		Buffer.BlockCopy(propertyBytes, 0, tileProperties, 0, propertyBytes.Length);

		return new Tileset(subTiles, formations, palette, paletteOffsets, tileProperties);
	}

	public List<WorldMapTrigger> LoadWorldMapTriggers(out ushort[] pointers)
	{
		var triggerBytes = Get(WorldMapTriggerOffset, WorldMapTriggerCount * WorldMapTriggerSize).Chunk(WorldMapTriggerSize);
		pointers = Get(WorldMapTriggerPointersOffset, 6).ToUShorts();

		var triggers = new List<WorldMapTrigger>();
		foreach (var bytes in triggerBytes)
		{
			if (bytes[2] == 0xFF)
			{
				triggers.Add(new WorldMapEvent(bytes));
			}
			else
			{
				triggers.Add(new WorldMapTeleport(bytes));
			}
		}

		return triggers;
	}

	public void SaveWorldMap(Map map)
	{
		var length = map.CompressedSize;
		int maxLength =
			map.MapType == MapType.Overworld ? OverworldRowDataMaxLength :
			map.MapType == MapType.Underworld ? UnderworldRowDataMaxLength :
			map.MapType == MapType.Moon ? MoonRowDataMaxLength : 0;
		if (length > maxLength)
		{
			throw new IndexOutOfRangeException($"World map data is too big: {length} bytes used, {maxLength} bytes allowed");
		}

		byte[] pointerBytes = new byte[2*map.Height];
		map.GetCompressedData(out byte[] data, out ushort[] pointers);
		Buffer.BlockCopy(pointers, 0, pointerBytes, 0, pointerBytes.Length);

		if (map.MapType == MapType.Overworld)
		{
			Put(OverworldRowDataOffset, data);
			Put(OverworldRowPointersOffset, pointerBytes);
		}
		else if (map.MapType == MapType.Underworld)
		{
			Put(UnderworldRowDataOffset, data);
			Put(UnderworldRowPointersOffset, pointerBytes);
		}
		else if (map.MapType == MapType.Moon)
		{
			Put(MoonRowDataOffset, data);
			Put(MoonRowPointersOffset, pointerBytes);
		}
	}

	public void SaveWorldMapTileset(MapType mapType, Tileset tileset)
	{
		byte[] propertyBytes = new byte[MapTileCount*2];
		Buffer.BlockCopy(tileset.TileProperties, 0, propertyBytes, 0, propertyBytes.Length);

		if (mapType == MapType.Overworld)
		{
			Put(OverworldTilePropertiesOffset, propertyBytes);
		}
		else if (mapType == MapType.Underworld)
		{
			Put(UnderworldTilePropertiesOffset, propertyBytes);
		}
		else if (mapType == MapType.Moon)
		{
			Put(MoonTilePropertiesOffset, propertyBytes);
		}
	}

	public void SaveWorldMapTriggers(List<WorldMapTrigger> triggers, ushort[] pointers)
	{
		var triggerBytes = triggers.SelectMany(trigger => trigger.Bytes).ToArray();
		Put(WorldMapTriggerOffset, triggerBytes);
		Put(WorldMapTriggerPointersOffset, Blob.FromUShorts(pointers));
	}
}
