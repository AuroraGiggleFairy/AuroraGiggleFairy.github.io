using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityAnimalRabbit : EntityAnimal
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
		if ((bool)component)
		{
			component.center = new Vector3(0f, 0.15f, 0f);
			component.size = new Vector3(0.4f, 0.4f, 0.4f);
		}
		base.Awake();
		Transform transform = base.transform.Find("Graphics/BlobShadowProjector");
		if ((bool)transform)
		{
			transform.gameObject.SetActive(value: false);
		}
	}

	public override bool IsAttackValid()
	{
		return false;
	}
}
