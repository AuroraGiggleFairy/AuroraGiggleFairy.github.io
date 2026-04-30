using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionBlockAnimateBlock : ActionBaseBlockAction
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public string animationBool;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string animationInteger;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string animationTrigger;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool animationBoolValue = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int animationIntegerValue;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAnimationBool = "animation_bool";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAnimationBoolValue = "animation_bool_value";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAnimationInteger = "animation_integer";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAnimationIntegerValue = "animation_integer_value";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropAnimationTrigger = "animation_trigger";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BlockChangeInfo UpdateBlock(World world, Vector3i currentPos, BlockValue blockValue)
	{
		if (!blockValue.isair)
		{
			return new BlockChangeInfo(0, currentPos, blockValue, _updateLight: true);
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ProcessChanges(World world, List<BlockChangeInfo> blockChanges)
	{
		for (int i = 0; i < blockChanges.Count; i++)
		{
			BlockChangeInfo blockChangeInfo = blockChanges[i];
			Chunk chunk = (Chunk)world.GetChunkFromWorldPos(blockChangeInfo.pos);
			if (chunk == null)
			{
				continue;
			}
			BlockEntityData blockEntity = world.ChunkClusters[chunk.ClrIdx].GetBlockEntity(blockChangeInfo.pos);
			if (blockEntity != null)
			{
				if (blockEntity.transform == null)
				{
					GameManager.Instance.StartCoroutine(WaitForBEDTransform(blockEntity));
				}
				else
				{
					AnimateBlock(blockEntity);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator WaitForBEDTransform(BlockEntityData bed)
	{
		for (int frames = 0; frames < 10; frames++)
		{
			yield return 0;
			if (bed == null)
			{
				break;
			}
			if (bed.transform != null)
			{
				AnimateBlock(bed);
				break;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AnimateBlock(BlockEntityData bed)
	{
		Animator[] componentsInChildren = bed.transform.GetComponentsInChildren<Animator>();
		if (componentsInChildren == null)
		{
			return;
		}
		for (int num = componentsInChildren.Length - 1; num >= 0; num--)
		{
			Animator animator = componentsInChildren[num];
			animator.enabled = true;
			if (animationBool != null)
			{
				animator.SetBool(animationBool, animationBoolValue);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageAnimateBlock>().Setup(bed.pos, animationBool, animationBoolValue));
			}
			if (animationInteger != null)
			{
				animator.SetInteger(animationInteger, animationIntegerValue);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageAnimateBlock>().Setup(bed.pos, animationInteger, animationIntegerValue));
			}
			if (animationTrigger != null)
			{
				animator.SetTrigger(animationTrigger);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageAnimateBlock>().Setup(bed.pos, animationTrigger));
			}
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(PropAnimationBool, ref animationBool);
		properties.ParseBool(PropAnimationBoolValue, ref animationBoolValue);
		properties.ParseString(PropAnimationInteger, ref animationInteger);
		properties.ParseInt(PropAnimationIntegerValue, ref animationIntegerValue);
		properties.ParseString(PropAnimationTrigger, ref animationTrigger);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionBlockAnimateBlock
		{
			animationBool = animationBool,
			animationBoolValue = animationBoolValue,
			animationInteger = animationInteger,
			animationIntegerValue = animationIntegerValue,
			animationTrigger = animationTrigger
		};
	}
}
