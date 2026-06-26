using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemDronePartStackGrid : XUiC_ItemPartStackGrid
{
	[field: PublicizedFrom(EAccessModifier.Private)]
	public EntityDrone CurrentVehicle { get; set; }

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = GetChildrenByType<XUiC_ItemDronePartStack>();
		itemControllers = childrenByType;
	}
}
