using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF4
{
	public abstract class WorldMapTrigger
	{
		public readonly byte[] Bytes;

		protected WorldMapTrigger(byte[] bytes)
		{
			Bytes = bytes;
		}

		public byte X
		{
			get => Bytes[0];
			set => Bytes[0] = value;
		}
		public byte Y
		{
			get => Bytes[1];
			set => Bytes[1] = value;
		}
	}

	public class WorldMapTeleport : WorldMapTrigger
	{
		public WorldMapTeleport(byte[] bytes) : base(bytes) {}

		public byte DestinationMap
		{
			get => Bytes[2];
			set => Bytes[2] = value;
		}

		public byte DestinationX
		{
			get => (byte)(Bytes[3] & 0x3F);
			set
			{
				if ((value & 0xC0) != 0)
					throw new ArgumentOutOfRangeException(nameof(value), "X coordinate must be between 0 and 64");

				Bytes[3] = (byte)(Bytes[3] & 0xC0 | value);
			}
		}
		public byte DestinationY
		{
			get => Bytes[4];
			set => Bytes[4] = value;
		}

		public FacingDirection FacingDirection
		{
			get => (FacingDirection)(Bytes[3] & 0xC0);
			set => Bytes[3] = (byte)(Bytes[3] & 0x3F | (byte)value);
		}
	}

	public enum FacingDirection
	{
		North = 0x00,
		East = 0x40,
		South = 0x80,
		West = 0xC0
	}

	public class WorldMapEvent : WorldMapTrigger
	{
		public WorldMapEvent(byte[] bytes) : base(bytes) {}

		public byte EventCall
		{
			get => Bytes[3];
			set => Bytes[3] = value;
		}
	}
}
