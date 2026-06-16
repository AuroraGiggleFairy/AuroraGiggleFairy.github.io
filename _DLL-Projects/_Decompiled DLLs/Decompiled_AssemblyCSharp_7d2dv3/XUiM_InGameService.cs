using System.Text;

public class XUiM_InGameService : XUiModel
{
	public static string GetServiceStats(XUi _xui, InGameService _service)
	{
		if (_service == null)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (_service.ServiceType == InGameService.InGameServiceTypes.VendingRent)
		{
			TraderInfo traderInfo = (_xui.Trader.Trader as TileEntityVendingMachine).TraderData.TraderInfo;
			stringBuilder.Append(StringFormatHandler(Localization.Get("xuiCost"), traderInfo.RentCost));
			stringBuilder.Append(StringFormatHandler(Localization.Get("xuiGameTime"), traderInfo.RentTimeInDays, Localization.Get("xuiGameDays")));
		}
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string StringFormatHandler(string _title, object _value)
	{
		return $"{_title}: [REPLACE_COLOR]{_value}[-]\n";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string StringFormatHandler(string _title, object _value, string _units)
	{
		return $"{_title}: [REPLACE_COLOR]{_value} {_units}[-]\n";
	}
}
