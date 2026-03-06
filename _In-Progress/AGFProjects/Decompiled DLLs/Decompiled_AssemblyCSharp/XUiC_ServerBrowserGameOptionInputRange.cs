using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowserGameOptionInputRange : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput valuemin;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput valuemax;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameInfoInt gameInfoInt;

	public Action<XUiC_ServerBrowserGameOptionInputRange> OnValueChanged;

	public override void Init()
	{
		base.Init();
		gameInfoInt = EnumUtils.Parse<GameInfoInt>(viewComponent.ID);
		valuemin = GetChildById("valuemin").GetChildByType<XUiC_TextInput>();
		valuemin.OnChangeHandler += ControlText_OnChangeHandler;
		valuemax = GetChildById("valuemax").GetChildByType<XUiC_TextInput>();
		valuemax.OnChangeHandler += ControlText_OnChangeHandler;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ControlText_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (OnValueChanged != null)
		{
			OnValueChanged(this);
		}
	}

	public XUiC_ServersList.UiServerFilter GetFilter()
	{
		string name = gameInfoInt.ToStringCached();
		bool flag = valuemin.Text.Length > 0 && valuemin.Text != "-";
		bool flag2 = valuemax.Text.Length > 0 && valuemax.Text != "-";
		Func<XUiC_ServersList.ListEntry, bool> func = null;
		IServerListInterface.ServerFilter.EServerFilterType type = IServerListInterface.ServerFilter.EServerFilterType.Any;
		int filterMin = 0;
		int filterMax = 0;
		if (flag && !flag2)
		{
			filterMin = StringParsers.ParseSInt32(valuemin.Text);
			func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(gameInfoInt) >= filterMin;
			type = IServerListInterface.ServerFilter.EServerFilterType.IntMin;
		}
		else if (flag && flag2)
		{
			filterMin = StringParsers.ParseSInt32(valuemin.Text);
			filterMax = StringParsers.ParseSInt32(valuemax.Text);
			func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) =>
			{
				int value = _entry.gameServerInfo.GetValue(gameInfoInt);
				return value >= filterMin && value <= filterMax;
			};
			type = IServerListInterface.ServerFilter.EServerFilterType.IntRange;
		}
		else if (!flag && flag2)
		{
			filterMax = StringParsers.ParseSInt32(valuemax.Text);
			func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(gameInfoInt) <= filterMax;
			type = IServerListInterface.ServerFilter.EServerFilterType.IntMax;
		}
		return new XUiC_ServersList.UiServerFilter(name, XUiC_ServersList.EnumServerLists.Regular, func, type, filterMin, filterMax);
	}
}
