using System.Collections.Generic;
using System.Linq;
using Platform;

public class PlayerInteractions
{
	[PublicizedFrom(EAccessModifier.Private)]
	public PersistentPlayerList playerList;

	public static event PlayerIteractionEvent OnNewPlayerInteraction;

	public void JoinedMultiplayerServer(PersistentPlayerList ppl)
	{
		if (PlatformManager.MultiPlatform.PlayerInteractionsRecorder != null)
		{
			Log.Out("[PlayerInteractions] JoinedMultplayerServer");
			if (playerList != null && ppl != playerList)
			{
				playerList.RemovePlayerEventHandler(OnPersistentPlayerEvent);
				playerList = null;
			}
			playerList = ppl;
			RecordInteractionForActivePersistentPlayers(playerList, PlayerInteractionType.Login);
			playerList.AddPlayerEventHandler(OnPersistentPlayerEvent);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnPersistentPlayerEvent(PersistentPlayerData ppData, PersistentPlayerData otherPlayer, EnumPersistentPlayerDataReason reason)
	{
		PlayerInteraction playerInteraction = default(PlayerInteraction);
		switch (reason)
		{
		case EnumPersistentPlayerDataReason.Login:
			Log.Out("[PlayerInteractions] persistent player login");
			playerInteraction = CreateInteraction(ppData, PlayerInteractionType.Login);
			PlatformManager.MultiPlatform.PlayerInteractionsRecorder.RecordPlayerInteraction(playerInteraction);
			break;
		case EnumPersistentPlayerDataReason.Disconnected:
			Log.Out("[PlayerInteractions] persistent player disconnect");
			playerInteraction = CreateInteraction(ppData, PlayerInteractionType.Disconnect);
			PlatformManager.MultiPlatform.PlayerInteractionsRecorder.RecordPlayerInteraction(playerInteraction);
			break;
		}
		PlayerInteractions.OnNewPlayerInteraction?.Invoke(playerInteraction);
	}

	public void Shutdown()
	{
		if (playerList != null)
		{
			Log.Out("[PlayerInteractions] Shutdown, record disconnect for all currently connected players");
			RecordInteractionForActivePersistentPlayers(playerList, PlayerInteractionType.Disconnect);
			playerList?.RemovePlayerEventHandler(OnPersistentPlayerEvent);
			playerList = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void RecordInteractionForActivePersistentPlayers(PersistentPlayerList ppl, PlayerInteractionType interactionType)
	{
		IEnumerable<PlayerInteraction> enumerable = from ppd in ppl.Players.Values.ToList()
			where ppd.EntityId != -1
			select CreateInteraction(ppd, interactionType);
		PlatformManager.MultiPlatform.PlayerInteractionsRecorder.RecordPlayerInteractions(enumerable);
		foreach (PlayerInteraction item in enumerable)
		{
			PlayerInteractions.OnNewPlayerInteraction?.Invoke(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PlayerInteraction CreateInteraction(PersistentPlayerData ppd, PlayerInteractionType type)
	{
		return new PlayerInteraction(ppd.PlayerData, type);
	}
}
