namespace Platform;

public struct PlayerInteraction(PlayerData _playerData, PlayerInteractionType _type)
{
	public PlayerData PlayerData = _playerData;

	public PlayerInteractionType Type = _type;
}
