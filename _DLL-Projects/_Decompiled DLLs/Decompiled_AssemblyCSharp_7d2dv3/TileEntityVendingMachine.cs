using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

public class TileEntityVendingMachine : TileEntity, ILockable, ITrader
{
	public enum RentResult
	{
		Allowed,
		AlreadyRented,
		AlreadyRentingVM,
		NotEnoughMoney
	}

	[Preserve]
	public class VendingMachineLockContext : ILockContext
	{
		public TraderData TraderData;

		public VendingMachineLockContext()
		{
		}

		public VendingMachineLockContext(TraderData _traderData)
		{
			TraderData = _traderData.Clone();
		}

		public void Read(PooledBinaryReader _br)
		{
			if (TraderData == null)
			{
				TraderData = new TraderData();
			}
			TraderData.Read(_br);
		}

		public void Write(PooledBinaryWriter _bw)
		{
			TraderData.Write(_bw);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ver = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs ownerID;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PlatformUserIdentifierAbs> allowedUserIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public string passwordHash;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong rentalEndTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong nextAutoBuy;

	[PublicizedFrom(EAccessModifier.Private)]
	public int rentalEndDay;

	[PublicizedFrom(EAccessModifier.Private)]
	public float autoBuyThresholdStep = 1f / 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float autoBuyThreshold = 1f / 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int minimumAutoBuyCount = 5;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public TraderData TraderData { get; set; }

	public bool IsRentable => TraderData.TraderInfo.Rentable;

	public float RentTimeRemaining => rentalEndDay - GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);

	public int RentalEndDay => rentalEndDay;

	public TileEntityVendingMachine(Chunk _chunk)
		: base(_chunk)
	{
		allowedUserIds = new List<PlatformUserIdentifierAbs>();
		isLocked = true;
		ownerID = null;
		passwordHash = "";
		rentalEndTime = 0uL;
		rentalEndDay = 0;
		nextAutoBuy = 0uL;
		TraderData = new TraderData();
	}

	public override TileEntity Clone()
	{
		return new TileEntityVendingMachine(null)
		{
			bUserAccessing = bUserAccessing,
			TraderData = TraderData.Clone(),
			allowedUserIds = new List<PlatformUserIdentifierAbs>(allowedUserIds),
			isLocked = isLocked,
			ownerID = ownerID,
			passwordHash = passwordHash,
			rentalEndTime = rentalEndTime,
			rentalEndDay = rentalEndDay,
			nextAutoBuy = nextAutoBuy
		};
	}

