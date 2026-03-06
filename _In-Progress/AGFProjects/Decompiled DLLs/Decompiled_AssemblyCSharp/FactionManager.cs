using System;
using System.IO;
using UnityEngine;

public class FactionManager
{
	public enum Relationship
	{
		Hate = 0,
		Dislike = 200,
		Neutral = 400,
		Like = 600,
		Love = 800,
		Leader = 1001
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const float SAVE_TIME_SEC = 60f;

	public static FactionManager Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public static byte Version = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Faction[] Factions = new Faction[255];

	[PublicizedFrom(EAccessModifier.Private)]
	public float saveTime = 60f;

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo dataSaveThreadInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public float rel;

	public void PrintData()
	{
		for (int i = 0; i < Factions.Length; i++)
		{
			if (Factions[i] != null)
			{
				Log.Out(Factions[i].ToString());
			}
		}
	}

	public static void Init()
	{
		Instance = new FactionManager();
	}

	public void Update()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && GameManager.Instance.World != null && GameManager.Instance.World.Players != null && GameManager.Instance.World.Players.Count != 0 && !GameManager.Instance.gameStateManager.IsGameStarted())
		{
			saveTime -= Time.deltaTime;
			if (saveTime <= 0f && (dataSaveThreadInfo == null || dataSaveThreadInfo.HasTerminated()))
			{
				saveTime = 60f;
				Save();
			}
		}
	}

	public Relationship GetRelationshipTier(EntityAlive checkingEntity, EntityAlive targetEntity)
	{
		if (checkingEntity == null || targetEntity == null)
		{
			return Relationship.Neutral;
		}
		rel = GetRelationshipValue(checkingEntity, targetEntity);
		if (rel < 200f)
		{
			return Relationship.Hate;
		}
		if (rel < 400f)
		{
			return Relationship.Dislike;
		}
		if (rel < 600f)
		{
			return Relationship.Neutral;
		}
		if (rel < 800f)
		{
			return Relationship.Like;
		}
		if (rel < 1001f)
		{
			return Relationship.Love;
		}
		return Relationship.Leader;
	}

	public Faction CreateFaction(string _name = "", bool _playerFaction = true, string _icon = "")
	{
		Faction faction = new Faction(_name, _playerFaction, _icon);
		AddFaction(faction);
		return faction;
	}

	public void AddFaction(Faction _faction)
	{
		for (int i = (_faction.IsPlayerFaction ? 8 : 0); i < Factions.Length; i++)
		{
			if (Factions[i] == null)
			{
				Factions[i] = _faction;
				_faction.ID = (byte)i;
				break;
			}
		}
	}

	public void RemoveFaction(byte _id)
	{
		Factions[_id] = null;
	}

	public Faction GetFaction(byte _id)
	{
		return Factions[_id];
	}

	public Faction GetFactionByName(string _name)
	{
		for (int i = 0; i < Factions.Length; i++)
		{
			if (Factions[i].Name == _name)
			{
				return Factions[i];
			}
		}
		return null;
	}

	public float GetRelationshipValue(EntityAlive checkingEntity, EntityAlive targetEntity)
	{
		if (checkingEntity == null || targetEntity == null)
		{
			return 400f;
		}
		if (checkingEntity.factionId == targetEntity.factionId)
		{
			return 800f;
		}
		if (Factions[checkingEntity.factionId] != null && Factions[targetEntity.factionId] != null)
		{
			return Factions[checkingEntity.factionId].GetRelationship(targetEntity.factionId);
		}
		return 400f;
	}

	public void SetRelationship(byte _myFaction, byte _targetFaction, sbyte _modification)
	{
		if (Factions[_myFaction] != null)
		{
			Factions[_myFaction].ModifyRelationship(_targetFaction, _modification);
		}
	}

	public void ModifyRelationship(byte _myFaction, byte _targetFaction, sbyte _modification)
	{
		if (Factions[_myFaction] != null)
		{
			Factions[_myFaction].ModifyRelationship(_targetFaction, _modification);
		}
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(Version);
		for (int i = 0; i < Factions.Length; i++)
		{
			_bw.Write(Factions[i] != null);
			if (Factions[i] != null)
			{
				Factions[i].Write(_bw);
			}
		}
	}

	public void Read(BinaryReader _br)
	{
		_br.ReadByte();
		for (int i = 0; i < 255; i++)
		{
			if (_br.ReadBoolean())
			{
				Factions[i] = new Faction();
				Factions[i].Read(_br);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int saveFactionDataThreaded(ThreadManager.ThreadInfo _threadInfo)
	{
		PooledExpandableMemoryStream pooledExpandableMemoryStream = (PooledExpandableMemoryStream)_threadInfo.parameter;
		string text = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "factions.dat");
		if (!SdDirectory.Exists(GameIO.GetSaveGameDir()))
		{
			return -1;
		}
		if (SdFile.Exists(text))
		{
			SdFile.Copy(text, string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "factions.dat.bak"), overwrite: true);
		}
		pooledExpandableMemoryStream.Position = 0L;
		StreamUtils.WriteStreamToFile(pooledExpandableMemoryStream, text);
		MemoryPools.poolMemoryStream.FreeSync(pooledExpandableMemoryStream);
		return -1;
	}

	public void Save()
	{
		if (dataSaveThreadInfo == null || !ThreadManager.ActiveThreads.ContainsKey("factionDataSave"))
		{
			PooledExpandableMemoryStream pooledExpandableMemoryStream = MemoryPools.poolMemoryStream.AllocSync(_bReset: true);
			using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(_bReset: false))
			{
				pooledBinaryWriter.SetBaseStream(pooledExpandableMemoryStream);
				Write(pooledBinaryWriter);
			}
			dataSaveThreadInfo = ThreadManager.StartThread("factionDataSave", null, saveFactionDataThreaded, null, pooledExpandableMemoryStream, null, _useRealThread: false, _isSilent: true);
		}
	}

	public void Load()
	{
		string path = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "factions.dat");
		if (!SdDirectory.Exists(GameIO.GetSaveGameDir()) || !SdFile.Exists(path))
		{
			return;
		}
		try
		{
			using Stream baseStream = SdFile.OpenRead(path);
			using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader.SetBaseStream(baseStream);
			Read(pooledBinaryReader);
		}
		catch (Exception)
		{
			path = string.Format("{0}/{1}", GameIO.GetSaveGameDir(), "factions.dat.bak");
			if (!SdFile.Exists(path))
			{
				return;
			}
			using Stream baseStream2 = SdFile.OpenRead(path);
			using PooledBinaryReader pooledBinaryReader2 = MemoryPools.poolBinaryReader.AllocSync(_bReset: false);
			pooledBinaryReader2.SetBaseStream(baseStream2);
			Read(pooledBinaryReader2);
		}
	}
}
