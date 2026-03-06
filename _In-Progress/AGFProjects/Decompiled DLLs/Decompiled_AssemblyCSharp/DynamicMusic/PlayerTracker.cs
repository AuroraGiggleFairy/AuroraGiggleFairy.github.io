using System.Collections.Generic;
using MusicUtils.Enums;
using UnityEngine;

namespace DynamicMusic;

public class PlayerTracker : AbstractFilter, IFilter<SectionType>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const float cMaxHomeDistance = 50f;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Entity> traders = new List<Entity>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 boundingBoxRange = new Vector3(200f, 200f, 200f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Entity> npcs = new List<Entity>();

	public EntityPlayerLocal player
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameManager.Instance.World.GetPrimaryPlayer();
		}
	}

	public bool isPlayerInTraderArea
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			TraderArea traderAreaAt = GameManager.Instance.World.GetTraderAreaAt(player.GetBlockPosition());
			if (traderAreaAt == null)
			{
				return false;
			}
			return IsTraderAreaOpen(traderAreaAt);
		}
	}

	public bool isPlayerHome
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!player.GetSpawnPoint().IsUndef())
			{
				return (player.GetSpawnPoint().position - player.position).magnitude < 50f;
			}
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsTraderAreaOpen(TraderArea _ta)
	{
		Vector3 center = _ta.Position.ToVector3() + _ta.PrefabSize.ToVector3() / 2f;
		Bounds bb = new Bounds(center, _ta.PrefabSize.ToVector3());
		GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityTrader), bb, traders);
		if (traders.Count <= 0)
		{
			return false;
		}
		EntityTrader entityTrader = traders[0] as EntityTrader;
		if (entityTrader.TraderInfo == null)
		{
			return false;
		}
		traders.Clear();
		return entityTrader.TraderInfo.IsOpen;
	}

	public override List<SectionType> Filter(List<SectionType> _sectionTypes)
	{
		if (player != null && player.IsAlive())
		{
			if (isPlayerInTraderArea)
			{
				_sectionTypes.Clear();
				_sectionTypes.Add(determineTrader());
				return _sectionTypes;
			}
			_sectionTypes.Remove(SectionType.TraderBob);
			_sectionTypes.Remove(SectionType.TraderHugh);
			_sectionTypes.Remove(SectionType.TraderJen);
			_sectionTypes.Remove(SectionType.TraderJoel);
			_sectionTypes.Remove(SectionType.TraderRekt);
			switch (player.ThreatLevel.Category)
			{
			case ThreatLevelType.Safe:
				_sectionTypes.Remove(SectionType.Suspense);
				_sectionTypes.Remove(SectionType.Combat);
				_sectionTypes.Remove(SectionType.Bloodmoon);
				if (isPlayerHome)
				{
					_sectionTypes.Remove(SectionType.Exploration);
					break;
				}
				_sectionTypes.Remove(SectionType.HomeDay);
				_sectionTypes.Remove(SectionType.HomeNight);
				if (isPlayerInPOI())
				{
					_sectionTypes.Remove(SectionType.Exploration);
				}
				break;
			case ThreatLevelType.Spooked:
				_sectionTypes.Remove(SectionType.HomeDay);
				_sectionTypes.Remove(SectionType.HomeNight);
				_sectionTypes.Remove(SectionType.Exploration);
				_sectionTypes.Remove(SectionType.Combat);
				_sectionTypes.Remove(SectionType.Bloodmoon);
				break;
			case ThreatLevelType.Panicked:
				_sectionTypes.Clear();
				_sectionTypes.Add(SectionType.Combat);
				_sectionTypes.Add(SectionType.Bloodmoon);
				break;
			}
		}
		else
		{
			_sectionTypes.Clear();
			_sectionTypes.Add(SectionType.None);
		}
		return _sectionTypes;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPlayerInPOI()
	{
		if (player.prefab != null || GamePrefs.GetString(EnumGamePrefs.GameWorld).Equals("Playtesting"))
		{
			return player.PlayerStats.LightInsidePer > 0.2f;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SectionType determineTrader()
	{
		npcs.Clear();
		GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityTrader), new Bounds(player.position, boundingBoxRange), npcs);
		if (npcs.Count > 0)
		{
			EntityTrader entityTrader = npcs[0] as EntityTrader;
			if (entityTrader != null)
			{
				return entityTrader.NPCInfo?.DmsSectionType ?? SectionType.None;
			}
		}
		return SectionType.None;
	}

	public override string ToString()
	{
		return "PlayerTracker:\n" + $"Is Player in a trader station: {isPlayerInTraderArea}\n" + $"Is Player home: {isPlayerHome}\n" + $"Player Threat Level: {player.ThreatLevel.Category}";
	}
}
