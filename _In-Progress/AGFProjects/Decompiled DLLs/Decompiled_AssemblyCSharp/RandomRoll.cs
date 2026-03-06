using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class RandomRoll : TargetedCompareRequirementBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum SeedType
	{
		Item,
		Player,
		Random
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 minMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public SeedType seedType;

	[PublicizedFrom(EAccessModifier.Private)]
	public int seedAdditive;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameRandom rand;

	public override bool IsValid(MinEventParams _params)
	{
		if (!base.IsValid(_params))
		{
			return false;
		}
		if (cvarName != null)
		{
			value = target.Buffs.GetCustomVar(cvarName);
		}
		if (seedType == SeedType.Item)
		{
			rand = GameRandomManager.Instance.CreateGameRandom(_params.Seed);
		}
		else if (seedType == SeedType.Player)
		{
			rand = GameRandomManager.Instance.CreateGameRandom(_params.Self.entityId);
		}
		else
		{
			rand = GameRandomManager.Instance.CreateGameRandom(Environment.TickCount);
		}
		float randomFloat = rand.RandomFloat;
		GameRandomManager.Instance.FreeGameRandom(rand);
		return invert != RequirementBase.compareValues(Utils.FastLerp(minMax.x, minMax.y, randomFloat), operation, value);
	}

	public override void GetInfoStrings(ref List<string> list)
	{
		list.Add($"roll[{minMax.x.ToCultureInvariantString()}-{minMax.y.ToCultureInvariantString()}] {operation.ToStringCached()} {value.ToCultureInvariantString()}");
	}

	public override bool ParseXAttribute(XAttribute _attribute)
	{
		bool flag = base.ParseXAttribute(_attribute);
		if (!flag)
		{
			switch (_attribute.Name.LocalName)
			{
			case "min_max":
				minMax = StringParsers.ParseVector2(_attribute.Value);
				return true;
			case "seed_type":
				seedType = EnumUtils.Parse<SeedType>(_attribute.Value, _ignoreCase: true);
				return true;
			case "seed_additive":
				seedAdditive = StringParsers.ParseSInt32(_attribute.Value);
				return true;
			}
		}
		return flag;
	}
}
