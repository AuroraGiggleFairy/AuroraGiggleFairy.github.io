using System;

namespace SharpEXR;

[Flags]
public enum EXRVersionFlags
{
	IsSinglePartTiled = 0x200,
	LongNames = 0x400,
	NonImageParts = 0x800,
	MultiPart = 0x1000
}
