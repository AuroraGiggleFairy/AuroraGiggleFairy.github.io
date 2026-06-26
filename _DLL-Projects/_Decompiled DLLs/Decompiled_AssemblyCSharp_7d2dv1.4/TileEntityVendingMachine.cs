using System.Collections.Generic;
using Platform;
using UnityEngine;

public class TileEntityVendingMachine : TileEntityTrader, ILockable
{
	public enum RentResult
	{
		Allowed,
		AlreadyRented,
		AlreadyRentingVM,
		NotEnoughMoney
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new const int ver = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs ownerID;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PlatformUserIdentifierAbs> allowedUserIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public string password;

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

	public bool IsRentable => TraderData.TraderInfo.Rentable;

	public float RentTimeRemaining => rentalEndDay - GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);

	public int RentalEndDay => rentalEndDay;

	public TileEntityVendingMachine(Chunk _chunk)
		: base(_chunk)
	{
		allowedUserIds = new List<PlatformUserIdentifierAbs>();
		isLocked = true;
		ownerID = null;
		password = "";
		rentalEndTime = 0uL;
		rentalEndDay = 0;
		TraderData = new TraderData();
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

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityVendingMachine(TileEntityVendingMachine _other)
		: base((Chunk)null)
	{
		allowedUserIds.AddRange(_other.allowedUserIds);
		isLocked = _other.isLocked;
		ownerID = _other.ownerID;
		password = _other.password;
		bUserAccessing = _other.bUserAccessing;
		rentalEndTime = _other.rentalEndTime;
		rentalEndDay = _other.rentalEndDay;
		TraderData = new TraderData(_other.TraderData);
		nextAutoBuy = _other.nextAutoBuy;
	}

	public override TileEntity Clone()
	{
		return new TileEntityVendingMachine(this);
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
		return !string.IsNullOrEmpty(password);
	}

	public string GetPassword()
	{
		return password;
	}

	public bool CheckPassword(string _password, PlatformUserIdentifierAbs _userIdentifier, out bool changed)
	{
		changed = false;
		if (_userIdentifier != null && _userIdentifier.Equals(ownerID))
		{
			if (Utils.HashString(_password) != password)
			{
				changed = true;
				password = Utils.HashString(_password);
				allowedUserIds.Clear();
				setModified();
			}
			return true;
		}
		if (Utils.HashString(_password) == password)
		{
			allowedUserIds.Add(_userIdentifier);
			setModified();
			return true;
		}
		return false;
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		int num = _br.ReadInt32();
		isLocked = _br.ReadBoolean();
		ownerID = PlatformUserIdentifierAbs.FromStream(_br);
		password = _br.ReadString();
		allowedUserIds = new List<PlatformUserIdentifierAbs>();
		int num2 = _br.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			allowedUserIds.Add(PlatformUserIdentifierAbs.FromStream(_br));
		}
		if (num > 1)
		{
			rentalEndDay = _br.ReadInt32();
		}
		else
		{
			rentalEndTime = _br.ReadUInt64();
			rentalEndDay = GameUtils.WorldTimeToDays(rentalEndTime);
		}
		TraderData.Read(0, _br);
		if (TraderData.TraderInfo.Rentable)
		{
			nextAutoBuy = _br.ReadUInt64();
		}
		syncNeeded = false;
	}

	public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write(2);
		_bw.Write(isLocked);
		ownerID.ToStream(_bw);
		_bw.Write(password);
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
			base.EntityId = lockable.EntityId;
			SetLocked(lockable.IsLocked());
			SetOwner(lockable.GetOwner());
			allowedUserIds = new List<PlatformUserIdentifierAbs>(lockable.GetUsers());
			password = lockable.GetPassword();
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
		password = "";
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
						if (TraderData.GetMarkupByIndex(j) <= 0)
						{
							ItemStack itemStack = TraderData.PrimaryInventory[j];
							if (!((itemStack.itemValue.ItemClass.IsBlock() ? Block.list[itemStack.itemValue.type].EconomicValue : itemStack.itemValue.ItemClass.EconomicValue) <= 0f) && itemStack.itemValue.ItemClass.SellableToTrader)
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
						if (TraderData.GetMarkupByIndex(k) > 0)
						{
							continue;
						}
						ItemStack itemStack2 = TraderData.PrimaryInventory[k];
						if (!((itemStack2.itemValue.ItemClass.IsBlock() ? Block.list[itemStack2.itemValue.type].EconomicValue : itemStack2.itemValue.ItemClass.EconomicValue) <= 0f) && itemStack2.itemValue.ItemClass.SellableToTrader)
						{
							if (num2 == num3)
							{
								int count = itemStack2.count;
								int buyPrice = XUiM_Trader.GetBuyPrice(LocalPlayerUI.GetUIForPrimaryPlayer().xui, itemStack2.itemValue, count, null, k);
								TraderData.PrimaryInventory.RemoveAt(k);
								TraderData.RemoveMarkup(k);
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
