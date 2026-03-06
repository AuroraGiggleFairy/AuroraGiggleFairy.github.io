using System.Collections.Generic;

namespace Platform;

public interface IPlayerInteractionsRecorder
{
	void Init(IPlatform owner);

	void RecordPlayerInteraction(PlayerInteraction interaction);

	void RecordPlayerInteractions(IEnumerable<PlayerInteraction> interactions);

	void Destroy();
}
