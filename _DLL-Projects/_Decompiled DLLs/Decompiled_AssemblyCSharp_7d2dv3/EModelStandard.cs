using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EModelStandard : EModelBase
{
	public override void PostInit()
	{
		base.PostInit();
		Transform transform = GetModelTransform();
		if ((bool)transform)
		{
			SetColliderLayers(transform, 0);
		}
	}
}
