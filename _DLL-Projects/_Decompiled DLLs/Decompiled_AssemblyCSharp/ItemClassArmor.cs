using UnityEngine.Scripting;

[Preserve]
public class ItemClassArmor : ItemClass
{
	public const string PropEquipSlot = "EquipSlot";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PropArmorGroup = "ArmorGroup";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PropIsCosmetic = "IsCosmetic";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PropKeepOnDeath = "KeepOnDeath";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PropAllowUnEquip = "AllowUnEquip";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PropAutoEquip = "AutoEquip";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PropReplaceByTag = "ReplaceByTag";

	public EquipmentSlots EquipSlot = EquipmentSlots.Count;

	public string[] ArmorGroup;

	public bool IsCosmetic = true;

	public int CosmeticID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool keepOnDeath;

	public bool AllowUnEquip = true;

	public bool AutoEquip;

	public string ReplaceByTag;

	public override bool IsEquipment => true;

	public override bool KeepOnDeath()
	{
		return keepOnDeath;
	}

	public override void Init()
	{
		base.Init();
		string optionalValue = "";
		Properties.ParseString("ArmorGroup", ref optionalValue);
		ArmorGroup = optionalValue.Split(',');
		if (Properties.Values.ContainsKey("EquipSlot"))
		{
			EquipSlot = EnumUtils.Parse<EquipmentSlots>(Properties.Values["EquipSlot"]);
		}
		Properties.ParseBool("IsCosmetic", ref IsCosmetic);
		Properties.ParseBool("KeepOnDeath", ref keepOnDeath);
		Properties.ParseBool("AllowUnEquip", ref AllowUnEquip);
		Properties.ParseBool("AutoEquip", ref AutoEquip);
		Properties.ParseString("ReplaceByTag", ref ReplaceByTag);
	}
}
