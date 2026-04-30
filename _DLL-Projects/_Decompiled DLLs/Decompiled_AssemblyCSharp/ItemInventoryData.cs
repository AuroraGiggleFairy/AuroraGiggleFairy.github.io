using System.Collections.Generic;
using UnityEngine;

public class ItemInventoryData
{
	public enum SoundPlayType
	{
		None = -2,
		IdleReady,
		Idle
	}

	public ItemClass item;

	public ItemStack itemStack;

	public readonly EntityAlive holdingEntity;

	public int holdingEntitySoundID = -2;

	public World world;

	public IGameManager gameManager;

	public List<ItemActionData> actionData;

	public WorldRayHitInfo hitInfo;

	public int slotIdx;

	public Transform model => holdingEntity.inventory.models[slotIdx];

	public ItemValue itemValue
	{
		get
		{
			return holdingEntity.inventory[slotIdx];
		}
		set
		{
			holdingEntity.inventory[slotIdx] = value;
		}
	}

	public void Changed()
	{
		holdingEntity.inventory.Changed();
	}

	public ItemInventoryData(ItemClass _item, ItemStack _itemStack, IGameManager _gameManager, EntityAlive _holdingEntity, int _slotIdx)
	{
		item = _item;
		itemStack = _itemStack;
		world = _holdingEntity.world;
		holdingEntity = _holdingEntity;
		gameManager = _gameManager;
		slotIdx = _slotIdx;
		hitInfo = new WorldRayHitInfo();
		actionData = new List<ItemActionData>();
		actionData.Add(null);
		actionData.Add(null);
	}
}
