using System;
using UnityEngine.Scripting;

[Preserve]
public class TEFeaturePickup : TEFeatureAbs, IFeatureSavedInPrefab
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropTakeDelay = "TakeDelay";

	[PublicizedFrom(EAccessModifier.Protected)]
	public float TakeDelay = 2f;

	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		_featureData.Props.ParseFloat(PropTakeDelay, ref TakeDelay);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyFromInternal(TileEntityComposite _other)
	{
	}

	public override bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		return LocalPlayerUI.GetUIForPrimaryPlayer() != null;
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
		_addCallback(new BlockActivationCommand("Take", "hand", _enabled: false), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
	}

	public override bool AllowBlockActivationCommand(ITileEntityFeature _module, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		if (!base.AllowBlockActivationCommand(_module, _commandName, _world, _blockPos, _blockValue, _entityFocusing))
		{
			return false;
		}
		if (!Equals(_module))
		{
			return true;
		}
		if (_world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer()) && base.Parent.PlayerPlaced)
		{
			return TakeDelay > 0f;
		}
		return false;
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		if (CommandIs(_commandName, "Take"))
		{
			_player.AimingGun = false;
			Block.TakeItemWithTimer(_blockPos, _blockValue, _player, TakeDelay);
			return true;
		}
		return false;
	}

	public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.Read(_br, _eStreamMode);
		int num = ((_eStreamMode == TileEntity.StreamModeRead.Persistency) ? _br.ReadUInt16() : 2);
		if (num < 2)
		{
			_br.ReadBoolean();
		}
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write((ushort)2);
		}
	}
}
