using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityAnimalStag : EntityAnimal
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
		if ((bool)component)
		{
			component.center = new Vector3(0f, 0.85f, 0f);
			component.size = new Vector3(0.8f, 1.6f, 0.8f);
		}
		base.Awake();
	}
}
