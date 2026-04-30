using System;
using System.Text.RegularExpressions;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowserGameOptionInputAdvanced : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput valueField;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameInfoInt gameInfoInt;

	public Action<XUiC_ServerBrowserGameOptionInputAdvanced> OnValueChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServersList.UiServerFilter filter;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex rangeMatcher = new Regex("^\\s*(\\d+)\\s*-\\s*(\\d+)\\s*$");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex comparisonMatcher = new Regex("^\\s*(|<|<=|=|==|>=|>|!|!=)\\s*(\\d+)\\s*$");

	public override void Init()
	{
		base.Init();
		gameInfoInt = EnumUtils.Parse<GameInfoInt>(viewComponent.ID);
		valueField = GetChildById("value").GetChildByType<XUiC_TextInput>();
		valueField.OnChangeHandler += ControlText_OnChangeHandler;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ControlText_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		if (!Parse())
		{
			valueField.ActiveTextColor = Color.red;
			return;
		}
		valueField.ActiveTextColor = Color.white;
		if (OnValueChanged != null)
		{
			OnValueChanged(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool Parse()
	{
		string name = gameInfoInt.ToStringCached();
		string text = valueField.Text;
		if (text.Length == 0)
		{
			filter = new XUiC_ServersList.UiServerFilter(name, XUiC_ServersList.EnumServerLists.Regular);
			return true;
		}
		Match match = rangeMatcher.Match(text);
		if (match.Success)
		{
			if (!StringParsers.TryParseSInt32(match.Groups[1].Value, out var minVal))
			{
				return false;
			}
			if (!StringParsers.TryParseSInt32(match.Groups[2].Value, out var maxVal))
			{
				return false;
			}
			filter = new XUiC_ServersList.UiServerFilter(name, XUiC_ServersList.EnumServerLists.Regular, [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) =>
			{
				int value3 = _entry.gameServerInfo.GetValue(gameInfoInt);
				return value3 >= minVal && value3 <= maxVal;
			}, IServerListInterface.ServerFilter.EServerFilterType.IntRange, minVal, maxVal);
			return true;
		}
		match = comparisonMatcher.Match(text);
		if (match.Success)
		{
			string value = match.Groups[1].Value;
			if (!StringParsers.TryParseSInt32(match.Groups[2].Value, out var value2))
			{
				return false;
			}
			int intMinValue = 0;
			int intMaxValue = 0;
			Func<XUiC_ServersList.ListEntry, bool> func;
			IServerListInterface.ServerFilter.EServerFilterType type;
			switch (value)
			{
			case "<":
				func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(gameInfoInt) < value2;
				type = IServerListInterface.ServerFilter.EServerFilterType.IntMax;
				intMaxValue = value2 - 1;
				break;
			case "<=":
			case "=<":
				func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(gameInfoInt) <= value2;
				type = IServerListInterface.ServerFilter.EServerFilterType.IntMax;
				intMaxValue = value2;
				break;
			case "":
			case "=":
			case "==":
				func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(gameInfoInt) == value2;
				type = IServerListInterface.ServerFilter.EServerFilterType.IntValue;
				intMinValue = value2;
				break;
			case ">=":
			case "=>":
				func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(gameInfoInt) >= value2;
				type = IServerListInterface.ServerFilter.EServerFilterType.IntMin;
				intMinValue = value2;
				break;
			case ">":
				func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(gameInfoInt) > value2;
				type = IServerListInterface.ServerFilter.EServerFilterType.IntMin;
				intMinValue = value2 + 1;
				break;
			case "!":
			case "!=":
				func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(gameInfoInt) != value2;
				type = IServerListInterface.ServerFilter.EServerFilterType.IntNotValue;
				intMinValue = value2;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			filter = new XUiC_ServersList.UiServerFilter(name, XUiC_ServersList.EnumServerLists.Regular, func, type, intMinValue, intMaxValue);
			return true;
		}
		return false;
	}

	public XUiC_ServersList.UiServerFilter GetFilter()
	{
		return filter;
	}
}
