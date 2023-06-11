namespace FF4Lib;

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
