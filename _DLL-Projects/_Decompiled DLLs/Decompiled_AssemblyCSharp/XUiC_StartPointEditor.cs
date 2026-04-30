using UnityEngine.Scripting;

[Preserve]
public class XUiC_StartPointEditor : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxInt cbxHeading;

	[PublicizedFrom(EAccessModifier.Private)]
	public SpawnPoint spawnPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public SelectionBox selectionBox;

	[PublicizedFrom(EAccessModifier.Private)]
	public long headingOnOpen;

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		cbxHeading = GetChildById("cbxHeading").GetChildByType<XUiC_ComboBoxInt>();
		cbxHeading.OnValueChanged += CbxHeading_OnValueChanged;
		((XUiC_SimpleButton)GetChildById("btnCancel")).OnPressed += BtnCancel_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnOk")).OnPressed += BtnOk_OnPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(base.WindowGroup);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnOk_OnPressed(XUiController _sender, int _mouseButton)
	{
		headingOnOpen = cbxHeading.Value;
		base.xui.playerUI.windowManager.Close(base.WindowGroup);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxHeading_OnValueChanged(XUiController _sender, long _oldvalue, long _newvalue)
	{
		spawnPoint.spawnPosition.heading = (selectionBox.facingDirection = _newvalue);
		RefreshBindings();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		selectionBox = SelectionBoxManager.Instance.Selection?.box;
		if (!(selectionBox == null))
		{
			spawnPoint = GameManager.Instance.GetSpawnPointList().Find(Vector3i.Parse(selectionBox.name));
			cbxHeading.Value = (headingOnOpen = (long)spawnPoint.spawnPosition.heading);
			RefreshBindings();
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		spawnPoint.spawnPosition.heading = (selectionBox.facingDirection = headingOnOpen);
		spawnPoint = null;
		selectionBox = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		if (!(_bindingName == "position"))
		{
			if (_bindingName == "cardinal")
			{
				_value = GameUtils.GetClosestDirection(spawnPoint?.spawnPosition.heading ?? 0f).ToStringCached();
				return true;
			}
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
		_value = spawnPoint?.spawnPosition.position.ToCultureInvariantString() ?? "";
		return true;
	}
}
