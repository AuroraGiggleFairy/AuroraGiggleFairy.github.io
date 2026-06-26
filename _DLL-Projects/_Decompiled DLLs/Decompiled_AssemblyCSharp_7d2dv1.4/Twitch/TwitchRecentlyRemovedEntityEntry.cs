namespace Twitch;

public class TwitchRecentlyRemovedEntityEntry
{
	public Entity SpawnedEntity;

	public int SpawnedEntityID = -1;

	public TwitchActionEntry Action;

	public TwitchEventActionEntry Event;

	public TwitchVoteEntry Vote;

	public float TimeRemaining;

	public TwitchRecentlyRemovedEntityEntry(TwitchSpawnedEntityEntry entry)
	{
		SpawnedEntity = entry.SpawnedEntity;
		SpawnedEntityID = entry.SpawnedEntityID;
		Action = entry.Action;
		Event = entry.Event;
		Vote = entry.Vote;
		TimeRemaining = 60f;
	}
}
