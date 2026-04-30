using System.IO;
using UnityEngine.Scripting;

[Preserve]
public class EntityNPC : EntityAlive
{
	public string npcID = "";

	public override string LocalizedEntityName => Localization.Get(EntityName);

	public NPCInfo NPCInfo
	{
		get
		{
			if (npcID != "")
			{
				return NPCInfo.npcInfoList[npcID];
			}
			return null;
		}
	}

	public override void CopyPropertiesFromEntityClass()
	{
		base.CopyPropertiesFromEntityClass();
		EntityClass entityClass = EntityClass.list[base.entityClass];
		if (entityClass.Properties.Values.ContainsKey(EntityClass.PropNPCID))
		{
			npcID = entityClass.Properties.Values[EntityClass.PropNPCID];
		}
	}

	public override void Read(byte _version, BinaryReader _br)
	{
		base.Read(_version, _br);
		bag.SetSlots(GameUtils.ReadItemStack(_br));
	}

	public override void Write(BinaryWriter _bw, bool _bNetworkWrite)
	{
		base.Write(_bw, _bNetworkWrite);
		GameUtils.WriteItemStack(_bw, bag.GetSlots());
	}

	public override bool IsSavedToFile()
	{
		if (GetSpawnerSource() != EnumSpawnerSource.Dynamic || IsDead())
		{
			return base.IsSavedToFile();
		}
		return false;
	}

	public override float GetSeeDistance()
	{
		return 80f;
	}

	public override void VisiblityCheck(float _distanceSqr, bool _masterIsZooming)
	{
		bool bVisible = _distanceSqr < (float)(_masterIsZooming ? 14400 : 8100);
		emodel.SetVisible(bVisible);
	}

	public override bool CanBePushed()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool canDespawn()
	{
		if (world.GetPlayers().Count == 0)
		{
			return base.canDespawn();
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isRadiationSensitive()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDetailedHeadBodyColliders()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isGameMessageOnDeath()
	{
		return false;
	}

	public virtual void PlayVoiceSetEntry(string name, EntityPlayer player, bool ignoreTime = true, bool showReactionAnim = true)
	{
	}
}
