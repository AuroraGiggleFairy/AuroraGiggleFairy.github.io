using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorldEditorCreateWorld : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_WorldEditor owner;

	[XuiBindComponent("btnConfirm", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnConfirm;

	[XuiBindComponent("btnCancel", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCancel;

	[XuiBindComponent("txtName", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtName;

	[XuiBindComponent("txtSize", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtSize;

	[XuiBindComponent("cmbSize", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<int> cmbSize;

	[XuiXmlBinding("minworldsize")]
	public int MinWorldSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return 1024;
		}
	}

	[XuiXmlBinding("maxworldsize")]
	public int MaxWorldSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return 16384;
		}
	}

	[XuiXmlBinding("iscustomsize")]
	public bool IsCustomSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxList<int> xUiC_ComboBoxList = cmbSize;
			if (xUiC_ComboBoxList == null)
			{
				return false;
			}
			return xUiC_ComboBoxList.Value == -1;
		}
	}

	public int NewWorldSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!IsCustomSize)
			{
				return cmbSize?.Value ?? (-1);
			}
			if (txtSize != null && int.TryParse(txtSize.Text, out var result))
			{
				return result;
			}
			return -1;
		}
	}

	[XuiXmlBinding("customsizevalid")]
	public bool CustomSizeValid
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (txtSize != null && int.TryParse(txtSize.Text, out var result) && result >= MinWorldSize && result <= MaxWorldSize)
			{
				return result % MinWorldSize == 0;
			}
			return false;
		}
	}

	[XuiXmlBinding("customnamevalid")]
	public bool CustomNameValid
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (txtName != null && txtName.Text.Trim().Length > 0)
			{
				return !SdDirectory.Exists(GameIO.GetGameDir("Data/Worlds") + "/" + txtName.Text);
			}
			return false;
		}
	}

	[XuiXmlBinding("startable")]
	public bool Startable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (CustomNameValid)
			{
				if (IsCustomSize)
				{
					return CustomSizeValid;
				}
				return true;
			}
			return false;
		}
	}

	public override void Init()
	{
		base.Init();
		txtSize.UIInputController.OnScroll += [PublicizedFrom(EAccessModifier.Private)] (XUiController _sender, float _args) =>
		{
			cmbSize.ScrollEvent(_sender, _args);
		};
	}

	public override void OnOpen()
	{
		base.OnOpen();
		txtName.Text = GamePrefs.GetString(EnumGamePrefs.CreateLevelName);
		if (int.TryParse(GamePrefs.GetString(EnumGamePrefs.CreateLevelDim), out var result))
		{
			if (cmbSize.Elements.Contains(result))
			{
				cmbSize.Value = result;
			}
			else
			{
				cmbSize.SelectedIndex = 0;
				txtSize.Text = result.ToString();
			}
		}
		else
		{
			txtSize.Text = "";
		}
		IsDirty = true;
	}

	public override void OnClose()
	{
		base.OnClose();
		owner = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		handleDirtyUpdateDefault();
		if (XUiUtils.HotkeysAllowedFor(viewComponent ?? children[0].ViewComponent))
		{
			if (xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
			{
				BtnCancel_OnPressed(this, 0);
			}
			if (xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
			{
				BtnConfirm_OnPressed(this, 0);
			}
		}
	}

	[XuiBindEvent("OnChangeHandler", "txtName")]
	[XuiBindEvent("OnChangeHandler", "txtSize")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtInput_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		IsDirty = true;
	}

	[XuiBindEvent("OnValueChanged", "cmbSize")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CmbSize_OnValueChanged(XUiController _sender, int _oldValue, int _newValue)
	{
		if (_newValue < 0 && PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
		{
			txtSize.SetSelected(_selected: true, _delayed: true);
		}
		IsDirty = true;
	}

	[XuiBindEvent("OnPress", "btnCancel")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCancel_OnPressed(XUiController _sender, int _mouseButton)
	{
		xui.playerUI.windowManager.Close(windowGroup);
	}

	[XuiBindEvent("OnPress", "btnConfirm")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnConfirm_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (btnConfirm.ViewComponent.Enabled)
		{
			new GameModeEditWorld().ResetGamePrefs();
			string text = txtName.Text.Trim();
			int newWorldSize = NewWorldSize;
			GamePrefs.Set(EnumGamePrefs.GameWorld, text);
			GamePrefs.Set(EnumGamePrefs.CreateLevelName, text);
			GamePrefs.Set(EnumGamePrefs.CreateLevelDim, newWorldSize.ToString());
			MicroStopwatch microStopwatch = new MicroStopwatch(_bStart: true);
			MicroStopwatch microStopwatch2 = new MicroStopwatch(_bStart: true);
			WorldStaticData.Cleanup(null);
			Log.Out($"WSD.Cleanup took {microStopwatch2.ElapsedMilliseconds} ms");
			microStopwatch2.ResetAndRestart();
			WorldStaticData.Reset(null);
			Log.Out($"WSD.Reset took {microStopwatch2.ElapsedMilliseconds} ms");
			microStopwatch2.ResetAndRestart();
			PathAbstractions.AbstractedLocation worldLocation = XUiC_WorldEditor.LocationForNewWorld(text);
			GameUtils.CreateEmptyFlatLevel(text, worldLocation, newWorldSize);
			Log.Out($"Creating empty world took {microStopwatch.ElapsedMilliseconds} ms");
			XUiC_WorldEditor xUiC_WorldEditor = owner;
			xui.playerUI.windowManager.Close(windowGroup);
			xUiC_WorldEditor.StartEditor();
		}
	}

	public static void Open(XUiC_WorldEditor _owner)
	{
		XUiC_WorldEditorCreateWorld childByType = _owner.xui.GetChildByType<XUiC_WorldEditorCreateWorld>();
		childByType.owner = _owner;
		_owner.xui.playerUI.windowManager.Open(childByType.windowGroup, _bModal: false);
	}
}
