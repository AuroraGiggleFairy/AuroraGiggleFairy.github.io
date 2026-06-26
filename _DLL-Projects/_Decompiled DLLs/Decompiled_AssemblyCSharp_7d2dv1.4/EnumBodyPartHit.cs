using System;

[Flags]
public enum EnumBodyPartHit
{
	None = 0,
	Torso = 1,
	Head = 2,
	LeftUpperArm = 4,
	RightUpperArm = 8,
	LeftUpperLeg = 0x10,
	RightUpperLeg = 0x20,
	LeftLowerArm = 0x40,
	RightLowerArm = 0x80,
	LeftLowerLeg = 0x100,
	RightLowerLeg = 0x200,
	Special = 0x400,
	UpperArms = 0xC,
	LowerArms = 0xC0,
	Arms = 0xCC,
	UpperLegs = 0x30,
	LowerLegs = 0x300,
	Legs = 0x330,
	BitsUsed = 0xB
}
