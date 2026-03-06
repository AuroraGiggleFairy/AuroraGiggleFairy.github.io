using System.Collections;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockQuestActivate : Block
{
	public const int cMetaSpawned = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public float activateTime;

	public FastTags<TagGroup.Global> ValidQuestTags = FastTags<TagGroup.Global>.none;

	public static string PropActivateTime = "ActivateTime";

	public static string PropQuestTags = "ValidQuestTags";

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[2]
	{
		new BlockActivationCommand("activate", "electric_switch", _enabled: false),
		new BlockActivationCommand("trigger", "wrench", _enabled: true)
	};

	public override bool AllowBlockTriggers => true;

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!_world.IsEditor())
		{
			if (_blockValue.meta2 != 1)
			{
				return "";
			}
			if (!QuestEventManager.Current.ActiveQuestBlocks.Contains(_blockPos))
			{
				return "";
			}
		}
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
		string arg2 = _blockValue.Block.GetLocalizedBlockName();
		return string.Format(Localization.Get("questBlockActivate"), arg, arg2);
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (!_world.IsEditor() && _blockValue.meta2 != 1)
		{
			return true;
		}
		if (!(_commandName == "activate"))
		{
			if (_commandName == "trigger")
			{
				XUiC_TriggerProperties.Show(_player.PlayerUI.xui, _cIdx, _blockPos, _showTriggers: true, _showTriggeredBy: false);
				return true;
			}
			return false;
		}
		if (!QuestEventManager.Current.ActiveQuestBlocks.Contains(_blockPos))
		{
			return false;
		}
		if (activateTime > 0f)
		{
			LocalPlayerUI playerUI = _player.PlayerUI;
			playerUI.windowManager.Open("timer", _bModal: true);
			XUiC_Timer childByType = playerUI.xui.GetChildByType<XUiC_Timer>();
			TimerEventData timerEventData = new TimerEventData();
			timerEventData.Data = new object[4] { _cIdx, _blockValue, _blockPos, _player };
			timerEventData.CloseOnHit = true;
			timerEventData.Event += EventData_Event;
			if ((_blockValue.meta & 1) == 0)
			{
				timerEventData.alternateTime = _player.rand.RandomRange(activateTime * 0.25f, activateTime * 0.5f);
				timerEventData.AlternateEvent += EventData_AlternateEvent;
			}
			timerEventData.CloseEvent += EventData_CloseEvent;
			childByType.SetTimer(activateTime, timerEventData);
			Manager.BroadcastPlay(_blockPos.ToVector3(), "generator_start");
			_blockValue.meta2 = 2;
			(_world as World).SetBlockRPC(_cIdx, _blockPos, _blockValue);
			setControllerState(_world, _cIdx, null, _blockPos, QuestGeneratorController.GeneratorStates.RebootState);
		}
		else
		{
			QuestEventManager.Current.BlockActivated(_blockValue.Block.GetBlockName(), _blockPos);
			HandleTrigger(_player, (World)_world, _cIdx, _blockPos, _blockValue);
			Manager.BroadcastPlay(_blockPos.ToVector3(), "generator_start");
			_blockValue.meta2 = 4;
			(_world as World).SetBlockRPC(_cIdx, _blockPos, _blockValue);
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventData_AlternateEvent(TimerEventData timerData)
	{
		object[] obj = (object[])timerData.Data;
		int clrIdx = (int)obj[0];
		BlockValue blockValue = (BlockValue)obj[1];
		Vector3i vector3i = (Vector3i)obj[2];
		EntityPlayer entityPlayer = (EntityPlayer)obj[3];
		World world = GameManager.Instance.World;
		GameEventManager.Current.HandleAction("quest_restorepower_generator", entityPlayer, entityPlayer, twitchActivated: false, vector3i);
		blockValue.meta |= 1;
		blockValue.meta2 = 1;
		world.SetBlockRPC(clrIdx, vector3i, blockValue);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventData_CloseEvent(TimerEventData timerData)
	{
		object[] obj = (object[])timerData.Data;
		int clrIdx = (int)obj[0];
		BlockValue blockValue = (BlockValue)obj[1];
		Vector3i blockPos = (Vector3i)obj[2];
		World world = GameManager.Instance.World;
		blockValue.meta2 = 1;
		world.SetBlockRPC(clrIdx, blockPos, blockValue);
		setControllerState(world, clrIdx, null, blockPos, QuestGeneratorController.GeneratorStates.Off);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventData_Event(TimerEventData timerData)
	{
		object[] obj = (object[])timerData.Data;
		int num = (int)obj[0];
		BlockValue blockValue = (BlockValue)obj[1];
		Vector3i blockPos = (Vector3i)obj[2];
		EntityPlayerLocal player = obj[3] as EntityPlayerLocal;
		World world = GameManager.Instance.World;
		QuestEventManager.Current.BlockActivated(blockValue.Block.GetBlockName(), blockPos);
		HandleTrigger(player, world, num, blockPos, blockValue);
		blockValue.meta2 = 4;
		world.SetBlockRPC(num, blockPos, blockValue);
		setControllerState(world, num, null, blockPos, QuestGeneratorController.GeneratorStates.On);
	}

	public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
		QuestEventManager.Current.BlockDestroyed(_blockValue.Block, _blockPos);
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		QuestGeneratorController.GeneratorStates meta = (QuestGeneratorController.GeneratorStates)_newBlockValue.meta2;
		if (meta == QuestGeneratorController.GeneratorStates.On)
		{
			QuestEventManager.Current.BlockActivated(_newBlockValue.Block.GetBlockName(), _blockPos);
		}
		setControllerState(_world, _clrIdx, _chunk, _blockPos, meta);
	}

	public override void Init()
	{
		base.Init();
		base.Properties.ParseFloat(PropActivateTime, ref activateTime);
		if (base.Properties.Values.ContainsKey(PropQuestTags))
		{
			ValidQuestTags = FastTags<TagGroup.Global>.Parse(base.Properties.Values[PropQuestTags]);
		}
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		cmds[0].enabled = !_world.IsEditor();
		cmds[1].enabled = _world.IsEditor() && !GameUtils.IsWorldEditor();
		return cmds;
	}

	public override void OnTriggerAddedFromPrefab(BlockTrigger _trigger, Vector3i _blockPos, BlockValue _blockValue, FastTags<TagGroup.Global> _questTags)
	{
		if (GameManager.Instance.World.IsEditor())
		{
			return;
		}
		World world = GameManager.Instance.World;
		base.OnTriggerAddedFromPrefab(_trigger, _blockPos, _blockValue, _questTags);
		if (!ValidQuestTags.IsEmpty)
		{
			if (_questTags.IsEmpty)
			{
				_blockValue.meta2 = 0;
				world.SetBlock(_trigger.Chunk.ClrIdx, _trigger.ToWorldPos(), _blockValue, bNotify: true, updateLight: false);
				setControllerState(world, _trigger.Chunk.ClrIdx, _trigger.Chunk, _trigger.LocalChunkPos, QuestGeneratorController.GeneratorStates.OnNoQuest);
			}
			else if (!_questTags.Test_AnySet(ValidQuestTags))
			{
				_blockValue.meta2 = 0;
				world.SetBlock(_trigger.Chunk.ClrIdx, _trigger.ToWorldPos(), _blockValue, bNotify: true, updateLight: false);
				setControllerState(world, _trigger.Chunk.ClrIdx, _trigger.Chunk, _trigger.LocalChunkPos, QuestGeneratorController.GeneratorStates.OnNoQuest);
			}
		}
	}

	public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
		ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
		if (chunkCluster != null && chunkCluster.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z)) is Chunk chunk)
		{
			BlockTrigger blockTrigger = chunk.GetBlockTrigger(World.toBlock(_blockPos));
			if (blockTrigger != null)
			{
				GameManager.Instance.StartCoroutine(resetTriggerLater(blockTrigger, _blockValue, FastTags<TagGroup.Global>.none));
			}
		}
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
		if (chunkCluster != null && chunkCluster.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z)) is Chunk chunk)
		{
			QuestGeneratorController.GeneratorStates meta = (QuestGeneratorController.GeneratorStates)_blockValue.meta2;
			setControllerState(_world, _cIdx, chunk, _blockPos, meta, isInit: true);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator resetTriggerLater(BlockTrigger _trigger, BlockValue _blockValue, FastTags<TagGroup.Global> _questTag)
	{
		EntityPlayerLocal player = GameManager.Instance.World.GetPrimaryPlayer();
		if (player != null)
		{
			while (!player.QuestJournal.CheckRallyMarkerActivation())
			{
				yield return new WaitForSeconds(0.1f);
			}
		}
		if (!_questTag.Test_AnySet(QuestEventManager.restorePowerTag))
		{
			if (_trigger.NeedsTriggered == BlockTrigger.TriggeredStates.NotTriggered || _trigger.NeedsTriggered == BlockTrigger.TriggeredStates.NeedsTriggered)
			{
				_trigger.NeedsTriggered = BlockTrigger.TriggeredStates.NeedsTriggered;
				if (_trigger.TriggerDataOwner != null && !_trigger.TriggerDataOwner.NeedsTriggerUpdate)
				{
					_trigger.TriggerDataOwner.NeedsTriggerUpdate = true;
				}
			}
		}
		else
		{
			_trigger.NeedsTriggered = BlockTrigger.TriggeredStates.HasTriggered;
		}
	}

	public override void OnTriggerRefresh(BlockTrigger _trigger, BlockValue _bv, FastTags<TagGroup.Global> _questTag)
	{
		base.OnTriggerRefresh(_trigger, _bv, _questTag);
		GameManager.Instance.StartCoroutine(resetTriggerLater(_trigger, _bv, _questTag));
	}

	public void SetupForQuest(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> blockChanges)
	{
		if (!_world.IsEditor() && !_blockValue.ischild)
		{
			_blockValue.meta2 = 1;
			blockChanges.Add(new BlockChangeInfo(_chunk.ClrIdx, _blockPos, _blockValue));
			setControllerState(_world, _chunk.ClrIdx, _chunk, _blockPos, QuestGeneratorController.GeneratorStates.Off);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setControllerState(WorldBase _world, int _clrIdx, Chunk _chunk, Vector3i _blockPos, QuestGeneratorController.GeneratorStates generatorState, bool isInit = false)
	{
		if (_chunk == null)
		{
			ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
			if (chunkCluster == null)
			{
				return;
			}
			_chunk = chunkCluster.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z)) as Chunk;
			if (_chunk == null)
			{
				return;
			}
		}
		BlockEntityData blockEntity = _chunk.GetBlockEntity(_blockPos);
		if (blockEntity != null && blockEntity.bHasTransform)
		{
			QuestGeneratorController component = blockEntity.transform.GetComponent<QuestGeneratorController>();
			if (component != null)
			{
				component.SetGeneratorState(generatorState, isInit);
			}
		}
	}
}
