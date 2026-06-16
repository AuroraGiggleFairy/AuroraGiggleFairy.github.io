using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureCombine : TEFeatureAbs, IFeatureSavedInPrefab
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropOpenSound = "OpenSound";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropCloseSound = "CloseSound";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropCombineCompleteSound = "CombineCompleteSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string openSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string closeSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string combineCompleteSound;

	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		DynamicProperties props = _featureData.Props;
		props.ParseString(PropOpenSound, ref openSound);
		props.ParseString(PropCloseSound, ref closeSound);
		props.ParseString(PropCombineCompleteSound, ref combineCompleteSound);
	}

	public override string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
	{
		base.GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, _activateHotkeyMarkup, _focusedTileEntityName);
		return _blockValue.Block.GetLocalizedBlockName() + "\n" + Localization.Get("useWorkstation");
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
		_addCallback(new BlockActivationCommand("Open", "campfire", _enabled: false), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
	}

	public override bool AllowBlockActivationCommand(ITileEntityFeature _module, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		if (!base.AllowBlockActivationCommand(_module, _commandName, _world, _blockPos, _blockValue, _entityFocusing))
		{
			return false;
		}
		Equals(_module);
		return true;
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		if (CommandIs(_commandName, "Open"))
		{
			_player.AimingGun = false;
			LockManager.Instance.LockRequestLocal(this, null, 0);
			return true;
		}
		return false;
	}

	public override void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
		ShowUI(_success);
	}

	public override void OnUnlockedServer(int _unlockingPlayerID, ushort _channel)
	{
		base.OnUnlockedServer(_unlockingPlayerID, _channel);
		Manager.BroadcastPlayByLocalPlayer(ToWorldPos().ToVector3() + Vector3.one * 0.5f, closeSound);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowUI(bool _lockGranted)
	{
		LocalPlayerUI uIForPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (!_lockGranted)
		{
			GameManager.ShowTooltip(uIForPrimaryPlayer.entityPlayer, Localization.Get("ttNoInteractItem"), string.Empty, "ui_denied");
			return;
		}
		((XUiC_CombineWindowGroup)((XUiWindowGroup)uIForPrimaryPlayer.windowManager.GetWindow("combine")).Controller).SetTileEntity(this);
		uIForPrimaryPlayer.windowManager.Open("combine", _bModal: true);
		Manager.BroadcastPlayByLocalPlayer(ToWorldPos().ToVector3() + Vector3.one * 0.5f, openSound);
	}

	public void HandlePlayComplete()
	{
		Manager.BroadcastPlayByLocalPlayer(ToWorldPos().ToVector3() + Vector3.one * 0.5f, combineCompleteSound);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyFromInternal(TileEntityComposite _other)
	{
		_other.TryGetSelfOrFeature<TEFeatureCombine>(out var _);
	}
}
