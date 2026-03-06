namespace Platform.MultiPlatform;

public class RichPresence : IRichPresence
{
	public void Init(IPlatform _owner)
	{
	}

	public void UpdateRichPresence(IRichPresence.PresenceStates _state)
	{
		PlatformManager.NativePlatform.RichPresence?.UpdateRichPresence(_state);
		PlatformManager.CrossplatformPlatform?.RichPresence?.UpdateRichPresence(_state);
	}
}