	public RentResult CanRent()
	{
		if (ownerID != null && !ownerID.Equals(PlatformManager.InternalLocalUserIdentifier))
		{
			return RentResult.AlreadyRented;
		}
		if (GameManager.Instance.World.GetPrimaryPlayer().PlayerUI.xui.PlayerInventory.CurrencyAmount < TraderData.TraderInfo.RentCost)
		{
			return RentResult.NotEnoughMoney;
		}
		if (checkAlreadyRentingVM())
		{
			return RentResult.AlreadyRentingVM;
		}
		return RentResult.Allowed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkAlreadyRentingVM()
	{
		EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		Vector3i rentedVMPosition = primaryPlayer.RentedVMPosition;
		if (rentedVMPosition == ToWorldPos() || rentedVMPosition == Vector3i.zero)
		{
			return false;
		}
		return primaryPlayer.RentalEndDay > GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
	}

	public bool IsLocked()
	{
		return isLocked;
	}

	public void SetLocked(bool _isLocked)
	{
		isLocked = _isLocked;
		setModified();
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		ownerID = _userIdentifier;
		setModified();
	}

	public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
	{
		if ((_userIdentifier != null && _userIdentifier.Equals(ownerID)) || allowedUserIds.Contains(_userIdentifier))
		{
			return true;
		}
		return false;
	}

	public bool LocalPlayerIsOwner()
	{
		return IsOwner(PlatformManager.InternalLocalUserIdentifier);
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		return _userIdentifier?.Equals(ownerID) ?? false;
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return ownerID;
	}

	public bool HasPassword()
	{
		return !string.IsNullOrEmpty(passwordHash);
	}

	public string GetPasswordHash()
	{
		return passwordHash;
	}

	public bool SetPasswordHash(string _passwordHash, PlatformUserIdentifierAbs _userIdentifier)
	{
		if (IsOwner(_userIdentifier) && _passwordHash != passwordHash)
		{
			passwordHash = _passwordHash;
			allowedUserIds.Clear();
			SetModified();
			return true;
		}
		return false;
	}

	public bool CheckPasswordHash(string _passwordHash, PlatformUserIdentifierAbs _userIdentifier)
	{
		if (IsOwner(_userIdentifier) || !HasPassword())
		{
			return true;
		}
		if (_passwordHash == passwordHash)
		{
			allowedUserIds.Add(_userIdentifier);
			SetModified();
			return true;
		}
		return false;
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		int num = _br.ReadInt32();
		if (TraderData == null)
		{
			TraderData traderData = (TraderData = new TraderData());
		}
		if (allowedUserIds == null)
		{
			allowedUserIds = new List<PlatformUserIdentifierAbs>();
		}
		allowedUserIds.Clear();
		if (num < 3)
		{
			TraderData.Read(_br);
			int num2 = _br.ReadInt32();
			isLocked = _br.ReadBoolean();
			ownerID = PlatformUserIdentifierAbs.FromStream(_br);
			passwordHash = _br.ReadString();
			int num3 = _br.ReadInt32();
			for (int i = 0; i < num3; i++)
			{
				allowedUserIds.Add(PlatformUserIdentifierAbs.FromStream(_br));
			}
			if (num2 > 1)
			{
				rentalEndDay = _br.ReadInt32();
				rentalEndTime = 0uL;
			}
			else
			{
				rentalEndTime = _br.ReadUInt64();
				rentalEndDay = GameUtils.WorldTimeToDays(rentalEndTime);
			}
			TraderData.Read(_br);
			nextAutoBuy = (TraderData.TraderInfo.Rentable ? _br.ReadUInt64() : 0);
		}
		else
		{
			isLocked = _br.ReadBoolean();
			ownerID = PlatformUserIdentifierAbs.FromStream(_br);
			passwordHash = _br.ReadString();
			int num4 = _br.ReadInt32();
			for (int j = 0; j < num4; j++)
			{
				allowedUserIds.Add(PlatformUserIdentifierAbs.FromStream(_br));
			}
			rentalEndDay = _br.ReadInt32();
			rentalEndTime = 0uL;
			TraderData.Read(_br);
			nextAutoBuy = (TraderData.TraderInfo.Rentable ? _br.ReadUInt64() : 0);
		}
	}

	public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write(3);
		_bw.Write(isLocked);
		ownerID.ToStream(_bw);
		_bw.Write(passwordHash);
		_bw.Write(allowedUserIds.Count);
		for (int i = 0; i < allowedUserIds.Count; i++)
		{
			allowedUserIds[i].ToStream(_bw);
		}
		_bw.Write(rentalEndDay);
		TraderData.Write(_bw);
		if (TraderData.TraderInfo.Rentable)
		{
			_bw.Write(nextAutoBuy);
		}
	}

	public override void UpgradeDowngradeFrom(TileEntity _other)
	{
		base.UpgradeDowngradeFrom(_other);
		if (_other is ILockable)
		{
			ILockable lockable = _other as ILockable;
			SetLocked(lockable.IsLocked());
			SetOwner(lockable.GetOwner());
			allowedUserIds = new List<PlatformUserIdentifierAbs>(lockable.GetUsers());
			passwordHash = lockable.GetPasswordHash();
			setModified();
		}
	}

	public List<PlatformUserIdentifierAbs> GetUsers()
	{
		return allowedUserIds;
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.VendingMachine;
	}

	public override bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		return LocalPlayerUI.GetUIForPrimaryPlayer() != null;
	}

	public override void OnLockedServer(bool _success, int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
		if (_success && _context is VendingMachineLockContext vendingMachineLockContext)
		{
			GameManager.Instance.traderManager.TraderInventoryRequested(TraderData, _lockingPlayerID);
			vendingMachineLockContext.TraderData = TraderData.Clone();
		}
	}

