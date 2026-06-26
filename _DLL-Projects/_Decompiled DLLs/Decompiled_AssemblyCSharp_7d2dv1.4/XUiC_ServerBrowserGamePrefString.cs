using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowserGamePrefString : XUiController, IServerBrowserFilterControl
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput value;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameInfoString gameInfoString;

	public Action<IServerBrowserFilterControl> OnValueChanged;

	public GameInfoString GameInfoString => gameInfoString;

	public override void Init()
	{
		base.Init();
		gameInfoString = EnumUtils.Parse<GameInfoString>(viewComponent.ID);
		value = GetChildById("value").GetChildByType<XUiC_TextInput>();
		value.OnChangeHandler += ControlText_OnChangeHandler;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetGameInfoName()
	{
		return gameInfoString.ToStringCached();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ControlText_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		OnValueChanged?.Invoke(this);
	}

	public void Reset()
	{
		value.Text = "";
		OnValueChanged?.Invoke(this);
	}

	public void SetValue(string _value)
	{
		value.Text = _value;
		OnValueChanged?.Invoke(this);
	}

	public string GetValue()
	{
		return value.Text;
	}

	public XUiC_ServersList.UiServerFilter GetFilter()
	{
		string name = gameInfoString.ToStringCached();
		string input = value.Text.Trim();
		if (input.Length == 0)
		{
			return new XUiC_ServersList.UiServerFilter(name, XUiC_ServersList.EnumServerLists.Regular);
		}
		Func<XUiC_ServersList.ListEntry, bool> func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(gameInfoString).ContainsCaseInsensitive(input);
		return new XUiC_ServersList.UiServerFilter(name, XUiC_ServersList.EnumServerLists.Regular, func, IServerListInterface.ServerFilter.EServerFilterType.StringContains, 0, 0, _boolValue: false, input);
	}
}
