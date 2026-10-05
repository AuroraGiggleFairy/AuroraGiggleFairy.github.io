using System.Collections.Generic;
using Audio;
using UnityEngine;

namespace DoomAutoPickup;

public class AutoPickup : MonoBehaviour
{
	public const string Sound = "doom_dsitemup";

	public const string Effect = "doomBuffPickup";

	private EntityLootContainer _bag;

	private bool _taken;

	public void Configure(EntityLootContainer bag)
	{
		_bag = bag;
		_taken = false;
	}

	public void Touched(Collider other)
	{
		if (_taken || (Object)(object)other == (Object)null || (Object)(object)_bag == (Object)null || ((Entity)_bag).bag == null || SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			return;
		}
		EntityPlayer componentInParent = ((Component)((Component)other).transform).GetComponentInParent<EntityPlayer>();
		if ((Object)(object)componentInParent == (Object)null || ((Entity)componentInParent).IsDead())
		{
			return;
		}
		Bag bag = ((Entity)_bag).bag;
		if (!bag.Touched)
		{
			GameManager.Instance.lootManager.LootBagOpened(bag, (Entity)(object)_bag, ((Entity)componentInParent).entityId);
			bag.Touched = true;
		}
		ItemStack[] array = Taken(bag);
		if (array.Length == 0 || !Fits(componentInParent, array))
		{
			return;
		}
		_taken = true;
		bag.SetSlots(ItemStack.CreateArray(bag.GetSlots().Length));
		EntityPlayerLocal val = (EntityPlayerLocal)(object)((componentInParent is EntityPlayerLocal) ? componentInParent : null);
		if ((Object)(object)val != (Object)null)
		{
			Give(val, array);
			return;
		}
		ClientInfo val2 = SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForEntityId(((Entity)componentInParent).entityId);
		if (val2 != null)
		{
			val2.SendPackage((NetPackage)(object)NetPackageManager.GetPackage<NetPackageDoomAutoPickup>().Setup(array));
		}
	}

	private static bool Fits(EntityPlayer player, ItemStack[] stacks)
	{
		Bag bag = ((Entity)player).bag;
		Inventory inventory = ((EntityAlive)player).inventory;
		for (int i = 0; i < stacks.Length; i++)
		{
			if ((bag == null || !bag.CanTakeItem(stacks[i])) && (inventory == null || !inventory.CanStack(stacks[i])))
			{
				return false;
			}
		}
		return true;
	}

	private static ItemStack[] Taken(Bag contents)
	{
		ItemStack[] slots = contents.GetSlots();
		List<ItemStack> list = new List<ItemStack>();
		foreach (ItemStack val in slots)
		{
			if (val != null && !val.IsEmpty())
			{
				list.Add(val.Clone());
			}
		}
		return list.ToArray();
	}

	public static void Give(EntityPlayerLocal local, ItemStack[] stacks)
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)local == (Object)null || stacks == null)
		{
			return;
		}
		bool flag = false;
		foreach (ItemStack val in stacks)
		{
			if (val != null && !val.IsEmpty())
			{
				ItemStack val2 = val.Clone();
				if (!((InventoryBase)((Entity)local).bag).AddItem(val2) && !((InventoryBase)((EntityAlive)local).inventory).AddItem(val2))
				{
					GameManager.Instance.ItemDropServer(val2, ((Entity)local).GetPosition(), Vector3.zero, ((Entity)local).entityId, 60f, false);
					continue;
				}
				((Entity)local).AddUIHarvestingItem(val.Clone(), false);
				flag = true;
			}
		}
		if (flag)
		{
			Manager.PlayInsidePlayerHead("doom_dsitemup", -1, 0f, false, false);
			((EntityAlive)local).Buffs.AddBuff("doomBuffPickup", -1, true, false, -1f);
		}
	}
}
