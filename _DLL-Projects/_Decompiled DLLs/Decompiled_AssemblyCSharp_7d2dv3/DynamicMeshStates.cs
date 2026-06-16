using System;

[Flags]
public enum DynamicMeshStates : short
{
	None = 0,
	ThreadUpdating = 1,
	SaveRequired = 2,
	LoadRequired = 4,
	UnloadMark1 = 8,
	UnloadMark2 = 0x10,
	UnloadMark3 = 0x20,
	MarkedForDelete = 0x40,
	LoadBoosted = 0x80,
	MainThreadLoadRequest = 0x100,
	FileMissing = 0x200,
	Generating = 0x400
}
