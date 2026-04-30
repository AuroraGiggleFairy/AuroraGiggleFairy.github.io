using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerBrowserGameOptionInputSimple : XUiController
{
	[PublicizedFrom(EAccessModifier.Private)]
	public enum EComparisonType
	{
		Smaller,
		SmallerEquals,
		Equals,
		LargerEquals,
		Larger
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Label comparisonLabel;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_TextInput value;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameInfoInt gameInfoInt;

	[PublicizedFrom(EAccessModifier.Private)]
	public EComparisonType currentComparison = EComparisonType.Equals;

	public Action<XUiC_ServerBrowserGameOptionInputSimple> OnValueChanged;

	public override void Init()
	{
		base.Init();
		gameInfoInt = EnumUtils.Parse<GameInfoInt>(viewComponent.ID);
		value = GetChildById("value").GetChildByType<XUiC_TextInput>();
		value.OnChangeHandler += ControlText_OnChangeHandler;
		XUiController childById = GetChildById("comparison");
		childById.OnPress += ComparisonLabel_OnPress;
		comparisonLabel = (XUiV_Label)childById.ViewComponent;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ComparisonLabel_OnPress(XUiController _sender, int _mouseButton)
	{
		currentComparison = currentComparison.CycleEnum(EComparisonType.Smaller, EComparisonType.Larger);
		switch (currentComparison)
		{
		case EComparisonType.Smaller:
			comparisonLabel.Text = "<";
			break;
		case EComparisonType.SmallerEquals:
			comparisonLabel.Text = "<=";
			break;
		case EComparisonType.Equals:
			comparisonLabel.Text = "=";
			break;
		case EComparisonType.LargerEquals:
			comparisonLabel.Text = ">=";
			break;
		case EComparisonType.Larger:
			comparisonLabel.Text = ">";
			break;
		}
		if (OnValueChanged != null)
		{
			OnValueChanged(this);
		}
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
		if (value.Text.Length == 0 || value.Text == "-")
		{
			return new XUiC_ServersList.UiServerFilter(name, XUiC_ServersList.EnumServerLists.Regular);
		}
		int filterVal = StringParsers.ParseSInt32(value.Text);
		IServerListInterface.ServerFilter.EServerFilterType eServerFilterType = IServerListInterface.ServerFilter.EServerFilterType.Any;
		int intMinValue = 0;
		int intMaxValue = 0;
		Func<XUiC_ServersList.ListEntry, bool> func;
		switch (currentComparison)
		{
		case EComparisonType.Smaller:
			func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(gameInfoInt) < filterVal;
			eServerFilterType = IServerListInterface.ServerFilter.EServerFilterType.IntMax;
			intMaxValue = filterVal - 1;
			break;
		case EComparisonType.SmallerEquals:
			func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(gameInfoInt) <= filterVal;
			eServerFilterType = IServerListInterface.ServerFilter.EServerFilterType.IntMax;
			intMaxValue = filterVal;
			break;
		case EComparisonType.Equals:
			func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(gameInfoInt) == filterVal;
			eServerFilterType = IServerListInterface.ServerFilter.EServerFilterType.IntValue;
			intMinValue = filterVal;
			break;
		case EComparisonType.LargerEquals:
			func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(gameInfoInt) >= filterVal;
			eServerFilterType = IServerListInterface.ServerFilter.EServerFilterType.IntMin;
			intMinValue = filterVal;
			break;
		case EComparisonType.Larger:
			func = [PublicizedFrom(EAccessModifier.Internal)] (XUiC_ServersList.ListEntry _entry) => _entry.gameServerInfo.GetValue(gameInfoInt) > filterVal;
			eServerFilterType = IServerListInterface.ServerFilter.EServerFilterType.IntMin;
			intMinValue = filterVal + 1;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return new XUiC_ServersList.UiServerFilter(name, XUiC_ServersList.EnumServerLists.Regular, func, eServerFilterType, intMinValue, intMaxValue);
	}
}
