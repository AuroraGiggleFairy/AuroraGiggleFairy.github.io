using UnityEngine.Scripting;

[Preserve]
public class AIDirectorPlayerManagementComponent : AIDirectorComponent
{
	public DictionaryList<int, AIDirectorPlayerState> trackedPlayers = new DictionaryList<int, AIDirectorPlayerState>();

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryPooledObject<AIDirectorPlayerState> playerPool = new MemoryPooledObject<AIDirectorPlayerState>(32);

	public override void Tick(double _dt)
	{
		base.Tick(_dt);
		TickPlayerStates(_dt);
	}

	public void AddPlayer(EntityPlayer _player)
	{
		if (!trackedPlayers.dict.ContainsKey(_player.entityId))
		{
			AIDirectorPlayerState aIDirectorPlayerState = playerPool.Alloc(_bReset: false);
			if (aIDirectorPlayerState != null)
			{
				trackedPlayers.Add(_player.entityId, aIDirectorPlayerState.Construct(_player));
			}
		}
	}

	public void RemovePlayer(EntityPlayer _player)
	{
		if (trackedPlayers.dict.TryGetValue(_player.entityId, out var value))
		{
			trackedPlayers.Remove(_player.entityId);
			value.Reset();
			playerPool.Free(value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickPlayerStates(double _dt)
	{
		for (int i = 0; i < trackedPlayers.list.Count; i++)
		{
			AIDirectorPlayerState ps = trackedPlayers.list[i];
			TickPlayerState(ps, _dt);
		}
	}

	public void UpdatePlayerInventory(int entityId, AIDirectorPlayerInventory inventory)
	{
		if (trackedPlayers.dict.TryGetValue(entityId, out var value))
		{
			value.Inventory = inventory;
		}
	}

	public void UpdatePlayerInventory(EntityPlayerLocal player)
	{
		UpdatePlayerInventory(player.entityId, AIDirectorPlayerInventory.FromEntity(player));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TickPlayerState(AIDirectorPlayerState _ps, double _dt)
	{
		_ps.Dead = _ps.Player.IsDead();
	}
}
