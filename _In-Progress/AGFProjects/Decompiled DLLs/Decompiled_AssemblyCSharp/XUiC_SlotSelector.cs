using UnityEngine.Scripting;

[Preserve]
public class XUiC_SlotSelector : XUiC_Selector
{
	public event XUiEvent_SelectedSlotChanged OnSelectedSlotChanged;

	public override void OnOpen()
	{
		base.OnOpen();
		currentValue.Text = ((EquipmentSlots)selectedIndex).ToStringCached();
	}

	public override void BackPressed()
	{
		if (selectedIndex < 0)
		{
			selectedIndex = 11;
		}
		currentValue.Text = ((EquipmentSlots)selectedIndex).ToStringCached();
		if (this.OnSelectedSlotChanged != null)
		{
			this.OnSelectedSlotChanged(selectedIndex);
		}
	}

	public override void ForwardPressed()
	{
		if (selectedIndex >= 12)
		{
			selectedIndex = 0;
		}
		currentValue.Text = ((EquipmentSlots)selectedIndex).ToStringCached();
		if (this.OnSelectedSlotChanged != null)
		{
			this.OnSelectedSlotChanged(selectedIndex);
		}
	}
}
