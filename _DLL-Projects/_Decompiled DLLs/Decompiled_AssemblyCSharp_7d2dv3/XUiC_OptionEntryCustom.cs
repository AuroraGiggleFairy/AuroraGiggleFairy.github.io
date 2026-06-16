using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionEntryCustom : XUiC_OptionEntryAbs
{
	public delegate void ApplyChangesDelegate(bool _isImmediateApply);

	public Func<bool> IsChangedDelegate;

	public Func<bool> IsDefaultDelegate;

	public Action GetSettingValue;

	public Action DiscardChanges;

	public ApplyChangesDelegate ApplyChanges;

	public Action ResetDefaults;

	public override bool IsChanged => IsChangedDelegate?.Invoke() ?? false;

	public override bool IsDefault => IsDefaultDelegate?.Invoke() ?? true;

	[XuiBindEvent("OnValueChangedGeneric", "comboGeneric")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnComboValueChanged(XUiController _sender)
	{
		SelectionChanged();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void initCurrentValue()
	{
		if (GetSettingValue == null)
		{
			Log.Error("[XUi] " + GetType().Name + ": Can not init current value, delegate not assigned. Hierarchy: " + GetXuiHierarchy());
			return;
		}
		GetSettingValue();
		XUiC_OptionEntryAbs.DebugLog("GOT CURRENT CUSTOMVALUE FOR ");
	}

	public override void DiscardCurrentChange()
	{
		DiscardChanges?.Invoke();
		if (base.ApplyImmediately)
		{
			ApplyChanges?.Invoke(_isImmediateApply: true);
		}
		XUiC_OptionEntryAbs.DebugLog("DISCARDED CUSTOMVALUE FOR ");
	}

	public override void ApplySelection()
	{
		if (ApplyChanges == null)
		{
			Log.Error("[XUi] " + GetType().Name + ": Can not apply changes, delegate not assigned. Hierarchy: " + GetXuiHierarchy());
			return;
		}
		ApplyChanges(_isImmediateApply: false);
		XUiC_OptionEntryAbs.DebugLog("SAVED CUSTOMVALUE FOR ");
		IsDirty = true;
	}

	public override void ResetToDefault()
	{
		if (!base.ApplyDefaults)
		{
			return;
		}
		if (ResetDefaults == null)
		{
			Log.Error("[XUi] " + GetType().Name + ": Can not reset to defaults, delegate not assigned. Hierarchy: " + GetXuiHierarchy());
			return;
		}
		ResetDefaults();
		if (base.ApplyImmediately)
		{
			ApplyChanges?.Invoke(_isImmediateApply: true);
		}
		XUiC_OptionEntryAbs.DebugLog("RESET CUSTOMVALUE FOR ");
		invokeValueChanged();
	}

	public void SelectionChanged()
	{
		if (base.ApplyImmediately)
		{
			ApplyChanges?.Invoke(_isImmediateApply: true);
		}
		invokeValueChanged();
	}
}
