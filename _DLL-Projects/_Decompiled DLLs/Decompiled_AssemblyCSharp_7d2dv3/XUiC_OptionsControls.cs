using System.Collections.Generic;
using InControl;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_OptionsControls : XUiC_OptionsControlsBase
{
	public static string ID = "";

	[XuiXmlBinding("mouse_sensitivity_min")]
	public float MouseSensitivityMin => 0.01f;

	[XuiXmlBinding("mouse_sensitivity_max")]
	public float MouseSensitivityMax => 1.5f;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void createControlsEntries()
	{
		SortedDictionary<PlayerActionData.ActionTab, SortedDictionary<PlayerActionData.ActionGroup, List<PlayerAction>>> sortedDictionary = new SortedDictionary<PlayerActionData.ActionTab, SortedDictionary<PlayerActionData.ActionGroup, List<PlayerAction>>>();
		PlayerActionsBase[] array = new PlayerActionsBase[5]
		{
			xui.playerUI.playerInput,
			xui.playerUI.playerInput.VehicleActions,
			xui.playerUI.playerInput.PermanentActions,
			xui.playerUI.playerInput.GUIActions,
			PlayerActionsGlobal.Instance
		};
		for (int i = 0; i < array.Length; i++)
		{
			foreach (PlayerAction action in array[i].Actions)
			{
				if (action.UserData is PlayerActionData.ActionUserData { doNotDisplay: false, appliesToInputType: var appliesToInputType } actionUserData && appliesToInputType != PlayerActionData.EAppliesToInputType.None && appliesToInputType != PlayerActionData.EAppliesToInputType.ControllerOnly)
				{
					if (!sortedDictionary.TryGetValue(actionUserData.actionGroup.actionTab, out var value))
					{
						value = new SortedDictionary<PlayerActionData.ActionGroup, List<PlayerAction>>();
						sortedDictionary.Add(actionUserData.actionGroup.actionTab, value);
					}
					if (!value.TryGetValue(actionUserData.actionGroup, out var value2))
					{
						value2 = new List<PlayerAction>();
						value.Add(actionUserData.actionGroup, value2);
					}
					value2.Add(action);
				}
			}
		}
		int num = 1;
		foreach (KeyValuePair<PlayerActionData.ActionTab, SortedDictionary<PlayerActionData.ActionGroup, List<PlayerAction>>> item in sortedDictionary)
		{
			TabSelector.SetTabCaption(num, Localization.Get(item.Key.tabNameKey));
			XUiC_BindingEntry[] childControllers = TabSelector.GetTab(num).GetChildControllers<XUiC_BindingEntry>("");
			int num2 = 0;
			int num3 = 0;
			foreach (KeyValuePair<PlayerActionData.ActionGroup, List<PlayerAction>> item2 in item.Value)
			{
				if (num2 > 0)
				{
					childControllers[num3].Action = null;
					num3++;
				}
				num2++;
				foreach (PlayerAction item3 in item2.Value)
				{
					childControllers[num3].Action = item3;
					num3++;
				}
			}
			num++;
		}
		IsDirty = true;
	}
}
