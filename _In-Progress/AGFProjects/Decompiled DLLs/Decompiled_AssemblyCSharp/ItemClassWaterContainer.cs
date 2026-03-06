using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemClassWaterContainer : ItemClass
{
	[PublicizedFrom(EAccessModifier.Private)]
	public const string PropWaterCapacity = "WaterCapacity";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PropInitFillRatio = "InitialFillRatio";

	[PublicizedFrom(EAccessModifier.Private)]
	public float initialFillRatio;

	[field: PublicizedFrom(EAccessModifier.Private)]
	public int MaxMass
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void Init()
	{
		base.Init();
		float optionalValue = 0f;
		Properties.ParseFloat("WaterCapacity", ref optionalValue);
		MaxMass = Mathf.Clamp((int)(optionalValue * 19500f), 0, 65535);
		Properties.ParseFloat("InitialFillRatio", ref initialFillRatio);
		initialFillRatio = Mathf.Clamp(initialFillRatio, 0f, 1f);
	}

	public override int GetInitialMetadata(ItemValue _itemValue)
	{
		return (int)((float)MaxMass * initialFillRatio);
	}
}
