using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FF4
{
	public class Map
	{
		private readonly byte[,] _map;
		private readonly byte[][] _compressedRows;
		private readonly int[] _compressedRowLengths;

		public MapType MapType;

		public byte this[int y, int x]
		{
			get { return _map[y, x]; }
			set
			{
				_map[y, x] = value;
				CompressRow(y);
			}
		}

		public int Length => _compressedRowLengths.Sum();

		public Map(MapType mapType, byte[] data, ushort[] pointers)
		{
			MapType = mapType;

			if (mapType == MapType.Overworld)
			{
				var rowCount = FF4Rom.OverworldRowCount;
				_map = new byte[rowCount, FF4Rom.OverworldRowLength];
				_compressedRows = new byte[rowCount][];
				_compressedRowLengths = new int[rowCount];

				for (int y = 0; y < rowCount; y++)
				{
					var dataOffset = pointers[y];
					var x = 0;
					byte tile = data[dataOffset++];
					while (tile != 0xFF)
					{
						if (tile == 0x00)
						{
							_map[y, x++] = 0x00;
							_map[y, x++] = 0x70;
							_map[y, x++] = 0x71;
							_map[y, x++] = 0x72;
						}
						else if (tile == 0x10)
						{
							_map[y, x++] = 0x10;
							_map[y, x++] = 0x73;
							_map[y, x++] = 0x74;
							_map[y, x++] = 0x75;
						}
						else if (tile == 0x20)
						{
							_map[y, x++] = 0x20;
							_map[y, x++] = 0x76;
							_map[y, x++] = 0x77;
							_map[y, x++] = 0x78;
						}
						else if (tile == 0x30)
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
		}

		public void GetCompressedData(out byte[] data, out ushort[] pointers)
		{
			// Just for now until we cover all execution paths.
			data = null;
			pointers = null;

			if (MapType == MapType.Overworld)
			{
				data = new byte[FF4Rom.OverworldRowDataMaxLength];
				pointers = new ushort[FF4Rom.OverworldRowCount];
				int dataOffset = 0;
				for (int y = 0; y < FF4Rom.OverworldRowCount; y++)
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
		}

		private void CompressRow(int y)
		{
			int rowLength = FF4Rom.OverworldRowLength;
			_compressedRows[y] = new byte[rowLength + 1]; // extra 0xFF at the end

			int x = 0, dataOffset = 0;
			while (x < rowLength)
			{
				if (_map[y, x] == 0x00 || _map[y, x] == 0x10 || _map[y, x] == 0x20 || _map[y, x] == 0x30)
				{
					_compressedRows[y][dataOffset++] = _map[y, x];
					x += 4;
				}
				else if (x == rowLength - 1)
				{
					_compressedRows[y][dataOffset++] = _map[y, x++];
				}
				else if (_map[y, x + 1] == _map[y, x])
				{
					_compressedRows[y][dataOffset++] = (byte)(_map[y, x++] + 0x80);

					byte repeatCount = 0;
					while (x < rowLength && _map[y, x - 1] == _map[y, x])
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
}
