public enum DynamicItemState : byte
{
	Waiting,
	UpdateRequired,
	Empty,
	LoadRequested,
	Loading,
	Loaded,
	ReadyToDelete,
	Invalid
}
