using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;

public class TileEntityPowered : TileEntity, IPowered
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public const int ver = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool wiresDirty;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isPlayerPlaced;

	public PowerItem.PowerItemTypes PowerItemType = PowerItem.PowerItemTypes.Consumer;

	[PublicizedFrom(EAccessModifier.Protected)]
	public PowerItem PowerItem;

	public Vector3 WireOffset = Vector3.zero;

	public float CenteredPitch;

	public float CenteredYaw;

	public string WindowGroupToOpen = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool needBlockData;

	[PublicizedFrom(EAccessModifier.Private)]
	public int requiredPower;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPowered;

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform blockTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<IWireNode> currentWireNodes = new List<IWireNode>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3i> wireDataList = new List<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool activateDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i parentPosition = new Vector3i(-9999, -9999, -9999);

	public int RequiredPower
	{
		get
		{
			if (needBlockData && !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				ushort valuesFromBlock = (ushort)GameManager.Instance.World.GetBlock(ToWorldPos()).type;
				SetValuesFromBlock(valuesFromBlock);
				needBlockData = false;
			}
			return requiredPower;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			requiredPower = value;
		}
	}

	public virtual int PowerUsed => RequiredPower;

	public int ChildCount => wireDataList.Count;

	public bool IsPlayerPlaced
	{
		get
		{
			return isPlayerPlaced;
		}
		set
		{
			isPlayerPlaced = value;
			setModified();
		}
	}

	public bool IsPowered
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (PowerItem != null)
				{
					return PowerItem.IsPowered;
				}
				return false;
			}
			return isPowered;
		}
	}

	public Transform BlockTransform
	{
		get
		{
			return blockTransform;
		}
		set
		{
			blockTransform = value;
			BlockValue blockValue = GameManager.Instance.World.GetBlock(ToWorldPos());
			if (blockTransform != null)
			{
				Transform transform = blockTransform.Find("WireOffset");
				if (transform != null)
				{
					Vector3 wireOffset = blockValue.Block.shape.GetRotation(blockValue) * transform.localPosition;
					WireOffset = wireOffset;
					return;
				}
			}
			if (blockValue.Block.Properties.Values.ContainsKey("WireOffset"))
			{
				Vector3 wireOffset2 = blockValue.Block.shape.GetRotation(blockValue) * StringParsers.ParseVector3(blockValue.Block.Properties.Values["WireOffset"]);
				WireOffset = wireOffset2;
			}
		}
	}

	public TileEntityPowered(Chunk _chunk)
		: base(_chunk)
	{
	}

	public Vector3i GetParent()
	{
		return parentPosition;
	}

	public bool HasParent()
	{
		return parentPosition.y != -9999;
	}

	public PowerItem GetPowerItem()
	{
		return PowerItem;
	}

	public override void OnReadComplete()
	{
		base.OnReadComplete();
		InitializePowerData();
		CheckForNewWires();
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		int num = _br.ReadInt32();
		isPlayerPlaced = _br.ReadBoolean();
		PowerItemType = (PowerItem.PowerItemTypes)_br.ReadByte();
		needBlockData = true;
		int num2 = _br.ReadByte();
		wireDataList.Clear();
		for (int i = 0; i < num2; i++)
		{
			Vector3i item = StreamUtils.ReadVector3i(_br);
			wireDataList.Add(item);
		}
		parentPosition = StreamUtils.ReadVector3i(_br);
		if (_eStreamMode == StreamModeRead.FromServer)
		{
			isPowered = _br.ReadBoolean();
		}
		activateDirty = true;
		wiresDirty = true;
		if (num <= 0)
		{
			return;
		}
		if (_eStreamMode == StreamModeRead.FromServer)
		{
			bool flag = false;
			if (LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.HasWindow(XUiC_PowerCameraWindowGroup.ID))
			{
				XUiC_PowerCameraWindowGroup xUiC_PowerCameraWindowGroup = (XUiC_PowerCameraWindowGroup)((XUiWindowGroup)LocalPlayerUI.GetUIForPrimaryPlayer().windowManager.GetWindow(XUiC_PowerCameraWindowGroup.ID)).Controller;
				flag = IsUserAccessing() && xUiC_PowerCameraWindowGroup != null && xUiC_PowerCameraWindowGroup.TileEntity == this;
			}
			if (!flag)
			{
				CenteredPitch = _br.ReadSingle();
				CenteredYaw = _br.ReadSingle();
			}
			else
			{
				_br.ReadSingle();
				_br.ReadSingle();
			}
		}
		else
		{
			CenteredPitch = _br.ReadSingle();
			CenteredYaw = _br.ReadSingle();
		}
	}

	public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write(1);
		_bw.Write(isPlayerPlaced);
		_bw.Write((byte)PowerItemType);
		_bw.Write((byte)wireDataList.Count);
		for (int i = 0; i < wireDataList.Count; i++)
		{
			StreamUtils.Write(_bw, wireDataList[i]);
		}
		StreamUtils.Write(_bw, parentPosition);
		if (_eStreamMode == StreamModeWrite.ToClient)
		{
			_bw.Write(IsPowered);
		}
		_bw.Write(CenteredPitch);
		_bw.Write(CenteredYaw);
	}

	public void CheckForNewWires()
	{
		if (GameManager.Instance == null || !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		for (int i = 0; i < wireDataList.Count; i++)
		{
			Vector3 childPosition = wireDataList[i].ToVector3();
			if (PowerItem.GetChild(childPosition) == null)
			{
				PowerItem powerItemByWorldPos = PowerManager.Instance.GetPowerItemByWorldPos(wireDataList[i]);
				PowerManager.Instance.SetParent(powerItemByWorldPos, PowerItem);
			}
		}
	}

	public void DrawWires()
	{
		if (BlockTransform == null)
		{
			wiresDirty = true;
			return;
		}
		WireManager instance = WireManager.Instance;
		bool flag = instance.ShowPulse;
		bool wiresShowing = instance.WiresShowing;
		if (wireDataList.Count > 0)
		{
			World world = GameManager.Instance.World;
			if (flag)
			{
				flag = world.CanPlaceBlockAt(ToWorldPos(), world.gameManager.GetPersistentLocalPlayer());
			}
		}
		for (int i = 0; i < wireDataList.Count; i++)
		{
			Vector3i blockPos = wireDataList[i];
			if (GameManager.Instance.World.GetChunkFromWorldPos(blockPos) is Chunk chunk)
			{
				TileEntityPowered tileEntityPowered = GameManager.Instance.World.GetTileEntity(chunk.ClrIdx, blockPos) as TileEntityPowered;
				bool flag2 = false;
				if (tileEntityPowered != null && tileEntityPowered.BlockTransform != null)
				{
					flag2 = true;
				}
				if (!flag2)
				{
					wiresDirty = true;
					return;
				}
			}
		}
		int num = 0;
		for (int j = 0; j < wireDataList.Count; j++)
		{
			Vector3i blockPos2 = wireDataList[j];
			if (!(GameManager.Instance.World.GetChunkFromWorldPos(blockPos2) is Chunk chunk2))
			{
				continue;
			}
			TileEntityPowered tileEntityPowered2 = GameManager.Instance.World.GetTileEntity(chunk2.ClrIdx, blockPos2) as TileEntityPowered;
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && GameManager.IsDedicatedServer && tileEntityPowered2 != null && (PowerItemType != PowerItem.PowerItemTypes.TripWireRelay || tileEntityPowered2.PowerItemType != PowerItem.PowerItemTypes.TripWireRelay))
			{
				continue;
			}
			if (num >= currentWireNodes.Count)
			{
				IWireNode wireNodeFromPool = WireManager.Instance.GetWireNodeFromPool();
				currentWireNodes.Add(wireNodeFromPool);
			}
			currentWireNodes[num].SetStartPosition(BlockTransform.position + Origin.position);
			currentWireNodes[num].SetStartPositionOffset(WireOffset);
			if (tileEntityPowered2 != null)
			{
				if (PowerItemType == PowerItem.PowerItemTypes.ElectricWireRelay && tileEntityPowered2.PowerItemType == PowerItem.PowerItemTypes.ElectricWireRelay)
				{
					currentWireNodes[num].SetPulseColor(new Color32(0, 97, byte.MaxValue, byte.MaxValue));
					currentWireNodes[num].SetWireRadius(0.005f);
					currentWireNodes[num].SetWireDip(0f);
					ElectricWireController electricWireController = currentWireNodes[num].GetGameObject().GetComponent<ElectricWireController>();
					if (electricWireController == null)
					{
						electricWireController = currentWireNodes[num].GetGameObject().AddComponent<ElectricWireController>();
					}
					electricWireController.TileEntityParent = this as TileEntityPoweredMeleeTrap;
					electricWireController.TileEntityChild = tileEntityPowered2 as TileEntityPoweredMeleeTrap;
					electricWireController.WireNode = currentWireNodes[num];
					electricWireController.Init(base.chunk.GetBlock(base.localChunkPos).Block.Properties);
					electricWireController.WireNode.SetWireCanHide(_canHide: false);
				}
				else if (PowerItemType == PowerItem.PowerItemTypes.TripWireRelay && tileEntityPowered2.PowerItemType == PowerItem.PowerItemTypes.TripWireRelay)
				{
					currentWireNodes[num].SetPulseColor(Color.magenta);
					currentWireNodes[num].SetWireRadius(0.0035f);
					currentWireNodes[num].SetWireDip(0f);
					TripWireController tripWireController = currentWireNodes[num].GetGameObject().GetComponent<TripWireController>();
					if (tripWireController == null)
					{
						tripWireController = currentWireNodes[num].GetGameObject().AddComponent<TripWireController>();
					}
					tripWireController.TileEntityParent = this as TileEntityPoweredTrigger;
					tripWireController.TileEntityChild = tileEntityPowered2 as TileEntityPoweredTrigger;
					tripWireController.WireNode = currentWireNodes[num];
					tripWireController.WireNode.SetWireCanHide(_canHide: false);
				}
				else
				{
					UnityEngine.Object.Destroy(currentWireNodes[num].GetGameObject().GetComponent<ElectricWireController>());
					UnityEngine.Object.Destroy(currentWireNodes[num].GetGameObject().GetComponent<TripWireController>());
					currentWireNodes[num].SetWireCanHide(_canHide: true);
				}
			}
			currentWireNodes[num].SetEndPosition(blockPos2.ToVector3());
			if (tileEntityPowered2 != null)
			{
				currentWireNodes[num].SetEndPositionOffset(tileEntityPowered2.WireOffset + new Vector3(0.5f, 0.5f, 0.5f));
			}
			currentWireNodes[num].BuildMesh();
			currentWireNodes[num].TogglePulse(flag);
			currentWireNodes[num].SetVisible(wiresShowing);
			num++;
		}
		for (int k = num; k < currentWireNodes.Count; k++)
		{
			IWireNode wireNode = currentWireNodes[num];
			WireManager.Instance.ReturnToPool(wireNode);
			currentWireNodes.Remove(wireNode);
		}
		wiresDirty = false;
	}

	public void AddWireData(Vector3i child)
	{
		wireDataList.Add(child);
		SendWireData();
	}

	public void SendWireData()
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageWireActions>().Setup(NetPackageWireActions.WireActions.SendWires, ToWorldPos(), wireDataList));
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWireActions>().Setup(NetPackageWireActions.WireActions.SendWires, ToWorldPos(), wireDataList));
		}
	}

	public void CreateWireDataFromPowerItem()
	{
		wireDataList.Clear();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			for (int i = 0; i < PowerItem.Children.Count; i++)
			{
				wireDataList.Add(PowerItem.Children[i].Position);
			}
		}
	}

	public void RemoveWires()
	{
		for (int i = 0; i < currentWireNodes.Count; i++)
		{
			WireManager.Instance.ReturnToPool(currentWireNodes[i]);
		}
		currentWireNodes.Clear();
	}

	public void MarkWireDirty()
	{
		wiresDirty = true;
	}

	public void MarkChanged()
	{
		SetModified();
	}

	public void InitializePowerData()
	{
		if (GameManager.Instance == null)
		{
			return;
		}
		ushort num = (ushort)GameManager.Instance.World.GetBlock(ToWorldPos()).type;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			PowerItem = PowerManager.Instance.GetPowerItemByWorldPos(ToWorldPos());
			if (PowerItem == null)
			{
				CreatePowerItemForTileEntity(num);
			}
			else
			{
				num = PowerItem.BlockID;
			}
			PowerItem.AddTileEntity(this);
			SetModified();
			activateDirty = true;
		}
		SetValuesFromBlock(num);
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			DrawWires();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetValuesFromBlock(ushort blockID)
	{
		if (Block.list[blockID].Properties.Values.ContainsKey("RequiredPower"))
		{
			RequiredPower = Convert.ToInt32(Block.list[blockID].Properties.Values["RequiredPower"]);
		}
		else
		{
			RequiredPower = 5;
		}
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		if (BlockTransform != null)
		{
			if (wiresDirty)
			{
				DrawWires();
			}
			if (activateDirty)
			{
				Activate(PowerItem.IsPowered);
				activateDirty = false;
			}
		}
	}

	public PowerItem CreatePowerItemForTileEntity(ushort blockID)
	{
		if (PowerItem == null)
		{
			PowerItem = CreatePowerItem();
			PowerItem.Position = ToWorldPos();
			PowerItem.BlockID = blockID;
			PowerItem.SetValuesFromBlock();
			PowerManager.Instance.AddPowerNode(PowerItem);
		}
		return PowerItem;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual PowerItem CreatePowerItem()
	{
		return PowerItem.CreateItem(PowerItemType);
	}

	public override void OnUnload(World world)
	{
		base.OnUnload(world);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			PowerItem.RemoveTileEntity(this);
		}
		RemoveWires();
	}

	public virtual bool Activate(bool activated)
	{
		return false;
	}

	public virtual bool ActivateOnce()
	{
		return false;
	}

	public Vector3 GetWireOffset()
	{
		return WireOffset;
	}

	public int GetRequiredPower()
	{
		return RequiredPower;
	}

	public virtual bool CanHaveParent(IPowered powered)
	{
		return true;
	}

	public void SetParentWithWireTool(IPowered newParentTE, int wiringEntityID)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			PowerItem powerItem = newParentTE.GetPowerItem();
			PowerItem parent = PowerItem.Parent;
			PowerManager.Instance.SetParent(PowerItem, powerItem);
			if (parent != null && parent.TileEntity != null)
			{
				parent.TileEntity.CreateWireDataFromPowerItem();
				parent.TileEntity.SendWireData();
				parent.TileEntity.RemoveWires();
				parent.TileEntity.DrawWires();
			}
			newParentTE.CreateWireDataFromPowerItem();
			newParentTE.SendWireData();
			newParentTE.RemoveWires();
			newParentTE.DrawWires();
			Manager.BroadcastPlay(ToWorldPos().ToVector3(), powerItem.IsPowered ? "wire_live_connect" : "wire_dead_connect");
		}
		else
		{
			parentPosition = ((TileEntity)newParentTE).ToWorldPos();
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWireActions>().Setup(NetPackageWireActions.WireActions.SetParent, ToWorldPos(), new List<Vector3i> { parentPosition }, wiringEntityID));
			Manager.BroadcastPlay(ToWorldPos().ToVector3(), IsPowered ? "wire_live_connect" : "wire_dead_connect");
		}
		SetModified();
	}

	public void RemoveParentWithWiringTool(int wiringEntityID)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (PowerItem.Parent != null)
			{
				Vector3i position = PowerItem.Parent.Position;
				PowerItem parent = PowerItem.Parent;
				PowerItem.RemoveSelfFromParent();
				if (parent.TileEntity != null)
				{
					parent.TileEntity.CreateWireDataFromPowerItem();
					parent.TileEntity.SendWireData();
					parent.TileEntity.RemoveWires();
					parent.TileEntity.DrawWires();
				}
				Manager.BroadcastPlay(position.ToVector3(), PowerItem.IsPowered ? "wire_live_break" : "wire_dead_break");
			}
		}
		else
		{
			Vector3i tileEntityPosition = ToWorldPos();
			Vector3 position2 = tileEntityPosition.ToVector3();
			parentPosition = new Vector3i(-9999, -9999, -9999);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageWireActions>().Setup(NetPackageWireActions.WireActions.RemoveParent, tileEntityPosition, new List<Vector3i>(), wiringEntityID));
			Manager.BroadcastPlay(position2, IsPowered ? "wire_live_break" : "wire_dead_break");
		}
		SetModified();
	}

	public void SetWireData(List<Vector3i> wireChildren)
	{
		wireDataList = wireChildren;
		RemoveWires();
		DrawWires();
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Powered;
	}
}
