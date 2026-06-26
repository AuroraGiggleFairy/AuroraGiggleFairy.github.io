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
		if (world.IsEditor() || _tileEntity.bTouched)
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
			entityPlayer.FireEvent(MinEventTypes.onSelfOpenLootContainer);
			float containerMod = 0f;
			float containerBonus = 0f;
			if (_tileEntity.EntityId == -1)
			{
				containerMod = _tileEntity.LootStageMod;
				containerBonus = _tileEntity.LootStageBonus;
			}
			int num2 = (lootContainer.useUnmodifiedLootstage ? entityPlayer.unModifiedGameStage : entityPlayer.GetHighestPartyLootStage(containerMod, containerBonus));
			IList<ItemStack> list = lootContainer.Spawn(Random, _tileEntity.items.Length, num2, 0f, entityPlayer, _containerTags, lootContainer.UniqueItems, lootContainer.IgnoreLootProb);
			for (int i = 0; i < list.Count; i++)
			{
				_tileEntity.items[i] = list[i].Clone();
			}
			entityPlayer.FireEvent(MinEventTypes.onSelfLootContainer);
		}
	}
}
