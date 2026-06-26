namespace Platform;

public interface IJoinSessionGameInviteListener
{
	void Init(IPlatform _owner);

	void Update();

	void Cancel();

	bool HasPendingIntent();

	bool IsProcessingIntent(out bool _checkRestartAtMainMenu);
}
