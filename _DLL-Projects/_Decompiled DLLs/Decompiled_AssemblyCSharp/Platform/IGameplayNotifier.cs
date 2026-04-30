namespace Platform;

public interface IGameplayNotifier
{
	void Init(IPlatform platform);

	void GameplayStart(bool isOnlineMultiplayer, bool isCrossplayEnabled);

	void EndOnlineMultiplayer();

	void GameplayEnd();
}
