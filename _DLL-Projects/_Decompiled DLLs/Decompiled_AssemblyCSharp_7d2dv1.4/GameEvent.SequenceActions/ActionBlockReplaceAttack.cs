using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions;

[Preserve]
public class ActionBlockReplaceAttack : ActionBlockReplace
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<Vector3i> blocksAdded = new List<Vector3i>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public float timeAlive = -1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string removeSound = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool refundOnRemove;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTimeAlive = "time_alive";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRemoveSound = "remove_sound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropRefundOnRemove = "refund_on_remove";

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BlockChangeInfo UpdateBlock(World world, Vector3i currentPos, BlockValue blockValue)
	{
		if (blockTo == null)
		{
			return null;
		}
		if (emptyOnly && !blockValue.isair)
		{
			return null;
		}
		if (!blockValue.Block.blockMaterial.CanDestroy)
		{
			return null;
		}
		BlockValue blockValue2 = Block.GetBlockValue(blockTo[random.RandomRange(0, blockTo.Length)]);
		if (blockValue.type != blockValue2.type)
		{
			if (!blockValue2.isair)
			{
				if (blocksAdded == null)
				{
					blocksAdded = new List<Vector3i>();
				}
				blocksAdded.Add(currentPos);
			}
			return new BlockChangeInfo(0, currentPos, blockValue2, _updateLight: true);
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ChangesComplete()
	{
		base.ChangesComplete();
		if (blocksAdded == null)
		{
			return;
		}
		GameEventManager.SpawnedBlocksEntry spawnedBlocksEntry = GameEventManager.Current.RegisterSpawnedBlocks(blocksAdded, base.Owner.Target, base.Owner.Requester, base.Owner, timeAlive, removeSound, (base.Owner.Target != null) ? base.Owner.Target.position : base.Owner.TargetPosition, refundOnRemove);
		if (base.Owner.Requester != null)
		{
			if (base.Owner.Requester is EntityPlayerLocal)
			{
				GameEventManager.Current.HandleGameBlocksAdded(base.Owner.Name, spawnedBlocksEntry.BlockGroupID, blocksAdded, base.Owner.Tag);
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(NetPackageGameEventResponse.ResponseTypes.BlocksAdded, base.Owner.Name, spawnedBlocksEntry.BlockGroupID, blocksAdded, base.Owner.Tag), _onlyClientsAttachedToAnEntity: false, base.Owner.Requester.entityId);
			}
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseFloat(PropTimeAlive, ref timeAlive);
		properties.ParseString(PropRemoveSound, ref removeSound);
		properties.ParseBool(PropRefundOnRemove, ref refundOnRemove);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BaseAction CloneChildSettings()
	{
		return new ActionBlockReplaceAttack
		{
			blockTo = blockTo,
			emptyOnly = emptyOnly,
			timeAlive = timeAlive,
			removeSound = removeSound,
			refundOnRemove = refundOnRemove
		};
	}
}
