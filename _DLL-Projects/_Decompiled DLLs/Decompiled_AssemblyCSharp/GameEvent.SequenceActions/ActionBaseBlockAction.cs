using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionBaseBlockAction : BaseAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3i minOffset = Vector3i.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3i maxOffset = Vector3i.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string blockTags;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string excludeTags;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int spacing;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int innerOffset = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool safeAllowed;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool checkSafe;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool allowTerrain;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float randomChance = -1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int maxCount = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBlockTags = "block_tags";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropExcludeTags = "exclude_tags";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMinOffset = "min_offset";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMaxOffset = "max_offset";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSpacing = "spacing";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRandomChance = "random_chance";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSafeAllowed = "safe_allowed";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropInnerOffset = "inner_offset";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAllowTerrain = "allow_terrain";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropMaxCount = "max_count";

	[PublicizedFrom(EAccessModifier.Protected)]
	public GameRandom random;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 startPoint = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool NeedsDamage()
	{
		return false;
	}

	public override ActionCompleteStates OnPerformAction()
	{
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		startPoint = ((base.Owner.TargetPosition.y != 0f) ? base.Owner.TargetPosition : base.Owner.Target.position);
		World world = GameManager.Instance.World;
		random = world.GetGameRandom();
		FastTags<TagGroup.Global> other = ((blockTags != null) ? FastTags<TagGroup.Global>.Parse(blockTags) : FastTags<TagGroup.Global>.none);
		FastTags<TagGroup.Global> other2 = ((excludeTags != null) ? FastTags<TagGroup.Global>.Parse(excludeTags) : FastTags<TagGroup.Global>.none);
		IChunk _chunk = null;
		if (base.Owner.Target != null && !base.Owner.Target.onGround)
		{
			return ActionCompleteStates.InComplete;
		}
		for (int i = minOffset.y; i <= maxOffset.y; i++)
		{
			for (int j = minOffset.z; j <= maxOffset.z; j += spacing + 1)
			{
				int num = (int)Utils.FastAbs(j);
				for (int k = minOffset.x; k <= maxOffset.x; k += spacing + 1)
				{
					if ((innerOffset != -1 && Utils.FastAbs(k) <= (float)innerOffset && num <= innerOffset) || (randomChance > 0f && random.RandomFloat > randomChance))
					{
						continue;
					}
					Vector3i vector3i = new Vector3i(Utils.Fastfloor(startPoint.x + (float)k), Utils.Fastfloor(startPoint.y + (float)i), Utils.Fastfloor(startPoint.z + (float)j));
					if (vector3i.y < 0 || !world.GetChunkFromWorldPos(vector3i, ref _chunk) || world.GetTraderAreaAt(vector3i) != null || (checkSafe && (safeAllowed || !world.CanPlaceBlockAt(vector3i, null))))
					{
						continue;
					}
					int x = World.toBlockXZ(vector3i.x);
					int z = World.toBlockXZ(vector3i.z);
					BlockValue blockValue = (NeedsDamage() ? _chunk.GetBlock(x, vector3i.y, z) : _chunk.GetBlockNoDamage(x, vector3i.y, z));
					if (!blockValue.ischild && (allowTerrain || !blockValue.Block.shape.IsTerrain()) && (blockTags == null || blockValue.Block.Tags.Test_AnySet(other)) && (excludeTags == null || !blockValue.Block.Tags.Test_AnySet(other2)) && CheckValid(world, vector3i))
					{
						BlockChangeInfo blockChangeInfo = UpdateBlock(world, vector3i, blockValue);
						if (blockChangeInfo != null)
						{
							list.Add(blockChangeInfo);
						}
					}
				}
			}
		}
		if (list.Count > 0)
		{
			if (maxCount != -1 && maxCount < list.Count)
			{
				int num2 = list.Count - maxCount;
				for (int l = 0; l < num2; l++)
				{
					list.RemoveAt(random.RandomRange(list.Count));
				}
			}
			ChangesComplete();
			ProcessChanges(world, list);
			return ActionCompleteStates.Complete;
		}
		return ActionCompleteStates.InCompleteRefund;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void ProcessChanges(World world, List<BlockChangeInfo> blockChanges)
	{
		world.SetBlocksRPC(blockChanges);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void ChangesComplete()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool CheckValid(World world, Vector3i currentPos)
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual BlockChangeInfo UpdateBlock(World world, Vector3i currentPos, BlockValue blockValue)
	{
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator UpdateBlocks(List<BlockChangeInfo> blockChanges)
	{
		yield return new WaitForSeconds(0.5f);
		GameManager.Instance.World.SetBlocksRPC(blockChanges);
	}

	public override BaseAction Clone()
	{
		ActionBaseBlockAction obj = base.Clone() as ActionBaseBlockAction;
		obj.minOffset = minOffset;
		obj.maxOffset = maxOffset;
		obj.spacing = spacing;
		obj.randomChance = randomChance;
		obj.safeAllowed = safeAllowed;
		obj.checkSafe = checkSafe;
		obj.blockTags = blockTags;
		obj.excludeTags = excludeTags;
		obj.innerOffset = innerOffset;
		obj.allowTerrain = allowTerrain;
		obj.maxCount = maxCount;
		return obj;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseVec(PropMinOffset, ref minOffset);
		properties.ParseVec(PropMaxOffset, ref maxOffset);
		properties.ParseInt(PropSpacing, ref spacing);
		properties.ParseInt(PropInnerOffset, ref innerOffset);
		properties.ParseFloat(PropRandomChance, ref randomChance);
		if (properties.Contains(PropSafeAllowed))
		{
			properties.ParseBool(PropSafeAllowed, ref safeAllowed);
			checkSafe = true;
		}
		properties.ParseString(PropBlockTags, ref blockTags);
		properties.ParseString(PropExcludeTags, ref excludeTags);
		properties.ParseBool(PropAllowTerrain, ref allowTerrain);
		properties.ParseInt(PropMaxCount, ref maxCount);
	}
}
