using System;

[Flags]
public enum EBlockRotationClasses
{
	None = 0,
	Basic90 = 1,
	Headfirst = 2,
	Sideways = 4,
	Basic45 = 8,
	Basic90And45 = 9,
	No45 = 7,
	Advanced = 6,
	All = 0xF
}
