using System;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityLootContainer : EntityItem
{
	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] isInventory;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] isBag;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int deathUpdateTime;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public int timeStayAfterDeath;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bRemoved;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool forceInventoryCreate;

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
		if (lootContainer != null)
		{
			lootContainer.entityId = entityId;
		}
	}

	public void SetContent(ItemStack[] _inventory)
	{
		isInventory = _inventory;
		lootContainer = null;
		forceInventoryCreate = true;
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
		if (lootContainer != null && deathUpdateTime > 0)
		{
			bool flag = GameManager.Instance.GetEntityIDForLockedTileEntity(lootContainer) != -1;
			if (!bRemoved && !lootContainer.IsUserAccessing() && !flag && ((lootContainer.bTouched && lootContainer.IsEmpty()) || deathUpdateTime >= timeStayAfterDeath - 1))
			{
				removeBackpack();
			}
		}
		deathUpdateTime++;
		if (world.IsRemote() || (!forceInventoryCreate && lootContainer != null))
		{
			return;
		}
		lootContainer = new TileEntityLootContainer((Chunk)null);
		lootContainer.bTouched = false;
		lootContainer.entityId = entityId;
		lootContainer.lootListName = GetLootList();
		lootContainer.SetContainerSize(LootContainer.GetLootContainer(lootContainer.lootListName).size);
		if (isInventory != null)
		{
			lootContainer.bTouched = true;
			for (int i = 0; i < isInventory.Length; i++)
			{
				if (!isInventory[i].IsEmpty())
				{
					lootContainer.AddItem(isInventory[i]);
				}
			}
		}
		lootContainer.SetModified();
		forceInventoryCreate = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeBackpack()
	{
		deathUpdateTime = timeStayAfterDeath;
		bRemoved = true;
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
			return lootListOnDeath;
		}
		return OverrideLootList;
	}

	public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		base.Write(_bw, _bNetworkWrite);
		_bw.Write((ushort)((isInventory != null) ? ((uint)isInventory.Length) : 0u));
		int num = 0;
		while (isInventory != null && num < isInventory.Length)
		{
			isInventory[num].Write(_bw);
			num++;
		}
		_bw.Write((ushort)((isBag != null) ? ((uint)isBag.Length) : 0u));
		int num2 = 0;
		while (isBag != null && num2 < isBag.Length)
		{
			isBag[num2].Write(_bw);
			num2++;
		}
		_bw.Write(OverrideLootList);
		_bw.Write(OverrideName);
	}

	public override void Read(byte _version, BinaryReader _br)
	{
		base.Read(_version, _br);
		int num = _br.ReadUInt16();
		isInventory = new ItemStack[num];
		for (int i = 0; i < num; i++)
		{
			isInventory[i] = new ItemStack();
			isInventory[i].Read(_br);
		}
		num = _br.ReadUInt16();
		isBag = new ItemStack[num];
		for (int j = 0; j < num; j++)
		{
			isBag[j] = new ItemStack();
			isBag[j].Read(_br);
		}
		if (_version >= 30)
		{
			OverrideLootList = _br.ReadString();
			OverrideName = _br.ReadString();
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
}
