using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class vp_DamageInfo
{
	public float Damage;

	public Transform Source;

	public Transform OriginalSource;

	public vp_DamageInfo(float damage, Transform source)
	{
		Damage = damage;
		Source = source;
		OriginalSource = source;
	}

	public vp_DamageInfo(float damage, Transform source, Transform originalSource)
	{
		Damage = damage;
		Source = source;
		OriginalSource = originalSource;
	}
}
