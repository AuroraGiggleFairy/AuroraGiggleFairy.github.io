using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsUsername : XUiC_OptionsDialogBase
{
	public static string ID = "";

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtUsername;

	public override bool SupportsDefaults
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return false;
		}
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
	}

	[XuiBindEvent("OnSubmitHandler", "txtUsername")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtUsername_OnSubmitHandler(XUiController _sender, string _text)
	{
		Event_ApplyOnPress(_sender, -1);
	}

	[XuiBindEvent("OnChangeHandler", "txtUsername")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtUsername_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!_changeFromCode)
		{
			SetChanged();
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void doSaveChangesInternal()
	{
		base.doSaveChangesInternal();
		GamePrefs.Set(EnumGamePrefs.PlayerName, txtUsername.Text);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		txtUsername.Text = GamePrefs.GetString(EnumGamePrefs.PlayerName);
	}
}
