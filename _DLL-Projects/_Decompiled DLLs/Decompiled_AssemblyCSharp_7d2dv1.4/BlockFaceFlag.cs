using System;

[Flags]
public enum BlockFaceFlag
{
	None = 0,
	Top = 1,
	Bottom = 2,
	North = 4,
	West = 8,
	South = 0x10,
	East = 0x20,
	All = 0x3F,
	Solid = 0x3F,
	Axials = 0x3C
}
