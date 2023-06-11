namespace FF4Lib;

public class WorldTileset
{
	private readonly ushort[][] _tilesetBytes;
	public ushort[] TileProperties { get; private set; }

	public ushort[] this[int tileIndex] => _tilesetBytes[tileIndex];

	public WorldTileset(byte[] subTiles, byte[] formations, ushort[] palette, byte[] paletteOffsets, ushort[] tileProperties)
	{
		RgbToBgr(palette);

		var tileCount = FF4Rom.MapTileCount;
		_tilesetBytes = new ushort[tileCount][];
		for (int i = 0; i < tileCount; i++)
		{
			_tilesetBytes[i] = new ushort[16 * 16];
			var subTileIndex = formations[i];
			CopySubTileToTile(subTiles, 32 * subTileIndex, _tilesetBytes[i], 0, palette, paletteOffsets[subTileIndex]);
			subTileIndex = formations[i + tileCount];
			CopySubTileToTile(subTiles, 32 * subTileIndex, _tilesetBytes[i], 8, palette, paletteOffsets[subTileIndex]);
			subTileIndex = formations[i + 2 * tileCount];
			CopySubTileToTile(subTiles, 32 * subTileIndex, _tilesetBytes[i], 8 * 16, palette, paletteOffsets[subTileIndex]);
			subTileIndex = formations[i + 3 * tileCount];
			CopySubTileToTile(subTiles, 32 * subTileIndex, _tilesetBytes[i], 8 * 16 + 8, palette, paletteOffsets[subTileIndex]);
		}

		TileProperties = tileProperties;
	}

	private void CopySubTileToTile(byte[] subTiles, int subTilesOffset, ushort[] tile, int tileOffset, ushort[] palette, int paletteOffset)
	{
		for (int y = 0; y < 8; y++)
		{
			for (int x = 0; x < 4; x++)
			{
				byte twoPixels = subTiles[subTilesOffset + 4 * y + x];

				byte pixel = (byte)(twoPixels & 0x0F);
				tile[tileOffset + 16 * y + 2 * x] = palette[paletteOffset + pixel];

				pixel = (byte)((twoPixels & 0xF0) >> 4);
				tile[tileOffset + 16 * y + 2 * x + 1] = palette[paletteOffset + pixel];
			}
		}
	}

	private void RgbToBgr(ushort[] palette)
	{
		for (int i = 0; i < palette.Length; i++)
		{
			palette[i] = (ushort)(
				((palette[i] & 0x001F) << 10) |
				((palette[i] & 0x03E0)) |
				((palette[i] & 0x7C00) >> 10));
		}
	}
}
