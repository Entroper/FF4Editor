namespace FF4Lib;

public abstract class WorldMapTrigger
{
	public readonly byte[] Bytes;

	public WorldMapTriggerType Type { get; set; }

	protected WorldMapTrigger(byte[] bytes, WorldMapTriggerType type)
	{
		Bytes = bytes;
		Type = type;
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

public enum WorldMapTriggerType : byte
{
	Teleport,
	Event = 0xFF
}

public class WorldMapTeleport : WorldMapTrigger
{
	public WorldMapTeleport(byte[] bytes) : base(bytes, WorldMapTriggerType.Teleport)
	{
		if (DestinationMap == 0xFF)
		{
			DestinationMap = 0x00;
		}
	}

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

public enum FacingDirection : byte
{
	North = 0x00,
	East = 0x40,
	South = 0x80,
	West = 0xC0
}

public class WorldMapEvent : WorldMapTrigger
{
	public WorldMapEvent(byte[] bytes) : base(bytes, WorldMapTriggerType.Event)
	{
		Bytes[2] = 0xFF;
	}

	public byte EventCall
	{
		get => Bytes[3];
		set => Bytes[3] = value;
	}
}
