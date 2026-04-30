using System;

[Flags]
public enum ChunkProtectionLevel
{
	None = 0,
	NearOfflinePlayer = 1,
	NearBedroll = 2,
	NearSupplyCrate = 4,
	NearQuestObjective = 8,
	NearDroppedBackpack = 0x10,
	NearVehicle = 0x20,
	NearLandClaim = 0x40,
	OfflinePlayer = 0x80,
	Bedroll = 0x100,
	SupplyCrate = 0x200,
	QuestObjective = 0x400,
	DroppedBackpack = 0x800,
	Trader = 0x1000,
	Drone = 0x2000,
	Vehicle = 0x4000,
	LandClaim = 0x8000,
	CurrentlySynced = 0x10000,
	All = -1
}
