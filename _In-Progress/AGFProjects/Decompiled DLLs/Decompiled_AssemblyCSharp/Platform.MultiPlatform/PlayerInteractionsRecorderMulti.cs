using System.Collections.Generic;

namespace Platform.MultiPlatform;

public class PlayerInteractionsRecorderMulti : IPlayerInteractionsRecorder
{
	public void Init(IPlatform owner)
	{
	}

	public void RecordPlayerInteraction(PlayerInteraction interaction)
	{
		PlatformManager.NativePlatform?.PlayerInteractionsRecorder?.RecordPlayerInteraction(interaction);
		PlatformManager.CrossplatformPlatform?.PlayerInteractionsRecorder?.RecordPlayerInteraction(interaction);
	}

	public void RecordPlayerInteractions(IEnumerable<PlayerInteraction> interactions)
	{
		PlatformManager.NativePlatform?.PlayerInteractionsRecorder?.RecordPlayerInteractions(interactions);
		PlatformManager.CrossplatformPlatform?.PlayerInteractionsRecorder?.RecordPlayerInteractions(interactions);
	}

	public void Destroy()
	{
	}
}
