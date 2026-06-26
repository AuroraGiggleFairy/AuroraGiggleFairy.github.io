using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionBlockHealth : ActionBaseBlockAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public enum HealthStates
	{
		OneHealth,
		Half,
		Full,
		Remove,
		RemoveNoBreak
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public HealthStates healthState = HealthStates.Full;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int amount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropHealthState = "health_state";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropHealthAmount = "health_amount";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool NeedsDamage()
	{
		if (healthState != HealthStates.Remove)
		{
			return healthState == HealthStates.RemoveNoBreak;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BlockChangeInfo UpdateBlock(World world, Vector3i currentPos, BlockValue blockValue)
	{
		if (!blockValue.isair)
		{
			switch (healthState)
			{
			case HealthStates.Full:
				if (blockValue.damage != 0)
				{
					blockValue.damage = 0;
					return new BlockChangeInfo(0, currentPos, blockValue);
				}
				break;
			case HealthStates.Half:
			{
				int num4 = blockValue.Block.MaxDamage / 2;
				if (blockValue.damage != num4)
				{
					blockValue.damage = num4;
					return new BlockChangeInfo(0, currentPos, blockValue);
				}
				break;
			}
			case HealthStates.OneHealth:
			{
				int num3 = blockValue.Block.MaxDamage - 1;
				if (blockValue.damage != num3)
				{
					blockValue.damage = num3;
					return new BlockChangeInfo(0, currentPos, blockValue);
				}
				break;
			}
			case HealthStates.Remove:
			{
				int num2 = blockValue.damage + amount;
				if (blockValue.damage != num2)
				{
					blockValue.damage = num2;
					if (blockValue.damage >= blockValue.Block.MaxDamage)
					{
						blockValue = blockValue.Block.DowngradeBlock;
					}
					return new BlockChangeInfo(0, currentPos, blockValue);
				}
				break;
			}
			case HealthStates.RemoveNoBreak:
			{
				int num = Mathf.Min(blockValue.Block.MaxDamage - 1, blockValue.damage + amount);
				if (blockValue.damage != num)
				{
					blockValue.damage = num;
					return new BlockChangeInfo(0, currentPos, blockValue);
				}
				break;
			}
			}
		}
		return null;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		Properties.ParseEnum(PropHealthState, ref healthState);
		Properties.ParseInt(PropHealthAmount, ref amount);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionBlockHealth
		{
			healthState = healthState,
			amount = amount
		};
	}
}
