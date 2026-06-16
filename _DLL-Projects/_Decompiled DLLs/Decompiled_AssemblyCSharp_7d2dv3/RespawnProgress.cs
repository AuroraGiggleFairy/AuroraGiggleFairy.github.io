public enum RespawnProgress : byte
{
	Done,
	WaitingForVideoToPlay,
	WaitingForRespawnTime,
	WaitingForSpawnWindowToOpen,
	WaitingForSpawnWindowToClose,
	WaitingForSpawnPointSelection,
	ClampingToValidWorldPos,
	RetryingRespawn
}
