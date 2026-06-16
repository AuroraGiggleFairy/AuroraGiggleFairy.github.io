using System.Collections.Generic;

public class LootManager
{
	public GameRandom Random;

	[PublicizedFrom(EAccessModifier.Private)]
	public WorldBase world;

	public static float[] POITierMod;

	public static float[] POITierBonus;

	public LootManager(WorldBase _world)
	{
		world = _world;
		Random = _world.GetGameRandom();
	}

	public void LootContainerOpened(ITileEntityLootable _tileEntity, int _entityIdThatOpenedIt, FastTags<TagGroup.Global> _containerTags)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || world.IsEditor() || _tileEntity.bTouched)
		{
			return;
		}
		_tileEntity.bTouched = true;
		_tileEntity.worldTimeTouched = world.GetWorldTime();
		LootContainer lootContainer = LootContainer.GetLootContainer(_tileEntity.lootListName);
		if (lootContainer == null)
		{
			return;
		}
		bool num = _tileEntity.IsEmpty();
		_tileEntity.bTouched = true;
		_tileEntity.worldTimeTouched = world.GetWorldTime();
		if (!num)
		{
			return;
		}
		EntityPlayer entityPlayer = (EntityPlayer)world.GetEntity(_entityIdThatOpenedIt);
		if (!(entityPlayer == null))
		{
			entityPlayer.MinEventContext.TileEntity = _tileEntity;
			entityPlayer.MinEventContext.BlockValue = _tileEntity.blockValue;
			if (entityPlayer.isEntityRemote)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageMinEventFire>().Setup(entityPlayer.entityId, -1, MinEventTypes.onSelfOpenLootContainer, _tileEntity.blockValue), _onlyClientsAttachedToAnEntity: false, entityPlayer.entityId);
			}
			else
			{
				entityPlayer.FireEvent(MinEventTypes.onSelfOpenLootContainer);
			}
			float lootStageMod = _tileEntity.LootStageMod;
			float lootStageBonus = _tileEntity.LootStageBonus;
			int num2 = (lootContainer.useUnmodifiedLootstage ? entityPlayer.unModifiedGameStage : entityPlayer.GetHighestPartyLootStage(lootStageMod, lootStageBonus));
			IList<ItemStack> list = lootContainer.Spawn(Random, _tileEntity.items.Length, num2, 0f, entityPlayer, _containerTags, lootContainer.UniqueItems, lootContainer.IgnoreLootProb, ignoreLootAbundance: false);
			for (int i = 0; i < list.Count; i++)
			{
				_tileEntity.items[i] = list[i].Clone();
			}
			entityPlayer.FireEvent(MinEventTypes.onSelfLootContainer);
		}
	}

	public void LootBagOpened(Bag _bag, Entity _bagOwner, int _entityIdThatOpenedIt)
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || world.IsEditor() || _bag == null || _bagOwner == null || _bag.Touched)
		{
			return;
		}
		_bag.Touched = true;
		LootContainer lootContainer = LootContainer.GetLootContainer(_bagOwner.GetLootList());
		if (lootContainer == null || !_bag.IsEmpty())
		{
			return;
		}
		EntityPlayer entityPlayer = (EntityPlayer)world.GetEntity(_entityIdThatOpenedIt);
		if (!(entityPlayer == null))
		{
			if (entityPlayer.isEntityRemote)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageMinEventFire>().Setup(entityPlayer.entityId, -1, MinEventTypes.onSelfOpenLootContainer, BlockValue.Air), _onlyClientsAttachedToAnEntity: false, entityPlayer.entityId);
			}
			else
			{
				entityPlayer.FireEvent(MinEventTypes.onSelfOpenLootContainer);
			}
			int num = (lootContainer.useUnmodifiedLootstage ? entityPlayer.unModifiedGameStage : entityPlayer.GetHighestPartyLootStage(0f, 0f));
			IList<ItemStack> list = lootContainer.Spawn(Random, _bag.GetSlots().Length, num, 0f, entityPlayer, _bagOwner.EntityTags, lootContainer.UniqueItems, lootContainer.IgnoreLootProb, ignoreLootAbundance: false);
			ItemStack[] slots = _bag.GetSlots();
			for (int i = 0; i < list.Count; i++)
			{
				slots[i] = list[i].Clone();
			}
			entityPlayer.FireEvent(MinEventTypes.onSelfLootContainer);
		}
	}
}
