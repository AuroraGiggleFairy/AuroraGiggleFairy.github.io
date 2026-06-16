using System;
using System.Collections.Generic;

public static class TileEntityLegacyUtils
{
	public struct LegacyState(Vector3i _chunkPos, int _entityId, ulong _heapMapUpdateTime, ulong _heapMapLastTime)
	{
		public Vector3i chunkPos = _chunkPos;

		public int entityId = _entityId;

		public ulong heapMapUpdateTime = _heapMapUpdateTime;

		public ulong heapMapLastTime = _heapMapLastTime;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public struct LegacyLootContainerData
	{
		public LegacyState state;

		public string lootListName;

		public Vector2i containerSize;

		public bool bTouched;

		public ulong worldTimeTouched;

		public bool bPlayerBackpack;

		public ItemStack[] items;

		public bool bPlayerStorage;

		public bool hasPreferences;

		public PreferenceTracker preferences;

		public PackedBoolArray slotLocks;
	}

	public static bool TryReadLegacyType(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode, TileEntityType _type, Func<int, int, int, BlockValue> _getBlock, out TileEntity _tileEntity)
	{
		if (_eStreamMode != TileEntity.StreamModeRead.Persistency)
		{
			_tileEntity = null;
			return false;
		}
		switch (_type)
		{
		case TileEntityType.Loot:
		{
			_tileEntity = ReadLegacyLootIntoComposite(_br, _getBlock, out var _);
			return true;
		}
		case TileEntityType.SecureLoot:
			_tileEntity = ReadLegacySecureLootIntoComposite(_br, _getBlock);
			return true;
		case TileEntityType.SecureLootSigned:
			_tileEntity = ReadLegacySecureLootSignedIntoComposite(_br, _getBlock);
			return true;
		case TileEntityType.LandClaim:
			_tileEntity = ReadLegacyLandClaimIntoComposite(_br, _getBlock);
			return true;
		case TileEntityType.Sign:
			_tileEntity = ReadLegacySignIntoComposite(_br, _getBlock);
			return true;
		case TileEntityType.SecureDoor:
			_tileEntity = ReadLegacySecureDoorIntoComposite(_br, _getBlock);
			return true;
		case TileEntityType.GoreBlock:
			ReadLegacyGoreAndDiscard(_br);
			_tileEntity = null;
			return true;
		case TileEntityType.Trader:
			_tileEntity = null;
			return true;
		default:
			_tileEntity = null;
			return false;
		}
	}

	public static void MigrateLegacyMetadata(TileEntityComposite _teComposite, BlockValue _blockValue)
	{
		byte meta = _blockValue.meta;
		_blockValue.meta = 0;
		if (_teComposite.TryGetSelfOrFeature<TEFeatureDoor>(out var _typedTe))
		{
			bool open = (meta & 1) != 0;
			_typedTe.SetOpen(open);
			if (_teComposite.TryGetSelfOrFeature<TEFeatureLockable>(out var _typedTe2))
			{
				bool locked = (meta & 4) != 0;
				_typedTe2.SetLocked(locked);
			}
		}
	}

