namespace FF4Lib;

public class Map
{
	private readonly byte[,] _map;
	private readonly byte[][] _compressedRows;
	private readonly int[] _compressedRowLengths;

	public readonly MapType MapType;

	public readonly int Width;
	public readonly int Height;

	public byte this[int y, int x]
	{
		get => _map[y, x];
		set => _map[y, x] = value;
	}

	public int CompressedSize => _compressedRowLengths.Sum();

	public Map(MapType mapType, byte[] data, ushort[] pointers)
	{
		MapType = mapType;

		if (mapType == MapType.Overworld)
		{
			Height = FF4Rom.OverworldRowCount;
			Width = FF4Rom.OverworldRowLength;
		}
		else if (mapType == MapType.Underworld)
		{
			Height = FF4Rom.UnderworldRowCount;
			Width = FF4Rom.UnderworldRowLength;
		}
		else if (mapType == MapType.Moon)
		{
			Height = FF4Rom.MoonRowCount;
			Width = FF4Rom.MoonRowLength;
		}

		_map = new byte[Height, Width];
		_compressedRows = new byte[Height][];
		_compressedRowLengths = new int[Height];

		for (int y = 0; y < Height; y++)
		{
			var dataOffset = pointers[y];
			var x = 0;
			byte tile = data[dataOffset++];
			while (tile != 0xFF)
			{
				if (mapType == MapType.Overworld && tile == 0x00)
				{
					_map[y, x++] = 0x00;
					_map[y, x++] = 0x70;
					_map[y, x++] = 0x71;
					_map[y, x++] = 0x72;
				}
				else if (mapType == MapType.Overworld && tile == 0x10)
				{
					_map[y, x++] = 0x10;
					_map[y, x++] = 0x73;
					_map[y, x++] = 0x74;
					_map[y, x++] = 0x75;
				}
				else if (mapType == MapType.Overworld && tile == 0x20)
				{
					_map[y, x++] = 0x20;
					_map[y, x++] = 0x76;
					_map[y, x++] = 0x77;
					_map[y, x++] = 0x78;
				}
				else if (mapType == MapType.Overworld && tile == 0x30)
				{
					_map[y, x++] = 0x30;
					_map[y, x++] = 0x79;
					_map[y, x++] = 0x7A;
					_map[y, x++] = 0x7B;
				}
				else if (tile >= 0x80)
				{
					tile -= 0x80;
					var count = data[dataOffset++] + 1;
					for (int j = 0; j < count; j++)
					{
						_map[y, x++] = tile;
					}
				}
				else
				{
					_map[y, x++] = tile;
				}

				tile = data[dataOffset++];
			}

			CompressRow(y);
		}
	}

	public void GetCompressedData(out byte[] data, out ushort[] pointers)
	{
		if (MapType == MapType.Overworld)
		{
			data = new byte[FF4Rom.OverworldRowDataMaxLength];
		}
		else if (MapType == MapType.Underworld)
		{
			data = new byte[FF4Rom.UnderworldRowDataMaxLength];
		}
		else if (MapType == MapType.Moon)
		{
			data = new byte[FF4Rom.MoonRowDataMaxLength];
		}
		else
		{
			throw new Exception("Invalid world map type");
		}

		pointers = new ushort[Height];
		int dataOffset = 0;
		for (int y = 0; y < Height; y++)
		{
			Buffer.BlockCopy(_compressedRows[y], 0, data, dataOffset, _compressedRowLengths[y]);
			pointers[y] = (ushort)dataOffset;

			dataOffset += _compressedRowLengths[y];
		}

		while (dataOffset < data.Length)
		{
			data[dataOffset++] = 0xFF;
		}
	}

	public void CompressRow(int y)
	{
		_compressedRows[y] = new byte[Width + 1]; // extra 0xFF at the end

		int x = 0, dataOffset = 0;
		while (x < Width)
		{
			if (MapType == MapType.Overworld && (_map[y, x] == 0x00 || _map[y, x] == 0x10 || _map[y, x] == 0x20 || _map[y, x] == 0x30))
			{
				_compressedRows[y][dataOffset++] = _map[y, x];
				x += 4;
			}
			else if (x == Width - 1)
			{
				_compressedRows[y][dataOffset++] = _map[y, x++];
			}
			else if (_map[y, x + 1] == _map[y, x])
			{
				_compressedRows[y][dataOffset++] = (byte)(_map[y, x++] + 0x80);

				byte repeatCount = 0;
				while (x < Width && _map[y, x - 1] == _map[y, x])
				{
					x++;
					repeatCount++;
				}
				_compressedRows[y][dataOffset++] = repeatCount;
			}
			else
			{
				_compressedRows[y][dataOffset++] = _map[y, x++];
			}
		}

		_compressedRows[y][dataOffset++] = 0xFF;
		_compressedRowLengths[y] = dataOffset;
	}
}
