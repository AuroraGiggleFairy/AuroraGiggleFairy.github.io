using UnityEngine;

public abstract class TGMAbstract
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public float baseHeight;

	public DynamicProperties properties = new DynamicProperties();

	public bool IsSeedSet;

	public abstract void SetSeed(int _seed);

	public abstract float GetValue(float _x, float _z, float _biomeIntens);

	public virtual Vector3 GetNormal(float _x, float _z, float _biomeIntens)
	{
		return Vector3.up;
	}

	public virtual void Init()
	{
		baseHeight = (properties.Values.ContainsKey("BaseHeight") ? StringParsers.ParseFloat(properties.Values["BaseHeight"]) : (-15f));
	}

	public virtual float GetBaseHeight()
	{
		return baseHeight;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public TGMAbstract()
	{
	}
}
