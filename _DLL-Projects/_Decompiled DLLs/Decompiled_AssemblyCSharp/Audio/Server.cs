using System;
using System.Collections.Generic;
using UnityEngine;

namespace Audio;

public class Server : IDisposable
{
	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal m_localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, Client> m_players = new Dictionary<int, Client>();

	public void Play(Entity playOnEntity, string soundGroupName, float _occlusion, bool signalOnly = false)
	{
		if (GameManager.IsDedicatedServer && playOnEntity != null)
		{
			Manager.ConvertName(ref soundGroupName, playOnEntity);
			Manager.SignalAI(playOnEntity, playOnEntity.GetPosition(), soundGroupName, 1f);
		}
		if (signalOnly)
		{
			return;
		}
		foreach (KeyValuePair<int, Client> player in m_players)
		{
			if (Manager.IgnoresDistanceCheck(soundGroupName) || Entity.CheckDistance(playOnEntity, player.Value.entityId))
			{
				player.Value.Play(playOnEntity.entityId, soundGroupName, _occlusion);
			}
		}
	}

	public void Play(Vector3 position, string soundGroupName, float _occlusion, int entityId = -1)
	{
		if (GameManager.IsDedicatedServer)
		{
			Manager.ConvertName(ref soundGroupName);
			Manager.SignalAI(null, position, soundGroupName, 1f);
		}
		foreach (KeyValuePair<int, Client> player in m_players)
		{
			if (Manager.IgnoresDistanceCheck(soundGroupName) || Entity.CheckDistance(position, player.Value.entityId))
			{
				player.Value.Play(position, soundGroupName, _occlusion, entityId);
			}
		}
	}

	public void Stop(int playOnEntityId, string soundGroupName)
	{
		foreach (KeyValuePair<int, Client> player in m_players)
		{
			player.Value.Stop(playOnEntityId, soundGroupName);
		}
	}

	public void Stop(Vector3 position, string soundGroupName)
	{
		foreach (KeyValuePair<int, Client> player in m_players)
		{
			player.Value.Stop(position, soundGroupName);
		}
	}

	public void AttachLocalPlayer(EntityPlayerLocal localPlayer)
	{
		m_localPlayer = localPlayer;
	}

	public void EntityAddedToWorld(Entity entity, World world)
	{
		if (entity is EntityPlayer && (m_localPlayer == null || entity.entityId != m_localPlayer.entityId))
		{
			if (m_players.TryGetValue(entity.entityId, out var value))
			{
				Log.Warning("[AudioLog] AudioManagerServer: consistency error, client id '" + entity.entityId + "' already exists, but is being added again!");
				return;
			}
			value = new Client(entity.entityId);
			m_players[entity.entityId] = value;
		}
	}

	public void EntityRemovedFromWorld(Entity entity, World world)
	{
		if (m_players.TryGetValue(entity.entityId, out var value))
		{
			m_players.Remove(entity.entityId);
			value.Dispose();
		}
	}

	public void Dispose()
	{
		foreach (KeyValuePair<int, Client> player in m_players)
		{
			player.Value.Dispose();
		}
		m_players = null;
		m_localPlayer = null;
	}
}
