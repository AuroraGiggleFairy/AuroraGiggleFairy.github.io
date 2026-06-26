using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockHazard : BlockParticle
{
	public const int cMetaOriginalState = 1;

	public const int cMetaOn = 2;

	public Vector3 DamageOffset = Vector3.zero;

	public Vector3 DamageSize = Vector3.one;

	public Vector3 SecondaryOffset = Vector3.zero;

	public Vector3 SecondarySize = Vector3.one;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> buffActions;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> buffSecondaryActions;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDamageOffset = "DamageOffset";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDamageSize = "DamageSize";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDamageBuffs = "DamageBuffs";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSecondaryOffset = "SecondaryOffset";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSecondarySize = "SecondarySize";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSecondaryBuffs = "SecondaryBuffs";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropStartSound = "StartSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropStopSound = "StopSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string StartSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string StopSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[2]
	{
		new BlockActivationCommand("light", "electric_switch", _enabled: true),
		new BlockActivationCommand("trigger", "wrench", _enabled: true)
	};

	public override bool AllowBlockTriggers => true;

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey(PropDamageBuffs))
		{
			if (buffActions == null)
			{
				buffActions = new List<string>();
			}
			string[] array = base.Properties.Values[PropDamageBuffs].Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				buffActions.Add(array[i]);
			}
		}
		base.Properties.ParseVec(PropDamageOffset, ref DamageOffset);
		base.Properties.ParseVec(PropDamageSize, ref DamageSize);
		if (base.Properties.Values.ContainsKey(PropSecondaryBuffs))
		{
			if (buffSecondaryActions == null)
			{
				buffSecondaryActions = new List<string>();
			}
			string[] array2 = base.Properties.Values[PropSecondaryBuffs].Split(',');
			for (int j = 0; j < array2.Length; j++)
			{
				buffSecondaryActions.Add(array2[j]);
			}
		}
		base.Properties.ParseVec(PropSecondaryOffset, ref SecondaryOffset);
		base.Properties.ParseVec(PropSecondarySize, ref SecondarySize);
		if (base.Properties.Values.ContainsKey("Model"))
		{
			DataLoader.PreloadBundle(base.Properties.Values["Model"]);
		}
		base.Properties.ParseString(PropStartSound, ref StartSound);
		base.Properties.ParseString(PropStopSound, ref StopSound);
	}

	public override byte GetLightValue(BlockValue _blockValue)
	{
		if ((_blockValue.meta & 2) == 0)
		{
			return 0;
		}
		return base.GetLightValue(_blockValue);
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (_world.IsEditor())
		{
			PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
			string arg = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
			if ((_blockValue.meta & 2) != 0)
			{
				return string.Format(Localization.Get("useSwitchLightOff"), arg);
			}
			return string.Format(Localization.Get("useSwitchLightOn"), arg);
		}
		return null;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (!(_commandName == "light"))
		{
			if (_commandName == "trigger")
			{
				XUiC_TriggerProperties.Show(_player.PlayerUI.xui, _cIdx, _blockPos, _showTriggers: false, _showTriggeredBy: true);
			}
		}
		else if (_world.IsEditor() && toggleHazardStateForEditor(_world, _cIdx, _blockPos, _blockValue))
		{
			return true;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool toggleHazardStateForEditor(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		bool flag = (_blockValue.meta & 2) != 0;
		flag = !flag;
		_blockValue.meta = (byte)((_blockValue.meta & -3) | (flag ? 2 : 0));
		_blockValue.meta = (byte)((_blockValue.meta & -2) | (flag ? 1 : 0));
		_world.SetBlockRPC(_cIdx, _blockPos, _blockValue);
		return true;
	}

	public bool IsHazardOn(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (isMultiBlock && _blockValue.ischild)
		{
			Vector3i parentPos = multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return IsHazardOn(_world, parentPos, block);
		}
		return (_blockValue.meta & 2) != 0;
	}

	public bool OriginalHazardState(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (isMultiBlock && _blockValue.ischild)
		{
			Vector3i parentPos = multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return IsHazardOn(_world, parentPos, block);
		}
		return (_blockValue.meta & 1) != 0;
	}

	public BlockValue SetHazardState(BlockValue _blockValue, bool isOn)
	{
		_blockValue.meta = (byte)((_blockValue.meta & -3) | (isOn ? 2 : 0));
		return _blockValue;
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		if (!_newBlockValue.ischild)
		{
			IsHazardOn(_world, _blockPos, _newBlockValue);
			OriginalHazardState(_world, _blockPos, _newBlockValue);
			base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
			updateHazardState(_world, _chunk, _clrIdx, _blockPos, _newBlockValue);
			checkParticles(_world, _clrIdx, _blockPos, _newBlockValue);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateHazardState(WorldBase _world, Chunk _chunk, int _cIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		IChunk chunk = _chunk;
		if (chunk == null)
		{
			ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
			if (chunkCluster == null)
			{
				return false;
			}
			chunk = chunkCluster.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z));
			if (chunk == null)
			{
				return false;
			}
		}
		if (chunk == null)
		{
			return false;
		}
		BlockEntityData blockEntity = chunk.GetBlockEntity(_blockPos);
		if (blockEntity == null || !blockEntity.bHasTransform)
		{
			return false;
		}
		Transform transform = blockEntity.transform.Find("HazardDamage");
		if (transform == null)
		{
			GameObject gameObject = new GameObject("HazardDamage");
			gameObject.AddComponent<HazardDamageController>();
			transform = gameObject.transform;
			gameObject.AddComponent<BoxCollider>().isTrigger = true;
			transform.SetParent(blockEntity.transform);
		}
		transform.GetComponent<BoxCollider>().size = DamageSize;
		transform.localPosition = DamageOffset;
		transform.localRotation = Quaternion.identity;
		HazardDamageController component = transform.GetComponent<HazardDamageController>();
		if ((bool)component)
		{
			component.IsActive = IsHazardOn(_world, _blockPos, _blockValue);
			component.buffActions = buffActions;
		}
		if (buffSecondaryActions != null && buffSecondaryActions.Count != 0)
		{
			transform = blockEntity.transform.Find("SecondaryDamage");
			if (transform == null)
			{
				GameObject gameObject2 = new GameObject("SecondaryDamage");
				gameObject2.AddComponent<HazardDamageController>();
				transform = gameObject2.transform;
				gameObject2.AddComponent<BoxCollider>().isTrigger = true;
				transform.SetParent(blockEntity.transform);
			}
			transform.GetComponent<BoxCollider>().size = SecondarySize;
			transform.localPosition = SecondaryOffset;
			transform.localRotation = Quaternion.identity;
			component = transform.GetComponent<HazardDamageController>();
			if ((bool)component)
			{
				component.IsActive = IsHazardOn(_world, _blockPos, _blockValue);
				component.buffActions = buffSecondaryActions;
			}
		}
		return true;
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		if (!_blockValue.ischild)
		{
			base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
			updateHazardState(_world, null, _cIdx, _blockPos, _blockValue);
			checkParticles(_world, _cIdx, _blockPos, _blockValue);
		}
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		cmds[0].enabled = _world.IsEditor();
		cmds[1].enabled = _world.IsEditor() && !GameUtils.IsWorldEditor();
		return cmds;
	}

	public override void OnBlockReset(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (!_blockValue.ischild && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			bool flag = IsHazardOn(_world, _blockPos, _blockValue);
			bool flag2 = OriginalHazardState(_world, _blockPos, _blockValue);
			if (flag2 != flag)
			{
				_blockValue = SetHazardState(_blockValue, flag2);
				_world.SetBlockRPC(_chunk.ClrIdx, _blockPos, _blockValue);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void checkParticles(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (!_blockValue.ischild)
		{
			bool flag = _world.GetGameManager().HasBlockParticleEffect(_blockPos);
			if (IsHazardOn(_world, _blockPos, _blockValue) && !flag)
			{
				addParticles(_world, _clrIdx, _blockPos.x, _blockPos.y, _blockPos.z, _blockValue);
			}
			else if (!IsHazardOn(_world, _blockPos, _blockValue) && flag)
			{
				removeParticles(_world, _blockPos.x, _blockPos.y, _blockPos.z, _blockValue);
			}
		}
	}

	public override void OnTriggered(EntityPlayer _player, WorldBase _world, int cIdx, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		base.OnTriggered(_player, _world, cIdx, _blockPos, _blockValue, _blockChanges, _triggeredBy);
		bool flag = !IsHazardOn(_world, _blockPos, _blockValue);
		if (flag)
		{
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, StartSound);
		}
		else
		{
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, StopSound);
		}
		_blockValue = SetHazardState(_blockValue, flag);
		_blockChanges.Add(new BlockChangeInfo(cIdx, _blockPos, _blockValue));
	}

	public override int OnBlockDamaged(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _damagePoints, int _entityIdThatDamaged, ItemActionAttack.AttackHitInfo _attackHitInfo, bool _bUseHarvestTool, bool _bBypassMaxDamage, int _recDepth = 0)
	{
		if (IsHazardOn(_world, _blockPos, _blockValue) && _damagePoints > 0 && buffActions != null && buffActions.Count > 0)
		{
			EntityAlive entityAlive = _world.GetEntity(_entityIdThatDamaged) as EntityAlive;
			if (entityAlive != null && entityAlive as EntityTurret == null)
			{
				ItemAction itemAction = entityAlive.inventory.holdingItemData.item.Actions[0];
				if ((object)entityAlive != null && (!(itemAction is ItemActionRanged) || (itemAction is ItemActionRanged itemActionRanged && (itemActionRanged.Hitmask & 0x80) != 0)))
				{
					for (int i = 0; i < buffActions.Count; i++)
					{
						entityAlive.Buffs.AddBuff(buffActions[i], _blockPos, entityAlive.entityId);
					}
				}
			}
		}
		return base.OnBlockDamaged(_world, _clrIdx, _blockPos, _blockValue, _damagePoints, _entityIdThatDamaged, _attackHitInfo, _bUseHarvestTool, _bBypassMaxDamage, _recDepth);
	}
}
