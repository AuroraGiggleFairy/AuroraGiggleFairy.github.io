using System.Collections.Generic;
using System.Linq;
using Platform;

public class PlayerInteractions
{
	[PublicizedFrom(EAccessModifier.Private)]
	public PersistentPlayerList playerList;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public static PlayerInteractions Instance { get; } = new PlayerInteractions();

	public event PlayerIteractionEvent OnNewPlayerInteraction;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerInteractions()
	{
	}

	public void JoinedMultiplayerServer(PersistentPlayerList ppl)
	{
		Log.Out("[PlayerInteractions] JoinedMultplayerServer");
		RecordInteractionForActivePersistentPlayers(ppl, PlayerInteractionType.Login);
		SetPlayersList(ppl);
	}

	public void PlayerSpawnedInMultiplayerServer(PersistentPlayerList ppl, int spawningEntityId, RespawnType respawnReason)
	{
		SetPlayersList(ppl);
		if (respawnReason == RespawnType.NewGame || respawnReason == RespawnType.EnterMultiplayer || respawnReason == RespawnType.JoinMultiplayer || respawnReason == RespawnType.LoadedGame)
		{
			if (spawningEntityId == (LocalPlayerUI.GetUIForPrimaryPlayer()?.entityPlayer)?.entityId && spawningEntityId != -1)
			{
				RecordInteractionForActivePersistentPlayers(ppl, PlayerInteractionType.FirstSpawn);
				return;
			}
			PersistentPlayerData playerDataFromEntityID = ppl.GetPlayerDataFromEntityID(spawningEntityId);
			this.OnNewPlayerInteraction?.Invoke(CreateInteraction(playerDataFromEntityID, PlayerInteractionType.FirstSpawn));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetPlayersList(PersistentPlayerList ppl)
	{
		if (ppl != playerList)
		{
			if (playerList != null)
			{
				playerList.RemovePlayerEventHandler(OnPersistentPlayerEvent);
			}
			playerList = ppl;
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
			PlatformManager.MultiPlatform.PlayerInteractionsRecorder?.RecordPlayerInteraction(playerInteraction);
			break;
		case EnumPersistentPlayerDataReason.Disconnected:
			Log.Out("[PlayerInteractions] persistent player disconnect");
			playerInteraction = CreateInteraction(ppData, PlayerInteractionType.Disconnect);
			PlatformManager.MultiPlatform.PlayerInteractionsRecorder?.RecordPlayerInteraction(playerInteraction);
			break;
		}
		this.OnNewPlayerInteraction?.Invoke(playerInteraction);
	}

	public void Shutdown()
	{
		if (playerList != null)
		{
			Log.Out("[PlayerInteractions] Shutdown, record disconnect for all currently connected players");
			RecordInteractionForActivePersistentPlayers(playerList, PlayerInteractionType.Disconnect);
			playerList.RemovePlayerEventHandler(OnPersistentPlayerEvent);
			playerList = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RecordInteractionForActivePersistentPlayers(PersistentPlayerList ppl, PlayerInteractionType interactionType)
	{
		IEnumerable<PlayerInteraction> enumerable = from ppd in ppl.Players.Values.ToList()
			where ppd.EntityId != -1
			select CreateInteraction(ppd, interactionType);
		PlatformManager.MultiPlatform.PlayerInteractionsRecorder?.RecordPlayerInteractions(enumerable);
		foreach (PlayerInteraction item in enumerable)
		{
			this.OnNewPlayerInteraction?.Invoke(item);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerInteraction CreateInteraction(PersistentPlayerData ppd, PlayerInteractionType type)
	{
		return new PlayerInteraction(ppd.PlayerData, type);
	}
}
