using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockGameEvent : Block
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOnActivateEvent = "ActivateEvent";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOnDamageEvent = "DamageEvent";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOnTriggeredEvent = "TriggeredEvent";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOnAddedEvent = "AddedEvent";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDestroyOnEvent = "DestroyOnEvent";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSendDamageUpdate = "SendDamageUpdate";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string onActivateEvent = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string onDamageEvent = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string onTriggeredEvent = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string onAddedEvent = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool destroyOnEvent;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool sendDamageUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[2]
	{
		new BlockActivationCommand("activate", "electric_switch", _enabled: false),
		new BlockActivationCommand("trigger", "wrench", _enabled: true)
	};

	public override bool AllowBlockTriggers => true;

	public override void Init()
	{
		base.Init();
		base.Properties.ParseString(PropOnActivateEvent, ref onActivateEvent);
		base.Properties.ParseString(PropOnDamageEvent, ref onDamageEvent);
		base.Properties.ParseString(PropOnTriggeredEvent, ref onTriggeredEvent);
		base.Properties.ParseString(PropOnAddedEvent, ref onAddedEvent);
		base.Properties.ParseBool(PropDestroyOnEvent, ref destroyOnEvent);
		base.Properties.ParseBool(PropSendDamageUpdate, ref sendDamageUpdate);
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (onActivateEvent == "" && !_world.IsEditor())
		{
			return "";
		}
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string arg = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
		string arg2 = _blockValue.Block.GetLocalizedBlockName();
		return string.Format(Localization.Get("questBlockActivate"), arg, arg2);
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
		GameEventManager.Current.HandleAction(onAddedEvent, null, null, twitchActivated: false, _blockPos);
		if (destroyOnEvent)
		{
			DamageBlock(_world, 0, _blockPos, _blockValue, _blockValue.Block.MaxDamage - _blockValue.damage, -1);
		}
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (!(_commandName == "activate"))
		{
			if (_commandName == "trigger")
			{
				XUiC_TriggerProperties.Show(_player.PlayerUI.xui, _cIdx, _blockPos, _showTriggers: false, _showTriggeredBy: true);
				return true;
			}
			return false;
		}
		if (onActivateEvent != "")
		{
			GameEventManager.Current.HandleAction(onActivateEvent, null, _player, twitchActivated: false, _blockPos);
			if (destroyOnEvent)
			{
				DamageBlock(_world, _cIdx, _blockPos, _blockValue, _blockValue.Block.MaxDamage - _blockValue.damage, -1);
			}
		}
		return true;
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		cmds[0].enabled = !_world.IsEditor() && onActivateEvent != "";
		cmds[1].enabled = _world.IsEditor() && !GameUtils.IsWorldEditor();
		return cmds;
	}

	public override int OnBlockDamaged(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _damagePoints, int _entityIdThatDamaged, ItemActionAttack.AttackHitInfo _attackHitInfo, bool _bUseHarvestTool, bool _bBypassMaxDamage, int _recDepth = 0)
	{
		if (onDamageEvent != "")
		{
			if (GameEventManager.Current.GetTargetType(onDamageEvent) != GameEventActionSequence.TargetTypes.Block)
			{
				Debug.LogError("Game Event Target Type must be set to 'Block' to be used in BlockGameEvent.");
			}
			else
			{
				GameEventManager.Current.HandleAction(onDamageEvent, null, GameManager.Instance.World.GetEntity(_entityIdThatDamaged), twitchActivated: false, _blockPos);
				if (destroyOnEvent)
				{
					DamageBlock(_world, _clrIdx, _blockPos, _blockValue, _blockValue.Block.MaxDamage - _blockValue.damage, -1);
				}
			}
		}
		int num = base.OnBlockDamaged(_world, _clrIdx, _blockPos, _blockValue, _damagePoints, _entityIdThatDamaged, _attackHitInfo, _bUseHarvestTool, _bBypassMaxDamage, _recDepth);
		if (num > 0 && sendDamageUpdate && GameManager.Instance.World.GetEntity(_entityIdThatDamaged) is EntityPlayer)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				GameEventManager.Current.SendBlockDamageUpdate(_blockPos);
				return num;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(NetPackageGameEventResponse.ResponseTypes.BlockDamaged, _blockPos));
		}
		return num;
	}

	public override void OnTriggered(EntityPlayer _player, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		base.OnTriggered(_player, _world, _cIdx, _blockPos, _blockValue, _blockChanges, _triggeredBy);
		if (!(onTriggeredEvent != ""))
		{
			return;
		}
		if (GameEventManager.Current.GetTargetType(onTriggeredEvent) != GameEventActionSequence.TargetTypes.Block)
		{
			Debug.LogError("Game Event Target Type must be set to 'Block' to be used in BlockGameEvent.");
			return;
		}
		GameEventManager.Current.HandleAction(onTriggeredEvent, null, _player, twitchActivated: false, _blockPos);
		if (destroyOnEvent)
		{
			DamageBlock(_world, _cIdx, _blockPos, _blockValue, _blockValue.Block.MaxDamage - _blockValue.damage, -1);
		}
	}
}
