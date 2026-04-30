namespace Platform;

public interface IMultiplayerInvitationDialog
{
	bool CanShow { get; }

	void Init(IPlatform owner);

	void ShowInviteDialog();
}
