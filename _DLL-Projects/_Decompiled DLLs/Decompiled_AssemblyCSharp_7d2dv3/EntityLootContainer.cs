using System;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityLootContainer : EntityItem
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int deathUpdateTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int timeStayAfterDeath;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bRemoved;

	public string OverrideLootList = "";

	public string OverrideName = "";

	public override string LocalizedEntityName
	{
		get
		{
			if (!string.IsNullOrEmpty(OverrideName))
			{
				return Localization.Get(OverrideName);
			}
			return base.LocalizedEntityName;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		Collider[] componentsInChildren = base.transform.GetComponentsInChildren<Collider>();
		foreach (Collider obj in componentsInChildren)
		{
			obj.gameObject.tag = "E_BP_Body";
			obj.enabled = true;
			obj.gameObject.layer = 13;
			obj.gameObject.AddMissingComponent<RootTransformRefEntity>().RootTransform = base.transform;
		}
		SetDead();
	}

	public void SetContent(ItemStack[] _inventory)
	{
		SetContent(_inventory, 0);
	}

	public void SetContent(ItemStack[] _inventory, int _slotCountOverride)
	{
		int val = 0;
		if (_slotCountOverride > 0)
		{
			val = _slotCountOverride;
		}
		else
		{
			LootContainer lootContainer = LootContainer.GetLootContainer(GetLootList());
			if (lootContainer != null)
			{
				Vector2i size = lootContainer.size;
				val = size.x * size.y;
			}
			val = Math.Max(val, (_inventory != null) ? _inventory.Length : 0);
		}
		if (val > 0)
		{
			ItemStack[] array = ItemStack.CreateArray(val);
			int num = 0;
			while (_inventory != null && num < _inventory.Length && num < array.Length)
			{
				array[num] = _inventory[num].Clone();
				num++;
			}
			bag.SetSlots(array);
			bag.Touched = true;
		}
	}

	public override void CopyPropertiesFromEntityClass()
	{
		base.CopyPropertiesFromEntityClass();
		EntityClass entityClass = EntityClass.list[base.entityClass];
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropTimeStayAfterDeath))
		{
			timeStayAfterDeath = (int)(StringParsers.ParseFloat(entityClass.Properties.Values[EntityClass.PropTimeStayAfterDeath]) * 20f);
		}
		else
		{
			timeStayAfterDeath = 100;
		}
	}

	public override void OnUpdateEntity()
	{
		base.OnUpdateEntity();
		try
		{
			if (!(SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? LockManager.Instance.IsLockedServer(this, 0) : LockManager.Instance.IsLockedByLocalPlayer(this, 0)))
			{
				if (bag.Touched && bag.IsEmpty())
				{
					removeBackpack();
				}
				else if (deathUpdateTime >= timeStayAfterDeath - 1)
				{
					removeBackpack();
				}
				else
				{
					deathUpdateTime++;
				}
			}
		}
		finally
		{
		}
	}

	public override bool AllowActivationCommand(ReadOnlySpan<char> _commandName, EntityPlayerLocal _playerFocusing)
	{
		if (CommandIs(_commandName, "search"))
		{
			return bag != null;
		}
		return base.AllowActivationCommand(_commandName, _playerFocusing);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeBackpack()
	{
		deathUpdateTime = timeStayAfterDeath;
		bRemoved = true;
		MarkToUnload();
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale = 1f)
	{
		if (_strength >= 99999)
		{
			removeBackpack();
		}
		return base.DamageEntity(_damageSource, _strength, _criticalHit, impulseScale);
	}

	public override bool IsMarkedForUnload()
	{
		if (base.IsMarkedForUnload())
		{
			return bRemoved;
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createMesh()
	{
	}

	public override string GetLootList()
	{
		if (!(OverrideLootList != ""))
		{
			return lootList;
		}
		return OverrideLootList;
	}

	public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		base.Write(_bw, _bNetworkWrite);
		_bw.Write(OverrideLootList);
		_bw.Write(OverrideName);
		bag.Write(_bw);
	}

	public override void Read(byte _version, BinaryReader _br)
	{
		base.Read(_version, _br);
		if (_version < 35)
		{
			int num = _br.ReadUInt16();
			ItemStack[] array = ItemStack.CreateArray(num);
			for (int i = 0; i < num; i++)
			{
				array[i].Read(_br);
			}
			num = _br.ReadUInt16();
			for (int j = 0; j < num; j++)
			{
				new ItemStack().Read(_br);
			}
			if (_version >= 30)
			{
				OverrideLootList = _br.ReadString();
				OverrideName = _br.ReadString();
			}
			LootContainer lootContainer = LootContainer.GetLootContainer(GetLootList());
			int num2 = array.Length;
			if (lootContainer != null)
			{
				num2 = Math.Max(num2, lootContainer.size.x * lootContainer.size.y);
			}
			if (num2 > array.Length)
			{
				ItemStack[] array2 = ItemStack.CreateArray(num2);
				for (int k = 0; k < array.Length; k++)
				{
					array2[k] = array[k];
				}
				array = array2;
			}
			bag.SetSlots(array);
		}
		else
		{
			if (_version >= 30)
			{
				OverrideLootList = _br.ReadString();
				OverrideName = _br.ReadString();
			}
			if (_version >= 35)
			{
				bag.ReadInto(_br);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void HandleNavObject()
	{
		if (EntityClass.list[entityClass].NavObject != "")
		{
			NavObject = NavObjectManager.Instance.RegisterNavObject(EntityClass.list[entityClass].NavObject, this);
		}
	}

	public override string ToString()
	{
		return string.Format("[type={0}, name={1}]", GetType().Name, (itemClass != null) ? itemClass.GetItemName() : "?");
	}

	public override void OnUnlockedServer(int _unlockingPlayerId, ushort _channel)
	{
		if (bag.IsEmpty())
		{
			KillLootContainer();
		}
	}
}
