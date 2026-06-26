using UnityEngine.Scripting;

[Preserve]
public class XUiC_AssembleDroneWindow : XUiC_AssembleWindow
{
	public XUiC_DroneWindowGroup group;

	public override ItemStack ItemStack
	{
		set
		{
			group.CurrentVehicleEntity.LoadMods();
			base.ItemStack = value;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		isDirty = true;
	}

	public override void OnChanged()
	{
		group.OnItemChanged(ItemStack);
		isDirty = true;
	}
}
