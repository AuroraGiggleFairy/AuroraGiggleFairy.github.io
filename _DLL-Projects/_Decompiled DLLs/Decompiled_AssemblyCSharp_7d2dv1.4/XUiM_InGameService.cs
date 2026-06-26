using System.Text;

public class XUiM_InGameService : XUiModel
{
	public static string GetServiceStats(XUi _xui, InGameService service)
	{
		if (service == null)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (service.ServiceType == InGameService.InGameServiceTypes.VendingRent)
		{
			TileEntityVendingMachine tileEntityVendingMachine = _xui.Trader.TraderTileEntity as TileEntityVendingMachine;
			stringBuilder.Append(StringFormatHandler(Localization.Get("xuiCost"), tileEntityVendingMachine.TraderData.TraderInfo.RentCost));
			stringBuilder.Append(StringFormatHandler(Localization.Get("xuiGameTime"), tileEntityVendingMachine.TraderData.TraderInfo.RentTimeInDays, Localization.Get("xuiGameDays")));
		}
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string StringFormatHandler(string title, object value)
	{
		return $"{title}: [REPLACE_COLOR]{value}[-]\n";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string StringFormatHandler(string title, object value, string units)
	{
		return $"{title}: [REPLACE_COLOR]{value} {units}[-]\n";
	}
}
