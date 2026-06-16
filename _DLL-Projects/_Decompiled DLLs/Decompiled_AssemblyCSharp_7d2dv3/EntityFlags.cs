using System;

[Flags]
public enum EntityFlags : uint
{
	None = 0u,
	Player = 1u,
	Zombie = 2u,
	Animal = 4u,
	Bandit = 8u,
	Edible = 0x10u,
	All = uint.MaxValue,
	AIHearing = 0xEu,
	AISmelling = 6u
}
