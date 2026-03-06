using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class QuestTraderData
{
	public QuestJournal Owner;

	public Vector2 TraderPOI;

	public List<Vector2> TradersSentTo = new List<Vector2>();

	public Dictionary<int, List<Vector2>> CompletedPOIByTier = new Dictionary<int, List<Vector2>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int resetDay = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int resetStartTier = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int fullTierCount = 6;

	public QuestTraderData()
	{
	}

	public QuestTraderData(Vector2 traderPOI)
	{
		TraderPOI = traderPOI;
	}

	public void AddPOI(int tier, Vector2 poiPosition)
	{
		if (!CompletedPOIByTier.ContainsKey(tier))
		{
			CompletedPOIByTier.Add(tier, new List<Vector2>());
		}
		if (!CompletedPOIByTier[tier].Contains(poiPosition))
		{
			CompletedPOIByTier[tier].Add(poiPosition);
		}
		if (resetDay == -1)
		{
			resetDay = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
		}
	}

	public void ClearTier(int tier)
	{
		if (tier == -1)
		{
			resetDay = -1;
			for (int i = resetStartTier; i <= fullTierCount; i++)
			{
				if (CompletedPOIByTier.ContainsKey(i))
				{
					CompletedPOIByTier.Remove(i);
				}
			}
		}
		else if (CompletedPOIByTier.ContainsKey(tier))
		{
			CompletedPOIByTier.Remove(tier);
		}
	}

	public void CheckReset(EntityPlayer player)
	{
		if (resetDay == -1 || GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime) - resetDay < 7)
		{
			return;
		}
		resetDay = -1;
		for (int i = resetStartTier; i <= fullTierCount; i++)
		{
			if (CompletedPOIByTier.ContainsKey(i))
			{
				CompletedPOIByTier.Remove(i);
			}
		}
		if (!(player is EntityPlayerLocal))
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageNPCQuestList>().SetupClear(player.entityId, TraderPOI, -1), _onlyClientsAttachedToAnEntity: false, player.entityId);
		}
	}

	public List<Vector2> GetTierPOIs(int tier)
	{
		if (CompletedPOIByTier.ContainsKey(tier))
		{
			return CompletedPOIByTier[tier];
		}
		return null;
	}

	public void Read(BinaryReader _br, byte version)
	{
		TraderPOI = StreamUtils.ReadVector2(_br);
		int num = _br.ReadByte();
		CompletedPOIByTier.Clear();
		for (int i = 0; i < num; i++)
		{
			int key = _br.ReadByte();
			int num2 = _br.ReadInt32();
			if (num2 > 0)
			{
				List<Vector2> list = new List<Vector2>();
				for (int j = 0; j < num2; j++)
				{
					list.Add(StreamUtils.ReadVector2(_br));
				}
				CompletedPOIByTier.Add(key, list);
			}
		}
		int num3 = _br.ReadByte();
		TradersSentTo.Clear();
		for (int k = 0; k < num3; k++)
		{
			TradersSentTo.Add(StreamUtils.ReadVector2(_br));
		}
		resetDay = _br.ReadInt32();
	}

	public void Write(BinaryWriter _bw)
	{
		StreamUtils.Write(_bw, TraderPOI);
		_bw.Write((byte)CompletedPOIByTier.Count);
		foreach (int key in CompletedPOIByTier.Keys)
		{
			_bw.Write((byte)key);
			List<Vector2> list = CompletedPOIByTier[key];
			_bw.Write(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				StreamUtils.Write(_bw, list[i]);
			}
		}
		_bw.Write((byte)TradersSentTo.Count);
		for (int j = 0; j < TradersSentTo.Count; j++)
		{
			StreamUtils.Write(_bw, TradersSentTo[j]);
		}
		_bw.Write(resetDay);
	}
}
