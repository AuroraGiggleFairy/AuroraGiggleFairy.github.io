using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionConnectPower : ItemAction
{
	public class ConnectPowerData : ItemActionAttackData
	{
		public bool StartLink;

		public bool HasStartPoint;

		public LocalPlayerUI playerUI;

		public Vector3i startPoint;

		public bool inRange;

		public bool isFriendly;

		public WireNode wireNode;

		public ConnectPowerData(ItemInventoryData _invData, int _indexInEntityOfAction)
			: base(_invData, _indexInEntityOfAction)
		{
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 wireOffset = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxWireLength = 15;

	public override ItemActionData CreateModifierData(ItemInventoryData _invData, int _indexInEntityOfAction)
	{
		return new ConnectPowerData(_invData, _indexInEntityOfAction);
	}

	public override void ReadFrom(DynamicProperties _props)
	{
		base.ReadFrom(_props);
		if (_props.Values.ContainsKey("WireOffset"))
		{
			wireOffset = StringParsers.ParseVector3(_props.Values["WireOffset"]);
		}
		if (_props.Values.ContainsKey("MaxWireLength"))
		{
			maxWireLength = StringParsers.ParseSInt32(_props.Values["MaxWireLength"]);
		}
	}

	public override void StopHolding(ItemActionData _data)
	{
		base.StopHolding(_data);
		ConnectPowerData connectPowerData = (ConnectPowerData)_data;
		connectPowerData.HasStartPoint = false;
		if (connectPowerData.wireNode != null)
		{
			WireManager.Instance.RemoveActiveWire(connectPowerData.wireNode);
			Object.Destroy(connectPowerData.wireNode.gameObject);
			connectPowerData.wireNode = null;
		}
		if (connectPowerData.invData.world.GetTileEntity(0, connectPowerData.startPoint) is TileEntityPowered)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(NetPackageWireToolActions.WireActions.RemoveWire, Vector3i.zero, connectPowerData.invData.holdingEntity.entityId));
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(NetPackageWireToolActions.WireActions.RemoveWire, Vector3i.zero, connectPowerData.invData.holdingEntity.entityId));
			}
		}
		if (_data.invData.holdingEntity is EntityPlayerLocal)
		{
			((ConnectPowerData)_data).playerUI.nguiWindowManager.SetLabelText(EnumNGUIWindow.PowerInfo, null);
			WireManager.Instance.ToggleAllWirePulse(isPulseOn: false);
		}
	}

	public override void StartHolding(ItemActionData _data)
	{
		base.StartHolding(_data);
		if (_data.invData.holdingEntity is EntityPlayerLocal)
		{
			((ConnectPowerData)_data).playerUI = LocalPlayerUI.GetUIForPlayer(_data.invData.holdingEntity as EntityPlayerLocal);
			WireManager.Instance.ToggleAllWirePulse(isPulseOn: true);
		}
	}

	public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
	{
		if (_bReleased && !(Time.time - _actionData.lastUseTime < Delay))
		{
			_actionData.lastUseTime = Time.time;
			((ConnectPowerData)_actionData).StartLink = true;
		}
	}

	public override bool IsActionRunning(ItemActionData _actionData)
	{
		ConnectPowerData connectPowerData = (ConnectPowerData)_actionData;
		if (connectPowerData.StartLink && Time.time - connectPowerData.lastUseTime < 2f * AnimationDelayData.AnimationDelay[connectPowerData.invData.item.HoldType.Value].RayCast)
		{
			return true;
		}
		return false;
	}

	public override void OnHoldingUpdate(ItemActionData _actionData)
	{
		TileEntityPowered tileEntityPowered = null;
		ConnectPowerData connectPowerData = (ConnectPowerData)_actionData;
		Vector3i blockPos = _actionData.invData.hitInfo.hit.blockPos;
		bool flag = true;
		if (connectPowerData.invData.holdingEntity is EntityPlayerLocal && connectPowerData.playerUI == null)
		{
			connectPowerData.playerUI = LocalPlayerUI.GetUIForPlayer(connectPowerData.invData.holdingEntity as EntityPlayerLocal);
		}
		if (connectPowerData.playerUI != null && !connectPowerData.invData.world.CanPlaceBlockAt(blockPos, connectPowerData.invData.world.gameManager.GetPersistentLocalPlayer()))
		{
			connectPowerData.isFriendly = false;
			connectPowerData.playerUI.nguiWindowManager.SetLabelText(EnumNGUIWindow.PowerInfo, null);
			return;
		}
		connectPowerData.isFriendly = true;
		if (_actionData.invData.hitInfo.bHitValid)
		{
			int num = (int)(Constants.cDigAndBuildDistance * Constants.cDigAndBuildDistance);
			if (_actionData.invData.hitInfo.hit.distanceSq <= (float)num)
			{
				BlockValue block = _actionData.invData.world.GetBlock(blockPos);
				if (block.Block is BlockPowered blockPowered)
				{
					if (connectPowerData.playerUI != null)
					{
						Color value = Color.grey;
						int num2 = blockPowered.RequiredPower;
						if (blockPowered.isMultiBlock && block.ischild)
						{
							connectPowerData.playerUI.nguiWindowManager.SetLabelText(EnumNGUIWindow.PowerInfo, null);
							return;
						}
						Vector3i p = blockPos;
						ChunkCluster chunkCluster = _actionData.invData.world.ChunkClusters[_actionData.invData.hitInfo.hit.clrIdx];
						if (chunkCluster != null)
						{
							Chunk chunk = (Chunk)chunkCluster.GetChunkSync(World.toChunkXZ(p.x), p.y, World.toChunkXZ(p.z));
							if (chunk != null)
							{
								if (chunk.GetTileEntity(World.toBlock(p)) is TileEntityPowered tileEntityPowered2)
								{
									value = (tileEntityPowered2.IsPowered ? Color.yellow : Color.grey);
									num2 = tileEntityPowered2.PowerUsed;
								}
								else
								{
									value = Color.grey;
								}
							}
						}
						connectPowerData.playerUI.nguiWindowManager.SetLabel(EnumNGUIWindow.PowerInfo, $"{num2}W", value);
					}
					flag = false;
				}
			}
		}
		if (flag && connectPowerData.playerUI != null)
		{
			connectPowerData.playerUI.nguiWindowManager.SetLabelText(EnumNGUIWindow.PowerInfo, null);
		}
		if (connectPowerData.HasStartPoint)
		{
			if (connectPowerData.wireNode == null)
			{
				return;
			}
			float num3 = Vector3.Distance(connectPowerData.startPoint.ToVector3(), _actionData.invData.holdingEntity.position);
			if (num3 < (float)(maxWireLength - 5))
			{
				connectPowerData.inRange = true;
				connectPowerData.wireNode.wireColor = new Color(0f, 0f, 0f, 0f);
			}
			if (num3 > (float)(maxWireLength - 5))
			{
				connectPowerData.inRange = false;
				connectPowerData.wireNode.wireColor = Color.red;
			}
			if (num3 > (float)maxWireLength)
			{
				connectPowerData.HasStartPoint = false;
				if (connectPowerData.wireNode != null)
				{
					WireManager.Instance.RemoveActiveWire(connectPowerData.wireNode);
					Object.Destroy(connectPowerData.wireNode.gameObject);
					connectPowerData.wireNode = null;
				}
				if (!(connectPowerData.invData.world.GetChunkFromWorldPos(connectPowerData.startPoint) is Chunk chunk2))
				{
					return;
				}
				if (connectPowerData.invData.world.GetTileEntity(chunk2.ClrIdx, connectPowerData.startPoint) is TileEntityPowered)
				{
					if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(NetPackageWireToolActions.WireActions.RemoveWire, Vector3i.zero, _actionData.invData.holdingEntity.entityId));
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(NetPackageWireToolActions.WireActions.RemoveWire, Vector3i.zero, _actionData.invData.holdingEntity.entityId));
					}
				}
				_actionData.invData.holdingEntity.RightArmAnimationUse = true;
				connectPowerData.invData.holdingEntity.PlayOneShot("ui_denied");
			}
		}
		if (!connectPowerData.StartLink || Time.time - connectPowerData.lastUseTime < AnimationDelayData.AnimationDelay[connectPowerData.invData.item.HoldType.Value].RayCast)
		{
			return;
		}
		connectPowerData.StartLink = false;
		ConnectPowerData connectPowerData2 = (ConnectPowerData)_actionData;
		ItemInventoryData invData = _actionData.invData;
		_ = invData.hitInfo.lastBlockPos;
		if (!invData.hitInfo.bHitValid || invData.hitInfo.tag.StartsWith("E_"))
		{
			connectPowerData2.HasStartPoint = false;
			return;
		}
		if (connectPowerData.invData.itemValue.MaxUseTimes > 0 && connectPowerData.invData.itemValue.UseTimes >= (float)connectPowerData.invData.itemValue.MaxUseTimes)
		{
			EntityPlayerLocal player = _actionData.invData.holdingEntity as EntityPlayerLocal;
			if (item.Properties.Values.ContainsKey(ItemClass.PropSoundJammed))
			{
				Manager.PlayInsidePlayerHead(item.Properties.Values[ItemClass.PropSoundJammed]);
			}
			GameManager.ShowTooltip(player, "ttItemNeedsRepair");
			return;
		}
		if (connectPowerData.invData.itemValue.MaxUseTimes > 0)
		{
			_actionData.invData.itemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, _actionData.invData.itemValue, 1f, invData.holdingEntity, null, (_actionData.invData.itemValue.ItemClass != null) ? _actionData.invData.itemValue.ItemClass.ItemTags : FastTags<TagGroup.Global>.none);
			HandleItemBreak(_actionData);
		}
		if (connectPowerData2.HasStartPoint)
		{
			if (connectPowerData2.startPoint == invData.hitInfo.hit.blockPos || !connectPowerData2.inRange || Vector3.Distance(connectPowerData.startPoint.ToVector3(), invData.hitInfo.hit.blockPos.ToVector3()) > (float)maxWireLength)
			{
				return;
			}
			TileEntityPowered poweredBlock = GetPoweredBlock(invData);
			if (poweredBlock == null)
			{
				return;
			}
			TileEntityPowered poweredBlock2 = GetPoweredBlock(connectPowerData2.startPoint);
			if (poweredBlock2 == null)
			{
				return;
			}
			if (!poweredBlock.CanHaveParent(poweredBlock2))
			{
				GameManager.ShowTooltip(_actionData.invData.holdingEntity as EntityPlayerLocal, Localization.Get("ttCantHaveParent"));
				invData.holdingEntity.PlayOneShot("ui_denied");
				return;
			}
			if (poweredBlock2.ChildCount > 8)
			{
				GameManager.ShowTooltip(_actionData.invData.holdingEntity as EntityPlayerLocal, Localization.Get("ttWireLimit"));
				invData.holdingEntity.PlayOneShot("ui_denied");
				return;
			}
			poweredBlock.SetParentWithWireTool(poweredBlock2, invData.holdingEntity.entityId);
			_actionData.invData.holdingEntity.RightArmAnimationUse = true;
			connectPowerData2.HasStartPoint = false;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(NetPackageWireToolActions.WireActions.RemoveWire, Vector3i.zero, _actionData.invData.holdingEntity.entityId));
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(NetPackageWireToolActions.WireActions.RemoveWire, Vector3i.zero, _actionData.invData.holdingEntity.entityId));
			}
			EntityAlive holdingEntity = _actionData.invData.holdingEntity;
			string name = "wire_tool_" + (poweredBlock2.IsPowered ? "sparks" : "dust");
			Transform handTransform = GetHandTransform(holdingEntity);
			GameManager.Instance.SpawnParticleEffectServer(new ParticleEffect(name, handTransform.position + Origin.position, handTransform.rotation, holdingEntity.GetLightBrightness(), Color.white), invData.holdingEntity.entityId);
			if (connectPowerData.wireNode != null)
			{
				WireManager.Instance.RemoveActiveWire(connectPowerData.wireNode);
				Object.Destroy(connectPowerData.wireNode.gameObject);
				connectPowerData.wireNode = null;
			}
			DecreaseDurability(connectPowerData);
			return;
		}
		TileEntityPowered poweredBlock3 = GetPoweredBlock(invData);
		if (poweredBlock3 == null)
		{
			return;
		}
		_actionData.invData.holdingEntity.RightArmAnimationUse = true;
		connectPowerData2.startPoint = invData.hitInfo.hit.blockPos;
		connectPowerData2.HasStartPoint = true;
		EntityAlive holdingEntity2 = _actionData.invData.holdingEntity;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(NetPackageWireToolActions.WireActions.AddWire, connectPowerData2.startPoint, holdingEntity2.entityId));
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(NetPackageWireToolActions.WireActions.AddWire, connectPowerData2.startPoint, holdingEntity2.entityId));
		}
		Manager.BroadcastPlay(poweredBlock3.ToWorldPos().ToVector3(), poweredBlock3.IsPowered ? "wire_live_connect" : "wire_dead_connect");
		Transform handTransform2 = GetHandTransform(holdingEntity2);
		if (!(handTransform2 != null))
		{
			return;
		}
		Transform transform = handTransform2.FindInChilds("wire_mesh");
		if (!(transform == null))
		{
			if (connectPowerData2.wireNode != null)
			{
				WireManager.Instance.RemoveActiveWire(connectPowerData2.wireNode);
				Object.Destroy(connectPowerData2.wireNode.gameObject);
				connectPowerData2.wireNode = null;
			}
			WireNode component = ((GameObject)Object.Instantiate(Resources.Load("Prefabs/WireNode"))).GetComponent<WireNode>();
			component.LocalPosition = invData.hitInfo.hit.blockPos.ToVector3() - Origin.position;
			component.localOffset = poweredBlock3.GetWireOffset();
			component.localOffset.x += 0.5f;
			component.localOffset.y += 0.5f;
			component.localOffset.z += 0.5f;
			component.Source = transform.gameObject;
			component.sourceOffset = wireOffset;
			component.TogglePulse(isOn: false);
			component.SetPulseSpeed(360f);
			connectPowerData2.wireNode = component;
			WireManager.Instance.AddActiveWire(component);
			string name2 = "wire_tool_" + (poweredBlock3.IsPowered ? "sparks" : "dust");
			GameManager.Instance.SpawnParticleEffectServer(new ParticleEffect(name2, handTransform2.position + Origin.position, handTransform2.rotation, holdingEntity2.GetLightBrightness(), Color.white), invData.holdingEntity.entityId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform GetHandTransform(EntityAlive holdingEntity)
	{
		Transform transform = holdingEntity.RootTransform.Find("Graphics").FindInChilds(holdingEntity.GetRightHandTransformName(), onlyActive: true);
		Transform transform2 = null;
		if (transform != null && transform.childCount > 0)
		{
			return transform;
		}
		Transform transform3 = holdingEntity.RootTransform.Find("Camera").FindInChilds(holdingEntity.GetRightHandTransformName(), onlyActive: true);
		if (transform3 != null && transform3.childCount > 0)
		{
			return transform3;
		}
		return holdingEntity.emodel.GetRightHandTransform();
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void CheckForWireRemoveNeeded(EntityAlive _player, Vector3i _blockPos)
	{
		ConnectPowerData connectPowerData = (ConnectPowerData)_player.inventory.holdingItemData.actionData[1];
		if (connectPowerData.HasStartPoint && connectPowerData.startPoint == _blockPos)
		{
			DisconnectWire(connectPowerData);
		}
	}

	public override ItemClass.EnumCrosshairType GetCrosshairType(ItemActionData _actionData)
	{
		if (_actionData.invData.hitInfo.bHitValid && (_actionData as ConnectPowerData).isFriendly)
		{
			int num = (int)(Constants.cDigAndBuildDistance * Constants.cDigAndBuildDistance);
			if (_actionData.invData.hitInfo.hit.distanceSq <= (float)num)
			{
				Vector3i blockPos = _actionData.invData.hitInfo.hit.blockPos;
				Block block = _actionData.invData.world.GetBlock(blockPos).Block;
				if (block is BlockPowered)
				{
					return ItemClass.EnumCrosshairType.PowerItem;
				}
				if (block is BlockPowerSource)
				{
					return ItemClass.EnumCrosshairType.PowerSource;
				}
			}
		}
		return ItemClass.EnumCrosshairType.Plus;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPowered GetPoweredBlock(ItemInventoryData data)
	{
		Block block = data.world.GetBlock(data.hitInfo.hit.blockPos).Block;
		if (block is BlockPowered || block is BlockPowerSource)
		{
			Vector3i blockPos = data.hitInfo.hit.blockPos;
			ChunkCluster chunkCluster = data.world.ChunkClusters[data.hitInfo.hit.clrIdx];
			if (chunkCluster == null)
			{
				return null;
			}
			Chunk chunk = (Chunk)chunkCluster.GetChunkSync(World.toChunkXZ(blockPos.x), blockPos.y, World.toChunkXZ(blockPos.z));
			if (chunk == null)
			{
				return null;
			}
			TileEntity tileEntity = chunk.GetTileEntity(World.toBlock(blockPos));
			if (tileEntity == null)
			{
				if (block is BlockPowered)
				{
					tileEntity = (block as BlockPowered).CreateTileEntity(chunk);
				}
				else if (block is BlockPowerSource)
				{
					tileEntity = (block as BlockPowerSource).CreateTileEntity(chunk);
				}
				tileEntity.localChunkPos = World.toBlock(blockPos);
				BlockEntityData blockEntity = chunk.GetBlockEntity(blockPos);
				if (blockEntity != null)
				{
					((TileEntityPowered)tileEntity).BlockTransform = blockEntity.transform;
				}
				((TileEntityPowered)tileEntity).InitializePowerData();
				chunk.AddTileEntity(tileEntity);
			}
			return tileEntity as TileEntityPowered;
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPowered GetPoweredBlock(Vector3i tileEntityPos)
	{
		World world = GameManager.Instance.World;
		Block block = world.GetBlock(tileEntityPos).Block;
		if (block is BlockPowered || block is BlockPowerSource)
		{
			if (!(world.GetChunkFromWorldPos(tileEntityPos.x, tileEntityPos.y, tileEntityPos.z) is Chunk chunk))
			{
				return null;
			}
			TileEntity tileEntity = chunk.GetTileEntity(World.toBlock(tileEntityPos));
			if (tileEntity == null)
			{
				if (block is BlockPowered)
				{
					tileEntity = (block as BlockPowered).CreateTileEntity(chunk);
				}
				else if (block is BlockPowerSource)
				{
					tileEntity = (block as BlockPowerSource).CreateTileEntity(chunk);
				}
				tileEntity.localChunkPos = World.toBlock(tileEntityPos);
				BlockEntityData blockEntity = chunk.GetBlockEntity(tileEntityPos);
				if (blockEntity != null)
				{
					((TileEntityPowered)tileEntity).BlockTransform = blockEntity.transform;
				}
				((TileEntityPowered)tileEntity).InitializePowerData();
				chunk.AddTileEntity(tileEntity);
			}
			return tileEntity as TileEntityPowered;
		}
		return null;
	}

	public bool DisconnectWire(ConnectPowerData _actionData)
	{
		if (!_actionData.HasStartPoint)
		{
			return false;
		}
		_actionData.HasStartPoint = false;
		if (_actionData.wireNode != null)
		{
			WireManager.Instance.RemoveActiveWire(_actionData.wireNode);
			Object.Destroy(_actionData.wireNode.gameObject);
			_actionData.wireNode = null;
		}
		if (!(_actionData.invData.world.GetChunkFromWorldPos(_actionData.startPoint) is Chunk chunk))
		{
			return false;
		}
		if (_actionData.invData.world.GetTileEntity(chunk.ClrIdx, _actionData.startPoint) is TileEntityPowered tileEntityPowered)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(NetPackageWireToolActions.WireActions.RemoveWire, Vector3i.zero, _actionData.invData.holdingEntity.entityId));
			}
			else
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWireToolActions>().Setup(NetPackageWireToolActions.WireActions.RemoveWire, Vector3i.zero, _actionData.invData.holdingEntity.entityId));
			}
			Manager.BroadcastPlay(tileEntityPowered.ToWorldPos().ToVector3(), tileEntityPowered.IsPowered ? "wire_live_break" : "wire_dead_break");
			EntityAlive holdingEntity = _actionData.invData.holdingEntity;
			string name = "wire_tool_" + (tileEntityPowered.IsPowered ? "sparks" : "dust");
			Transform handTransform = GetHandTransform(holdingEntity);
			GameManager.Instance.SpawnParticleEffectServer(new ParticleEffect(name, handTransform.position + Origin.position, handTransform.rotation, holdingEntity.GetLightBrightness(), Color.white), holdingEntity.entityId);
		}
		_actionData.invData.holdingEntity.RightArmAnimationAttack = true;
		_actionData.invData.holdingEntity.PlayOneShot("ui_denied");
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DecreaseDurability(ConnectPowerData _actionData)
	{
		if (_actionData.invData.itemValue.MaxUseTimes > 0)
		{
			if (_actionData.invData.itemValue.UseTimes + 1f < (float)_actionData.invData.itemValue.MaxUseTimes)
			{
				_actionData.invData.itemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, _actionData.invData.itemValue, 1f, _actionData.invData.holdingEntity, null, _actionData.invData.itemValue.ItemClass.ItemTags);
				HandleItemBreak(_actionData);
			}
			else
			{
				_actionData.invData.holdingEntity.inventory.DecHoldingItem(1);
			}
		}
	}
}
