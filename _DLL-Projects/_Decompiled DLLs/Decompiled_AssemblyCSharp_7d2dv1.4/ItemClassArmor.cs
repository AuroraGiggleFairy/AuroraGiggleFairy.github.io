using UnityEngine.Scripting;

[Preserve]
public class ItemClassArmor : ItemClass
{
	public const string PropEquipSlot = "EquipSlot";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PropArmorGroup = "ArmorGroup";

	public EquipmentSlots EquipSlot = EquipmentSlots.Count;

	public string[] ArmorGroup;

	public override bool IsEquipment => true;

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
	}
}
