namespace Discord.Rest;

internal class ThreadInfo
{
	public string Name { get; }

	public bool IsArchived { get; }

	public ThreadArchiveDuration AutoArchiveDuration { get; }

	public bool IsLocked { get; }

	public int? SlowModeInterval { get; }

	internal ThreadInfo(string name, bool archived, ThreadArchiveDuration autoArchiveDuration, bool locked, int? rateLimit)
	{
		Name = name;
		IsArchived = archived;
		AutoArchiveDuration = autoArchiveDuration;
		IsLocked = locked;
		SlowModeInterval = rateLimit;
	}
}
