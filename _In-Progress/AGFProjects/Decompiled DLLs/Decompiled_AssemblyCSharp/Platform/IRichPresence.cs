namespace Platform;

public interface IRichPresence
{
	public enum PresenceStates
	{
		Menu,
		Loading,
		Connecting,
		InGame
	}

	void Init(IPlatform _owner);

	void UpdateRichPresence(PresenceStates _state);
}