	public override void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
		LocalPlayerUI uIForPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (!_success)
		{
			GameManager.ShowTooltip(uIForPrimaryPlayer.entityPlayer, Localization.Get("ttNoInteractPerson"), string.Empty, "ui_denied");
			uIForPrimaryPlayer.entityPlayer.OverrideFOV = -1f;
			uIForPrimaryPlayer.xui.Dialog.KeepZoomOnClose = false;
			return;
		}
		if (_context is VendingMachineLockContext { TraderData: not null } vendingMachineLockContext)
		{
			if (TraderData == null)
			{
				TraderData traderData = (TraderData = new TraderData());
			}
			TraderData.CopyFrom(vendingMachineLockContext.TraderData);
		}
		uIForPrimaryPlayer.xui.Trader.Trader = this;
		uIForPrimaryPlayer.windowManager.CloseAllOpenModalWindows();
		uIForPrimaryPlayer.windowManager.Open("trader", _bModal: true);
	}

	public override void OnUnlockedServer(int _unlockingPlayerID, ushort _channel)
	{
		TraderData.SetModified(this);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void ClearOwner()
	{
		ownerID = null;
	}

	public bool Rent()
	{
		if ((ownerID != null && !ownerID.Equals(PlatformManager.InternalLocalUserIdentifier)) || !TraderData.TraderInfo.Rentable)
		{
			return false;
		}
		XUi xui = GameManager.Instance.World.GetPrimaryPlayer().PlayerUI.xui;
		if (xui.PlayerInventory.CurrencyAmount >= TraderData.TraderInfo.RentCost)
		{
			ItemStack itemStack = new ItemStack(ItemClass.GetItem(TraderInfo.CurrencyItem), TraderData.TraderInfo.RentCost);
			xui.PlayerInventory.RemoveItem(itemStack);
			if (ownerID == null)
			{
				ownerID = PlatformManager.InternalLocalUserIdentifier;
				rentalEndDay = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime) + 30;
				SetAutoBuyTime(isInitial: true);
			}
			else
			{
				rentalEndDay += 30;
			}
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			primaryPlayer.RentedVMPosition = ToWorldPos();
			primaryPlayer.RentalEndDay = rentalEndDay;
			setModified();
			return true;
		}
		return false;
	}

	public void ClearVendingMachine()
	{
		TraderData.AvailableMoney = 0;
		TraderData.PrimaryInventory.Clear();
		ownerID = null;
		allowedUserIds.Clear();
		rentalEndTime = 0uL;
		passwordHash = "";
		setModified();
	}

	public bool TryAutoBuy(bool isInitial = true)
	{
		if (nextAutoBuy == 0L)
		{
			SetAutoBuyTime(isInitial: true);
		}
		if (GameManager.Instance.World.worldTime > nextAutoBuy)
		{
			GameRandom random = GameManager.Instance.lootManager.Random;
			if (random.RandomFloat < autoBuyThreshold && TraderData.PrimaryInventory.Count > minimumAutoBuyCount)
			{
				int num = random.RandomRange(1, Mathf.Max(TraderData.PrimaryInventory.Count / 10, 1));
				Log.Warning("Items Purchased: " + num);
				for (int i = 0; i < num; i++)
				{
					int num2 = 0;
					for (int j = 0; j < TraderData.PrimaryInventory.Count; j++)
					{
						if (TraderData.PrimaryInventory[j].Markup <= 0)
						{
							ItemStack item = TraderData.PrimaryInventory[j].Item;
							if (!((item.itemValue.ItemClass.IsBlock() ? Block.list[item.itemValue.type].EconomicValue : item.itemValue.ItemClass.EconomicValue) <= 0f) && item.itemValue.ItemClass.SellableToTrader)
							{
								num2++;
							}
						}
					}
					if (num2 <= 0)
					{
						continue;
					}
					int num3 = random.RandomRange(num2);
					num2 = 0;
					for (int k = 0; k < TraderData.PrimaryInventory.Count; k++)
					{
						if (TraderData.PrimaryInventory[k].Markup > 0)
						{
							continue;
						}
						ItemStack item2 = TraderData.PrimaryInventory[k].Item;
						if (!((item2.itemValue.ItemClass.IsBlock() ? Block.list[item2.itemValue.type].EconomicValue : item2.itemValue.ItemClass.EconomicValue) <= 0f) && item2.itemValue.ItemClass.SellableToTrader)
						{
							if (num2 == num3)
							{
								int count = item2.count;
								int buyPrice = XUiM_Trader.GetBuyPrice(LocalPlayerUI.GetUIForPrimaryPlayer().xui, item2.itemValue, count, null, k);
								TraderData.PrimaryInventory.RemoveAt(k);
								TraderData.AvailableMoney += buyPrice;
								break;
							}
							num2++;
						}
					}
				}
				autoBuyThreshold = autoBuyThresholdStep;
			}
			else
			{
				autoBuyThreshold += autoBuyThresholdStep;
			}
			SetAutoBuyTime(isInitial: false);
			return TryAutoBuy(isInitial: false);
		}
		return !isInitial;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetAutoBuyTime(bool isInitial)
	{
		uint num = 24000u;
		if (isInitial)
		{
			nextAutoBuy = GameManager.Instance.World.worldTime + num;
		}
		else
		{
			nextAutoBuy += num;
		}
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		if (!TraderData.TraderInfo.PlayerOwned && TraderData.TraderInfo.Rentable && ownerID != null && rentalEndDay <= GameUtils.WorldTimeToDays(world.worldTime))
		{
			ClearVendingMachine();
		}
	}
}