	public static Bag ReadLegacyLootIntoBag(PooledBinaryReader _br)
	{
		LegacyLootContainerData legacyLootContainerData = ReadLegacyLootContainerData(_br);
		Bag bag = new Bag(legacyLootContainerData.items.Length);
		bag.SetSlots(legacyLootContainerData.items);
		bag.LockedSlots = legacyLootContainerData.slotLocks;
		if (bag.LockedSlots != null)
		{
			bag.LockedSlots.Length = bag.SlotCount;
		}
		bag.Touched = legacyLootContainerData.bTouched;
		bag.preferences = (legacyLootContainerData.hasPreferences ? legacyLootContainerData.preferences : null);
		return bag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static LegacyLootContainerData ReadLegacyLootContainerData(PooledBinaryReader _br)
	{
		LegacyLootContainerData result = default(LegacyLootContainerData);
		int num = _br.ReadUInt16();
		Vector3i chunkPos = StreamUtils.ReadVector3i(_br);
		int entityId = _br.ReadInt32();
		ulong num2 = 0uL;
		ulong heapMapLastTime = 0uL;
		if (num > 1)
		{
			num2 = _br.ReadUInt64();
			heapMapLastTime = num2 - AIDirector.GetActivityWorldTimeDelay();
		}
		if (num >= 18)
		{
			num = _br.ReadUInt16();
		}
		if (num > 8)
		{
			if (_br.ReadBoolean())
			{
				result.lootListName = _br.ReadString();
			}
			result.containerSize = new Vector2i
			{
				x = _br.ReadUInt16(),
				y = _br.ReadUInt16()
			};
			result.bTouched = _br.ReadBoolean();
			result.worldTimeTouched = _br.ReadUInt32();
			result.bPlayerBackpack = _br.ReadBoolean();
			int num3 = Math.Min(_br.ReadInt16(), result.containerSize.x * result.containerSize.y);
			result.items = ItemStack.CreateArray(result.containerSize.x * result.containerSize.y);
			if (num < 3)
			{
				for (int i = 0; i < num3; i++)
				{
					result.items[i].Clear();
					result.items[i].ReadOld(_br);
				}
			}
			else
			{
				for (int j = 0; j < num3; j++)
				{
					result.items[j].Clear();
					result.items[j].Read(_br);
				}
			}
			result.bPlayerStorage = _br.ReadBoolean();
			if (num > 9)
			{
				result.hasPreferences = _br.ReadBoolean();
				if (result.hasPreferences)
				{
					result.preferences = new PreferenceTracker(-1);
					result.preferences.Read(_br);
				}
			}
			if (num >= 12)
			{
				result.slotLocks = new PackedBoolArray();
				result.slotLocks.Read(_br);
			}
			else
			{
				result.slotLocks = new PackedBoolArray(num3);
			}
			result.state = new LegacyState(chunkPos, entityId, num2, heapMapLastTime);
			return result;
		}
		throw new Exception("Outdated loot data");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TileEntityComposite ReadLegacyLootIntoComposite(PooledBinaryReader _br, Func<int, int, int, BlockValue> _getBlock, out BlockValue _blockValue)
	{
		LegacyLootContainerData legacyLootContainerData = ReadLegacyLootContainerData(_br);
		_blockValue = _getBlock(legacyLootContainerData.state.chunkPos.x, legacyLootContainerData.state.chunkPos.y, legacyLootContainerData.state.chunkPos.z);
		if (!(_blockValue.Block is BlockCompositeTileEntity block))
		{
			Log.Error("TileEntityComposite.ReadLegacyLootIntoComposite: failed to convert legacy TE data into a composite TE.");
			return null;
		}
		TileEntityComposite tileEntityComposite = new TileEntityComposite(block, legacyLootContainerData.state);
		TEFeatureStorage feature = tileEntityComposite.GetFeature<TEFeatureStorage>();
		if (feature != null)
		{
			feature.lootListName = legacyLootContainerData.lootListName;
			feature.SetContainerSize(legacyLootContainerData.containerSize);
			feature.bTouched = legacyLootContainerData.bTouched;
			feature.worldTimeTouched = legacyLootContainerData.worldTimeTouched;
			feature.bPlayerStorage = legacyLootContainerData.bPlayerStorage;
			feature.items = legacyLootContainerData.items;
			if (legacyLootContainerData.hasPreferences && legacyLootContainerData.preferences != null)
			{
				feature.preferences = legacyLootContainerData.preferences;
			}
			feature.SlotLocks = legacyLootContainerData.slotLocks;
		}
		return tileEntityComposite;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TileEntityComposite ReadLegacySecureLootIntoComposite(PooledBinaryReader _br, Func<int, int, int, BlockValue> _getBlock)
	{
		BlockValue _blockValue;
		TileEntityComposite tileEntityComposite = ReadLegacyLootIntoComposite(_br, _getBlock, out _blockValue);
		_br.ReadInt32();
		_br.ReadBoolean();
		bool locked = _br.ReadBoolean();
		PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromStream(_br);
		string passwordHash = _br.ReadString();
		int num = _br.ReadInt32();
		List<PlatformUserIdentifierAbs> list = null;
		for (int i = 0; i < num; i++)
		{
			if (list == null)
			{
				list = new List<PlatformUserIdentifierAbs>();
			}
			list.Add(PlatformUserIdentifierAbs.FromStream(_br));
		}
		if (tileEntityComposite == null)
		{
			return null;
		}
		tileEntityComposite.SetOwner(platformUserIdentifierAbs);
		TEFeatureLockable feature = tileEntityComposite.GetFeature<TEFeatureLockable>();
		if (feature != null)
		{
			feature.SetLocked(locked);
			feature.SetPasswordHash(passwordHash, platformUserIdentifierAbs);
			if (list != null)
			{
				foreach (PlatformUserIdentifierAbs item in list)
				{
					feature.CheckPasswordHash(passwordHash, item);
				}
			}
		}
		return tileEntityComposite;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TileEntityComposite ReadLegacySecureLootSignedIntoComposite(PooledBinaryReader _br, Func<int, int, int, BlockValue> _getBlock)
	{
		TileEntityComposite tileEntityComposite = ReadLegacySecureLootIntoComposite(_br, _getBlock);
		int num = _br.ReadInt32();
		_br.ReadBoolean();
		_br.ReadBoolean();
		PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromStream(_br);
		_br.ReadString();
		int num2 = _br.ReadInt32();
		for (int i = 0; i < num2; i++)
		{
			PlatformUserIdentifierAbs.FromStream(_br);
		}
		AuthoredText authoredText = null;
		authoredText = ((num <= 1) ? new AuthoredText(_br.ReadString(), tileEntityComposite?.Owner ?? platformUserIdentifierAbs) : AuthoredText.FromStream(_br));
		if (tileEntityComposite == null)
		{
			return null;
		}
		tileEntityComposite.GetFeature<TEFeatureSignable>()?.SetText(authoredText, _syncData: false);
		return tileEntityComposite;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TileEntityComposite ReadLegacyLandClaimIntoComposite(PooledBinaryReader _br, Func<int, int, int, BlockValue> _getBlock)
	{
		ushort num = _br.ReadUInt16();
		Vector3i chunkPos = StreamUtils.ReadVector3i(_br);
		int entityId = _br.ReadInt32();
		ulong num2 = 0uL;
		ulong heapMapLastTime = 0uL;
		if (num > 1)
		{
			num2 = _br.ReadUInt64();
			heapMapLastTime = num2 - AIDirector.GetActivityWorldTimeDelay();
		}
		_br.ReadInt32();
		PlatformUserIdentifierAbs owner = PlatformUserIdentifierAbs.FromStream(_br);
		bool showBounds = _br.ReadBoolean();
		if (!(_getBlock?.Invoke(chunkPos.x, chunkPos.y, chunkPos.z).Block is BlockCompositeTileEntity block))
		{
			Log.Error("TileEntityComposite.ReadLegacyLandClaimIntoComposite: failed to convert legacy TE data into a composite TE.");
			return null;
		}
		LegacyState state = new LegacyState(chunkPos, entityId, num2, heapMapLastTime);
		TileEntityComposite tileEntityComposite = new TileEntityComposite(block, state);
		if (tileEntityComposite.Owner == null)
		{
			tileEntityComposite.SetOwner(owner);
		}
		TEFeatureLandClaim feature = tileEntityComposite.GetFeature<TEFeatureLandClaim>();
		if (feature != null)
		{
			feature.ShowBounds = showBounds;
		}
		return tileEntityComposite;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TileEntityComposite ReadLegacySignIntoComposite(PooledBinaryReader _br, Func<int, int, int, BlockValue> _getBlock)
	{
		ushort num = _br.ReadUInt16();
		Vector3i chunkPos = StreamUtils.ReadVector3i(_br);
		int entityId = _br.ReadInt32();
		ulong num2 = 0uL;
		ulong heapMapLastTime = 0uL;
		if (num > 1)
		{
			num2 = _br.ReadUInt64();
			heapMapLastTime = num2 - AIDirector.GetActivityWorldTimeDelay();
		}
		int num3 = _br.ReadInt32();
		bool locked = _br.ReadBoolean();
		PlatformUserIdentifierAbs platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromStream(_br);
		AuthoredText authoredText = null;
		if (num3 > 1)
		{
			authoredText = AuthoredText.FromStream(_br);
		}
		string passwordHash = _br.ReadString();
		int num4 = _br.ReadInt32();
		List<PlatformUserIdentifierAbs> list = null;
		for (int i = 0; i < num4; i++)
		{
			if (list == null)
			{
				list = new List<PlatformUserIdentifierAbs>();
			}
			list.Add(PlatformUserIdentifierAbs.FromStream(_br));
		}
		if (num3 <= 1)
		{
			authoredText = new AuthoredText(_br.ReadString(), platformUserIdentifierAbs);
		}
		if (!(_getBlock?.Invoke(chunkPos.x, chunkPos.y, chunkPos.z).Block is BlockCompositeTileEntity block))
		{
			Log.Error("TileEntityComposite.ReadLegacySignIntoComposite: failed to convert legacy TE data into a composite TE.");
			return null;
		}
		LegacyState state = new LegacyState(chunkPos, entityId, num2, heapMapLastTime);
		TileEntityComposite tileEntityComposite = new TileEntityComposite(block, state);
		if (tileEntityComposite.Owner == null)
		{
			tileEntityComposite.SetOwner(platformUserIdentifierAbs);
		}
		tileEntityComposite.GetFeature<TEFeatureSignable>()?.SetText(authoredText);
		TEFeatureLockable feature = tileEntityComposite.GetFeature<TEFeatureLockable>();
		if (feature != null)
		{
			feature.SetLocked(locked);
			feature.SetPasswordHash(passwordHash, platformUserIdentifierAbs);
			if (list != null)
			{
				foreach (PlatformUserIdentifierAbs item in list)
				{
					feature.CheckPasswordHash(passwordHash, item);
				}
			}
		}
		return tileEntityComposite;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TileEntityComposite ReadLegacySecureDoorIntoComposite(PooledBinaryReader _br, Func<int, int, int, BlockValue> _getBlock)
	{
		BlockValue _blockValue;
		TileEntityComposite tileEntityComposite = ReadLegacyLootIntoComposite(_br, _getBlock, out _blockValue);
		int num = _br.ReadInt32();
		bool locked = false;
		PlatformUserIdentifierAbs platformUserIdentifierAbs = null;
		List<PlatformUserIdentifierAbs> list = null;
		string text = string.Empty;
		if (num > 0)
		{
			_br.ReadBoolean();
			locked = _br.ReadBoolean();
			platformUserIdentifierAbs = PlatformUserIdentifierAbs.FromStream(_br);
			int num2 = _br.ReadInt32();
			list = new List<PlatformUserIdentifierAbs>();
			for (int i = 0; i < num2; i++)
			{
				list.Add(PlatformUserIdentifierAbs.FromStream(_br));
			}
			text = _br.ReadString();
		}
		if (tileEntityComposite == null)
		{
			return null;
		}
		if (platformUserIdentifierAbs != null)
		{
			tileEntityComposite.SetOwner(platformUserIdentifierAbs);
		}
		TEFeatureLockable feature = tileEntityComposite.GetFeature<TEFeatureLockable>();
		if (feature != null)
		{
			feature.SetLocked(locked);
			if (!string.IsNullOrEmpty(text))
			{
				feature.SetPasswordHash(text, platformUserIdentifierAbs);
			}
			if (list != null)
			{
				foreach (PlatformUserIdentifierAbs item in list)
				{
					feature.CheckPasswordHash(text, item);
				}
			}
		}
		return tileEntityComposite;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ReadLegacyGoreAndDiscard(PooledBinaryReader _br)
	{
		uint num = _br.ReadUInt16();
		StreamUtils.ReadVector3i(_br);
		_br.ReadInt32();
		if (num > 1)
		{
			_br.ReadUInt64();
		}
		if (num > 8)
		{
			if (_br.ReadBoolean())
			{
				_br.ReadString();
			}
			Vector2i vector2i = new Vector2i
			{
				x = _br.ReadUInt16(),
				y = _br.ReadUInt16()
			};
			_br.ReadBoolean();
			_br.ReadUInt32();
			_br.ReadBoolean();
			int num2 = Math.Min(_br.ReadInt16(), vector2i.x * vector2i.y);
			ItemStack itemStack = new ItemStack();
			if (num < 3)
			{
				for (int i = 0; i < num2; i++)
				{
					itemStack.ReadOld(_br);
				}
			}
			else
			{
				for (int j = 0; j < num2; j++)
				{
					itemStack.Read(_br);
				}
			}
			_br.ReadBoolean();
			if (num > 9 && _br.ReadBoolean())
			{
				new PreferenceTracker(-1).Read(_br);
			}
			if (num >= 12)
			{
				new PackedBoolArray().Read(_br);
			}
			_br.ReadUInt64();
			return;
		}
		throw new Exception("Outdated loot data");
	}

	public static TraderData ReadLegacyTileEntityTraderData(PooledBinaryReader _br)
	{
		_ = (byte)_br.ReadInt32();
		ushort num = _br.ReadUInt16();
		StreamUtils.ReadVector3i(_br);
		_br.ReadInt32();
		if (num > 1)
		{
			_br.ReadUInt64();
		}
		TraderData traderData = new TraderData();
		_br.ReadInt32();
		traderData.Read(_br);
		return traderData;
	}
}
