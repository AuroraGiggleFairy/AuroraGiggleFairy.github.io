using System;

[Flags]
public enum EnumDecoAllowed : byte
{
	Everything = 0,
	SlopeLo = 1,
	SlopeHi = 2,
	SizeLo = 4,
	SizeHi = 8,
	StreetOnly = 0x10,
	Nothing = byte.MaxValue
}
