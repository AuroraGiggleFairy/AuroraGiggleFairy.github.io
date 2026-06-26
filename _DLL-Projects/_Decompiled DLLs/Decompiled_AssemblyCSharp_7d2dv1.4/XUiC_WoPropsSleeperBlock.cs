using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WoPropsSleeperBlock : XUiController
{
	public struct PriorityMultiplier(float _value)
	{
		public readonly float value = _value;

		public override string ToString()
		{
			return "x" + value.ToCultureInvariantString("0.0");
		}
	}

	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxList<PriorityMultiplier> cbxPriority;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSightRange;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtHearingPercent;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput txtSightAngle;

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntitySleeper tileEntitySleeper;

	public static void Open(LocalPlayerUI _playerUi, TileEntitySleeper _te)
	{
		_playerUi.xui.FindWindowGroupByName(ID).GetChildByType<XUiC_WoPropsSleeperBlock>().tileEntitySleeper = _te;
		_playerUi.windowManager.Open(ID, _bModal: true);
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.ID;
		cbxPriority = (XUiC_ComboBoxList<PriorityMultiplier>)GetChildById("cbxPriority");
		for (float num = 0.5f; num < 5.1f; num += 0.5f)
		{
			cbxPriority.Elements.Add(new PriorityMultiplier(num));
		}
		txtSightRange = (XUiC_TextInput)GetChildById("txtSightRange");
		txtHearingPercent = (XUiC_TextInput)GetChildById("txtHearingPercent");
		txtSightAngle = (XUiC_TextInput)GetChildById("txtSightAngle");
		((XUiC_SimpleButton)GetChildById("btnMonsterCloset")).OnPressed += BtnMonsterCloset_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnDefaults")).OnPressed += BtnDefaults_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnSave")).OnPressed += BtnSave_OnPressed;
		((XUiC_SimpleButton)GetChildById("btnCancel")).OnPressed += BtnCancel_OnPressed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnMonsterCloset_OnPressed(XUiController _sender, int _mouseButton)
	{
		updateCbxPriority(1.5f);
		txtSightRange.Text = "4";
		txtHearingPercent.Text = "10";
		txtSightAngle.Text = "60";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDefaults_OnPressed(XUiController _sender, int _mouseButton)
	{
		updateCbxPriority(1f);
		txtSightRange.Text = "-1";
		txtHearingPercent.Text = "100";
		txtSightAngle.Text = "-1";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		tileEntitySleeper.SetPriorityMultiplier(cbxPriority.Value.value);
		tileEntitySleeper.SetSightRange(StringParsers.ParseSInt32(txtSightRange.Text));
		tileEntitySleeper.SetHearingPercent((float)StringParsers.ParseSInt32(txtHearingPercent.Text) / 100f);
		tileEntitySleeper.SetSightAngle(StringParsers.ParseSInt32(txtSightAngle.Text));
		base.xui.playerUI.windowManager.Close(windowGroup.ID);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCbxPriority(float _priorityMultiplier)
	{
		for (int i = 0; i < cbxPriority.Elements.Count; i++)
		{
			if ((double)Mathf.Abs(_priorityMultiplier - cbxPriority.Elements[i].value) < 0.01)
			{
				cbxPriority.SelectedIndex = i;
				break;
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (tileEntitySleeper != null)
		{
			updateCbxPriority(tileEntitySleeper.GetPriorityMultiplier());
			txtSightRange.Text = tileEntitySleeper.GetSightRange().ToString();
			txtHearingPercent.Text = ((int)(tileEntitySleeper.GetHearingPercent() * 100f)).ToString();
			txtSightAngle.Text = tileEntitySleeper.GetSightAngle().ToString();
		}
	}
}
