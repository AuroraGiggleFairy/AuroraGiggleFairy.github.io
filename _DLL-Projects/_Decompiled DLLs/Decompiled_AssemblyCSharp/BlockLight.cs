using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockLight : Block
{
	public const int cMetaOriginalState = 1;

	public const int cMetaOn = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isRuntimeSwitch;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ignoreLightsOff;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[3]
	{
		new BlockActivationCommand("light", "electric_switch", _enabled: true),
		new BlockActivationCommand("edit", "tool", _enabled: true),
		new BlockActivationCommand("trigger", "wrench", _enabled: true)
	};

	public override bool AllowBlockTriggers => true;

	public BlockLight()
	{
		HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("RuntimeSwitch"))
		{
			isRuntimeSwitch = StringParsers.ParseBool(base.Properties.Values["RuntimeSwitch"]);
		}
		if (base.Properties.Values.ContainsKey("Model"))
		{
			DataLoader.PreloadBundle(base.Properties.Values["Model"]);
		}
		base.Properties.ParseBool("IgnoreLightsOff", ref ignoreLightsOff);
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
		switch (_commandName)
		{
		case "light":
			if (_world.IsEditor() && updateLightState(_world, _cIdx, _blockPos, _blockValue, _bSwitchLight: true, _enableState: false))
			{
				return true;
			}
			break;
		case "edit":
		{
			TileEntityLight te = (TileEntityLight)_world.GetTileEntity(_cIdx, _blockPos);
			if (_world.IsEditor())
			{
				XUiC_LightEditor.Open(_player.PlayerUI, te, _blockPos, _world as World, _cIdx, this);
				return true;
			}
			break;
		}
		case "trigger":
			XUiC_TriggerProperties.Show(_player.PlayerUI.xui, _cIdx, _blockPos, _showTriggers: false, _showTriggeredBy: true);
			break;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateLightState(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bSwitchLight = false, bool _enableState = true)
	{
		ChunkCluster chunkCluster = _world.ChunkClusters[_cIdx];
		if (chunkCluster == null)
		{
			return false;
		}
		IChunk chunkSync = chunkCluster.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z));
		if (chunkSync == null)
		{
			return false;
		}
		BlockEntityData blockEntity = chunkSync.GetBlockEntity(_blockPos);
		if (blockEntity == null || !blockEntity.bHasTransform)
		{
			return false;
		}
		bool flag = (_blockValue.meta & 2) != 0;
		TileEntityLight tileEntityLight = (TileEntityLight)_world.GetTileEntity(_cIdx, _blockPos);
		if (_bSwitchLight)
		{
			flag = !flag;
			_blockValue.meta = (byte)((_blockValue.meta & -3) | (flag ? 2 : 0));
			_world.SetBlockRPC(_cIdx, _blockPos, _blockValue);
		}
		Transform transform = blockEntity.transform.FindInChildren("MainLight");
		if ((bool)transform)
		{
			LightLOD component = transform.GetComponent<LightLOD>();
			if ((bool)component)
			{
				component.SwitchOnOff(flag);
				component.SetBlockEntityData(blockEntity);
				Light light = component.GetLight();
				if (tileEntityLight != null)
				{
					light.type = tileEntityLight.LightType;
					component.MaxIntensity = tileEntityLight.LightIntensity;
					light.color = tileEntityLight.LightColor;
					light.shadows = tileEntityLight.LightShadows;
					component.LightAngle = tileEntityLight.LightAngle;
					component.LightStateType = tileEntityLight.LightState;
					component.StateRate = tileEntityLight.Rate;
					component.FluxDelay = tileEntityLight.Delay;
					component.SetRange(tileEntityLight.LightRange);
					component.SetEmissiveColor(component.bSwitchedOn);
				}
				else
				{
					GameObject gameObject = DataLoader.LoadAsset<GameObject>(base.Properties.Values["Model"]);
					if (gameObject != null)
					{
						Transform transform2 = gameObject.transform.Find("MainLight");
						if (transform2 != null)
						{
							LightLOD component2 = transform2.GetComponent<LightLOD>();
							Light light2 = component2.GetLight();
							if (light != null && light2 != null)
							{
								light.type = light2.type;
								component.MaxIntensity = light2.intensity;
								light.color = light2.color;
								light.shadows = light2.shadows;
								component.LightAngle = light2.spotAngle;
								component.LightStateType = component2.LightStateType;
								component.StateRate = component2.StateRate;
								component.FluxDelay = component2.FluxDelay;
								component.SetRange(light2.range);
								component.SetEmissiveColor(component.bSwitchedOn);
							}
						}
					}
				}
			}
		}
		transform = blockEntity.transform.Find("Point light");
		if ((bool)transform)
		{
			LightLOD component3 = transform.GetComponent<LightLOD>();
			if ((bool)component3)
			{
				component3.SwitchOnOff(flag);
				component3.SetBlockEntityData(blockEntity);
			}
		}
		return true;
	}

	public bool IsLightOn(BlockValue _blockValue)
	{
		return (_blockValue.meta & 2) != 0;
	}

	public bool OriginalLightState(BlockValue _blockValue)
	{
		if (ignoreLightsOff)
		{
			return false;
		}
		return (_blockValue.meta & 1) != 0;
	}

	public BlockValue SetLightState(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, bool isOn)
	{
		_blockValue.meta = (byte)((_blockValue.meta & -3) | (isOn ? 2 : 0));
		return _blockValue;
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
		updateLightState(_world, _clrIdx, _blockPos, _newBlockValue);
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		updateLightState(_world, _cIdx, _blockPos, _blockValue);
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool flag = false;
		ChunkCluster chunkCluster = _world.ChunkClusters[_clrIdx];
		if (chunkCluster != null)
		{
			IChunk chunkSync = chunkCluster.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z));
			if (chunkSync != null)
			{
				BlockEntityData blockEntity = chunkSync.GetBlockEntity(_blockPos);
				if (blockEntity != null && blockEntity.bHasTransform)
				{
					Transform transform = blockEntity.transform.Find("MainLight");
					if (transform != null)
					{
						LightLOD component = transform.GetComponent<LightLOD>();
						if (component != null && component.GetLight() != null)
						{
							flag = true;
						}
					}
				}
			}
		}
		cmds[0].enabled = _world.IsEditor() || isRuntimeSwitch;
		cmds[1].enabled = _world.IsEditor() && flag;
		cmds[2].enabled = _world.IsEditor() && !GameUtils.IsWorldEditor();
		return cmds;
	}

	public TileEntityLight CreateTileEntity(Chunk chunk)
	{
		return new TileEntityLight(chunk);
	}

	public override bool IsTileEntitySavedInPrefab()
	{
		return true;
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (!_blockValue.ischild && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			bool flag = IsLightOn(_blockValue);
			if (OriginalLightState(_blockValue) != flag)
			{
				_blockValue.meta = (byte)((_blockValue.meta & -2) | (flag ? 1 : 0));
				_world.SetBlockRPC(_chunk.ClrIdx, _blockPos, _blockValue);
			}
		}
	}

	public override void OnBlockReset(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (!_blockValue.ischild && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			bool flag = IsLightOn(_blockValue);
			if (OriginalLightState(_blockValue) != flag)
			{
				_blockValue.meta = (byte)((_blockValue.meta & -2) | (flag ? 1 : 0));
				_world.SetBlockRPC(_chunk.ClrIdx, _blockPos, _blockValue);
			}
		}
	}

	public override void OnTriggered(EntityPlayer _player, WorldBase _world, int cIdx, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		base.OnTriggered(_player, _world, cIdx, _blockPos, _blockValue, _blockChanges, _triggeredBy);
		_blockValue = SetLightState(_world, cIdx, _blockPos, _blockValue, !IsLightOn(_blockValue));
		_blockChanges.Add(new BlockChangeInfo(cIdx, _blockPos, _blockValue));
	}
}
